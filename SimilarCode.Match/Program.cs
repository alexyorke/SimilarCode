using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Match
{
    public class Program
    {
        private BlockingCollection<string> _snippetsToCheck = new(50_000);
        private string ProgressBar = "";

        private async Task GetAnswers(string similarCodeDatabasePath)
        {
            using var answersRepo = new AnswersRepository(similarCodeDatabasePath);
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

        public async Task<Tuple<string, int>> Start(string needle, string databasePath)
        {
            Task getSnippets = new(() => GetAnswers(databasePath), TaskCreationOptions.LongRunning);
            getSnippets.Start();
            
            needle = needle.ToLower();
            needle = needle.Replace("\r\n", "\n");
            var needleLineCount = needle.Split('\n').Length;
            var mustMatchSubstr = Compress.FindLeastCompressableSubstring(needle.Replace(" ", "").Replace("\t", "").AsSpan(), 5).ToString();

            var bestSnippets = new ConcurrentBag<Tuple<string, int>>();

            int lowestPenalty = -1;
            _snippetsToCheck.GetConsumingEnumerable().AsParallel().WithDegreeOfParallelism(2).ForAll(snippet =>
            {
                if (Math.Abs(snippet.Split('\n').Length - needleLineCount) > 8) return;

                // optimization: check if highest entropy substring exists within snippet
                //if (!snippet.ToLower().Replace(" ", "").Replace("\t", "").Contains(mustMatchSubstr)) return;

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

            var bestSnippet = bestSnippets.ToList().MinBy(c => c.Item2);
            return bestSnippet;
        }
    }
}
