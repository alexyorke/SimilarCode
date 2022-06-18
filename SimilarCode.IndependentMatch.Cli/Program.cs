using System.Collections.Concurrent;
using Newtonsoft.Json;
using SimilarCode.Match;

namespace SimilarCode.IndependentMatch.Cli
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Train
    {
        public string label { get; set; }
        public string index { get; set; }
        public string code { get; set; }
    }
    public class Prediction
    {
        public string index { get; set; }
        public List<string> answers { get; set; }
    }

    internal static class Ext
    {
        public static void ShuffleMe<T>(this IList<T> list)
        {
            Random random = new Random();
            int n = list.Count;

            for (int i = list.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                T value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }
    }

    internal class Program
    {
        static void TruncateAnswers()
        {
            var answerLines = File.ReadAllLines(
                @"C:\Users\yorke\Desktop\codexglue\CodeXGLUE\Code-Code\Clone-detection-POJ-104\evaluator\answers.jsonl").Select(a => JsonConvert.DeserializeObject<Prediction>(a)).ToList();
            var predictionLines = File.ReadAllLines(
                @"C:\Users\yorke\Desktop\codexglue\CodeXGLUE\Code-Code\Clone-detection-POJ-104\evaluator\predictions_similarcode.jsonl").Select(p => JsonConvert.DeserializeObject<Prediction>(p)).ToList();
            var toWrite = new List<string>();

            foreach (var answer in answerLines)
            {
                if (predictionLines.Select(p => p.index).Contains(answer.index))
                {
                    toWrite.Add(JsonConvert.SerializeObject(answer));
                }
            }

            File.WriteAllLines(@"C:\Users\yorke\Desktop\codexglue\CodeXGLUE\Code-Code\Clone-detection-POJ-104\evaluator\answers_similarcode.jsonl", toWrite);
        }

        static void Main(string[] args)
        {
            var trainingData = new ConcurrentDictionary<string, string>();
            var predictions = new ConcurrentBag<Prediction>();

            // read {test,valid,train}.jsonl and concatenate them together; deserialize them
            // store them in a dictionary<index, code>
            // create answers dictionary<index, List<matching code snippets>
            // match each code snippet with each other, take top 499 and store in code snippets
            // write answers dictionary to file
            var lines = File.ReadAllLines(@"test.jsonl").Concat(File.ReadAllLines(@"train.jsonl"))
                .Concat(File.ReadAllLines(@"valid.jsonl")).ToList();
            foreach (var line in lines)
            {
                var trainingLine = JsonConvert.DeserializeObject<Train>(line);
                trainingData[trainingLine.index] = trainingLine.code;
            }

            var i = 0;
            Parallel.ForEach(trainingData, trainingDataItem =>
            {
                var snippetsToTrainWith = trainingData.Where(t => t.Key != trainingDataItem.Key);

                var matchedWithScores = new List<Tuple<int, string>>();
                foreach (var snippet in snippetsToTrainWith)
                {
                    var score = Gfg.GetMinimumPenaltyAsPercent(trainingDataItem.Value, snippet.Value);
                    matchedWithScores.Add(Tuple.Create((int)(score * 100), snippet.Key));
                }

                // sort matchedWithScores by int (highest first) then take top 499
                var topMatchedWithIndexes = matchedWithScores.OrderByDescending(x => x.Item1).Select(y => y.Item2).ToList();
                var prediction = new Prediction
                {
                    index = trainingDataItem.Key,
                    answers = topMatchedWithIndexes
                };

                predictions.Add(prediction);

                Console.WriteLine(i + "/" + trainingData.Count);
                Interlocked.Add(ref i, 1);
            });

            File.WriteAllLines(@"predictions_similarcode.jsonl", predictions.Select(JsonConvert.SerializeObject));
        }
    }
}