using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Match
{
    internal static partial class Extensions
    {
        public static int Occurences(this string str, string val)
        {
            int occurrences = 0;
            int startingIndex = 0;

            while ((startingIndex = str.IndexOf(val, startingIndex)) >= 0)
            {
                ++occurrences;
                ++startingIndex;
            }

            return occurrences;
        }
    }
    public class Program
    {
        private BlockingCollection<string> _snippetsToCheck = new();
        private string ProgressBar = "";
        private Regex removeWhitespace = new Regex(@"\s+", RegexOptions.Compiled);

        private async Task GetAnswers(string similarCodeDatabasePath, string mustContain)
        {
            var answersRepo = new AnswersRepository(similarCodeDatabasePath);
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
                    .Select(p => new { p.Content })
                    .ToList();

                prevId = currId;

                // TODO: this could be a bug because page_size could increment and miss the last entries
                currId += pageSize;

                foreach (var item in snippets.AsParallel()
                             .Where(x => removeWhitespace.Replace(x.Content,
                                     "")
                                 .Contains(mustContain,
                                     StringComparison.InvariantCultureIgnoreCase))
                             .Select(x => x.Content))
                    _snippetsToCheck.Add(item);

                // TODO: might not be thread safe
                ProgressBar = $"{currId}/{lastId} ({Math.Round(100 * ((double)currId/lastId), 3)})%";
                Console.WriteLine(ProgressBar);
            }

            Console.WriteLine("Finished loading database");
            _snippetsToCheck.CompleteAdding();
        }

        public async Task<Tuple<string, int>> Start(string needle, string databasePath)
        {
            needle = needle.ToLower();
            needle = needle.Replace("\r\n", "\n");
            var mustMatchSubstr = Compress
                .FindLeastCompressableSubstring(removeWhitespace.Replace(needle, "").AsSpan(), 10).ToString();

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
                MaxDegreeOfParallelism = 20
            }, snippet =>
            {
                if (Math.Abs(snippet.Occurences("\n") - needleLineCount) > 8) return;

                // optimization: check if highest entropy substring exists within snippet
                //if (!snippetWithoutWhitespace.Contains(mustMatchSubstr, StringComparison.InvariantCultureIgnoreCase)) return;

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
