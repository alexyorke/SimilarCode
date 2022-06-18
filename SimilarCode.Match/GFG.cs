using System;
using System.Buffers;
using System.Linq;

// https://www.geeksforgeeks.org/sequence-alignment-problem/
namespace SimilarCode.Match
{
    public class Gfg
    {
        public int GetMinimumPenalty<T>(T[] x, T[] y, int pxy = 3, int pgap = 2)
        {
            int i, j; // initialising variables

            int m = x.Length; // length of gene1
            int n = y.Length; // length of gene2

            // table for storing optimal substructure answers
            int[,] dp = new int[n + m + 1, n + m + 1];

            // initialising the table 
            for (i = 0; i <= (n + m); i++)
            {
                dp[i, 0] = i * pgap;
                dp[0, i] = i * pgap;
            }

            // calculating the minimum penalty
            for (i = 1; i <= m; i++)
            {
                for (j = 1; j <= n; j++)
                {
                    if (x[i - 1].Equals(y[j - 1]))
                    {
                        dp[i, j] = dp[i - 1, j - 1];
                    }
                    else
                    {
                        dp[i, j] = Math.Min(Math.Min(dp[i - 1, j - 1] + pxy,
                                dp[i - 1, j] + pgap),
                            dp[i, j - 1] + pgap);
                    }
                }
            }

            return dp[m,n];
        }
        
        public static int GetMinimumPenalty(ReadOnlySpan<char> x, ReadOnlySpan<char> y, int pxy = 3, int pgap = 2)
        {
            var n = y.Length;
            var m = x.Length;

            var dp = ArrayPool<int[]>.Shared.Rent(n + m + 1);
            for (int i = 0; i < n + m + 1; i++)
            {
                dp[i] = ArrayPool<int>.Shared.Rent(n + m + 1);
            }

            for (var i = 0; i <= (n + m); i++)
            {
                dp[i][0] = i * pgap;
                dp[0][i] = i * pgap;
            }

            for (var i = 1; i <= m; i++)
            {
                for (var j = 1; j <= n; j++)
                {
                    var z = dp[i - 1][j - 1];
                    dp[i][j] = z;
                    if (x[i - 1] == y[j - 1]) continue;
                    
                    var a = z + pxy;
                    var b = dp[i - 1][j] + pgap;
                    var c = dp[i][j - 1] + pgap;
                    dp[i][j] = Math.Min(Math.Min(a, b), c);
                }
            }

            var ret = dp[m][n];

            for (int i = 0; i < n + m + 1; i++)
            {
                ArrayPool<int>.Shared.Return(dp[i], false);
            }

            ArrayPool<int[]>.Shared.Return(dp, false);
            return ret;
        }

        public static int GetMinimumPenaltyOptimizedMem(ReadOnlySpan<char> x, ReadOnlySpan<char> y, ushort pxy = 3, ushort pgap = 2)
        {
            var Z = 10;
            var A = typeof(int);
            ushort n = (ushort)y.Length;
            ushort m = (ushort)x.Length;
            var dp = ArrayPool<ushort[]>.Shared.Rent(n + m + 1);
            // dp[0]..dp[1]
            for (ushort i = 0; i <= 1; i++)
            {
                dp[i] = ArrayPool<ushort>.Shared.Rent(n + m + 1);
            }

            // initialize first row
            for (ushort i = 0; i <= (n + m); i++)
            {
                dp[0][i] = (ushort)(i * pgap);
            }

            // initialize first two items from first column
            for (var i = 0; i <= 1; i++)
            {
                dp[i][0] = (ushort)(i * pgap);
            }

            for (var i = 1; i <= m; i++)
            {
                dp[i] = ArrayPool<ushort>.Shared.Rent(n + m + 1);
                dp[i][0] = (ushort)(i * pgap);
                for (var j = 1; j <= n; j++)
                {
                    var z = dp[i - 1][j - 1];
                    dp[i][j] = z;
                    if (x[i - 1] == y[j - 1])
                        continue;
                    var a = z + pxy;
                    var b = dp[i - 1][j] + pgap;
                    var c = dp[i][j - 1] + pgap;
                    dp[i][j] = (ushort)Math.Min(Math.Min(a, b), c);
                }

                if (i >= 2)
                {
                    ArrayPool<ushort>.Shared.Return(dp[i - 2], false);
                }
            }

            var ret = dp[m][n];
            ArrayPool<ushort>.Shared.Return(dp[m], false);
            if (m >= 1)
                ArrayPool<ushort>.Shared.Return(dp[m - 1], false);
            ArrayPool<ushort[]>.Shared.Return(dp, false);
            return ret;
        }

        public static decimal GetMinimumPenaltyAsPercent(ReadOnlySpan<char> x, ReadOnlySpan<char> y, int pxy = 3, int pgap = 2)
        {
            int worstPossibleScore = (int)(Math.Max(pxy, pgap) * Math.Max(x.Length, y.Length));
            var computedScore = GetMinimumPenaltyOptimizedMem(x, y, (ushort)pxy, (ushort)pgap);
            if (worstPossibleScore == 0) return 0;

            var finalScore = (decimal)Math.Pow(((worstPossibleScore - computedScore) / (double)worstPossibleScore), 2);

            // this should never occur, but if it does, then set it to maximum possible score
            return Math.Clamp(finalScore, 0, 1);
        }
    }
}