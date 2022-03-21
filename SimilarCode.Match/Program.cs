using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Match
{
    internal class Program
    {
        private static BlockingCollection<string> _snippetsToCheck = new(50_000);
        private static string ProgressBar = "";

        private static async Task GetAnswers()
        {
            using var answersRepo = new AnswersRepository(@"L:\stackoverflow\SimilarCode.db");
            var context = answersRepo.GetContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var answers = context.Answers.OrderBy(a => a.Id);
            const int pageSize = 200_000;

            var firstId = answers.First().Id;
            var lastId = answers.Last().Id;
            var prevId = firstId;
            var currId = firstId + pageSize;

            while (currId < lastId)
            {
                var snippets = context.Answers.Where(a => a.Id >= prevId && a.Id < currId)
                    .Include(a => a.CodeSnippetGroups)
                    .ThenInclude(b => b.CodeSnippets)
                    .SelectMany(snippet => snippet.CodeSnippetGroups)
                    .SelectMany(c => c.CodeSnippets)
                    //.Where(s => s.ProgrammingLanguage.Select(p => p.Language).Contains("cs"))
                    .Select(p => new {p.Content});

                prevId = currId;

                // TODO: this could be a bug because page_size could increment and miss the last entries
                currId += pageSize;

                await foreach (var snippet in snippets.ToAsyncEnumerable())
                {
                    _snippetsToCheck.Add(snippet.Content);
                }

                // TODO: might not be thread safe
                ProgressBar = $"{currId}/{lastId} ({Math.Round(100 * ((double)currId/lastId), 3)})%";
            }

            _snippetsToCheck.CompleteAdding();
        }

        private static async Task Main(string[] args)
        {
            Task getSnippets = new(() => GetAnswers(), TaskCreationOptions.LongRunning);
            getSnippets.Start();
            
            var needle =
                "public static void compressFile(string inFile, string outFile){\nSystem.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);\r\n            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);\r\n            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);\r\n            try\r\n            {\r\n                CopyStream(inFileStream, outZStream);\r\n\t\t\t\toutZStream.finish();\r\n            }\r\n            finally\r\n            {\r\n                outZStream.Close();\r\n                outFileStream.Close();\r\n                inFileStream.Close();\r\n            }\r\n        }";
            needle = needle.ToLower();
            needle = needle.Replace("\r\n", "\n");
            var needleLineCount = needle.Split('\n').Length;
            
            var bestSnippets = new ConcurrentBag<Tuple<string, int>>();

            int lowestPenalty = -1;
            _snippetsToCheck.GetConsumingEnumerable().AsParallel().WithDegreeOfParallelism(4).ForAll(snippet =>
            {
                if (Math.Abs(snippet.Split('\n').Length - needleLineCount) > 8) return;
                var penalty = Gfg.GetMinimumPenaltyOptimizedMem(needle,
                    snippet.AsSpan());

                if (lowestPenalty == -1 || penalty < lowestPenalty)
                {
                    Interlocked.Exchange(ref lowestPenalty, penalty);
                    bestSnippets.Add(Tuple.Create(snippet, penalty));
                    Console.Clear();
                    Console.WriteLine(snippet);
                    Console.WriteLine(ProgressBar);
                }
            });

            await Task.WhenAll(getSnippets);

            var bestSnippet = bestSnippets.ToList().OrderBy(c => c.Item2).FirstOrDefault();
            Console.WriteLine(bestSnippet);
        }
    }
}
