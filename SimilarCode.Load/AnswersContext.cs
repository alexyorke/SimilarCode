using Microsoft.EntityFrameworkCore;
using SimilarCode.Load.Models;

namespace SimilarCode.Load
{
    public class AnswersContext : DbContext
    {
        public DbSet<Answer> Answers { get; set; }
        public DbSet<CodeSnippet> CodeSnippets { get; set; }
        public DbSet<CodeSnippetGrouping> CodeSnippetGroupings { get; set; }
        public DbSet<ProgrammingLanguage> ProgrammingLanguage { get; set; }
        public static string? DbPath { get; set;  }

        public AnswersContext()
        {
            DbPath ??= @"L:\stackoverflow\SimilarCode.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }
    }
}