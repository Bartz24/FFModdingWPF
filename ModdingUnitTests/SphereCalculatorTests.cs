using Bartz24.Data;
using Bartz24.RandoWPF.Data.Areas;

namespace Bartz24.RandoWPF.Tests;

[TestClass]
public class SphereCalculatorTests
{
    private class TestSeedGenerator : SeedGenerator
    {
        protected override void SetRandomizers()
        {
        }
    }

    /// <summary>
    /// A real (non fake) location holding an item, in the one test area.
    /// </summary>
    private class TestLocation : ItemLocation
    {
        [RowIndex(0)]
        public override List<string> Areas { get; set; }
        [RowIndex(1)]
        public override string Name { get; set; }
        [RowIndex(2)]
        public override string ID { get; set; }
        [RowIndex(3)]
        public override ItemReq Requirements { get; set; }
        [RowIndex(4)]
        public override List<string> Traits { get; set; }
        [RowIndex(5)]
        public override int BaseDifficulty { get; set; }
        public override string LocationImagePath { get; set; }
        private (string, int)? Item { get; set; }

        public TestLocation(SeedGenerator generator, string id, string requirements, string item) : base(generator, Row(id, requirements))
        {
            Item = item == null ? null : (item, 1);
        }

        public override bool CanReplace(ItemLocation location) => true;
        public override void SetItem(string newItem, int newCount) => Item = (newItem, newCount);
        public override (string Item, int Amount)? GetItem(bool orig) => Item;
    }

    private static string[] Row(string id, string requirements) => ["Initial", id, id, requirements, "", "0"];

    [TestMethod]
    public void FakeChecksDoNotAdvanceSpheres()
    {
        using SeedGenerator generator = new TestSeedGenerator();
        AreaGraph graph = new(generator);
        graph.Areas["Initial"] = new Area(["Initial"]);

        var fakeA = new FakeLocation(generator, Row("fakeA", ""), "EventA");
        var fakeB = new FakeLocation(generator, Row("fakeB", "I(EventA|1)"), "EventB");
        var key = new TestLocation(generator, "key", "I(EventB|1)", "Key");
        var afterKey = new TestLocation(generator, "afterKey", "I(Key|1)", "Potion");
        var fakeC = new FakeLocation(generator, Row("fakeC", "I(Key|1)"), "EventC");
        var afterFakeC = new TestLocation(generator, "afterFakeC", "I(EventC|1)", "Ether");

        var calculator = new SphereCalculator<ItemLocation>(generator, graph);
        calculator.CalculateSpheres(new OrderedSet<ItemLocation>([afterFakeC, fakeC, afterKey, key, fakeB, fakeA]));

        // A fake check's item is there as soon as the check can be done, chains of them included...
        Assert.AreEqual(0, calculator.Spheres[fakeA]);
        Assert.AreEqual(0, calculator.Spheres[fakeB]);
        Assert.AreEqual(0, calculator.Spheres[key]);
        // ...while real items still take a sphere to be found
        Assert.AreEqual(1, calculator.Spheres[afterKey]);
        Assert.AreEqual(1, calculator.Spheres[fakeC]);
        Assert.AreEqual(1, calculator.Spheres[afterFakeC]);
    }

    [TestMethod]
    public void UnreachableFakeChecksAreNotAnError()
    {
        using SeedGenerator generator = new TestSeedGenerator();
        AreaGraph graph = new(generator);
        graph.Areas["Initial"] = new Area(["Initial"]);

        // A fake check without the FakeLocation class (like FF13-2's) that a flag turned off can never reach
        var real = new TestLocation(generator, "real", "", "Potion");
        var never = new TestLocation(generator, "never", "I(Nothing|1)", "Event");
        never.Traits.Add("Fake");

        var calculator = new SphereCalculator<ItemLocation>(generator, graph);
        // Reporting an error would throw here: the test generator has no logger
        calculator.CalculateSpheres(new OrderedSet<ItemLocation>([real, never]));

        Assert.AreEqual(0, calculator.Spheres[real]);
        Assert.IsFalse(calculator.Spheres.ContainsKey(never));
    }
}
