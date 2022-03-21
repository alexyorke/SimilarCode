using SimilarCode.Load.Models;

namespace SimilarCode.Load.VirusScanners
{
    public class NullCodeTreeTransformer : ICodeTreeTransformer
    {
        public CodeSnippet Transform(CodeSnippet code)
        {
            return code;
        }
    }
}