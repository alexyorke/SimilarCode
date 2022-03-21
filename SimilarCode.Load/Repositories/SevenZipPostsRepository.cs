using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SharpCompress.Archives.SevenZip;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    internal class SevenZipPostsRepository : IPostsRepository
    {
        private const string PostTypeIdAnswer = "2";
        private readonly string? _sourcePath;

        public SevenZipPostsRepository(string? sourcePath)
        {
            this._sourcePath = sourcePath;
        }

        public IEnumerable<(long ReadOffset, long totalSize, Answer answer)> GetAllAnswersAsync()
        {
            using var archive = SevenZipArchive.Open(this._sourcePath);
            var firstFile = archive.Entries.First();
            var totalSize = firstFile.Size;
            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = false
            };
            var firstFileStream = new StreamWithReadOnlyPosition(firstFile.OpenEntryStream());
            using var sr = new StreamReader(firstFileStream);
            using var xmlReader = XmlReader.Create(sr, settings);
            xmlReader.ReadToDescendant("posts");
            xmlReader.ReadToDescendant("row");

            do
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "row")
                {
                    continue;
                }

                if (xmlReader.GetAttribute("PostTypeId") != PostTypeIdAnswer)
                {
                    continue;
                }

                var body = xmlReader.GetAttribute("Body");

                if (body == null)
                {
                    continue;
                }

                var answer = new Answer
                {
                    Id = Convert.ToInt32(xmlReader.GetAttribute("Id")),
                    Body = body
                };

                yield return (firstFileStream.ReadOffset, totalSize, answer);

            } while (xmlReader.Read());
        }

    }
}