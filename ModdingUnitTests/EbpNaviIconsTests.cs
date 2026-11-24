using Bartz24.FF12;
using System.Buffers.Binary;
using System.Text;

namespace Bartz24.RandoWPF.Tests;

[TestClass]
public class EbpNaviIconsTests
{
    private const string PlanMap = @"..\..\..\..\FF12Rando\bin\data\ps2data\plan_master\us\plan_map";

    // North Sprawl has navi data; the Grand Arcade does not.
    private static readonly string WithNavi = Path.Combine(PlanMap, @"slm_a\slm_a01\global\slm_a01.ebp");
    private static readonly string WithoutNavi = Path.Combine(PlanMap, @"alc_a\alc_a01\global\alc_a01.ebp");

    private static EbpNaviIcons.Entry[] Added =
    {
        new(10, 20, 10, 20, 63, 0x8000),
        new(30, 40, 30, 40, 63, 0x8001)
    };

    [TestMethod]
    public void BuildingKeepsExistingIconsAndAddsNewOnes()
    {
        byte[] ebp = File.ReadAllBytes(WithNavi);
        List<EbpNaviIcons.Entry>[] before = EbpNaviIcons.ReadSections(ebp);
        Assert.IsTrue(before[5].Count > 0, "test map should have minimap icons");

        byte[] blob = EbpNaviIcons.BuildWithEntries(ebp, EbpNaviIcons.MapScreenSection, Added);
        List<EbpNaviIcons.Entry>[] after = EbpNaviIcons.ReadBlob(blob, 0);

        for (int i = 0; i < EbpNaviIcons.SectionCount; i++)
        {
            IEnumerable<EbpNaviIcons.Entry> expected = i == EbpNaviIcons.MapScreenSection ? before[i].Concat(Added) : before[i];
            CollectionAssert.AreEqual(expected.ToList(), after[i], $"section {i}");
        }
    }

    [TestMethod]
    public void UnchangedSectionsRebuildToTheOriginalBytes()
    {
        byte[] ebp = File.ReadAllBytes(WithNavi);
        int offset = (int)BinaryPrimitives.ReadUInt32LittleEndian(ebp.AsSpan(0x20));
        byte[] rebuilt = EbpNaviIcons.BuildBlob(EbpNaviIcons.ReadSections(ebp));

        CollectionAssert.AreEqual(ebp.Skip(offset).Take(rebuilt.Length).ToArray(), rebuilt);
    }

    [TestMethod]
    public void MapWithoutNaviDataGetsAFreshBlob()
    {
        byte[] ebp = File.ReadAllBytes(WithoutNavi);
        Assert.AreEqual(0u, BinaryPrimitives.ReadUInt32LittleEndian(ebp.AsSpan(0x20)), "test map should have no navi data");

        byte[] blob = EbpNaviIcons.BuildWithEntries(ebp, EbpNaviIcons.MapScreenSection, Added);

        Assert.AreEqual("NAVIICN2", Encoding.ASCII.GetString(blob, 0, 8));
        Assert.AreEqual(EbpNaviIcons.SectionCount, BinaryPrimitives.ReadUInt16LittleEndian(blob.AsSpan(8)));
        Assert.AreEqual(0x32, BinaryPrimitives.ReadUInt16LittleEndian(blob.AsSpan(0x0A + 4 * EbpNaviIcons.MapScreenSection)));

        List<EbpNaviIcons.Entry>[] sections = EbpNaviIcons.ReadBlob(blob, 0);
        CollectionAssert.AreEqual(Added, sections[EbpNaviIcons.MapScreenSection]);
        Assert.AreEqual(Added.Length, sections.Sum(s => s.Count));
    }
}
