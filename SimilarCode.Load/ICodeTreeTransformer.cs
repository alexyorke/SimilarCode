using SimilarCode.Load.Models;

namespace SimilarCode.Load
{
    public interface ICodeTreeTransformer
    {
        public CodeSnippet Transform(CodeSnippet code);
    }
}