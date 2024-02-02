using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;
using Microsoft.VisualBasic.Logging;
using Bartz24.FF13;

namespace Bartz24.RandoWPF.Tests;

[TestClass()]
public class RandomNumTests
{
    private static readonly int DistributionCount = 100000;
    private static readonly int Seed = 5451237;

    [TestInitialize]
    public void TestInitialize()
    {
        RandomNum.SetRand(new Random(Seed));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        RandomNum.ClearRand();
    }

    [TestMethod()]
    [DataRow(0, 99)]
    [DataRow(5, 6)]
    [DataRow(-9, 18)]
    [DataRow(0, 10000)]
    public void RandIntTest(int low, int high)
    {
        IEnumerable<int> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.RandInt(low, high)).GroupBy(i => i).Select(g => g.Count());
        enumerable.ForEach(i => Assert.AreEqual(DistributionCount / (high - low + 1), i, 0.01 * DistributionCount));
    }

    [TestMethod()]
    [DataRow((long)0, (long)99)]
    [DataRow((long)5, (long)6)]
    [DataRow((long)-9, (long)18)]
    [DataRow((long)0, (long)10000)]
    public void RandLongTest(long low, long high)
    {
        IEnumerable<int> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.RandLong(low, high)).GroupBy(i => i).Select(g => g.Count());
        enumerable.ForEach(i => Assert.AreEqual(DistributionCount / (high - low + 1), i, 0.01 * DistributionCount));
    }

    [TestMethod()]
    [DataRow(50, 10, 0, 100)]
    [DataRow(2000, 1000, 0, 10000)]
    public void RandIntNormTest(double center, double std, int low, int high)
    {
        IEnumerable<IGrouping<int, int>> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.RandIntNorm(center, std, low, high)).GroupBy(i => i);
        enumerable.ForEach(g => Assert.AreEqual(ExpectedCount(g.Key, std, center), g.Count(), 0.01 * DistributionCount));

        static double ExpectedCount(double x, double std, double center) => DistributionCount * 1 / (std * Math.Sqrt(2 * Math.PI)) * Math.Exp(-0.5 * Math.Pow((x - center) / std, 2));
    }

    [TestMethod()]
    [DataRow(100)]
    [DataRow(2)]
    [DataRow(5000)]
    public void SelectRandomTest(int listCount)
    {
        List<string> list = GetMockList(listCount);
        IEnumerable<int> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.SelectRandom(list)).GroupBy(s => s).Select(g => g.Count());
        enumerable.ForEach(i => Assert.AreEqual(DistributionCount / listCount, i, 0.01 * DistributionCount));
    }

    [TestMethod()]
    [DataRow(100)]
    [DataRow(2)]
    [DataRow(5000)]
    public void SelectRandomWeightedTestUniform(int listCount)
    {
        List<string> list = GetMockList(listCount);
        IEnumerable<int> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.SelectRandomWeighted(list, _ => 1)).GroupBy(s => s).Select(g => g.Count());
        List<int> listInt = enumerable.ToList();
        enumerable.ForEach(i => Assert.AreEqual(DistributionCount / listCount, i, 0.01 * DistributionCount));
    }

    [TestMethod()]
    [DataRow(100)]
    [DataRow(2)]
    [DataRow(5000)]
    public void SelectRandomWeightedTestWithZeros(int listCount)
    {
        List<string> list = GetMockList(listCount);

        static long weightFunc(string s) => Math.Abs(s.GetHashCode()) % 3 == 0 ? 0 : Math.Abs(s.GetHashCode()) % 57;
        IEnumerable<IGrouping<string, string>> enumerable = Enumerable.Range(0, DistributionCount).Select(_ => RandomNum.SelectRandomWeighted(list, weightFunc)).GroupBy(s => s);
        enumerable.ForEach(g => Assert.AreEqual(weightFunc(g.Key) * DistributionCount / (double)list.Select(s => weightFunc(s)).Sum(), g.Count(), 0.01 * DistributionCount));
    }

    private static List<string> GetMockList(int listCount)
    {
        return Enumerable.Range(0, listCount).Select(i => RandomNum.RandLong(0, (long)1e9).GetHashCode().ToString()).ToList();
    }
}