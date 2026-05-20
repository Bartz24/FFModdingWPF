using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando;

public partial class ShopRando : Randomizer
{
    public readonly DataStoreWDB<DataStoreShop> shopsOrig = new();
    public DataStoreWDB<DataStoreShop> shops = new();
    public readonly Dictionary<string, ShopData> shopData = new();

    public ShopRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Shop Data...");
        shopsOrig.LoadWDB(Generator, "LR", @"\db\resident\shop.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Shop Data...", 50, 100);
        shops.LoadWDB(Generator, "LR", @"\db\resident\shop.wdb");

        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        FileHelpers.ReadCSVFile(@"data\shops.csv", row =>
        {
            ShopData s = new(row);
            shopData.Add(s.ID, s);

            string[] fakeData = [s.Area, s.ID + " Shop", "_shop" + s.ID, "", "", "0"];
            FakeLocation fake = new(Generator, fakeData, s.ID + "_Shop");
            fake.Requirements = s.Requirements;
            treasureRando.ItemLocations.Add(fake.ID, fake);

        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        RandoUI.SetUIProgressIndeterminate("Randomizing Shop Data...");
        equipRando.itemWeapons.Values.Where(i => equipRando.items.Keys.Contains(i.record) && ((i.u4WeaponKind == (int)WeaponKind.Weapon && i.u4AccessoryPos == 0) || i.u4WeaponKind == (int)WeaponKind.Shield)).ForEach(i =>
        {
            equipRando.items[i.record].uSellPrice = (int)(2 * equipRando.items[i.record].uSellPrice / Math.Log10(Math.Pow(equipRando.items[i.record].uSellPrice, 1.5) / 1.5));
            equipRando.items[i.record].uSellPrice = equipRando.items[i.record].uSellPrice.RoundToSignificantDigits((int)Math.Max(2, Math.Ceiling(Math.Log10(equipRando.items[i.record].uSellPrice) - 2)));
        });

        RandoUI.SetUIProgressDeterminate("Randomizing Shop Data...", 20, 100);
        if (LRFlags.Items.Shops.FlagEnabled)
        {
            LRFlags.Items.Shops.SetRand();

            Dictionary<string, List<string>> uniqueShops = new()
            {
                { "shop_ptl", new List<string>() },
                { "shop_equ_wd00", new List<string>() }
            };
            shops.Values.Where(s => s.u3Category == (int)ShopCategory.Forge).Shuffle().Take(3).ForEach(s => uniqueShops.Add(s.record, new List<string>()));

            Dictionary<string, List<string>> shopsDict = new();

            shopData.Keys.Select(s => shopsOrig[s]).ForEach(s => shopsDict.Add(s.record, s.GetItems().Where(i =>
                  s.u3Category == (int)ShopCategory.Inn ||
                  (s.u3Category == (int)ShopCategory.Libra && (
                                !i.StartsWith("libra") ||
                                treasureRando.ItemLocations.Values.Where(t => treasureRando.ItemLocations[t.ID].GetItem(false).Value.Item1 == i).Count() == 0))
                  ).ToList())
            );

            Dictionary<string, int> maxSizes = shopsDict.Keys.ToDictionary(k => k, k =>
                shopsDict[k].Count + RandomNum.NextInt(
                    shopsOrig[k].u3Category is ((int)ShopCategory.Ark) or ((int)ShopCategory.Items) ? 1 : 3,
                    (shopsOrig[k].u3Category is ((int)ShopCategory.Ark) or ((int)ShopCategory.Items) ? 6 : 18) - shopsDict[k].Count)
            );

            for (int n = 4; n <= 8; n++)
            {
                shopsOrig.Values.Where(s => s.u3Category == (int)ShopCategory.Libra && shopsDict.ContainsKey(s.record)).ForEach(s => shopsDict[s.record].Add("mat_abi_0_0" + n));
            }

            for (int n = 3; n <= 8; n++)
            {
                shopsOrig.Values.Where(s => s.u3Category == (int)ShopCategory.Libra && shopsDict.ContainsKey(s.record)).ForEach(s => shopsDict[s.record].Add("mat_cus_0_0" + n));
            }

            foreach (string equip in equipRando.RemainingEquip.Where(a => equipRando.itemData[a].Category != "Accessory").ToList())
            {
                AddToRandomShop(uniqueShops, shopsDict, equip, true);
                equipRando.RemainingEquip.Remove(equip);
            }

            foreach (string equip in equipRando.RemainingEquip.Shuffle().Take((int)(equipRando.RemainingEquip.Count * 0.4)))
            {
                AddToRandomShop(uniqueShops, shopsDict, equip, true);
            }

            foreach (string adorn in equipRando.RemainingAdorn.Where(a => equipRando.itemData[a].Traits.Contains("Always")).ToList())
            {
                AddToRandomShop(uniqueShops, shopsDict, adorn);
                equipRando.RemainingAdorn.Remove(adorn);
            }

            foreach (string adorn in equipRando.RemainingAdorn.Shuffle().Take((int)(equipRando.RemainingAdorn.Count * 0.2)))
            {
                AddToRandomShop(uniqueShops, shopsDict, adorn);
            }

            RandoUI.SetUIProgressDeterminate("Randomizing Shop Data...", 70, 100);

            List<string> possibleItems = new();
            possibleItems.AddRange(equipRando.itemData.Values.Where(i => i.Category == "Item").Select(i => i.ID));
            foreach (string shop in shopsDict.Keys.Shuffle().OrderBy(s => s != "shop_ptl_pt00"))
            {
                string unique = uniqueShops.Keys.Where(k => shop.StartsWith(k)).FirstOrDefault();
                while (shopsDict[shop].Count < maxSizes[shop])
                {
                    IList<string> possible = possibleItems.Where(i =>
                        IsValid(i, (ShopCategory)shopsOrig[shop].u3Category)
                        && !shopsDict[shop].Contains(i)
                        && ((uniqueShops.Keys.Where(k => shop.StartsWith(k)).Count() == 0 && !uniqueShops.Values.SelectMany(l => l).Contains(i)) || uniqueShops.Keys.Where(k => shop.StartsWith(k)).Count() > 0)
                    ).Shuffle();
                    if (possible.Count == 0)
                    {
                        break;
                    }

                    string next = possible.First();
                    shopsDict[shop].Add(next);

                    if (!string.IsNullOrEmpty(unique) && !uniqueShops[unique].Contains(next))
                    {
                        uniqueShops[unique].Add(next);
                    }
                }

                if (shopsOrig[shop].u3Category == (int)ShopCategory.Libra)
                {
                    shopsDict[shop].AddRange(treasureRando.ShopRandoLists[shop]);
                }

                shops[shop].SetItems(shopsDict[shop].OrderBy(s => s.StartsWith("e")).ThenByDescending(s => equipRando.items[s].uItemNum).ToList());
                shops[shop].SetItemFlags(Enumerable.Range(0, 32).Select(_ => 0).ToList());
            }

            RandomNum.ClearRand();
        }

        void AddToRandomShop(Dictionary<string, List<string>> uniqueShops, Dictionary<string, List<string>> shopsDict, string equip, bool multiple = false)
        {
            // Get the next shop to add to
            string next = shopsDict.Keys.Where(k => shopsDict[k].Count < 32 && IsValid(equip, (ShopCategory)shopsOrig[k].u3Category) && !shopsDict[k].Contains(equip)).Shuffle().FirstOrDefault();

            if (next == null)
            {
                return;
            }

            // Add to random shop
            shopsDict[next].Add(equip);

            // Add to unique shop if this is a unique shop
            string unique = uniqueShops.Keys.Where(k => next.StartsWith(k)).FirstOrDefault();
            if (!string.IsNullOrEmpty(unique))
            {
                uniqueShops[unique].Add(equip);
            }
            // Otherwise, have a chance to add to another shop
            else if (multiple && RandomNum.RandInt(0, 99) < 60)
            {
                AddToRandomShop(uniqueShops, shopsDict, equip, true);
            }
        }
    }

    private bool IsValid(string item, ShopCategory shop)
    {
        return shop is ShopCategory.Ark or ShopCategory.Items
            ? item.StartsWith("it")
            : shop == ShopCategory.Forge
            ? (item.StartsWith("e") && item.Length == 4) || item.StartsWith("wea") || item.StartsWith("shi")
            : shop == ShopCategory.Outfitters
&& ((item.StartsWith("e") && item.Length == 4) || item.StartsWith("cos") || item.StartsWith("acc"));
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Shop Data...");
        shops.SaveWDB(Generator, @"\db\resident\shop.wdb");
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        EquipRando equipRando = Generator.Get<EquipRando>();
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        TextRando textRando = Generator.Get<TextRando>();
        HTMLPage page = new("Shops", "template/documentation.html");

        shopData.Keys.Select(s => shops[s]).Where(s => s.u3Category is ((int)ShopCategory.Ark) or ((int)ShopCategory.Forge) or ((int)ShopCategory.Items) or ((int)ShopCategory.Libra) or ((int)ShopCategory.Outfitters)).ForEach(shop =>
           {
               ShopData shopInfo = shopData[shop.record];
               string name = $"{shopInfo.Area} {shopInfo.SubArea} - {textRando.mainSysUS[shop.sShopNameLabel]}";
               if (shopInfo.DayStart != -1)
               {
                   name += $" - Day {shopInfo.DayStart}";
                   if (shopInfo.DayEnd != -1)
                   {
                       name += $" - {shopInfo.DayEnd}";
                   }
                   else
                   {
                       name += "+";
                   }
               }

               if (!string.IsNullOrEmpty(shopInfo.AdditionalInfo))
               {
                   name += $" ({shopInfo.AdditionalInfo})";
               }

               page.HTMLElements.Add(new Table(name, (new string[] { "New Contents" }).ToList(), (new int[] { 100 }).ToList(), shop.GetItems().Select(itemID =>
               {
                   string name;
                   if (itemID == "")
                   {
                       name = "Gil";
                   }
                   else if (abilityRando.abilities.Keys.Contains(itemID))
                   {
                       name = textRando.mainSysUS[abilityRando.abilities[itemID].sStringResId];
                   }
                   else
                   {
                       name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId].Replace("{Var83 182}", "Omega");
                       if (name.Contains("{End}"))
                       {
                           name = name.Substring(0, name.IndexOf("{End}"));
                       }
                   }

                   return (new string[] { name }).ToList();
               }).ToList()));
           });
        pages.Add("shops", page);
        return pages;
    }
}
