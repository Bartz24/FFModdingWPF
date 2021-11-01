using Bartz24.Data;
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
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            if (LRFlags.Other.Shops.FlagEnabled)
            {
                LRFlags.Other.Shops.SetRand();

                Dictionary<string, List<string>> shopsDict = new Dictionary<string, List<string>>();
                shopsOrig.Values.ForEach(s => shopsDict.Add(s.name, s.GetItems().Where(i => s.u3Category == (int)ShopCategory.Inn || s.u3Category == (int)ShopCategory.Libra || i.StartsWith("e") && i.Length == 4).ToList()));
                Dictionary<string, int> maxSizes = shopsDict.Keys.ToDictionary(k => k, k =>
                    shopsDict[k].Count + RandomNum.RandInt(
                        shopsOrig[k].u3Category == (int)ShopCategory.Ark || shopsOrig[k].u3Category == (int)ShopCategory.Items ? 1 : 3, 
                        (shopsOrig[k].u3Category == (int)ShopCategory.Ark || shopsOrig[k].u3Category == (int)ShopCategory.Items ? 6 : 18) - shopsDict[k].Count)
                );

                foreach (string equip in treasureRando.RemainingEquip)
                {
                    string next = shopsDict.Keys.Where(k => shopsDict[k].Count < 32 && isValid(equip, (ShopCategory)shopsOrig[k].u3Category)).ToList().Shuffle().First();
                    shopsDict[next].Add(equip);
                }
                List<string> possibleItems = new List<string>();
                possibleItems.AddRange(treasureRando.RemainingEquip);
                possibleItems.AddRange(GetItems());
                foreach (string shop in shopsDict.Keys)
                {
                    while (shopsDict[shop].Count < maxSizes[shop])
                    {
                        IList<string> possible = possibleItems.Where(s => isValid(s, (ShopCategory)shopsOrig[shop].u3Category) && !shopsDict[shop].Contains(s)).ToList().Shuffle();
                        if (possible.Count() == 0)
                            break;
                        string next = possible.First();
                        shopsDict[shop].Add(next);
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
    }
}
