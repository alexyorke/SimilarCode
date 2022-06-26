using System.Collections.Concurrent;
using FluentArgs;
using SimilarCode.Load.Models;
using SimilarCode.Match;

namespace SimilarCode.Match.Cli
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            const bool debug = true;
            var debugCliOptions = new CliOptions
            {
                DbPath = @"C:\Users\Alex Yorke\Desktop\SimilarCode.db",
                ToMatchPath = @"C:\Users\Alex Yorke\Desktop\test_similar_code.cs",
                MaxThreads = Environment.ProcessorCount
            };

            var cliOptions = debug ? debugCliOptions : await GetCliOptions(args);
            var matcher = new Match.Program();
            var output = await matcher.Start(File.ReadAllText(cliOptions.ToMatchPath), cliOptions.DbPath);
            Console.WriteLine(output.Item1);
        }

        private static async Task<CliOptions?> GetCliOptions(string[] args)
        {
            CliOptions cliOptions = new();
            TxlOptions txlOptions;

            var parsedArgsSuccessfully = await FluentArgsBuilder.New()
                .DefaultConfigsWithAppDescription("The path to a folder or file to match")
                .Parameter("-i", "--input")
                .WithDescription("Path to the folder or file to match")
                .WithExamples("~/workspace/path/to/file.cs")
                .IsRequired()
                .Parameter("-x", "--txl")
                .WithDescription("Path to txl.exe or txl binary. If not specified, defaults to txl on your PATH")
                .WithExamples("/path/to/txl.exe")
                .IsOptional()
                .Parameter("-g", "--grammars")
                .WithDescription("Folder containing TXL grammars")
                .WithExamples("~/workspace/NiCad-x.y/txl")
                .IsRequired()
                .Parameter("-o", "--output")
                .WithDescription("Where to save the report")
                .WithExamples("~/workspace/output.json")
                .IsRequired()
                .Parameter<uint>("-t", "--threads")
                .WithDescription("Max number of threads. Defaults to ProcessorCount.")
                .WithExamples("16")
                .IsRequired()
                .Call(chosenMaxThreads => output => grammars => txl => toMatchPath =>
                {
                    txlOptions = new TxlOptions(txl, grammars);
                    cliOptions.MaxThreads = (int)chosenMaxThreads;
                    cliOptions.DbPath = output;
                    cliOptions.ToMatchPath = toMatchPath;
                })
                .ParseAsync(args).ConfigureAwait(false);

            if (!parsedArgsSuccessfully) return null;

            return cliOptions;
        }
    }

    internal class CliOptions
    {
        public string DbPath
        {
            get;
            set;
        }

        public string ToMatchPath
        {
            get;
            set;
        }

        public int MaxThreads
        {
            get;
            set;
        }
    }
}