// See https://aka.ms/new-console-template for more information

using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics;
using MoreLinq;
using Vektonn.Index;
using Vostok.Logging.Abstractions;

List<Vektonn.Index.DenseVector> CreateRandomVectorList()
{
    var denseVectors = new List<DenseVector>();

    var rnd = new Random();
    for (int i = 0; i < 32_000_000; i++)
    {
        var v = new List<double>(64);
        for (var j = 0; j < 64; j++)
        {
            v.Add((double)rnd.Next(0, ushort.MaxValue));
        }

        denseVectors.Add(new DenseVector(v.ToArray()));
    }

    return denseVectors;
}


var vectors = CreateRandomVectorList();
Console.WriteLine("Finished loading. Matching...");


var indexStoreFactory = new IndexStoreFactory<int, object>(new SilentLog());

var indexStore = indexStoreFactory.Create<DenseVector>(
    Algorithms.FaissIndexL2,
    64,
    withDataStorage: true,
    idComparer: EqualityComparer<int>.Default);

var indexDataPoints = vectors
    .Select((vector, index) =>
        new IndexDataPointOrTombstone<int, object, DenseVector>(
            new IndexDataPoint<int, object, DenseVector>(
                Id: index,
                Vector: vector,
                Data: 0
            )
        )
    )
    .ToArray();

const int indexBatchSize = 1000;
foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
    indexStore.UpdateIndex(batch);

var queryResults = indexStore.FindNearest(vectors.Take(100).ToArray(), limitPerQuery: 10, retrieveVectors: true);

foreach (var queryResult in queryResults)
{
    foreach (IndexFoundDataPoint<int, object, DenseVector> dp in queryResult.NearestDataPoints)
        Console.WriteLine($"Distance: {dp.Distance}, Vector: {dp.Vector}, Id: {dp.Id}, Metadata: {dp.Data}");
}