using SimilarCode.Load.Models;
using SimpleExec;

namespace SimilarCode.Load.Cli
{
    internal class TxlTransformer : ICodeTreeTransformer
    {
        private readonly TxlOptions options;
        private readonly CodeTreeTransformerOptions transformerOptions;

        public TxlTransformer(TxlOptions options, CodeTreeTransformerOptions transformerOptions)
        {
            this.options = options;
            this.transformerOptions = transformerOptions;

        }
        public CodeSnippet? Transform(CodeSnippet code)
        {
            using var tmpFile = new TemporaryFile(code.Content);

            foreach (var language in transformerOptions.Languages)
            {
                var extractedFunctionsTask = WithLanguage(language, "functions", tmpFile);
                var extractedBlocksTask = WithLanguage(language, "blocks", tmpFile);

                var extractedFunctions = extractedFunctionsTask.Result;
                var extractedBlocks = extractedBlocksTask.Result;

                if (extractedFunctions.success || extractedBlocks.success)
                {
                    var (functions, blocks) = SeperateFunctionsAndBlocks(extractedFunctions.output, extractedBlocks.output);
                    var snippet = new CodeSnippet
                    {
                        Content = string.Join('\n', functions) + "\n\n" + string.Join('\n', blocks),
                        CodeSnippetGrouping = code.CodeSnippetGrouping,
                        Id = code.Id,
                        ProgrammingLanguage = code.ProgrammingLanguage
                    };
                    return snippet;
                }
            }

            return null;
        }

        private async Task<(string output, bool success)> WithLanguage(string programmingLang,
            string type, TemporaryFile tmpFile)
        {
            var grammarPath = Path.Join(options.GrammarsFolder, $"{programmingLang}-extract-{type}.ctxl");
            var args = $"-w 999999 -l {tmpFile.Path} {grammarPath} - {tmpFile.Path}";
            bool success = false;
            string stdout = string.Empty;
            string stderr = string.Empty;

            try
            {
                (stdout, stderr) = await Command.ReadAsync(options.PathToTxl, args);
                if (!string.IsNullOrWhiteSpace(stdout) && string.IsNullOrWhiteSpace(stderr)) success = true;
            }
            catch (ExitCodeReadException)
            {

            }

            return (stdout.Trim(), success);
        }

        private static (IEnumerable<string> seperatedFunctions, IEnumerable<string> seperatedBlocks) SeperateFunctionsAndBlocks(string functions, string blocks)
        {
            var separatingStrings = new[] { "</source>" };

            string RemoveNiCadHeader(string l)
            {
                // remove NiCad header
                var lines = l.Trim().Replace("\r\n", "\n").Split('\n');
                lines[0] = string.Empty;
                return string.Join("\n", lines.Select(l => l.TrimEnd(' '))).Trim();
            }

            var separatedFunctions = functions
                .Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)
                .Select(RemoveNiCadHeader);

            var separatedBlocks = blocks
                .Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)
                .Select(RemoveNiCadHeader);

            return (separatedFunctions, separatedBlocks);
            // remove all blocks that are just functions
            //formattedBlocks = formattedBlocks.Where(b => !formattedFunctions.Select(f => f.Contains(b) && !string.IsNullOrWhiteSpace(f) && !string.IsNullOrWhiteSpace(b)).Any());
            //return formattedFunctions.Where(f => f.Split('\n').Length > 3).Select(f => f.Trim()).Concat(formattedBlocks.Where(f => f.Split('\n').Length >= 3)).Select(f => f.Trim()).ToList();
        }
    }
}