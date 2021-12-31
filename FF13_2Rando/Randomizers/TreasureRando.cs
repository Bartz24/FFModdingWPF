using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
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

namespace FF13_2Rando
{
    public class TreasureRando : Randomizer
    {
        DataStoreDB3<DataStoreRTreasurebox> treasuresOrig = new DataStoreDB3<DataStoreRTreasurebox>();
        public DataStoreDB3<DataStoreRTreasurebox> treasures = new DataStoreDB3<DataStoreRTreasurebox>();
        Dictionary<string, TreasureData> treasureData = new Dictionary<string, TreasureData>();

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
            treasuresOrig.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
            treasures.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);

            treasureData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\treasures.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    TreasureData t = new TreasureData(csv.Record);
                    treasureData.Add(t.ID, t);
                }
            }
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
            database[newName].s8NextTreasureBoxResourceId_string = next;
            database[newName].iItemCount = count;
        }

        public override void Randomize(Action<int> progressSetter)
        {
            if (FF13_2Flags.Items.Treasures.FlagEnabled)
            {
                FF13_2Flags.Items.Treasures.SetRand();

                List<string> keys = treasureData.Keys.ToList().Shuffle().ToList();

                Dictionary<string, string> placement = new Dictionary<string, string>();

                List<string> newKeys = keys.Where(k => !placement.ContainsValue(k)).ToList().Shuffle().ToList();
                foreach (string k in keys.Where(k => !placement.ContainsKey(k)))
                {
                    placement.Add(k, newKeys[0]);
                    newKeys.RemoveAt(0);
                }

                foreach (string key in keys)
                {
                    string repKey = placement[key];
                    treasures[key].s11ItemResourceId_string = treasuresOrig[repKey].s11ItemResourceId_string;
                    treasures[key].iItemCount = treasuresOrig[repKey].iItemCount;
                }

                RandomNum.ClearRand();

            }
        }
        /*
        private Tuple<bool, Dictionary<string, string>> GetImportantPlacement(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> hintsCountRem, List<string> locations, List<string> important, int initialCount)
        {
            Dictionary<string, int> items = GetItemsAvailable(soFar);
            List<string> remaining = important.Where(t => !soFar.ContainsValue(t)).ToList().Shuffle().ToList();

            foreach (string rep in remaining)
            {
                List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].Requirements.IsValid(items) && IsAllowed(t, rep)).ToList().Shuffle().ToList();
                if (possible.Count == 0)
                    return new Tuple<bool, Dictionary<string, string>>(false, soFar);
            }

            foreach (string rep in remaining)
            {
                // Only key items and EP abilities are affected by location/depth logic
                if (IsKeyItem(rep) || IsEPAbility(rep))
                {
                    List<string> nextLocations = new List<string>();
                    nextLocations.AddRange(hintsCountRem.Keys.Where(l => !IsHintable(rep) || (hintsCountRem[l] > 0 && IsHintable(rep))).ToList().Shuffle());
                    // If there are no more locations with available spots, just add to any location
                    nextLocations.AddRange(hintsCountRem.Keys.Where(l => !nextLocations.Contains(l)).ToList().Shuffle());

                    foreach (string loc in nextLocations)
                    {
                        List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].Requirements.IsValid(items) && treasureData[t].Location == loc && IsAllowed(t, rep)).ToList();
                        while (possible.Count > 0)
                        {
                            Tuple<string, int> nextPlacement = SelectNext(soFar, depths, items, possible, rep);
                            string next = nextPlacement.Item1;
                            int depth = nextPlacement.Item2;
                            string hint = null;
                            if (LRFlags.Other.HintsMain.FlagEnabled)
                                hint = AddHint(items, next, rep);
                            soFar.Add(next, rep);
                            depths.Add(next, depth);
                            if (IsHintable(rep))
                                hintsCountRem[loc]--;
                            if (soFar.Count == initialCount + important.Count)
                                return new Tuple<bool, Dictionary<string, string>>(true, soFar);
                            Tuple<bool, Dictionary<string, string>> result = GetImportantPlacement(soFar, depths, hintsCountRem, locations, important, initialCount);
                            if (result.Item1)
                                return result;
                            else
                            {
                                possible.Remove(next);
                                soFar.Remove(next);
                                depths.Remove(next);
                                if (IsHintable(rep))
                                    hintsCountRem[loc]++;
                                if (hint != null)
                                    hintsMain.Values.ForEach(l => l.Remove(hint));
                            }
                        }
                    }
                }
                else
                {
                    List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].Requirements.IsValid(items) && IsAllowed(t, rep)).ToList();
                    while (possible.Count > 0)
                    {
                        string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                        soFar.Add(next, rep);
                        if (soFar.Count == initialCount + important.Count)
                            return new Tuple<bool, Dictionary<string, string>>(true, soFar);
                        Tuple<bool, Dictionary<string, string>> result = GetImportantPlacement(soFar, depths, hintsCountRem, locations, important, initialCount);
                        if (result.Item1)
                            return result;
                        else
                        {
                            possible.Remove(next);
                            soFar.Remove(next);
                        }
                    }
                }
            }
            return new Tuple<bool, Dictionary<string, string>>(false, soFar);
        }

        private Tuple<string, int> SelectNext(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (LRFlags.Items.KeyDepth.SelectedValue == LRFlags.Items.KeyDepth.Values[LRFlags.Items.KeyDepth.Values.Count - 1])
            {
                KeyValuePair<string, int> pair = possible.ToDictionary(s => s, s => GetNextDepth(items, soFar, depths, s)).OrderByDescending(p => p.Value).First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                int index = LRFlags.Items.KeyDepth.Values.IndexOf(LRFlags.Items.KeyDepth.SelectedValue);
                float expBase = 1;
                if (index == 0)
                    expBase = 1;
                if (index == 1)
                    expBase = 1.05f;
                if (index == 2)
                    expBase = 1.1f;
                if (index == 3)
                    expBase = 1.25f;
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, soFar, depths, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)Math.Pow(expBase, possDepths[s]));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }

        private int GetNextDepth(Dictionary<string, int> items, Dictionary<string, string> soFar, Dictionary<string, int> depths, string location)
        {
            int val = treasureData[location].Requirements.GetPossibleRequirements().Select(item =>
            {
                return soFar.Keys.Where(t => treasuresOrig[soFar[t]].s11ItemResourceId_string == item).Select(t => depths[t]).DefaultIfEmpty(0).Max();
            }).DefaultIfEmpty(0).Max() + 1 + treasureData[location].Difficulty;
            return RandomNum.RandInt(Math.Max(1, val - 2), val + 2);
        }

        private bool RequiresLogic(string t)
        {
            if (treasureData[t].Traits.Contains("Same"))
                return false;
            if (IsKeyItem(t))
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("libra"))
                return true;
            if (IsEPAbility(t))
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("it"))
                return true;
            if (treasuresOrig[t].s11ItemResourceId_string == "")
                return true;
            if (treasuresOrig[t].iItemCount > 1)
                return true;
            return false;
        }

        private bool IsAllowed(string old, string rep)
        {
            if (treasureData[old].Traits.Contains("Missable"))
            {
                if (IsKeyItem(rep))
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("libra"))
                    return false;
                if (!LRFlags.Items.EPMissable.Enabled && IsEPAbility(rep))
                    return false;
            }
            if (treasureData[old].Traits.Contains("CoP"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.Key.Values.IndexOf(LRFlags.Items.Key.SelectedValue) < LRFlags.Items.Key.Values.IndexOf("CoP"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Grindy"))
            {
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.Key.Values.IndexOf(LRFlags.Items.Key.SelectedValue) < LRFlags.Items.Key.Values.IndexOf("Grindy"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Quest"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("it"))
                    return false;
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.Key.Values.IndexOf(LRFlags.Items.Key.SelectedValue) < LRFlags.Items.Key.Values.IndexOf("Quests"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Trade") || treasureData[old].Traits.Contains("Battle"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (treasuresOrig[rep].iItemCount > 1)
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string == "")
                    return false;
            }

            if (IsKeyItem(rep) && !treasureData[rep].Traits.Contains("Pilgrim") && (!IsKeyItem(old) || treasureData[old].Traits.Contains("Pilgrim")) && LRFlags.Items.Key.Values.IndexOf(LRFlags.Items.Key.SelectedValue) < LRFlags.Items.Key.Values.IndexOf("Treasures"))
                return false;
            if ((!IsKeyItem(rep) || treasureData[rep].Traits.Contains("Pilgrim")) && IsKeyItem(old) && !treasureData[old].Traits.Contains("Pilgrim") && LRFlags.Items.Key.Values.IndexOf(LRFlags.Items.Key.SelectedValue) < LRFlags.Items.Key.Values.IndexOf("Treasures"))
                return false;

            return true;
        }

        private bool IsEPAbility(string t, bool orig = true)
        {
            DataStoreDB3<DataStoreRTreasurebox> db = orig ? treasuresOrig : treasures;
            return db[t].s11ItemResourceId_string.StartsWith("ti") || db[t].s11ItemResourceId_string == "at900_00";
        }

        private bool IsKeyItem(string t, bool orig = true)
        {
            DataStoreDB3<DataStoreRTreasurebox> db = orig ? treasuresOrig : treasures;
            return db[t].s11ItemResourceId_string.StartsWith("key") || db[t].s11ItemResourceId_string == "cos_fa00";
        }

        private Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> soFar)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            soFar.ForEach(p =>
            {
                string item = treasuresOrig[p.Value].s11ItemResourceId_string;
                int amount = treasuresOrig[p.Value].iItemCount;
                if (dict.ContainsKey(item))
                    dict[item] += amount;
                else
                    dict.Add(item, amount);
            });
            return dict;

        }

        private string AddHint(Dictionary<string, int> items, string old, string rep)
        {
            if (IsHintable(rep))
            {
                List<HintData> possible = hintData.Values.Where(h => h.Requirements.IsValid(items) && !h.Requirements.GetPossibleRequirements().Contains(rep)).ToList().Shuffle().OrderBy(h => hintsMain[h.ID].Count).ToList();

                string next = possible.First().ID;
                hintsMain[next].Add(old);
                return next;
            }
            return null;
        }

        private bool IsHintable(string rep)
        {
            if (IsKeyItem(rep) && !treasureData[rep].Traits.Contains("Pilgrim"))
                return true;
            if (treasureData[rep].Traits.Contains("Pilgrim") && LRFlags.Other.HintsPilgrim.FlagEnabled)
                return true;
            if (LRFlags.Other.HintsEP.FlagEnabled && (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("ti") || treasuresOrig[rep].s11ItemResourceId_string == "at900_00"))
                return true;
            return false;
        }

        public List<string> GetRandomizableEquip()
        {
            Func<string, bool> isEquip = s => (s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi")) && s != "cos_fa00";
            List<string> list = new List<string>();
            list.AddRange(treasuresOrig.Values.Where(t => isEquip(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

            return list;
        }*/

        public override void Save()
        {
            treasures.SaveDB3(@"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        }

        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("", (new string[] { "ID (Actual Location TBD)", "New Contents" }).ToList(), (new int[] { 60, 40 }).ToList(), treasureData.Values.Select(t =>
            {
                string itemID = treasures[t.ID].s11ItemResourceId_string;
                string name = GetItemName(itemID);
                return new string[] { t.ID, $"{name} x {treasures[t.ID].iItemCount}" }.ToList();
            }).ToList()));
            return page;
        }

        private string GetItemName(string itemID)
        {
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            TextRando textRando = randomizers.Get<TextRando>("Text");
            string name;
            if (itemID == "")
                name = "Gil";
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
            public TreasureData(string[] row)
            {
                ID = row[0];
            }
        }
    }
}
