using System.Collections.Generic;
using System.Linq;
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
                    .Select(codeBlock => new CodeSnippet { Content = codeBlock, ContentLowerNoWhitespace = RemoveWhitespace.Replace(codeBlock, "").ToLowerInvariant() }).ToList()
            };
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