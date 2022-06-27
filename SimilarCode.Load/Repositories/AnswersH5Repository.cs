using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HDF5CSharp;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    public class AnswersH5Repository : IDisposable, ICodeRepository
    {
        private long fileId;
        private ChunkedDataset<double> dset;
        private readonly string filename;

        public AnswersH5Repository(string filename)
        {
            this.filename = filename;
        }

        public Task AddRangeAsync(IEnumerable<Answer> answers)
        {
            foreach (var answer in answers)
            {
                foreach (var codeSnippetGroup in answer.CodeSnippetGroups)
                {
                    foreach (var codeSnippet in codeSnippetGroup.CodeSnippets)
                    {
                        var v = string.Join(",", Utilities.ConvertSnippetToVector(codeSnippet.ContentLowerNoWhitespace));
                        File.AppendAllText(filename, answer.Id + "," + v + "\n");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    public static class Utilities
    {
        public static List<char> programmingChars = new List<char>
        {
            '!',
            '"',
            '#',
            '$',
            '%',
            '&',
            '\'',
            '(',
            ')',
            '*',
            '+',
            ',',
            '-',
            '.',
            '/',
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            ':',
            ';',
            '<',
            '=',
            '>',
            '?',
            '@',
            '[',
            '\\',
            ']',
            '^',
            '_',
            '`',
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z',
            '{',
            '|',
            '}',
            '~'
        };

        public static List<int> ConvertSnippetToVector(string code)
        {
            var arr = new int[programmingChars.Count];
            foreach (var c in code)
            {
                var i = programmingChars.IndexOf(c);
                if (i != -1)
                {
                    arr[i]++;
                }
            }

            return new List<int>(arr);
        }
    }
}