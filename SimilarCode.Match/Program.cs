using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Match
{
    public class Program
    {
        private BlockingCollection<StrongBox<string>> _snippetsToCheck = new(200_000);
        private Regex removeWhitespace = new Regex(@"\s+", RegexOptions.Compiled);

        private async Task GetAnswers(string similarCodeDatabasePath, string mustContain)
        {
            var pages = new List<Page>();

            var answersRepo = new AnswersRepository(similarCodeDatabasePath);
            {
                var context = answersRepo.GetContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var answers = context.Answers.OrderBy(a => a.Id);
                const int pageSize = 100_000;

                var firstId = answers.First().Id;
                var lastId = answers.Last().Id;
                var prevId = firstId;
                var currId = firstId + pageSize;

                while (currId < lastId)
                {
                    pages.Add(new Page { currId = currId, prevId = prevId, lastId=lastId });
                    prevId = currId;

                    currId += pageSize;
                }
            }
            Console.WriteLine(Environment.ProcessorCount);
            pages.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll(page =>
            {
                var answersRepo = new AnswersRepository(similarCodeDatabasePath);
                using var context = answersRepo.GetContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var snippets = context.Answers.Where(a => a.Id >= page.prevId && a.Id < page.currId)
                    .Include(a => a.CodeSnippetGroups)
                    .ThenInclude(b => b.CodeSnippets)
                    .SelectMany(snippet => snippet.CodeSnippetGroups)
                    .SelectMany(c => c.CodeSnippets)
                    //.Where(s => s.ProgrammingLanguage.Select(p => p.Language).Contains("cs"))
                    .Select(p => new { p.Content, p.ContentLowerNoWhitespace });

                foreach (var item in snippets.Where(x => x.ContentLowerNoWhitespace.Contains(mustContain))
                             .Select(x => x.Content))
                {
                    _snippetsToCheck.Add(new StrongBox<string>(item));
                }

                // TODO: might not be thread safe
                var ProgressBar = $"Finished: {page.currId}/{page.lastId} ({Math.Round(100 * ((double)page.currId / page.lastId), 3)})%";
                Console.WriteLine(ProgressBar);
            });

            Console.WriteLine("Finished loading database");
            _snippetsToCheck.CompleteAdding();
        }

        public async Task<Tuple<string, int>> Start(string needle, string databasePath)
        {
            needle = needle.ToLower();
            needle = needle.Replace("\r\n", "\n");
            var mustMatchSubstr = Compress
                .FindLeastCompressableSubstring(removeWhitespace.Replace(needle, "").AsSpan(), 2).ToString();

            Task getSnippets = new(() => GetAnswers(databasePath, mustMatchSubstr), TaskCreationOptions.LongRunning);
            getSnippets.Start();

            var needleLineCount = needle.Split('\n').Length;

            var bestSnippets = new ConcurrentBag<Tuple<string, int>>();

            int lowestPenalty = Int32.MaxValue;
            var hasStarted = false;
            var totalSnippets = _snippetsToCheck.Count;
            var snippetsProcessedSoFar = 0;

            Parallel.ForEach(_snippetsToCheck.GetConsumingEnumerable(), new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, snippetLoop =>
            {
                var snippet = snippetLoop.Value;
                if (Math.Abs(snippet.Occurences("\n") - needleLineCount) > 8) return;

                // if the best possible theoretical penalty is worst than the one that we have so far,
                // don't bother trying to match it because it's impossible it will be better
                int bestPossibleScore = (int)(Math.Min(3, 2) * Math.Min(snippet.Length, needle.Length));
                if (bestPossibleScore > lowestPenalty)
                {
                    return;
                }

                // TODO: find similar snippets based on character frequency (and theoretical largest penalty)

                var penalty = Gfg.GetMinimumPenaltyOptimizedMem(needle,
                    snippet.AsSpan());

                if (penalty < lowestPenalty)
                {
                    Interlocked.Exchange(ref lowestPenalty, penalty);
                    bestSnippets.Add(Tuple.Create(snippet, penalty));
                }

                Interlocked.Increment(ref snippetsProcessedSoFar);
                if (snippetsProcessedSoFar % 100_000 == 0)
                {
                    Console.WriteLine(snippetsProcessedSoFar + "/" + totalSnippets);
                }
            });

            await getSnippets;

            var bestSnippet = bestSnippets.ToList().MinBy(c => c.Item2);
            return bestSnippet;
        }
    }
}
