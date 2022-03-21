using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    internal class UncompressedPostsRepository : IPostsRepository
    {
        private const string PostTypeIdAnswer = "2";
        private readonly string? _sourcePath;

        public UncompressedPostsRepository(string? sourcePath)
        {
            this._sourcePath = sourcePath;
        }

        public IEnumerable<(long ReadOffset, long totalSize, Answer answer)> GetAllAnswersAsync()
        {
            var totalSize = new System.IO.FileInfo(this._sourcePath).Length;

            using var sr = new StreamReader(this._sourcePath);
            using var xmlReader = XmlReader.Create(sr);
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

                yield return (sr.BaseStream.Position, totalSize, answer);

            } while (xmlReader.Read());
        }

    }
}