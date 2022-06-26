using System.Collections.Concurrent;
using SimilarCode.Load.Models;
using SimilarCode.Load.Repositories;

namespace SimilarCode.Load.Cli
{
    static internal class Database
    {
        public static async Task DbWriterCore(string? dbPath, BlockingCollection<ProgressItem<Answer>> dbWriter)
        {
            using var progress = new ProgressBar();

            foreach (var answersToWrite in dbWriter.GetConsumingEnumerable().ChunksOf(100_000))
            {
                using ICodeRepository answersRepo = new AnswersH5Repository(dbPath);
                await answersRepo.AddRangeAsync(answersToWrite.Select(a => a.Item).ToList());

                // update progress bar
                var lastProgressItem = answersToWrite.Last();
                progress.Report(lastProgressItem.CompletedUnits / (double)lastProgressItem.TotalUnits);
            }
        }
    }
}