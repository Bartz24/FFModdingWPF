using Bartz24.RandoWPF;

namespace ModdingUnitTests;

[TestClass]
public class StatPointTests
{
    private static readonly int Seed = 5451237;

    [TestInitialize]
    public void TestInitialize()
    {
        RandomNum.SetRand(new Random(Seed));
    }

    [TestMethod]
    public void TestNoZeroOrNegativeValues()
    {
        // Repeat 100000 times and ensure that no zero or negative values are generated when the chances for those are set to 0.
        for (int i = 0; i < 100000; i++)
        {
            // Get seed from the current iteration to ensure reproducibility in case of a failure. This will help identify which seed caused the issue.
            (int, int)[] bounds = {
                    (-2000, 5000),
                    (-2000, 5000),
                    (-5000, 50000),
                    (-25, 50),
                    (-90, 75)
                };
            float[] weights = { 1, 1, 1 / 200f, 10, 5 };
            int[] chances = { 40, 40, 5, 5, 10 };
            int[] zeros = { 10, 10, 85, 60, 80 };
            int[] negs = { 15, 15, 40, 10, 5 };
            var statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
            statPoints.Randomize([150, 120, 0, 0, 0]);

            // Assert that there is at least 1 positive non-zero value in the generated stat points.
            bool valid = false;
            for (int j = 0; j < bounds.Length; j++)
            {
                if (statPoints[j] > 0)
                {
                    valid = true;
                    break;
                }
            }

            Assert.IsTrue(valid, $"No positive non-zero values were generated in iteration {i}.");
        }
    }
}
