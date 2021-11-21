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
        Dictionary<string, HintData> hintData = new Dictionary<string, HintData>();
        Dictionary<string, List<string>> hintsMain = new Dictionary<string, List<string>>();
        Dictionary<string, string> hintsNotesLocations = new Dictionary<string, string>();
        Dictionary<string, int> hintsNotesCount = new Dictionary<string, int>();

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

            hintsMain.Clear();
            hintData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\hints.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    HintData h = new HintData(csv.Record);
                    hintData.Add(h.ID, h);
                    hintsMain.Add(h.ID, new List<string>());
                }
            }

            AddTreasure("tre_ti000", "ti000_00", 1, "");
            AddTreasure("tre_ti810", "ti810_00", 1, "");
            AddTreasure("tre_ti830", "ti830_00", 1, "");
            AddTreasure("tre_ti840", "ti840_00", 1, "");
            AddTreasure("tre_y_kagi1", "key_y_kagi1", 1, "");
            AddTreasure("tre_kyu_pass", "key_kyu_pass", 1, "");
            AddTreasure("tre_d_seki_1", "key_d_sekiban", 1, "");
            AddTreasure("tre_d_seki_2", "key_d_sekiban", 1, "");
            AddTreasure("tre_d_seki_3", "key_d_sekiban", 1, "");

            AddTreasure("ran_bhuni_p", "false", 1, "");

            hintsNotesLocations.Clear();
            hintsNotesCount.Clear();
            treasureData.Keys.Where(k => treasuresOrig[k].s11ItemResourceId_string.StartsWith("libra")).ForEach(k => hintsNotesLocations.Add(treasuresOrig[k].s11ItemResourceId_string, null));
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

                List<string> newKeys = keys.Where(k => !placement.ContainsValue(k)).ToList().Shuffle().ToList();
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
                        string next = equipRando.GetAbilities(-1).Shuffle().First().sScriptId_string;
                        treasures[key].s11ItemResourceId_string = next + lv;
                    }
                }

                RandomNum.ClearRand();

                if (LRFlags.Other.HintsNotes.FlagEnabled)
                {
                    LRFlags.Other.HintsNotes.SetRand();

                    List<string> locations = treasureData.Values.Select(t => t.Location).Distinct().ToList().Shuffle().ToList();
                    hintsNotesLocations.Keys.ForEach(h =>
                    {
                        hintsNotesLocations[h] = locations[0];
                        locations.RemoveAt(0);
                    });
                    hintsNotesLocations.Values.ForEach(l =>
                    {
                        int locationCount = treasureData.Keys.Where(t => placement.ContainsKey(t) && treasureData[t].Location == l && IsHintable(t, placement[t])).Count();
                        hintsNotesCount.Add(l, locationCount);
                    });

                    RandomNum.ClearRand();
                }
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
                        string hint = null;
                        if (LRFlags.Other.HintsMain.FlagEnabled)
                            hint = AddHint(soFar, next, rep);
                        if (soFar.Count == important.Count)
                            return new Tuple<bool, Dictionary<string, string>>(true, soFar);
                        Tuple<bool, Dictionary<string, string>> result = GetImportantPlacement(soFar, locations, important);
                        if (result.Item1)
                            return result;
                        else
                        {
                            soFar.Remove(next);
                            if (hint != null)
                                hintsMain.Values.ForEach(l => l.Remove(hint));
                        }
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
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("libra"))
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
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("libra"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("CoP"))
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("ti") || treasuresOrig[rep].s11ItemResourceId_string == "at900_00")
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key") && !LRFlags.Other.CoP.FlagEnabled)
                    return false;
            }
            if (treasureData[old].Traits.Contains("Grindy"))
            {
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key") && !LRFlags.Other.CoP.FlagEnabled)
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

        private string AddHint(Dictionary<string, string> soFar, string old, string rep)
        {
            if (IsHintable(old, rep))
            {
                Dictionary<string, int> items = GetItemsAvailable(soFar);
                List<HintData> possible = hintData.Values.Where(h => h.Requirements.IsValid(items) && !h.Requirements.GetPossibleRequirements().Contains(rep)).ToList().Shuffle().OrderBy(h => hintsMain[h.ID].Count).ToList();

                string next = possible.First().ID;
                hintsMain[next].Add(old);
                return next;
            }
            return null;
        }

        private bool IsHintable(string old, string rep)
        {
            if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("key_"))
                return true;
            if (LRFlags.Other.HintsEP.FlagEnabled && (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("ti") || treasuresOrig[rep].s11ItemResourceId_string == "at900_00"))
                return true;
            return false;
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

            SaveHints();
        }

        private void SaveHints()
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = randomizers.Get<TextRando>("Text");

            if (LRFlags.Other.HintsMain.FlagEnabled)
            {
                hintsMain.Keys.ForEach(h =>
                {
                    textRando.mainSysUS["$" + h] = string.Join("{Text NewLine}{Text NewLine}", hintsMain[h].Select(t => treasures[t]).Select(t => $"{treasureData[t.name].Name} has {GetItemName(t.s11ItemResourceId_string)} x {t.iItemCount}"));
                });
            }

            if (LRFlags.Other.HintsNotes.FlagEnabled)
            {
                hintsNotesLocations.Keys.ForEach(i =>
                {
                    textRando.mainSysUS[equipRando.items[i].sHelpStringId_string] = $"{hintsNotesLocations[i]} has {hintsNotesCount[hintsNotesLocations[i]]} important checks.";
                });
            }
        }

        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("", (new string[] { "Name", "New Contents", "Location (for Hints)" }).ToList(), (new int[] { 40, 30, 30 }).ToList(), treasureData.Values.Select(t =>
            {
                string itemID = treasures[t.ID].s11ItemResourceId_string;
                string name = GetItemName(itemID);
                return new string[] { t.Name, $"{name} x {treasures[t.ID].iItemCount}", t.Location }.ToList();
            }).ToList()));
            return page;
        }

        private string GetItemName(string itemID)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = randomizers.Get<TextRando>("Text");
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

            return name;
        }

        public class TreasureData
        {
            public string ID { get; set; }
            public string Location { get; set; }
            public string Name { get; set; }
            public List<string> Traits { get; set; }
            public ItemReq Requirements { get; set; }
            public TreasureData(string[] row)
            {
                ID = row[0];
                Location = row[1];
                Name = row[2];
                Traits = row[3].Split("|").ToList();
                Requirements = ItemReq.Parse(row[4]);
            }
        }

        public class HintData
        {
            public string ID { get; set; }
            public ItemReq Requirements { get; set; }
            public HintData(string[] row)
            {
                ID = row[0];
                Requirements = ItemReq.Parse(row[1]);
            }
        }
    }
}
