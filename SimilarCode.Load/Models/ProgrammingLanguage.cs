using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimilarCode.Load.Models
{
    public class ProgrammingLanguage
    {
        private sealed class LanguageEqualityComparer : IEqualityComparer<ProgrammingLanguage>
        {
            public bool Equals(ProgrammingLanguage x, ProgrammingLanguage y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Language == y.Language;
            }

            public int GetHashCode(ProgrammingLanguage obj)
            {
                return obj.Language.GetHashCode();
            }
        }

        public static IEqualityComparer<ProgrammingLanguage> LanguageComparer { get; } = new LanguageEqualityComparer();

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Language { get; set; }
    }
}