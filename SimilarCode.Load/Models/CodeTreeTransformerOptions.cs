using System.Collections.Generic;

namespace SimilarCode.Load.Models
{
    public class CodeTreeTransformerOptions
    {
        public IReadOnlyList<string> Languages = new List<string> { "c", "cs", "java", "php", "py", "rb", "rs" };
    }
}