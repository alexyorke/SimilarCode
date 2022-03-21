using System.Collections.Generic;

namespace SimilarCode.Load.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public List<CodeSnippetGrouping> CodeSnippetGroups { get; set; } = new();

        public string Body;
    }
}