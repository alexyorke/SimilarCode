﻿using System.Collections.Generic;
using System.Linq;

namespace SimilarCode.Match
{
    internal static class Extensions
    {
        public static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}