using Bartz24.Data;

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

    [TestMethod()]
    [DataRow(2, 0, 9, 8, 1.60, new double[] { 0, 0, 0, 0, 0, 0, 0.15, 0.27, 0.32, 0.27 })]
    [DataRow(3, 0, 9, 1, 1.6, new double[] { 0.25, 0.30, 0.25, 0.14, 0.05, 0, 0, 0, 0, 0 })]
    public void RandomTruncGaussianTest(int delta, int low, int high, int center, double sigma, double[] expectedProbs)
    {
        int trials = 100000;
        int[] counts = new int[high - low + 1];
        for (int i = 0; i < trials; i++)
        {
            int value = RandomNum.RandomTruncGaussian(center, sigma, delta, low, high);
            if (value >= low && value <= high)
            {
                counts[value - low]++;
            }
        }

        for (int i = 0; i < counts.Length; i++)
        {
            double actualProb = counts[i] / (double)trials;
            Assert.AreEqual(expectedProbs[i], actualProb, 0.01, $"Expected Probs: {string.Join(", ", expectedProbs)}, Actual Probs: {string.Join(", ", counts.Select(c => c / (double)trials))}");
        }
    }

    [TestMethod]
    [DataRow(["Cure", "Curasa"])]
    [DataRow(["Cure", "Curasa", "Esuna"])]
    [DataRow(["Cure", "Curasa", "Esuna", "Raise"])]
    public void ShuffleTest(string[] items)
    {
        // Verify distribution of each ordering is approximately uniform
        int trials = 100000;
        Dictionary<string, int> counts = new();
        foreach (var perm in GetPermutations(items.ToList()))
        {
            string key = string.Join(",", perm);
            counts[key] = 0;
        }

        for (int i = 0; i < trials; i++)
        {
            var shuffled = items.Shuffle();
            string key = string.Join(",", shuffled);
            counts[key]++;
        }

        double expectedCount = trials / counts.Count;
        foreach (var count in counts.Values)
        {
            Assert.AreEqual(expectedCount, count, 0.02 * trials);
        }
    }

    private List<List<string>> GetPermutations(List<string> list)
    {
        var result = new List<List<string>>();

        if (list.Count == 0)
        {
            result.Add(new List<string>());
            return result;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var current = list[i];

            var remaining = new List<string>(list);
            remaining.RemoveAt(i);

            foreach (var perm in GetPermutations(remaining))
            {
                perm.Insert(0, current);
                result.Add(perm);
            }
        }

        return result;
    }

}