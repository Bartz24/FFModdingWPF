﻿using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class ShopRando : Randomizer
    {
        DataStoreDB3<DataStoreShop> shopsOrig = new DataStoreDB3<DataStoreShop>();
        DataStoreDB3<DataStoreShop> shops = new DataStoreDB3<DataStoreShop>();

        public ShopRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Shops...";
        }
        public override string GetID()
        {
            return "Shops";
        }

        public override void Load()
        {
            shopsOrig.LoadDB3("LR", @"\db\resident\shop.wdb");
            shops.LoadDB3("LR", @"\db\resident\shop.wdb");

            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            equipRando.items["mat_z_000"].uPurchasePrice = 1100;
            equipRando.items["mat_z_001"].uPurchasePrice = 2700;
            equipRando.items["mat_z_002"].uPurchasePrice = 1100;
            equipRando.items["mat_z_003"].uPurchasePrice = 1200;
            equipRando.items["mat_z_004"].uPurchasePrice = 3000;
            equipRando.items["mat_z_007"].uPurchasePrice = 2200;
            equipRando.items["mat_z_008"].uPurchasePrice = 3600;
            equipRando.items["mat_z_009"].uPurchasePrice = 1900;
            equipRando.items["mat_z_010"].uPurchasePrice = 4200;
            equipRando.items["mat_z_011"].uPurchasePrice = 3800;
            equipRando.items["mat_z_012"].uPurchasePrice = 3000;
            equipRando.items["mat_z_013"].uPurchasePrice = 3800;
            equipRando.items["mat_z_014"].uPurchasePrice = 9400;
            equipRando.items["mat_z_015"].uPurchasePrice = 3000;
            equipRando.items["mat_z_016"].uPurchasePrice = 5000;
            equipRando.items["mat_z_017"].uPurchasePrice = 2300;
            equipRando.items["mat_z_018"].uPurchasePrice = 5800;
            equipRando.items["mat_z_019"].uPurchasePrice = 3800;
            equipRando.items["mat_z_020"].uPurchasePrice = 3800;
            equipRando.items["mat_z_021"].uPurchasePrice = 4200;
            equipRando.items["mat_z_022"].uPurchasePrice = 5800;
            equipRando.items["mat_z_024"].uPurchasePrice = 12500;
            equipRando.items["mat_z_028"].uPurchasePrice = 12500;
            equipRando.items["mat_z_029"].uPurchasePrice = 12500;
            equipRando.items["mat_z_030"].uPurchasePrice = 12500;
            equipRando.items["mat_z_031"].uPurchasePrice = 8300;
            equipRando.items["mat_z_032"].uPurchasePrice = 7500;
            equipRando.items["mat_z_033"].uPurchasePrice = 6800;
            equipRando.items["mat_z_035"].uPurchasePrice = 12500;
            equipRando.items["mat_z_036"].uPurchasePrice = 12500;
            equipRando.items["mat_z_044"].uPurchasePrice = 12500;
            equipRando.items["mat_z_045"].uPurchasePrice = 5800;

        }
        public override void Randomize(Action<int> progressSetter)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");

            equipRando.itemWeapons.Values.Where(i => equipRando.items.Keys.Contains(i.name) && (i.u4WeaponKind == (int)WeaponKind.Weapon && i.u4AccessoryPos == 0 || i.u4WeaponKind == (int)WeaponKind.Shield)).ForEach(i =>
            {
                equipRando.items[i.name].uSellPrice = (int)(2 * equipRando.items[i.name].uSellPrice / Math.Log10(Math.Pow(equipRando.items[i.name].uSellPrice, 1.5) / 1.5));
                equipRando.items[i.name].uSellPrice = equipRando.items[i.name].uSellPrice.RoundToSignificantDigits((int)Math.Max(2, Math.Ceiling(Math.Log10(equipRando.items[i.name].uSellPrice) - 2)));
            });

            if (LRFlags.Other.Shops.FlagEnabled)
            {
                LRFlags.Other.Shops.SetRand();

                Dictionary<string, List<string>> uniqueShops = new Dictionary<string, List<string>>();
                uniqueShops.Add("shop_ptl", new List<string>());
                uniqueShops.Add("shop_equ_wd00", new List<string>());
                shops.Values.Where(s => s.u3Category == (int)ShopCategory.Forge).ToList().Shuffle().Take(3).ForEach(s => uniqueShops.Add(s.name, new List<string>()));

                Dictionary<string, List<string>> shopsDict = new Dictionary<string, List<string>>();

                shopsOrig.Values.ForEach(s => shopsDict.Add(s.name, s.GetItems().Where(i => 
                s.u3Category == (int)ShopCategory.Inn || 
                (s.u3Category == (int)ShopCategory.Libra && (!i.StartsWith("libra") || treasureRando.treasures.Values.Where(t => t.s11ItemResourceId_string == i).Count() == 0)) || 
                i.StartsWith("e") && i.Length == 4).ToList()));
                Dictionary<string, int> maxSizes = shopsDict.Keys.ToDictionary(k => k, k =>
                    shopsDict[k].Count + RandomNum.RandInt(
                        shopsOrig[k].u3Category == (int)ShopCategory.Ark || shopsOrig[k].u3Category == (int)ShopCategory.Items ? 1 : 3, 
                        (shopsOrig[k].u3Category == (int)ShopCategory.Ark || shopsOrig[k].u3Category == (int)ShopCategory.Items ? 6 : 18) - shopsDict[k].Count)
                );
                List<string> libraMaterials = new List<string>();
                shopsOrig.Values.Where(s => s.u3Category == (int)ShopCategory.Libra).ForEach(_ => libraMaterials.AddRange(equipRando.items.Keys.Where(i => i.StartsWith("mat_z")).ToList().Shuffle()));

                foreach (string equip in treasureRando.RemainingEquip)
                {
                    string next = shopsDict.Keys.Where(k => shopsDict[k].Count < 32 && isValid(equip, (ShopCategory)shopsOrig[k].u3Category)).ToList().Shuffle().First();
                    shopsDict[next].Add(equip);
                    string unique = uniqueShops.Keys.Where(k => next.StartsWith(k)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(unique))
                        uniqueShops[unique].Add(equip);
                }
                List<string> possibleItems = new List<string>();
                possibleItems.AddRange(treasureRando.RemainingEquip);
                possibleItems.AddRange(GetItems());
                foreach (string shop in shopsDict.Keys.ToList().Shuffle().OrderBy(s => s != "shop_ptl_pt00"))
                {
                    string unique = uniqueShops.Keys.Where(k => shop.StartsWith(k)).FirstOrDefault();
                    while (shopsDict[shop].Count < maxSizes[shop])
                    {
                        IList<string> possible = possibleItems.Where(i =>
                            isValid(i, (ShopCategory)shopsOrig[shop].u3Category)
                            && !shopsDict[shop].Contains(i)
                            && (uniqueShops.Keys.Where(k => shop.StartsWith(k)).Count() == 0 && !uniqueShops.Values.SelectMany(l => l).Contains(i) || uniqueShops.Keys.Where(k => shop.StartsWith(k)).Count() > 0)
                        ).ToList().Shuffle();
                        if (possible.Count() == 0)
                            break;
                        string next = possible.First();
                        shopsDict[shop].Add(next);

                        if (!string.IsNullOrEmpty(unique) && !uniqueShops[unique].Contains(next))
                            uniqueShops[unique].Add(next);
                    }

                    if (shopsOrig[shop].u3Category == (int)ShopCategory.Libra)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < libraMaterials.Count; j++)
                            {
                                if (!shopsDict[shop].Contains(libraMaterials[j]))
                                {
                                    shopsDict[shop].Add(libraMaterials[j]);
                                    libraMaterials.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                    }

                    shops[shop].SetItems(shopsDict[shop].OrderByDescending(s => equipRando.items[s].uItemNum).ToList());
                    shops[shop].SetItemFlags(Enumerable.Range(0, 32).Select(_ => 0).ToList());
                }


                RandomNum.ClearRand();
            }
        }
        public List<string> GetRandomizableEquip()
        {
            if (!LRFlags.Other.Shops.FlagEnabled)
                return new List<string>();
            Func<string, bool> isEquip = s => s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi");
            List<string> list = new List<string>();
            list.AddRange(shopsOrig.Values.SelectMany(s => s.GetItems().Where(i => isEquip(i))));

            return list.Distinct().ToList();
        }
        public List<string> GetAdornments()
        {
            if (!LRFlags.Other.Shops.FlagEnabled)
                return new List<string>();
            Func<string, bool> isAdorn = s => s.StartsWith("e") && s.Length == 4;
            List<string> list = new List<string>();
            list.AddRange(shopsOrig.Values.SelectMany(s => s.GetItems().Where(i => isAdorn(i))));

            return list.Distinct().ToList();
        }
        public List<string> GetItems()
        {
            if (!LRFlags.Other.Shops.FlagEnabled)
                return new List<string>();
            Func<string, bool> isItem = s => s.StartsWith("it");
            List<string> list = new List<string>();
            list.AddRange(shopsOrig.Values.SelectMany(s => s.GetItems().Where(i => isItem(i))));

            return list.Distinct().ToList();
        }

        private bool isValid(string item, ShopCategory shop)
        {
            if (shop == ShopCategory.Ark || shop == ShopCategory.Items)
                return item.StartsWith("it");
            if (shop == ShopCategory.Forge)
                return item.StartsWith("e") && item.Length == 4 || item.StartsWith("wea") || item.StartsWith("shi");
            if (shop == ShopCategory.Outfitters)
                return item.StartsWith("e") && item.Length == 4 || item.StartsWith("cos");
            return false;
        }

        public override void Save()
        {
            shops.SaveDB3(@"\db\resident\shop.wdb");
        }

        public override HTMLPage GetDocumentation()
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = randomizers.Get<TextRando>("Text");
            HTMLPage page = new HTMLPage("Shops", "template/documentation.html");

            shops.Values.Where(s => s.u3Category == (int)ShopCategory.Ark || s.u3Category == (int)ShopCategory.Forge || s.u3Category == (int)ShopCategory.Items || s.u3Category == (int)ShopCategory.Libra || s.u3Category == (int)ShopCategory.Outfitters).ForEach(shop =>
             {

                 page.HTMLElements.Add(new Table($"{textRando.mainSysUS[shop.sShopNameLabel_string]} - Day {shop.u4Day} - ?", new string[] { "New Contents" }.ToList(), new int[] { 100 }.ToList(), shop.GetItems().Select(itemID =>
                 {
                     string name;
                     if (itemID == "")
                         name = "Gil";
                     else if (abilityRando.abilities.Keys.Contains(itemID))
                         name = textRando.mainSysUS[abilityRando.abilities[itemID].sStringResId_string];
                     else
                     {
                         name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId_string].Replace("{Var83 182}", "Omega");
                         if (name.Contains("{End}"))
                             name = name.Substring(0, name.IndexOf("{End}"));
                     }
                     return new string[] { name }.ToList();
                 }).ToList()));
             });
            return page;
        }
    }
}
