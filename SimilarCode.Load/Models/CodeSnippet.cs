using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimilarCode.Load.Models
{
    public class CodeSnippet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Content { get; set; }
        public List<ProgrammingLanguage> ProgrammingLanguage { get; set; } = new();
        public CodeSnippetGrouping CodeSnippetGrouping { get; set; }
    }
}