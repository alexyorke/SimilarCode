using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    public interface ICodeRepository : IDisposable
    {
        Task AddRangeAsync(IEnumerable<Answer> answers);
    }
}