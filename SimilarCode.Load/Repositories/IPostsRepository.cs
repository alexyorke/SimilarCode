using System.Collections.Generic;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    internal interface IPostsRepository
    {
        IEnumerable<(long ReadOffset, long totalSize, Answer answer)> GetAllAnswersAsync();
    }
}