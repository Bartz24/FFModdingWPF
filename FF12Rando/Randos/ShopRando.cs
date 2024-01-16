using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FF12Rando.TreasureRando;
using static System.Net.Mime.MediaTypeNames;

namespace FF12Rando;

public partial class ShopRando : Randomizer
{
    public DataStoreBPShop shops;
    public DataStoreBPShop shopsOrig;
    public DataStoreBPSection<DataStoreBazaar> bazaars;
    private readonly Dictionary<int, ShopData> shopData = new();

    public ShopRando(SeedGenerator randomizers) : base(randomizers) { }
    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Shop Data...");
        shops = new DataStoreBPShop();
        shops.LoadData(File.ReadAllBytes($"data\\randoShops.bin"));
        shopsOrig = new DataStoreBPShop();
        shopsOrig.LoadData(File.ReadAllBytes($"data\\vanillaShops.bin"));

        bazaars = new DataStoreBPSection<DataStoreBazaar>();
        bazaars.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin"));

        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        FileHelpers.ReadCSVFile(@"data\shops.csv", row =>
        {
            ShopData s = new(row);
            shopData.Add(s.ID, s);

            string[] fakeData = new string[] { s.Area, s.Name + " Shop", "_shop" + s.ID, "", "", "0"};            
            FF12FakeLocation fake = new(Generator, fakeData, s.Name + " Shop");
            fake.Requirements = s.Requirements;
            treasureRando.ItemLocations.Add(fake.ID, fake);

            s.ShopFakeLocationLink = fake.ID;
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Shop Data...");
        EquipRando equipRando = Generator.Get<EquipRando>();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        if (FF12Flags.Items.Bazaars.FlagEnabled)
        {
            FF12Flags.Items.Bazaars.SetRand();

            List<string> bazaarUsed = new();
            bazaars.DataList.Shuffle().ForEach(b =>
            {
                List<string> items = new();
                AddNewBazaarItem(treasureRando, items, bazaarUsed);

                if (RandomNum.RandInt(0, 99) < 40)
                {
                    AddNewBazaarItem(treasureRando, items, bazaarUsed);
                }

                if (RandomNum.RandInt(0, 99) < 8)
                {
                    AddNewBazaarItem(treasureRando, items, bazaarUsed);
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

        if (!FF12Flags.Items.AllowSeitengrat.FlagEnabled)
        {
            bazaars.DataList.ForEach(b =>
            {
                // Replace any Seitengrat 10B2 with the Dhanusha 10C7
                if (b.Item1ID == 0x10B2)
                {
                    b.Item1ID = 0x10C7;
                }

                if (b.Item2ID == 0x10B2)
                {
                    b.Item2ID = 0x10C7;
                }

                if (b.Item3ID == 0x10B2)
                {
                    b.Item3ID = 0x10C7;
                }
            });
        }

        if (FF12Flags.Items.Shops.FlagEnabled)
        {
            HashSet<string> shopItems = equipRando.itemData.Values.Where(i => 
                i.Category is "Weapon" or "Armor" or "Accessory" or "Item" or "Ability" 
                && i.Rank < 10
                && !i.Traits.Contains("Ignore")
                && !treasureRando.ItemPlacer.UsefulItemPlacer.UsedAbilities.Contains(i.ID)).Select(i => i.ID).ToHashSet();
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
                        List<string> possible = shopItems.Where(item => !items.Contains(item)).ToList();
                        if (s.Traits.Contains("Missable"))
                        {
                            possible = possible.Where(item => !item.StartsWith("30") && !item.StartsWith("40")).ToList();
                        }

                        string newItem = null;
                        while (newItem == null)
                        {
                            newItem = RandomNum.SelectRandomWeighted(possible, item =>
                            {
                                if (items.Contains(item))
                                {
                                    return 0;
                                }

                                return !s.Traits.Contains("Unique") && used.Contains(item)
                                    ? 0
                                    : item is "2000" or "0006" ? 25 : item.StartsWith("20") || item.StartsWith("21") ? 1 : 10;
                            }, true);

                            if (newItem == null)
                            {
                                used.Clear();
                            }
                        }

                        items.Add(newItem);

                        if (!s.Traits.Contains("Unique") && !used.Contains(newItem))
                        {
                            used.Add(newItem);
                        }

                        if (s.Traits.Contains("Unique") && !newItem.StartsWith("00") && !newItem.StartsWith("20") && !newItem.StartsWith("21"))
                        {
                            shopItems.Remove(newItem);
                        }
                    }

                    shop.SetItems(items.OrderBy(i => i).ToList());

                    if (FF12Flags.Items.ShopsShared.Enabled && !s.Traits.Contains("Unique"))
                    {
                        locationsShared.Add(s.Area, s.ID);
                    }
                }
            });

            if (FF12Flags.Items.JunkRankScaleShops.Enabled)
            {
                // Get all the shop slots with consumables, equipment, and abilities and group by their item type
                var grouping = shopData.Values.Where(s => !s.Traits.Contains("Unique")).Select(s => shops[s.ID]).SelectMany(shop =>
                {
                    return Enumerable.Range(0, shop.GetItems().Count).Select(index => (shop, index));
                }).GroupBy(itemSlot =>
                {
                    string id = itemSlot.shop.GetItems()[itemSlot.index];
                    int intId = Convert.ToInt32(id, 16);
                    return intId < 0x1000 ? ItemType.Consumable : intId is >= 0x3000 and < 0x5000 ? ItemType.Ability : ItemType.Equipment;
                });

                Dictionary<int, List<string>> newShops = new();

                // Group by type and sort the items by its item rank
                foreach (var group in grouping)
                {
                    List<string> items = group.Shuffle().Select(itemSlot => itemSlot.shop.GetItems()[itemSlot.index]).OrderBy(item =>
                    {
                        return equipRando.itemData[item].Rank;
                    }).ToList();
                    items = RandomNum.ShuffleLocalized(items, 5);

                    // Sort the shop slots by their shop sphere
                    var slots = group.Shuffle().OrderBy(itemSlot => 
                        treasureRando.ItemPlacer.SphereCalculator.Spheres.GetValueOrDefault(treasureRando.ItemLocations[shopData[itemSlot.shop.ID].ShopFakeLocationLink], 0)).ToList();

                    // Go in order and set the junk items
                    for (int i = 0; i < items.Count; i++)
                    {
                        var shop = shopData[slots[i].shop.ID];

                        if (!newShops.ContainsKey(shop.ID))
                        {
                            newShops.Add(shop.ID, new());
                        }

                        // If the item is a duplicate, find a replacement later in the list and swap with it.
                        // If a replacement does not exist, mark the slot as removed.
                        // The slot will be cleared after all the replacements.
                        string newItem = items[i];
                        if (newShops[shop.ID].Contains(newItem))
                        {
                            int swapIndex = -1;
                            for (int j = i + 1; j < items.Count; j++)
                            {
                                if (!newShops[shop.ID].Contains(items[j]))
                                {
                                    swapIndex = j;
                                    break;
                                }
                            }

                            // Just skip adding this item if there is no replacement
                            if (swapIndex == -1)
                            {
                                continue;
                            }

                            (items[i], items[swapIndex]) = (items[swapIndex], items[i]);
                            newItem = items[i];
                        }

                        newShops[slots[i].shop.ID].Add(newItem);
                    }
                }

                // Set the shop items
                foreach (var shop in shops.DataList)
                {
                    // Get the items for indices not changed
                    List<string> items = shop.GetItems()
                        .Where((_, index) => 
                            !grouping.SelectMany(g => g.ToList())
                                     .Any(p => p.shop.ID == shop.ID && p.index == index))
                        .ToList();

                    // Add new items
                    if (newShops.ContainsKey(shop.ID))
                    {
                        items.AddRange(newShops[shop.ID]);
                    }

                    // Remove duplicates, sort, and set items
                    shop.SetItems(items.Distinct().OrderBy(itemId => itemId).ToList());
                }
            }

            RandomNum.ClearRand();
        }
    }

    private void AddNewBazaarItem(TreasureRando treasureRando, List<string> items, List<string> bazaarUsed)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        List<string> possible;
        do
        {
            switch (RandomNum.RandInt(0, 3))
            {
                case 0:
                default: // Consumables
                    possible = equipRando.itemData.Values.Where(i => i.IntID is >= 0x0000 and < 0x1000).Select(i => i.ID).ToList();
                    break;
                case 1:// Equip
                    possible = equipRando.itemData.Values.Where(i => i.IntID is >= 0x1000 and < 0x2000).Select(i => i.ID).ToList();
                    break;
                case 2:// Abilities
                    possible = equipRando.itemData.Values.Where(i => i.IntID is >= 0x3000 and < 0x5000).Select(i => i.ID).ToList();
                    break;
                case 3:// Loot
                    possible = equipRando.itemData.Values.Where(i => i.IntID is >= 0x2000 and < 0x3000 and not 0x2112 and not 0x2113 and not 0x2116).Select(i => i.ID).ToList();
                    break;
            }

            possible.RemoveAll(i => bazaarUsed.Contains(i));
        }
        while (possible.Count == 0);
        string next = RandomNum.SelectRandom(possible);
        items.Add(next);
        bazaarUsed.Add(next);
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        TextRando textRando = Generator.Get<TextRando>();
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

        HTMLPage bazaarPage = new("Bazaars", "template/documentation.html");

        bazaarPage.HTMLElements.Add(new Table("Bazaars", (new string[] { "Requirements", "New Contents" }).ToList(), (new int[] { 50, 50 }).ToList(), bazaars.DataList.Select(bazaar =>
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
        pages.Add("bazaars", bazaarPage);
        return pages;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Shop Data...");
        if (FF12Flags.Items.Shops.FlagEnabled)
        {
            File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_039.bin", shops.Data);
            File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin", bazaars.Data);
        }
    }
}
