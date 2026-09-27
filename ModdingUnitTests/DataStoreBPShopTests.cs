using Bartz24.Data;
using Bartz24.FF12;

namespace Bartz24.RandoWPF.Tests;

[TestClass]
public class DataStoreBPShopTests
{
    private static readonly string[] ShopFiles =
    {
        @"..\..\..\..\FF12Rando\bin\data\vanillaShops.bin",
        @"..\..\..\..\FF12Rando\bin\data\randoShops.bin"
    };

    [TestInitialize]
    public void SetByteMode()
    {
        // FF12 is little endian; the app sets this at startup.
        DataExtensions.Mode = ByteMode.LittleEndian;
    }

    private static DataStoreBPShop Load(string path)
    {
        DataStoreBPShop shops = new();
        shops.LoadData(File.ReadAllBytes(path));
        return shops;
    }

    [TestMethod]
    public void ReadingAndWritingBackIsUnchanged()
    {
        foreach (string path in ShopFiles)
        {
            byte[] original = File.ReadAllBytes(path);
            CollectionAssert.AreEqual(original, Load(path).Data, $"{path} did not round trip");
        }
    }

    [TestMethod]
    public void AddingAndRemovingItemsKeepsEveryShopReadable()
    {
        foreach (string path in ShopFiles)
        {
            DataStoreBPShop shops = Load(path);

            // Grow the first shop, empty the second, leave the rest alone.
            List<string> grown = shops[0].GetItems();
            grown.AddRange(new[] { "0001", "0002", "0003" });
            shops[0].SetItems(grown);
            shops[1].SetItems(new List<string>());

            List<List<string>> expected = shops.DataList.Select(s => s.GetItems()).ToList();

            DataStoreBPShop reloaded = new();
            reloaded.LoadData(shops.Data);

            Assert.AreEqual(expected.Count, reloaded.DataList.Count, $"{path} lost shops");
            for (int i = 0; i < expected.Count; i++)
            {
                CollectionAssert.AreEqual(expected[i], reloaded[i].GetItems(), $"{path} shop {i} changed");
            }
        }
    }

    [TestMethod]
    public void EveryClanShopCanSellElixirsInTheVanillaShops()
    {
        // The non Missable clan shops, any of which can be picked to sell elixirs. With the shops not
        // randomized the elixir is added to the vanilla file, so it has to fit without disturbing
        // anything else.
        int[] clanShops = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        const string elixir = "0005";

        foreach (int id in clanShops)
        {
            DataStoreBPShop shops = Load(ShopFiles[0]);
            List<List<string>> expected = shops.DataList.Select(s => s.GetItems()).ToList();

            List<string> items = shops[id].GetItems();
            items.Add(elixir);
            shops[id].SetItems(items.OrderBy(i => i).ToList());

            DataStoreBPShop reloaded = new();
            reloaded.LoadData(shops.Data);

            CollectionAssert.Contains(reloaded[id].GetItems(), elixir, $"clan shop {id} lost the elixir");
            for (int i = 0; i < expected.Count; i++)
            {
                if (shops.DataList[i] != shops[id])
                {
                    CollectionAssert.AreEqual(expected[i], reloaded.DataList[i].GetItems(), $"adding to clan shop {id} changed shop {i}");
                }
            }
        }
    }

    [TestMethod]
    public void ResizingKeepsNodesAlignedAndInBounds()
    {
        DataStoreBPShop shops = Load(ShopFiles[1]);
        shops[0].SetItems(new List<string> { "0001" });

        byte[] data = shops.Data;
        Assert.AreEqual(0, data.Length % 8, "file is not padded to 8");

        // Every shop's items have to land inside the file, on a 4 byte aligned node.
        foreach (DataStoreShop shop in shops.DataList)
        {
            Assert.AreEqual(0u, shop.Offset % 4, $"shop {shop.ID} node is misaligned");
            Assert.IsTrue(shop.Offset + DataStoreBPShop.NodeHeaderSize + (shop.Count * 2) <= data.Length,
                $"shop {shop.ID} runs past the end of the file");
        }
    }
}
