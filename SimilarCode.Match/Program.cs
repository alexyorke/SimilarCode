using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace SimilarCode.Match
{
    public class Program
    {
        private int[][] _snippetsToCheck;

        private void GetAnswers(string similarCodeDatabasePath)
        {
            const int totalLines = 33496610;
            _snippetsToCheck = new int[totalLines][];
            var currLine = 0;

            foreach (var snippet in ReadCsv(similarCodeDatabasePath))
            {
                _snippetsToCheck[currLine] = snippet;
                if (currLine % 100_000 == 0)
                {
                    Console.WriteLine(currLine / (decimal)totalLines);
                }

                currLine++;
            }
        }

        private static IEnumerable<int[]> ReadCsv(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan);
            using var reader = new StreamReader(fs);
            while (reader.ReadLine() is { } line)
            {
                yield return Array.ConvertAll(line.Split(','), int.Parse);
            }
        }

        public Tuple<string, double> Start(string needle, string databasePath)
        {
            needle = needle.ToLower();
            var needleArray = Utilities.ConvertSnippetToVector(needle).Select(Convert.ToInt32).ToArray();

            Task getSnippets = new(() => GetAnswers(databasePath), TaskCreationOptions.LongRunning);
            getSnippets.Start();
            getSnippets.Wait();

            var start = DateTime.Now;

            var bestSnippets = new ConcurrentBag<Tuple<string, double>>();

            double lowestPenalty = Double.MaxValue;
            var totalSnippets = _snippetsToCheck.Length;
            var snippetsProcessedSoFar = 0;

            _snippetsToCheck.AsParallel().ForAll(answerAndSnippet =>
            {
                if (answerAndSnippet == null) return;
                var answerAndSnipperAsSpan = answerAndSnippet.AsSpan();

                var snippetBody = answerAndSnipperAsSpan[1..];
                double penalty = 0;

                for (int i = 0; i < needleArray.Length; i++)
                {
                    penalty += Math.Pow(needleArray[i] - snippetBody[i], 2);
                }

                if (penalty < lowestPenalty)
                {
                    var answerId = answerAndSnipperAsSpan[0];
                    Interlocked.Exchange(ref lowestPenalty, penalty);
                    bestSnippets.Add(Tuple.Create(answerId.ToString(), penalty));
                }

                Interlocked.Increment(ref snippetsProcessedSoFar);
                if (snippetsProcessedSoFar % 100_000 == 0)
                {
                    Console.WriteLine(snippetsProcessedSoFar + "/" + totalSnippets);
                }
            });

            var bestSnippet = bestSnippets.ToList().MinBy(c => c.Item2);

            var end = DateTime.Now;
            Console.WriteLine(end - start);
            return bestSnippet;
        }
    }
}
