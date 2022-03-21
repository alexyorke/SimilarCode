using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimilarCode.Load.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var input = new List<string> { "a", "b", "c", "d", "e" };
            var actual = input.ChunksOf(1).Select(i => i.ToList()).ToList();
            var expected = new List<List<string>>
                { new() { "a" }, new() { "b" }, new() { "c" }, new() { "d" }, new() { "e" } };
            Assert.AreEqual(actual.Count, expected.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var input = new List<string> { "a", "b", "c", "d", "e" };
            var actual = input.ChunksOf(5).Select(i => i.ToList()).ToList();
            var expected = new List<List<string>>
                { new() { "a", "b", "c", "d", "e" } };
            Assert.AreEqual(actual.Count, expected.Count);
        }
    }
}