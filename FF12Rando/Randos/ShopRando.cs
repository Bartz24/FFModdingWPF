﻿using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class ShopRando : Randomizer
{
    public DataStoreBPShop shops;
    public DataStoreBPShop shopsOrig;
    public DataStoreBPSection<DataStoreBazaar> bazaars;
    private readonly Dictionary<int, ShopData> shopData = new();

    public ShopRando(RandomizerManager randomizers) : base(randomizers) { }
    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Shop Data...", 0, -1);
        shops = new DataStoreBPShop();
        shops.LoadData(File.ReadAllBytes($"data\\randoShops.bin"));
        shopsOrig = new DataStoreBPShop();
        shopsOrig.LoadData(File.ReadAllBytes($"data\\vanillaShops.bin"));

        bazaars = new DataStoreBPSection<DataStoreBazaar>();
        bazaars.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin"));

        FileHelpers.ReadCSVFile(@"data\shops.csv", row =>
        {
            ShopData s = new(row);
            shopData.Add(s.ID, s);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Shop Data...", 0, -1);
        EquipRando equipRando = Randomizers.Get<EquipRando>();
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();

        if (FF12Flags.Items.Bazaars.FlagEnabled)
        {
            FF12Flags.Items.Bazaars.SetRand();

            bazaars.DataList.Shuffle().ForEach(b =>
            {
                List<string> items = new();
                if (treasureRando.remainingRandomizeItems.Where(item => IsMonograph(item)).Count() > 0)
                {
                    string item1 = treasureRando.remainingRandomizeItems.Where(item => IsMonograph(item)).Shuffle().First();
                    items.Add(item1);
                    treasureRando.remainingRandomizeItems.Remove(item1);
                }
                else if (treasureRando.remainingRandomizeItems.Where(item => equipRando.itemData.ContainsKey(item) && equipRando.itemData[item].Rank >= 10).Count() > 0)
                {
                    string item1 = treasureRando.remainingRandomizeItems.Where(item => equipRando.itemData.ContainsKey(item) && equipRando.itemData[item].Rank >= 10).Shuffle().First();
                    items.Add(item1);
                    treasureRando.remainingRandomizeItems.Remove(item1);
                }
                else
                {
                    string item1 = RandomNum.SelectRandomWeighted(treasureRando.remainingRandomizeItems, _ => 1);
                    items.Add(item1);
                    if (ShouldRemoveItem(item1))
                    {
                        treasureRando.remainingRandomizeItems.Remove(item1);
                    }
                }

                string newItem;
                if (RandomNum.RandInt(0, 99) < 25)
                {
                    newItem = RandomNum.SelectRandomWeighted(treasureRando.remainingRandomizeItems, _ => 1);
                    items.Add(newItem);
                    if (ShouldRemoveItem(newItem))
                    {
                        treasureRando.remainingRandomizeItems.Remove(newItem);
                    }
                }

                if (RandomNum.RandInt(0, 99) < 5)
                {
                    newItem = RandomNum.SelectRandomWeighted(treasureRando.remainingRandomizeItems, _ => 1);
                    items.Add(newItem);
                    if (ShouldRemoveItem(newItem))
                    {
                        treasureRando.remainingRandomizeItems.Remove(newItem);
                    }
                }

                b.Item1ID = Convert.ToUInt16(items[0], 16);
                b.Item1Amount = 1;
                if (b.Flag == BazaarType.Repeatable)
                {
                    b.Flag = BazaarType.NonRepeatable;
                }

                if (items.Count > 1)
                {
                    b.Item2ID = Convert.ToUInt16(items[1], 16);
                    b.Item2Amount = 1;
                }
                else
                {
                    b.Item2ID = 0xFFFF;
                    b.Item2Amount = 0;
                }

                if (items.Count > 2)
                {
                    b.Item3ID = Convert.ToUInt16(items[2], 16);
                    b.Item3Amount = 1;
                }
                else
                {
                    b.Item3ID = 0xFFFF;
                    b.Item3Amount = 0;
                }
            });

            RandomNum.ClearRand();
        }

        if (FF12Flags.Items.Shops.FlagEnabled)
        {
            FF12Flags.Items.Shops.SetRand();

            Dictionary<string, int> locationsShared = new();
            List<string> used = new();
            shopData.Values.Shuffle().OrderByDescending(s => s.Traits.Contains("Unique")).ForEach(s =>
            {
                DataStoreShop shop = shops[s.ID];
                if (FF12Flags.Items.ShopsShared.Enabled && !s.Traits.Contains("Unique") && locationsShared.ContainsKey(s.Area))
                {
                    shop.SetItems(shops[locationsShared[s.Area]].GetItems());
                }
                else
                {
                    List<string> items = new();
                    int count = Math.Min(FF12Flags.Items.ShopSize.Value, shop.Count);

                    for (int i = 0; i < count; i++)
                    {
                        string newItem = RandomNum.SelectRandomWeighted(treasureRando.remainingRandomizeItems, item =>
                        {
                            if (items.Contains(item))
                            {
                                return 0;
                            }

                            return s.Traits.Contains("Missable") && (item.StartsWith("30") || item.StartsWith("40"))
                                ? 0
                                : !s.Traits.Contains("Unique") && used.Count < treasureRando.remainingRandomizeItems.Count && used.Contains(item)
                                ? 0
                                : item == "2000" ? 25 : item.StartsWith("20") || item.StartsWith("21") ? 1 : 10;
                        });
                        items.Add(newItem);

                        if (!s.Traits.Contains("Unique") && !used.Contains(newItem))
                        {
                            used.Add(newItem);
                        }

                        if (s.Traits.Contains("Unique") && !newItem.StartsWith("00") && !newItem.StartsWith("20") && !newItem.StartsWith("21"))
                        {
                            treasureRando.remainingRandomizeItems.Remove(newItem);
                        }
                    }

                    shop.SetItems(items.OrderBy(i => i).ToList());

                    if (FF12Flags.Items.ShopsShared.Enabled && !s.Traits.Contains("Unique"))
                    {
                        locationsShared.Add(s.Area, s.ID);
                    }
                }
            });

            RandomNum.ClearRand();
        }
    }

    private bool ShouldRemoveItem(string newItem)
    {
        EquipRando equipRando = Randomizers.Get<EquipRando>();
        return !newItem.StartsWith("00") && !newItem.StartsWith("20") && !newItem.StartsWith("21")
&& (newItem.StartsWith("30") || newItem.StartsWith("40")
|| !equipRando.itemData.ContainsKey(newItem) || RandomNum.RandInt(0, 100) < Math.Pow(equipRando.itemData[newItem].Rank, 2));
    }

    private static bool IsMonograph(string item)
    {
        return item is "8065" or "8066" or "8067" or "8068" or "8069" or "806A" or "806B" or "806C";
    }

    public List<string> GetRandomizableShopItems()
    {
        return shopsOrig.DataList
            .SelectMany(s => s.ItemsList)
            .Select(i =>
            {
                string item = i.Item.ToString("X4");
                return item != "FFFF" && !item.StartsWith("60") && !item.StartsWith("61") ? item : null;
            }).Where(s => s != null).Distinct().ToList();
    }
    public List<string> GetRandomizableBazaarItems()
    {
        return bazaars.DataList
            .SelectMany(b =>
            {
                string[] items = new string[3];
                string item1 = b.Item1ID.ToString("X4");
                if (b.Item1Amount > 0)
                {
                    items[0] = item1;
                }

                string item2 = b.Item2ID.ToString("X4");
                if (b.Item2Amount > 0)
                {
                    items[1] = item2;
                }

                string item3 = b.Item3ID.ToString("X4");
                if (b.Item3Amount > 0)
                {
                    items[2] = item3;
                }

                return items;
            }).Where(s => s != null).Distinct().ToList();
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
        TextRando textRando = Randomizers.Get<TextRando>();
        HTMLPage page = new("Shops", "template/documentation.html");

        shopData.Values.ForEach(s =>
        {
            DataStoreShop shop = shops[s.ID];
            string name = $"{s.Area} - {s.Name}";

            page.HTMLElements.Add(new Table(name, (new string[] { "New Contents" }).ToList(), (new int[] { 100 }).ToList(), shop.GetItems().Select(itemID =>
            {
                return (new string[] { treasureRando.GetItemName(itemID) }).ToList();
            }).ToList()));
        });

        page.HTMLElements.Add(new Table("Bazaars", (new string[] { "Requirements", "New Contents" }).ToList(), (new int[] { 50, 50 }).ToList(), bazaars.DataList.Select(bazaar =>
        {
            List<string> reqs = new();
            if (bazaar.Ingredient1Amount > 0)
            {
                reqs.Add($"{treasureRando.GetItemName(bazaar.Ingredient1ID.ToString("X4"))} x {bazaar.Ingredient1Amount}");
            }

            if (bazaar.Ingredient2Amount > 0)
            {
                reqs.Add($"{treasureRando.GetItemName(bazaar.Ingredient2ID.ToString("X4"))} x {bazaar.Ingredient2Amount}");
            }

            if (bazaar.Ingredient3Amount > 0)
            {
                reqs.Add($"{treasureRando.GetItemName(bazaar.Ingredient3ID.ToString("X4"))} x {bazaar.Ingredient3Amount}");
            }

            List<string> items = new();
            if (bazaar.Item1Amount > 0)
            {
                items.Add($"{treasureRando.GetItemName(bazaar.Item1ID.ToString("X4"))} x {bazaar.Item1Amount}");
            }

            if (bazaar.Item2Amount > 0)
            {
                items.Add($"{treasureRando.GetItemName(bazaar.Item2ID.ToString("X4"))} x {bazaar.Item2Amount}");
            }

            if (bazaar.Item3Amount > 0)
            {
                items.Add($"{treasureRando.GetItemName(bazaar.Item3ID.ToString("X4"))} x {bazaar.Item3Amount}");
            }

            return (new string[] { string.Join(", ", reqs), string.Join(", ", items) }).ToList();
        }).ToList()));

        pages.Add("shops", page);
        return pages;
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Shop Data...", 0, -1);
        if (FF12Flags.Items.Shops.FlagEnabled)
        {
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_039.bin", shops.Data);
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin", bazaars.Data);
        }
    }
    public class ShopData : CSVDataRow
    {
        [RowIndex(0)]
        public string Name { get; set; }
        [RowIndex(1)]
        public int ID { get; set; }
        [RowIndex(2)]
        public string Area { get; set; }
        [RowIndex(3)]
        public List<string> Traits { get; set; }
        public ShopData(string[] row) : base(row)
        {
        }
    }
}
