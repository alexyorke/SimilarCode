using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimilarCode.Load.Models
{
    public class CodeSnippetGrouping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public List<CodeSnippet> CodeSnippets { get; set; } = new();
        public Answer Answer { get; set; }
    }
}