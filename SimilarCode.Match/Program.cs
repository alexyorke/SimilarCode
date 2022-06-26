using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Match
{
    public class Program
    {
        private List<int[]> _snippetsToCheck = new();

        private async Task GetAnswers(string similarCodeDatabasePath, string mustContain)
        {
            var currLine = 0;
            const int totalLines = 33496610;
            foreach (var snippet in ReadCsv(@"C:\Users\Alex Yorke\Desktop\SimilarCode.db"))
            {
                _snippetsToCheck.Add(Array.ConvertAll(snippet, int.Parse));

                if (currLine % 100_000 == 0)
                {
                    Console.WriteLine(currLine / (decimal)totalLines);
                }

                currLine++;
            }

            Console.WriteLine("Finished loading database");
            Console.WriteLine("Waiting 10 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        private IEnumerable<string[]> ReadCsv(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line.Split(',');
                }
            }
        }

        public async Task<Tuple<string, double>> Start(string needle, string databasePath)
        {
            needle = needle.ToLower();
            var needleThumbprint = Utilities.ConvertSnippetToVector(needle).ToArray();

            Task getSnippets = new(() => GetAnswers(databasePath, ""), TaskCreationOptions.LongRunning);
            getSnippets.Start();
            await getSnippets;
            var start = DateTime.Now;

            var bestSnippets = new ConcurrentBag<Tuple<string, double>>();

            double lowestPenalty = Double.MaxValue;
            var totalSnippets = _snippetsToCheck.Count;
            var snippetsProcessedSoFar = 0;

            Parallel.ForEach(_snippetsToCheck, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, snippet =>
            {
                // TODO: find similar snippets based on character frequency (and theoretical largest penalty)

                double penalty = 0;
                for (int i = 1; i < snippet.Length; i++)
                {
                    penalty += Math.Pow((snippet[i] - needleThumbprint[i]), 2);
                }

                if (penalty < lowestPenalty)
                {
                    Interlocked.Exchange(ref lowestPenalty, penalty);
                    bestSnippets.Add(Tuple.Create(snippet[0].ToString(), penalty));
                }

                Interlocked.Increment(ref snippetsProcessedSoFar);
                if (snippetsProcessedSoFar % 100_000 == 0)
                {
                    Console.WriteLine(snippetsProcessedSoFar + "/" + totalSnippets);
                }
            });

            await getSnippets;

            var bestSnippet = bestSnippets.ToList().MinBy(c => c.Item2);

            var end = DateTime.Now;
            Console.WriteLine(end - start);
            return bestSnippet;
        }
    }
}
