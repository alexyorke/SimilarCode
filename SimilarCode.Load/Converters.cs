using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimilarCode.Load.Models;
using SimilarCode.Load.Repositories;
using SimilarCode.Load.VirusScanners;

namespace SimilarCode.Load
{
    public static class Converters
    {
        public static async Task ProcessAnswers(CliOptions cliOptions, BlockingCollection<ProgressItem<Answer>> dbWriter, ICodeTreeTransformer? codeTreeTransformer = null)
        {
            codeTreeTransformer ??= new NullCodeTreeTransformer();
            using BlockingCollection<ProgressItem<Answer>> _answersToProcess = new(50_000);

            var readAnswersRunner = new Task(() =>
            {
                var ext = Path.GetExtension(cliOptions.SoPostsPath).ToUpperInvariant();
                IPostsRepository postsRepository = ext switch
                {
                    ".7Z" => PostsRepositoryFactory.GetRepository(cliOptions.SoPostsPath, DatabaseType.SevenZipFast),
                    ".XML" => PostsRepositoryFactory.GetRepository(cliOptions.SoPostsPath, DatabaseType.UncompressedFast),
                    _ => throw new InvalidDataException(
                        $"The database with extension {ext} does not have an associated provider.")
                };

                ReadAnswersCore(postsRepository, _answersToProcess);
            }, TaskCreationOptions.LongRunning);

            readAnswersRunner.Start();

            // process answers
            ISnippetExtractor stackoverflowExtractor = new StackOverflowSnippetExtractor();
            IMaliciousSoftwareScanner scanner = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new Amsi()
                : new NullCodeScanner();

            _answersToProcess.GetConsumingEnumerable().AsParallel().WithDegreeOfParallelism(cliOptions.MaxThreads).ForAll(answerProgressItem =>
            {
                var answer = answerProgressItem.Item;

                var codeSnippetGroupings = stackoverflowExtractor.Extract(answer.Body)
                    .Where(s =>
                        !s.CodeSnippets.Any(c => scanner.IsMalicious(c.Content))).ToList();

                foreach (var codeSnippetGrouping in codeSnippetGroupings)
                {
                    var changedSnippets = codeSnippetGrouping.CodeSnippets.Select(codeSnippet => codeTreeTransformer.Transform(codeSnippet)).ToList();
                    codeSnippetGrouping.CodeSnippets = changedSnippets;
                }

                answerProgressItem.Item.CodeSnippetGroups = codeSnippetGroupings;
                dbWriter.Add(answerProgressItem);
            });

            dbWriter.CompleteAdding();

            await readAnswersRunner;

            // publishing the project as .NET 6 self-contained executable for Linux causes the "using" statements to prematurely dispose
            // which is why they are being disposed of manually
            _answersToProcess.Dispose();
            scanner.Dispose();
        }

        private static void ReadAnswersCore(IPostsRepository postsRepository,
            BlockingCollection<ProgressItem<Answer>> answersToProcess)
        {
            var answers = postsRepository.GetAllAnswersAsync();
            foreach (var (readOffset, totalSize, answer) in answers)
            {
                var itemToAdd = new ProgressItem<Answer> { Item = answer, CompletedUnits = readOffset, TotalUnits = totalSize };
                answersToProcess.Add(itemToAdd);
            }

            answersToProcess.CompleteAdding();
        }
    }
}