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
        public DataStoreDB3<DataStoreRTreasurebox> treasures = new DataStoreDB3<DataStoreRTreasurebox>();
        Dictionary<string, List<string>> treasureData = new Dictionary<string, List<string>>();

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
            treasureData = File.ReadAllLines(@"data\treasures.csv").Select(s => s.Split(",")).ToDictionary(a => a[0], a => a[1].Split("|").ToList());

            AddTreasure("tre_ti000", "ti000_00", 1, "");
            AddTreasure("tre_ti810", "ti810_00", 1, "");
            AddTreasure("tre_ti830", "ti830_00", 1, "");
            AddTreasure("tre_ti840", "ti840_00", 1, "");
        }

        public void AddTreasure(string newName, string item, int count, string next)
        {
            AddTreasure(treasuresOrig, newName, item, count, next);
            AddTreasure(treasures, newName, item, count, next);
        }

        private void AddTreasure(DataStoreDB3<DataStoreRTreasurebox> database, string newName, string item, int count, string next)
        {
            database.InsertCopyAlphabetical(database.Keys[0], newName);
            database[newName].s11ItemResourceId_string = item;
            database[newName].s10NextTreasureBoxResourceId_string = next;
            database[newName].iItemCount = count;
        }

        public override void Randomize(Action<int> progressSetter)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            ShopRando shopRando = randomizers.Get<ShopRando>("Shops");

            if (LRFlags.Other.Treasures.FlagEnabled)
            {
                LRFlags.Other.Treasures.SetRand();

                RandomEquip = GetRandomizableEquip();
                RandomEquip.AddRange(shopRando.GetRandomizableEquip());
                RandomEquip = RandomEquip.Distinct().ToList();
                RemainingEquip = new List<string>(RandomEquip);

                List<string> keys = treasureData.Keys.ToList().Shuffle().OrderBy(k =>
                {
                    int val = 4;
                    if (treasureData[k].Contains("Same"))
                        val = 0;
                    if (!treasureData[k].Contains("Missable"))
                        val = 1;
                    if (treasureData[k].Contains("Quest"))
                        val += 2;
                    return val;
                }).ToList();

                Func<string, bool> isRequired = s => s.StartsWith("key_") || s.StartsWith("ti_");
                List<string> newOrder = keys.Shuffle().Where(k => !treasureData[k].Contains("Same")).OrderByDescending(k =>
                {
                    int val = isRequired(treasuresOrig[k].s11ItemResourceId_string) ? 2 : 0;
                    if (treasuresOrig[k].s11ItemResourceId_string.StartsWith("it") || treasuresOrig[k].s11ItemResourceId_string.StartsWith("ti"))
                        val += RandomNum.RandInt(0, 1);
                    return val;
                }).ToList();

                if (!LRFlags.Other.Pilgrims.FlagEnabled)
                {
                    keys = keys.Where(k => !treasureData[k].Contains("Pilgrim")).ToList();
                    newOrder = newOrder.Where(k => !treasureData[k].Contains("Pilgrim")).ToList();
                }

                if (!LRFlags.Other.EPLearns.FlagEnabled)
                {
                    keys = keys.Where(k => !treasureData[k].Contains("EP")).ToList();
                    newOrder = newOrder.Where(k => !treasureData[k].Contains("EP")).ToList();
                }

                for (int i = 0; i < keys.Count(); i++)
                {
                    bool isSame = treasureData[keys[i]].Contains("Same");
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
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        }
    }
}
