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
    public class TreasureRando : Randomizer
    {
        DataStoreDB3<DataStoreRTreasurebox> treasuresOrig = new DataStoreDB3<DataStoreRTreasurebox>();
        DataStoreDB3<DataStoreRTreasurebox> treasures = new DataStoreDB3<DataStoreRTreasurebox>();
        Dictionary<string, string> treasureData = new Dictionary<string, string>();

        public List<string> RandomEquip = new List<string>();
        public List<string> RemainingEquip = new List<string>();

        public TreasureRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Treasures...";
        }
        public override string GetID()
        {
            return "Treasures";
        }

        public override void Load()
        {
            treasuresOrig.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
            treasures.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
            treasureData = File.ReadAllLines(@"data\treasures.csv").Select(s => s.Split(",")).ToDictionary(a => a[0], a => a[1]);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            ShopRando shopRando = randomizers.Get<ShopRando>("Shops");
            LRFlags.Other.Treasures.SetRand();

            RandomEquip = GetRandomizableEquip();
            RandomEquip.AddRange(shopRando.GetRandomizableEquip());
            RandomEquip = RandomEquip.Distinct().ToList();
            RemainingEquip = new List<string>(RandomEquip);

            List<string> keys = treasureData.Keys.ToList().Shuffle().OrderBy(k =>
            {
                if (treasureData[k] == "Same")
                    return 0;
                if (treasureData[k] != "Missable")
                    return 1;
                return 2;
            }).ToList();

            Func<string, bool> isRequired = s => s.StartsWith("key_") || s.StartsWith("ti_");
            List<string> newOrder = keys.Shuffle().Where(k => treasureData[k] != "Same").OrderByDescending(k => isRequired(treasuresOrig[k].s11ItemResourceId_string)).ToList();

            for (int i = 0; i < keys.Count(); i++)
            {
                bool isSame = treasureData[keys[i]] == "Same";
                if (!isSame)
                {
                    treasures[keys[i]].s11ItemResourceId_string = treasuresOrig[newOrder[0]].s11ItemResourceId_string;
                    treasures[keys[i]].iItemCount = treasuresOrig[newOrder[0]].iItemCount;
                    newOrder.RemoveAt(0);
                }
                if (RandomEquip.Contains(treasures[keys[i]].s11ItemResourceId_string))
                {
                    Func<string, string, bool> sameCheck = (s1, s2) =>
                    {
                        if (s1.StartsWith("cos") && s2.StartsWith("cos"))
                            return true;
                        if (s1.StartsWith("wea") && s2.StartsWith("wea"))
                            return true;
                        if (s1.StartsWith("shi") && s2.StartsWith("shi"))
                            return true;
                        if (s1.StartsWith("e") && s2.StartsWith("e") && s1.Length == 4 && s2.Length == 4)
                            return true;
                        return false;
                    };
                    string next = RemainingEquip.Where(s => !isSame || sameCheck(s, treasures[keys[i]].s11ItemResourceId_string)).ToList().Shuffle().First();
                    RemainingEquip.Remove(next);
                    treasures[keys[i]].s11ItemResourceId_string = next;
                }

                if (equipRando.items.Keys.Contains(treasures[keys[i]].s11ItemResourceId_string) && equipRando.IsAbility(equipRando.items[treasures[keys[i]].s11ItemResourceId_string]))
                {
                    string lv = treasures[keys[i]].s11ItemResourceId_string.Substring(treasures[keys[i]].s11ItemResourceId_string.Length - 3);
                    string next = equipRando.GetAbilities(treasures[keys[i]].s11ItemResourceId_string, -1).ToList().Shuffle().First().sScriptId_string;
                    treasures[keys[i]].s11ItemResourceId_string = next + lv;
                }
            }

            RandomNum.ClearRand();
        }

        public List<string> GetRandomizableEquip()
        {
            Func<string, bool> isEquip = s => s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi");
            List<string> list = new List<string>();
            list.AddRange(treasuresOrig.Values.Where(t => isEquip(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

            return list;
        }

        public override void Save()
        {
            treasures.SaveDB3(@"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
        }
    }
}
