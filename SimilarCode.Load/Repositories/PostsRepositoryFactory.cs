using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using SharpCompress.Archives.SevenZip;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    internal static class PostsRepositoryFactory
    {
        public static IPostsRepository GetRepository(string? path, DatabaseType type)
        {
            return type switch
            {
                DatabaseType.SevenZip => new SevenZipPostsRepository(path),
                DatabaseType.Uncompressed => new UncompressedPostsRepository(path),
                DatabaseType.UncompressedFast => new UncompressedFastPostsRepository(path),
                _ => throw new InvalidOperationException("Posts repository type not found.")
            };
        }
    }

    internal class UncompressedFastPostsRepository : IPostsRepository
    {
        private const string PostTypeIdAnswer = "2";
        private readonly string? _sourcePath;

        public UncompressedFastPostsRepository(string? sourcePath)
        {
            this._sourcePath = sourcePath;
        }

        public IEnumerable<(long ReadOffset, long totalSize, Answer answer)> GetAllAnswersAsync()
        {
            var totalSize = 70_000_000;
            var readOffset = 0;

            var rowsToProcess = new BlockingCollection<string>(500_000);
            var answersWithProgress = new BlockingCollection<(long ReadOffset, long totalSize, Answer answer)> (500_000);

            var reader = new Task(async () =>
            {
                foreach (var line in File.ReadLines(this._sourcePath))
                {
                    rowsToProcess.Add(line);
                    readOffset++;
                }

                rowsToProcess.CompleteAdding();
            }, TaskCreationOptions.LongRunning);

            var rowProcessor = new Task(() =>
            {
                rowsToProcess.GetConsumingEnumerable().AsParallel().WithDegreeOfParallelism(20).ForAll(row => {
                    XmlDocument doc = new XmlDocument();

                    try
                    {
                        doc.LoadXml(row);
                    }
                    catch (XmlException e)
                    {
                        //Console.WriteLine(e);
                        return;
                    }

                    if (doc.DocumentElement.NodeType != XmlNodeType.Element || doc.DocumentElement.Name != "row")
                    {
                        return;
                    }

                    if (doc.DocumentElement.Attributes["PostTypeId"]?.InnerText != PostTypeIdAnswer)
                    {
                        return;
                    }

                    var body = doc.DocumentElement.Attributes["Body"]?.InnerText;

                    if (body == null)
                    {
                        return;
                    }

                    var answer = new Answer
                    {
                        Id = Convert.ToInt32(doc.DocumentElement.Attributes["Id"]?.InnerText),
                        Body = body
                    };

                    answersWithProgress.Add((readOffset, totalSize, answer));
                    });

                answersWithProgress.CompleteAdding();
            }, TaskCreationOptions.LongRunning);

            rowProcessor.Start();
            reader.Start();

            foreach (var item in answersWithProgress.GetConsumingEnumerable())
            {
                yield return item;
            }

            Task.WaitAll(rowProcessor, reader);
        }
    }
}