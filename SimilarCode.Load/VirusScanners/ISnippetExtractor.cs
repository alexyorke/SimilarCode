using System.Collections.Generic;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.VirusScanners
{
    internal interface ISnippetExtractor
    {
        public IEnumerable<CodeSnippetGrouping> Extract(string content);
    }
}