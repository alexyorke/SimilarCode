using System;
using SimilarCode.Load.Models;

namespace SimilarCode.Load.Repositories
{
    internal static class PostsRepositoryFactory
    {
        public static IPostsRepository GetRepository(string? path, DatabaseType type)
        {
            return type switch
            {
                DatabaseType.SevenZip => new SevenZipPostsRepository(path),
                DatabaseType.Uncompressed => new UncompressedPostsRepository(path),
                _ => throw new InvalidOperationException("Posts repository type not found.")
            };
        }
    }
}