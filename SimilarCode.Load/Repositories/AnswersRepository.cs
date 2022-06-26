using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    public class AnswersRepository : ICodeRepository
    {
        private readonly AnswersContext _context;
        public AnswersRepository(string? dbPath)
        {
            AnswersContext.DbPath = dbPath ?? @"L:\stackoverflow\SimilarCode.db";
            _context = new();
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            _context.Database.EnsureCreated();
        }

        public AnswersContext Context
        {
            get { return _context; }
        }

        public async Task AddRangeAsync(IEnumerable<Answer> answers)
        {
            _context.Answers.AddRange(answers);
            await _context.SaveChangesAsync();
        }

        public AnswersContext GetContext()
        {
            return _context;
        }

        public void Dispose()
        {
            
        }
    }
}