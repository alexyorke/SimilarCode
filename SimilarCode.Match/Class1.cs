using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace SimilarCode.Match
{
    internal static class Class1
    {
        public static void GitRepoTest()
        {
            var repoPath = @"C:\path/to/repo";
            using var repo = new Repository(repoPath);
            // TODO: this will skip the initial changes in HEAD commit
            var diffPairs = repo.Commits.Reverse().Zip(repo.Commits.Reverse().Skip(1)).Select(c => new {Left = c.First, Right = c.Second});
            foreach (var diffPair in diffPairs)
            {
                var changes = repo.Diff.Compare<TreeChanges>(diffPair.Left.Tree, diffPair.Right.Tree);
                var treeFileHandles = new List<FileStream>();
                var randomFileName = Path.GetRandomFileName();
                var tempPath = Path.GetTempPath();
                foreach (var change in changes)
                { 
                    Console.WriteLine(change.Path);
                    var blob = repo.Lookup(change.Oid, ObjectType.Blob) as Blob;
                    if (blob == null) continue;
                    using var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8);
                    var commitContent = content.ReadToEnd();
                    var root = Path.GetDirectoryName(Path.Join(tempPath, randomFileName,
                        $"{change.Path.Split(@"/").First()}@{diffPair.Right.Sha.Substring(0, 9)}", string.Join("/", change.Path.Split(@"/").Skip(1))));
                    var fileName = change.Path.Split(@"/").Last();
                    Directory.CreateDirectory(root);
                    var fs = new FileStream(Path.Join(root, fileName), FileMode.CreateNew, FileAccess.ReadWrite,
                        FileShare.None, 4096, FileOptions.DeleteOnClose | FileOptions.Encrypted);
                    treeFileHandles.Add(fs);
                    fs.Write(Encoding.ASCII.GetBytes(commitContent));
                }

                foreach (var handle in treeFileHandles)
                {
                    Console.WriteLine(handle.CanRead);
                    handle.Dispose();
                }
            }
        }
    }
}
