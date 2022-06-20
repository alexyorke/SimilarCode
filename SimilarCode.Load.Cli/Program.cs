using System.Collections.Concurrent;
using FluentArgs;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Cli
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            const bool debug = true;
            var debugCliOptions = new CliOptions
            {
                DbPath = @"C:\Users\Alex Yorke\Desktop\SimilarCode_new.db",
                MaxThreads = 20,
                SoPostsPath = @"Z:\stackoverflow\stackoverflow.com-Posts\Posts.xml"
            };

            var cliOptions = debug ? debugCliOptions : await GetCliOptions(args);

            using var dbWriter = new BlockingCollection<ProgressItem<Answer>>(200_000);
            var dbWriterRunner = new Task(async () => await Database.DbWriterCore(cliOptions.DbPath, dbWriter),
                TaskCreationOptions.LongRunning);

            dbWriterRunner.Start();

            await Converters.ProcessAnswers(cliOptions, dbWriter);
            await dbWriterRunner;
        }

        private static async Task<CliOptions?> GetCliOptions(string[] args)
        {
            CliOptions cliOptions = new();
            TxlOptions txlOptions;

            var parsedArgsSuccessfully = await FluentArgsBuilder.New()
                .DefaultConfigsWithAppDescription("Generates the SimilarCode database.")
                .Parameter("-p", "--posts")
                .WithDescription("Path to the 7z or decompressed stackoverflow.com posts")
                .WithExamples("~/workspace/stackoverflow.com-posts.7z or ~/workspace/stackoverflow.com-posts.xml")
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
                .WithDescription("SQLite file to output")
                .WithExamples("~/workspace/database.db")
                .IsRequired()
                .Parameter<uint>("-t", "--threads")
                .WithDescription("Max number of threads. Defaults to ProcessorCount.")
                .WithExamples("16")
                .IsRequired()
                .Call(chosenMaxThreads => output => grammars => txl => posts =>
                {
                    txlOptions = new TxlOptions(txl, grammars);
                    cliOptions.SoPostsPath = posts;
                    cliOptions.MaxThreads = (int)chosenMaxThreads;
                    cliOptions.DbPath = output;
                })
                .ParseAsync(args).ConfigureAwait(false);

            if (!parsedArgsSuccessfully) return null;

            return cliOptions;
        }
    }
}