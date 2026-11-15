using Bartz24.FF12;

namespace Bartz24.FF12.Tests;

[TestClass]
public class TextSanitizerTests
{
    [TestMethod]
    public void FitPartsKeepsShortRowsUnchanged()
    {
        List<TextPart> parts = new() { new("500 Gil", CanShorten: false), new("Potion", " x2") };

        Assert.AreEqual("500 Gil, Potion x2", TextSanitizer.FitParts(parts, ", ", 30));
    }

    [TestMethod]
    public void FitPartsKeepsEveryRewardVisible()
    {
        // Ticket #16: Bio was lost after gil + Artemis' Arrow
        List<TextPart> parts = new() { new("8000 Gil", CanShorten: false), new("Artemis' Arrow", " x12"), new("Bio") };

        string row = TextSanitizer.FitParts(parts, ", ", 30);

        Assert.IsTrue(TextSanitizer.Length(row) <= 30, row);
        StringAssert.StartsWith(row, "8000 Gil, ");
        StringAssert.Contains(row, "… x12, ");
        StringAssert.EndsWith(row, ", Bio");
    }

    [TestMethod]
    public void FitPartsShortensLongestNamesFirst()
    {
        List<TextPart> parts = new() { new("Aaaaaaaaaaaaaaaaaaaa"), new("Bbbbbbbbbbbbbbbbbbbb"), new("Ether") };

        string row = TextSanitizer.FitParts(parts, ", ", 30);

        Assert.IsTrue(TextSanitizer.Length(row) <= 30, row);
        StringAssert.EndsWith(row, ", Ether");
        string[] split = row.Split(", ");
        Assert.AreEqual(3, split.Length);
        Assert.AreEqual(split[0].Length, split[1].Length);
    }

    [TestMethod]
    public void FitPartsFallsBackWhenNothingCanFit()
    {
        List<TextPart> parts = new() { new("123456789 Gil", CanShorten: false), new("Potion", " x99"), new("Ether", " x99"), new("Elixir", " x99") };

        string row = TextSanitizer.FitParts(parts, ", ", 30);

        Assert.IsTrue(TextSanitizer.Length(row) <= 30, row);
        StringAssert.EndsWith(row, "…");
    }
}
