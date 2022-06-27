using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SimilarCode.Load.Models;
using SimilarCode.Load.VirusScanners;

namespace SimilarCode.Load
{
    internal class StackOverflowSnippetExtractor : ISnippetExtractor
    {
        private static readonly Regex RemoveWhitespace = new(@"\s+",  RegexOptions.Compiled);
        private static readonly Regex CodeBlocksMatcher = new(
            @"(?:<pre>|<pre class=\""[A-Za-z0-9 \-_]+\"">)<code>(.*?)</code></pre>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);

        private static List<char> programmingChars = new List<char>
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

        public IEnumerable<CodeSnippetGrouping> Extract(string content)
        {
            // normally, parsing HTML with a regex is considered very bad practice
            // however, our library produces very simple and predictable HTML, and the entities are encoded so we can use a regex
            // an HTML parser library could be used, however, HtmlAgilityPack and another library were too slow and represented about 20-30% of the execution time
            // the regex is so fast it doesn't show up on the execution report
            var codeBlocks = CodeBlocksMatcher.Matches(content).Select(n => n.Groups[1]).Select(p => p.Value).ToList();

            if (!codeBlocks.Any()) yield break;

            yield return new CodeSnippetGrouping
            {
                CodeSnippets = codeBlocks
                    .Select(c => Cleanup(c))
                    .Select(codeBlock => new CodeSnippet { Content = codeBlock,
                        ContentLowerNoWhitespace = RemoveWhitespace.Replace(codeBlock, "").ToLowerInvariant(),
                        ContentAsVector = string.Join(",", ConvertSnippetToVector(RemoveWhitespace.Replace(codeBlock, "").ToLowerInvariant()))
                    }).ToList()
            };
        }

        private static List<int> ConvertSnippetToVector(string code)
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

        private static string Cleanup(string code)
        {
            return ConvertToAscii(HttpUtility.HtmlDecode(code.Trim()));
        }

        private static string ConvertToAscii(string inputString)
        {
            return Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                    Encoding.UTF8.GetBytes(inputString)
                )
            );
        }
    }
}