public static class Compress
{
    private static float Entropy(Dictionary<char, float> freqTable, int inputLength)
    {
        float infoC = 0;
        float freq;
        foreach (KeyValuePair<char, float> letter in freqTable)
        {
            if (letter.Value == 0) continue;
            freq = letter.Value / inputLength;
            infoC += freq * (float)Math.Log2(freq);
        }

        return infoC;
    }

    public static void Main(string[] args)
    {
        var inputText =
            "public static string CompressString(string text)        {            byte[] buffer = Encoding.UTF8.GetBytes(text);            var memoryStream = new MemoryStream();            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))            {               gZipStream.Write(buffer, 0, buffer.Length);            }          memoryStream.Position = 0;            var compressedData = new byte[memoryStream.Length];            memoryStream.Read(compressedData, 0, compressedData.Length);            var gZipBuffer = new byte[compressedData.Length + 4];            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);            return Convert.ToBase64String(gZipBuffer);        }"
                .AsSpan();
        var maxSubstrLength = (int)Math.Round(inputText.Length * 0.10);

        for (int i = 0; i < 50; i++)
        {
            Console.WriteLine(FindLeastCompressableSubstring(inputText, maxSubstrLength).ToString());
        }
    }

    public static ReadOnlySpan<char> FindLeastCompressableSubstring(ReadOnlySpan<char> inputText, int maxSubstrLength)
    {
        //if (maxSubstrLength < 1) throw new ArgumentException("maxsubstrlength must be greater than or equal to 1");
        var coeffs = new float[inputText.Length];

        for (int slidingWindowSize = 2;
             slidingWindowSize < inputText.Length;
             slidingWindowSize++)
        {
            var freqTable = ComputeInitialFreqTable(inputText[..slidingWindowSize], inputText);

            // calculate initial entropy
            var entropy = Entropy(freqTable, slidingWindowSize);

            for (int i = 0; i < inputText.Length - slidingWindowSize; i++)
            {
                // update coeffs table
                for (int j = i; j < i + slidingWindowSize; j++)
                {
                    coeffs[j] += (entropy / (float)slidingWindowSize);
                }

                // remove previous letter from entropy
                if (i != 0)
                {
                    if (freqTable[inputText[i - 1]] != 0)
                    {
                        var freqToRemove = freqTable[inputText[i - 1]] / slidingWindowSize;
                        entropy -= freqToRemove * (float)Math.Log2(freqToRemove);
                    }

                    freqTable[inputText[i - 1]]--;
                }

                // add next letter to entropy
                if (freqTable[inputText[i + 1]] != 0)
                {
                    var freqToAdd = freqTable[inputText[i + 1]] / slidingWindowSize;
                    entropy += freqToAdd * (float)Math.Log2(freqToAdd);
                    
                }

                freqTable[inputText[i + 1]]++;
            }
        }

        double minSoFar = Double.MaxValue;
        var startIdx = 0;
        var coeffsList = coeffs.ToList();

        var endIdx = 0;
        for (int i = 0;
             i < coeffs.Length - maxSubstrLength;
             i++)
        {
            var s = coeffsList.GetRange(i, maxSubstrLength).Sum();
            if (s < minSoFar)
            {
                minSoFar = s;
                startIdx = i;
                endIdx = i + maxSubstrLength;
            }
        }

        return inputText.Slice(startIdx, endIdx - startIdx);
    }

    private static Dictionary<char, float> ComputeInitialFreqTable(ReadOnlySpan<char> input, ReadOnlySpan<char> entireInput)
    {
        Dictionary<char, float> freqTable = new();

        foreach (char c in input)
        {
            if (freqTable.ContainsKey(c))
                freqTable[c]++;
            else
                freqTable[c] = 1;
        }

        foreach (char c in entireInput)
        {
            if (!freqTable.ContainsKey(c)) freqTable[c] = 0;
        }

        return freqTable;
    }
}