using Bartz24.RandoWPF;

namespace ModdingUnitTests;

[TestClass]
public class StatPointTests
{
    private static readonly int Seed = 5451237;

    // LR shield settings (LRRando EquipRando).
    private static (int, int)[] ShieldBounds = {
        (-2000, 5000),
        (-2000, 5000),
        (-5000, 50000),
        (-25, 50),
        (0, 1000)
    };
    private static float[] ShieldWeights = { 1, 1, 1 / 150f, 8, 8 };
    private static int[] ShieldChances = { 5, 5, 30, 30, 30 };
    private static int[] ShieldZeros = { 90, 90, 40, 30, 20 };

    [TestInitialize]
    public void TestInitialize()
    {
        RandomNum.SetRand(new Xoshiro256StarStarRandom(Seed));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        RandomNum.ClearRand();
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

    [TestMethod]
    public void TestNetNegativeInputIsRandomized()
    {
        // A shield whose weighted vanilla stats add up to <= 0 (-200 * 1 + 20 * 8 = -40) used to
        // come out as all zeros with a -1 from the jitter. It has a positive stat, so it should
        // be randomized every time, from the points of its positive stats, into a usable item.
        AssertAlwaysRandomized(new int[] { 30, 30, 15, 5, 0 }, new int[] { -200, 0, 0, 0, 20 });
    }

    [TestMethod]
    public void TestLRNoGuardShieldIsRandomized()
    {
        // LR's Aquamarine Waltz: HP 2600, ATB -15. ATB -15 * 8 outweighs HP 2600 / 150.
        AssertAlwaysRandomized(new int[] { 30, 30, 15, 5, 0 }, new int[] { 0, 0, 2600, -15, 0 });
    }

    [TestMethod]
    public void TestTinyPositiveInputIsRandomized()
    {
        // FF12's Argyle Armlet: Str 1, worth 0 points at weight 0.8 once truncated.
        (int, int)[] bounds = { (0, 120), (0, 120), (0, 5000), (0, 99) };
        float[] weights = { 1.3f, 1.3f, 1 / 10f, 0.8f };
        int[] chances = { 30, 30, 10, 10 };
        int[] zeros = { 50, 50, 80, 80 };
        int[] negs = { 0, 0, 0, 0 };
        int[] vanilla = { 0, 0, 0, 1 };
        int changed = 0;
        for (int i = 0; i < 10000; i++)
        {
            var statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
            statPoints.Randomize(vanilla);

            int[] result = Enumerable.Range(0, bounds.Length).Select(j => statPoints[j]).ToArray();
            Assert.IsTrue(result.Max() > 0, $"All stats were zero or negative in iteration {i}.");
            if (!result.SequenceEqual(vanilla))
            {
                changed++;
            }
        }

        Assert.IsTrue(changed > 5000, $"Only {changed} of 10000 rolls differed from vanilla.");
    }

    private void AssertAlwaysRandomized(int[] negs, int[] vanilla)
    {
        int changed = 0;
        for (int i = 0; i < 10000; i++)
        {
            var statPoints = new StatPoints(ShieldBounds, ShieldWeights, ShieldChances, ShieldZeros, negs);
            statPoints.Randomize(vanilla);

            int[] result = Enumerable.Range(0, ShieldBounds.Length).Select(j => statPoints[j]).ToArray();
            Assert.IsTrue(result.Max() > 0, $"All stats were zero or negative in iteration {i}.");
            if (!result.SequenceEqual(vanilla))
            {
                changed++;
            }
        }

        Assert.IsTrue(changed > 9900, $"Only {changed} of 10000 rolls differed from vanilla.");
    }

    [TestMethod]
    public void TestAllNegativeInputKeepsVanilla()
    {
        // No positive stat at all: nothing to hand out, so the item stays as it was.
        int[] negs = { 30, 30, 15, 5, 0 };
        int[] vanilla = { -200, 0, 0, -10, 0 };
        for (int i = 0; i < 1000; i++)
        {
            var statPoints = new StatPoints(ShieldBounds, ShieldWeights, ShieldChances, ShieldZeros, negs);
            statPoints.Randomize(vanilla);

            CollectionAssert.AreEqual(vanilla, Enumerable.Range(0, ShieldBounds.Length).Select(j => statPoints[j]).ToArray(),
                $"Vanilla stats were not kept in iteration {i}.");
        }
    }

    [TestMethod]
    public void TestAllZeroInputKeepsVanilla()
    {
        // Nothing to hand out, so the item stays as it was rather than gaining random stats.
        int[] negs = { 30, 30, 15, 5, 0 };
        int[] vanilla = { 0, 0, 0, 0, 0 };
        for (int i = 0; i < 1000; i++)
        {
            var statPoints = new StatPoints(ShieldBounds, ShieldWeights, ShieldChances, ShieldZeros, negs);
            statPoints.Randomize(vanilla);

            CollectionAssert.AreEqual(vanilla, Enumerable.Range(0, ShieldBounds.Length).Select(j => statPoints[j]).ToArray(),
                $"Vanilla stats were not kept in iteration {i}.");
        }
    }

    [TestMethod]
    public void TestAlwaysNegativeStatCountsTowardsBudget()
    {
        // FF12 hidden stats: charge time is stored negated in an all-negative range and always
        // rolls it (zero chance 0, negative chance 100), so every roll gets its offset. A weak
        // weapon (Atk 20, CT 30) must still be randomized rather than kept vanilla because
        // -30 * 2.5 outweighs its Atk.
        (int, int)[] bounds = { (0, 255), (-100, -10) };
        float[] weights = { 1, 2.5f };
        int[] chances = { 70, 200 };
        int[] zeros = { 0, 0 };
        int[] negs = { 0, 100 };
        int[] vanilla = { 20, -30 };
        int changed = 0;
        for (int i = 0; i < 10000; i++)
        {
            var statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
            statPoints.Randomize(vanilla);
            if (statPoints[0] != vanilla[0] || statPoints[1] != vanilla[1])
            {
                changed++;
            }
        }

        Assert.IsTrue(changed > 9000, $"Only {changed} of 10000 rolls were randomized.");
    }

    [TestMethod]
    public void TestNetPositiveInputWithNegativeStatIsRandomized()
    {
        // -100 * 1 + 20 * 8 = 60 points, so this one is randomized and must end up usable.
        int[] negs = { 30, 30, 15, 5, 0 };
        int[] vanilla = { -100, 0, 0, 0, 20 };
        int changed = 0;
        for (int i = 0; i < 10000; i++)
        {
            var statPoints = new StatPoints(ShieldBounds, ShieldWeights, ShieldChances, ShieldZeros, negs);
            statPoints.Randomize(vanilla);

            int[] result = Enumerable.Range(0, ShieldBounds.Length).Select(j => statPoints[j]).ToArray();
            Assert.IsTrue(result.Max() > 0, $"All stats were zero or negative in iteration {i}.");
            if (!result.SequenceEqual(vanilla))
            {
                changed++;
            }
        }

        Assert.IsTrue(changed > 9000, $"Only {changed} of 10000 rolls were randomized.");
    }

    [TestMethod]
    public void TestNoNegativeValuesWhenNegChanceZero()
    {
        // With no negative chances, the +-1/Weight jitter must not push a stat below zero
        // even though the stat's full bounds allow negatives.
        int[] negs = { 0, 0, 0, 0, 0 };
        for (int i = 0; i < 100000; i++)
        {
            var statPoints = new StatPoints(ShieldBounds, ShieldWeights, ShieldChances, ShieldZeros, negs);
            statPoints.Randomize([100, 100, 3000, 5, 100]);

            for (int j = 0; j < ShieldBounds.Length; j++)
            {
                Assert.IsTrue(statPoints[j] >= 0, $"Stat {j} was {statPoints[j]} in iteration {i} with no negative chance.");
            }
        }
    }
}
