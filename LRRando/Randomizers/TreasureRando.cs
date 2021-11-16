using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        Dictionary<string, TreasureData> treasureData = new Dictionary<string, TreasureData>();

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

            treasureData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\treasures.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    TreasureData t = new TreasureData(csv.Record);
                    treasureData.Add(t.ID, t);
                }
            }

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

                List<string> keys = treasureData.Keys.ToList().Shuffle().ToList();

                if (!LRFlags.Other.Pilgrims.FlagEnabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("Pilgrim")).ToList();

                if (!LRFlags.Other.Key.FlagEnabled)
                    keys = keys.Where(k => !treasuresOrig[k].s11ItemResourceId_string.StartsWith("key_") || treasureData[k].Traits.Contains("Pilgrim")).ToList();

                if (!LRFlags.Other.EPLearns.FlagEnabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("EP")).ToList();

                Dictionary<string, string> placement = GetImportantPlacement(new Dictionary<string, string>(), keys, keys.Where(t => IsImportant(t)).ToList()).Item2;

                List<string> newKeys = keys.Where(k => !placement.ContainsValue(k)).ToList();
                foreach(string k in keys.Where(k => !placement.ContainsKey(k)))
                {
                    placement.Add(k, newKeys[0]);
                    newKeys.RemoveAt(0);
                }

                foreach (string key in keys)
                {
                    string repKey = placement[key];
                    treasures[key].s11ItemResourceId_string = treasuresOrig[repKey].s11ItemResourceId_string;
                    treasures[key].iItemCount = treasuresOrig[repKey].iItemCount;
                    bool isSame = treasureData[key].Traits.Contains("Same");
                    if (RandomEquip.Contains(treasures[key].s11ItemResourceId_string))
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
                        string next = RemainingEquip.Where(s => !isSame || sameCheck(s, treasures[key].s11ItemResourceId_string)).ToList().Shuffle().First();
                        RemainingEquip.Remove(next);
                        treasures[key].s11ItemResourceId_string = next;
                    }

                    if (equipRando.items.Keys.Contains(treasures[key].s11ItemResourceId_string) && equipRando.IsAbility(equipRando.items[treasures[key].s11ItemResourceId_string]))
                    {
                        string lv = treasures[key].s11ItemResourceId_string.Substring(treasures[key].s11ItemResourceId_string.Length - 3);
                        string next = equipRando.GetAbilities(treasures[key].s11ItemResourceId_string, -1).ToList().Shuffle().First().sScriptId_string;
                        treasures[key].s11ItemResourceId_string = next + lv;
                    }
                }

                RandomNum.ClearRand();
            }
        }

        private Tuple<bool, Dictionary<string, string>> GetImportantPlacement(Dictionary<string, string> soFar, List<string> locations, List<string> important)
        {
            Dictionary<string, int> items = GetItemsAvailable(soFar);
            List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].Requirements.IsValid(items)).ToList().Shuffle().ToList();
            possible = possible.OrderBy(next => locations.Where(t => !soFar.ContainsValue(t) && IsAllowed(next, t)).Count() > 1).ToList();
            if (locations.Where(t => !soFar.ContainsValue(t) && IsAllowed(possible[0], t)).Count() > 0)
            {
                foreach (string next in possible)
                {
                    List<string> replacements = important.Where(t => !soFar.ContainsValue(t) && IsAllowed(next, t)).ToList().Shuffle().ToList();
                    foreach (string rep in replacements)
                    {
                        soFar.Add(next, rep);
                        if (soFar.Count == important.Count)
                            return new Tuple<bool, Dictionary<string, string>>(true, soFar);
                        Tuple<bool, Dictionary<string, string>> result = GetImportantPlacement(soFar, locations, important);
                        if (result.Item1)
                            return result;
                        else
                            soFar.Remove(next);
                    }
                }
            }
            return new Tuple<bool, Dictionary<string, string>>(false, soFar);
        }

        private bool IsImportant(string t)
        {
            if (treasureData[t].Traits.Contains("Same"))
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("key"))
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("ti") || treasuresOrig[t].s11ItemResourceId_string == "at900_00")
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("it"))
                return true;
            return false;
        }

        private bool IsAllowed(string old, string rep)
        {
            if (treasureData[old].Traits.Contains("Same"))
            {
                    return old == rep;
            }
            if (treasureData[old].Traits.Contains("Missable"))
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("CoP") && LRFlags.Other.CoP.FlagEnabled)
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Grindy") && LRFlags.Other.CoP.FlagEnabled)
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Quest"))
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("ti") || treasuresOrig[rep].s11ItemResourceId_string == "at900_00")
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("it"))
                    return false;
            }
            return true;
        }

        private Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> soFar)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            soFar.ForEach(p =>
            {
                string item = treasuresOrig[p.Key].s11ItemResourceId_string;
                int amount = treasuresOrig[p.Key].iItemCount;
                if (dict.ContainsKey(item))
                    dict[item] += amount;
                else
                    dict.Add(item, amount);
            });
            return dict;

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

        public override HTMLPage GetDocumentation()
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = randomizers.Get<TextRando>("Text");
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("", new string[] { "Name", "New Contents" }.ToList(), new int[] { 60, 40 }.ToList(), treasureData.Values.Select(t =>
            {
                string itemID = treasures[t.ID].s11ItemResourceId_string;
                string name;
                if (itemID == "")
                    name = "Gil";
                else if (abilityRando.abilities.Keys.Contains(itemID))
                    name = textRando.mainSysUS[abilityRando.abilities[itemID].sStringResId_string];
                else
                {
                    name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId_string];
                    if (name.Contains("{End}"))
                        name = name.Substring(0, name.IndexOf("{End}"));
                }
                return new string[] { t.Name, $"{name} x {treasures[t.ID].iItemCount}"}.ToList();
            }).ToList()));
            return page;
        }

        public class TreasureData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<string> Traits { get; set; }
            public ItemReq Requirements { get; set; }
            public TreasureData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Traits = row[2].Split("|").ToList();
                Requirements = ItemReq.Parse(row[3]);
            }
        }
    }
}
