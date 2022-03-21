using System;
using System.Collections.Generic;
using System.Linq;

namespace SimilarCode.Load
{
    public static class Extensions
    {
        public static IEnumerable<IList<T>> ChunksOf<T>(this IEnumerable<T> sequence, int size)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            List<T> chunk = new List<T>(size);

            foreach (T element in sequence)
            {
                chunk.Add(element);
                if (chunk.Count == size)
                {
                    yield return chunk;
                    chunk = new List<T>(size);
                }
            }

            // if the sequence's count mod size is not size, then return the rest (which is less than size)
            if (chunk.Any()) yield return chunk;
        }
    }
}