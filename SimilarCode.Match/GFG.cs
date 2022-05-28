using System;
using System.Buffers;
using System.Linq;

// https://www.geeksforgeeks.org/sequence-alignment-problem/
namespace SimilarCode.Match
{
    public class Gfg
    {
        public void GetMinimumPenalty<T>(T[] x, T[] y, int pxy = 3, int pgap = 2)
        {
            int i, j; // initialising variables

            int m = x.Length; // length of gene1
            int n = y.Length; // length of gene2

            // table for storing optimal substructure answers
            int[,] dp = new int[n + m + 1, n + m + 1];
            for (int q = 0; q < n + m + 1; q++)
                for (int w = 0; w < n + m + 1; w++)
                    dp[q, w] = 0;

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

            // Reconstructing the solution
            int l = n + m; // maximum possible length

            i = m;
            j = n;

            int xpos = l;
            int ypos = l;

            // Final answers for the respective strings
            T[] xans = new T[l + 1];
            T[] yans = new T[l + 1];

            while (!(i == 0 || j == 0))
            {
                if (x[i - 1].Equals(y[j - 1]))
                {
                    xans[xpos--] = x[i - 1];
                    yans[ypos--] = y[j - 1];
                    i--;
                    j--;
                }
                else if (dp[i - 1, j - 1] + pxy == dp[i, j])
                {
                    xans[xpos--] = x[i - 1];
                    yans[ypos--] = y[j - 1];
                    i--;
                    j--;
                }
                else if (dp[i - 1, j] + pgap == dp[i, j])
                {
                    xans[xpos--] = x[i - 1];
                    yans[ypos--] = default;
                    i--;
                }
                else if (dp[i, j - 1] + pgap == dp[i, j])
                {
                    xans[xpos--] = default;
                    yans[ypos--] = y[j - 1];
                    j--;
                }
            }

            while (xpos > 0)
            {
                if (i > 0) xans[xpos--] = x[--i];
                else xans[xpos--] = default;
            }

            while (ypos > 0)
            {
                if (j > 0) yans[ypos--] = y[--j];
                else yans[ypos--] = default;
            }

            // Since we have assumed the answer to be n+m long, 
            // we need to remove the extra gaps in the starting 
            // id represents the index from which the arrays
            // xans, yans are useful
            int id = 1;
            for (i = l; i >= 1; i--)
            {
                if (object.ReferenceEquals(yans[i], null) && object.ReferenceEquals(xans[i], null))
                {
                    id = i + 1;
                    break;
                }
            }

            // Printing the final answer
            Console.Write($"Minimum Penalty in aligning the genes = {dp[m, n]}\n");
            Console.Write("The aligned genes are :\n");
            for (i = id; i <= l; i++)
            {
                if (object.ReferenceEquals(xans[i], null))
                {
                    Console.Write("_____");
                }
                else
                {
                    Console.Write(xans[i]);
                }
            }

            Console.Write("\n");
            var matchedCharCount = 0;
            for (i = id; i <= l; i++)
            {
                if (object.ReferenceEquals(yans[i], null))
                {
                    Console.Write("_____");
                }
                else
                {
                    Console.Write(xans[i]);
                }
                if (yans[i] != null && !yans[i].Equals(" ") && !yans[i].Equals("\t"))
                {
                    matchedCharCount++;
                }
            }

            //Console.WriteLine((decimal)matchedCharCount);
            return;
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

        public static int GetMinimumPenaltyOptimizedMem(ReadOnlySpan<char> x, ReadOnlySpan<char> y, int pxy = 3, int pgap = 2)
        {
            var n = y.Length;
            var m = x.Length;
            var dp = ArrayPool<int[]>.Shared.Rent(n + m + 1);
            // dp[0]..dp[1]
            for (int i = 0; i <= 1; i++)
            {
                dp[i] = ArrayPool<int>.Shared.Rent(n + m + 1);
            }

            // initialize first row
            for (var i = 0; i <= (n + m); i++)
            {
                dp[0][i] = i * pgap;
            }

            // initialize first two items from first column
            for (var i = 0; i <= 1; i++)
            {
                dp[i][0] = i * pgap;
            }

            for (var i = 1; i <= m; i++)
            {
                dp[i] = ArrayPool<int>.Shared.Rent(n + m + 1);
                dp[i][0] = i * pgap;
                for (var j = 1; j <= n; j++)
                {
                    var z = dp[i - 1][j - 1];
                    dp[i][j] = z;
                    if (x[i - 1] == y[j - 1])
                        continue;
                    var a = z + pxy;
                    var b = dp[i - 1][j] + pgap;
                    var c = dp[i][j - 1] + pgap;
                    dp[i][j] = Math.Min(Math.Min(a, b), c);
                }

                if (i >= 2)
                {
                    ArrayPool<int>.Shared.Return(dp[i - 2], false);
                }
            }

            var ret = dp[m][n];
            ArrayPool<int>.Shared.Return(dp[m], false);
            if (m >= 1)
                ArrayPool<int>.Shared.Return(dp[m - 1], false);
            ArrayPool<int[]>.Shared.Return(dp, false);
            return ret;
        }

        public static decimal GetMinimumPenaltyAsPercent(ReadOnlySpan<char> x, ReadOnlySpan<char> y, int pxy = 3, int pgap = 2)
        {
            int worstPossibleScore = (int)(Math.Max(pxy, pgap) * Math.Max(x.Length, y.Length));
            var computedScore = GetMinimumPenaltyOptimizedMem(x, y, pxy, pgap);
            if (worstPossibleScore == 0) return 0;

            var finalScore = (decimal)Math.Pow(((worstPossibleScore - computedScore) / (double)worstPossibleScore), 2);

            // this should never occur, but if it does, then set it to maximum possible score
            if (finalScore > 1) finalScore = 1;
            if (finalScore < 0) finalScore = 0;
            return finalScore;
        }
    }
}