using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VisualizeCloneReport
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            var tr = new FileStream(@"T:\so_internal_matches.xml", FileMode.Open);
            using XmlReader reader = XmlReader.Create(tr, settings);
            var cloneGroups = new Dictionary<string, HashSet<string>>();
            while (await reader.ReadAsync())
            {
                var cloneContainer = await reader.ReadOuterXmlAsync();
                var clones = XElement.Parse(cloneContainer).Elements("clone");
                foreach (var clone in clones)
                {
                    var filenames = clone.Elements("source").ToList().Select(s =>
                        $"{s.Attribute("file").Value}:{s.Attribute("startline").Value}-{s.Attribute("endline").Value}");
                    var sourceFilename = filenames.First();
                    var stackoverflowFilename = filenames.ElementAt(1);
                    if (sourceFilename == stackoverflowFilename) continue;

                    sourceFilename = ConvertPathToStackOverflowId(sourceFilename);
                    stackoverflowFilename = ConvertPathToStackOverflowId(stackoverflowFilename);
                    if (!cloneGroups.ContainsKey(sourceFilename))
                        cloneGroups[sourceFilename] = new HashSet<string>();

                    cloneGroups[sourceFilename].Add(stackoverflowFilename);
                }
            }

            await File.WriteAllTextAsync(@"T:\stackoverflow_similar_code.json", string.Join(Environment.NewLine, cloneGroups.Select(c => string.Join(Environment.NewLine, c.Value.Select(d =>
                $"{c.Key} -> {d}")))));
            foreach (var cloneGroup in cloneGroups)
            {
                Console.WriteLine(cloneGroup.Key);
                cloneGroup.Value.ToList().ForEach(Console.WriteLine);
                Console.WriteLine();
            }
        }

        private static string ConvertPathToStackOverflowUrl(string path)
        {
            string pattern = @"a/\d+\/(\d+)\#\d\.cs";

            Match m = Regex.Match(path, pattern);
            return $"https://stackoverflow.com/a/{m.Groups[1].Value}";
        }

        private static string ConvertPathToStackOverflowId(string path)
        {
            string pattern = @"a/\d+\/(\d+)\#\d\.cs";

            Match m = Regex.Match(path, pattern);
            return m.Groups[1].Value;
        }
    }
}
