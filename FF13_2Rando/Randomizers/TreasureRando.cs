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
        public DataStoreDB3<DataStoreRFragment> fragments = new DataStoreDB3<DataStoreRFragment>();
        Dictionary<string, TreasureData> treasureData = new Dictionary<string, TreasureData>();
        Dictionary<string, HintData> hintData = new Dictionary<string, HintData>();

        Dictionary<string, List<string>> hintsMain = new Dictionary<string, List<string>>();
        List<string> hintsNotesLocations = new List<string>();
        Dictionary<string, int> hintsNotesCount = new Dictionary<string, int>();
        Dictionary<string, int> hintsNotesUniqueCount = new Dictionary<string, int>();
        Dictionary<string, int> hintsNotesSharedCount = new Dictionary<string, int>();

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
            fragments.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_fragment.wdb", false);

            treasureData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\treasures.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1)
                    {
                        TreasureData t = new TreasureData(csv.Record);
                        treasureData.Add(t.ID, t);
                    }
                }
            }

            hintData.Clear();
            using (CsvParser csv = new CsvParser(new StreamReader(@"data\hints.csv"), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1)
                    {
                        HintData t = new HintData(csv.Record);
                        hintData.Add(t.ID, t);
                    }
                }
            }

            AddTreasure("ran_init_cp", "", 0, "");

            hintsNotesLocations.Clear();
            hintsNotesCount.Clear();
            hintsNotesLocations = hintData.Values.SelectMany(h => h.Areas).ToList();
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


                List<string> locations = treasureData.Values.SelectMany(t => t.Locations).Distinct().ToList().Shuffle().ToList();

                hintsNotesLocations.ForEach(l =>
                {
                    hintsNotesCount.Add(l, 0);
                });

                List<string> randomZeros = new List<string>();
                for (int j = 0; j < 10; j++)
                {
                    if (RandomNum.RandInt(0, 99) < 10)
                        randomZeros.Add(hintsNotesCount.Keys.Where(l => !randomZeros.Contains(l)).ToList().Shuffle().First());
                }

                float copMult = RandomNum.RandInt(12, 100) / 100f;

                for (int i = 0; i < keys.Where(t => IsHintable(t)).Count(); i++)
                {
                    Func<string, long> weight = loc => {
                        if (randomZeros.Contains(loc))
                            return 0;

                        int max = treasureData.Keys.Where(t => treasureData[t].Locations.Contains(loc)).Count();
                        long val = (long)(100 * Math.Pow(1 - (hintsNotesCount[loc] / (float)max), 4));

                        return val;
                    };
                    string next = RandomNum.SelectRandomWeighted(hintsNotesCount.Keys.ToList(), weight);
                    hintsNotesCount[next]++;
                }

                Dictionary<string, int> depths = new Dictionary<string, int>();
                Dictionary<string, int> hintsRem = hintsNotesCount.ToDictionary(p => p.Key, p => p.Value);
                Dictionary<string, string> placement = GetImportantPlacement(new Dictionary<string, string>(), depths, hintsRem, keys, keys.Where(t => RequiresLogic(t)).ToList(), 0).Item2;

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

                // Update hints again to reflect actual numbers
                hintsNotesLocations.ForEach(l =>
                {
                    int uniqueCount = treasureData.Keys.Where(t => placement.ContainsKey(t) && treasureData[t].Locations.Count == 1 && treasureData[t].Locations[0] == l && IsHintable(placement[t])).Count();
                    hintsNotesUniqueCount.Add(l, uniqueCount);

                    int sharedCount = treasureData.Keys.Where(t => placement.ContainsKey(t) && treasureData[t].Locations.Count > 1 && treasureData[t].Locations.Contains(l) && IsHintable(placement[t])).Count();
                    hintsNotesSharedCount.Add(l, sharedCount);
                });

                RandomNum.ClearRand();

            }

            if (FF13_2Flags.Other.InitCP.FlagEnabled)
            {
                treasures["ran_init_cp"].iItemCount = FF13_2Flags.Other.InitCPAmount.Value;
            }
        }
        private Tuple<bool, Dictionary<string, string>> GetImportantPlacement(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> hintsCountRem, List<string> locations, List<string> important, int initialCount)
        {
            HistoriaCruxRando cruxRando = randomizers.Get<HistoriaCruxRando>("Historia Crux");
            Dictionary<string, int> items = GetItemsAvailable(soFar);
            List<string> remaining = important.Where(t => !soFar.ContainsValue(t)).ToList().Shuffle().ToList();

            foreach (string rep in remaining)
            {
                List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].IsValid(items, this) && IsAllowed(t, rep)).ToList().Shuffle().ToList();
                if (possible.Count == 0)
                    return new Tuple<bool, Dictionary<string, string>>(false, soFar);
            }

            foreach (string rep in remaining)
            {
                // Only key items are affected by location/depth logic
                if (IsKeyItem(rep))
                {
                    List<string> allowedLocations = new List<string>();
                    allowedLocations.AddRange(hintsCountRem.Keys.Where(l => !IsHintable(rep) || (hintsCountRem[l] > 0 && IsHintable(rep))).ToList().Shuffle());
                    // If there are no more locations with available spots, just add to any location
                    allowedLocations.AddRange(hintsCountRem.Keys.Where(l => !allowedLocations.Contains(l)).ToList().Shuffle());

                    // Remove inaccessible locations
                    allowedLocations = allowedLocations.Intersect(GetLocationsAvailable(items)).ToList();



                    foreach (string loc in allowedLocations)
                    {
                        List<string> possible = locations.Where(t => 
                                                                !soFar.ContainsKey(t) && 
                                                                treasureData[t].IsValid(items, this) && 
                                                                treasureData[t].Locations.Contains(loc) &&
                                                                treasureData[t].RequiredLocations.Intersect(allowedLocations).Count() == treasureData[t].RequiredLocations.Count &&
                                                                IsAllowed(t, rep) &&
                                                                cruxRando.GetMogLevel(allowedLocations) >= treasureData[t].MogLevel).ToList();
                        while (possible.Count > 0)
                        {
                            Tuple<string, int> nextPlacement = SelectNext(soFar, depths, items, possible, rep);
                            string next = nextPlacement.Item1;
                            int depth = nextPlacement.Item2;
                            string hint = null;
                            //if (LRFlags.Other.HintsMain.FlagEnabled)
                                //hint = AddHint(soFar, depths, items, next, rep, depth);
                            soFar.Add(next, rep);
                            depths.Add(next, depth);
                            if (IsHintable(rep))
                                treasureData[next].Locations.ForEach(l => hintsCountRem[l]--);
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
                                    treasureData[next].Locations.ForEach(l => hintsCountRem[l]++);
                                //if (hint != null)
                                //hintsMain.Values.ForEach(l => l.Remove(hint));
                            }
                        }
                    }
                }
                else
                {
                    List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].IsValid(items, this) && IsAllowed(t, rep)).ToList();
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
            /*if (LRFlags.Items.KeyDepth.SelectedValue == LRFlags.Items.KeyDepth.Values[LRFlags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, soFar, depths, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else*/
            {
                //int index = LRFlags.Items.KeyDepth.Values.IndexOf(LRFlags.Items.KeyDepth.SelectedValue);
                float expBase = 1;
                /*if (index == 0)
                    expBase = 1;
                if (index == 1)
                    expBase = 1.05f;
                if (index == 2)
                    expBase = 1.1f;
                if (index == 3)
                    expBase = 1.25f;*/
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, soFar, depths, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)Math.Pow(expBase, possDepths[s]));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }

        private int GetNextDepth(Dictionary<string, int> items, Dictionary<string, string> soFar, Dictionary<string, int> depths, string location)
        {
            /*
            int reqsMax = GetReqsMaxDepth(soFar, depths, treasureData[location].Requirements);

            int minItems = 8;
            int keyItemsFound = soFar.Where(p => IsKeyItem(p.Value)).Count();
            // Early day/easier checks have higher "depths" as minItems is low to start chains
            float diffModifier = Math.Min(minItems, keyItemsFound) / (float)minItems;
            int maxDifficulty = treasureData.Values.Select(t => t.Difficulty).Max();
            int diffValue = (int)(diffModifier * treasureData[location].Difficulty + (1 - diffModifier) * (maxDifficulty - treasureData[location].Difficulty));

            int val = reqsMax + 1 + diffValue;
            return RandomNum.RandInt(Math.Max(reqsMax + 1, val - 2), val + 2);*/
            return 1;
        }

        private int GetReqsMaxDepth(Dictionary<string, string> soFar, Dictionary<string, int> depths, ItemReq req)
        {
            return req.GetPossibleRequirements().Select(item =>
            {
                return soFar.Keys.Where(t => treasuresOrig[soFar[t]].s11ItemResourceId_string == item).Select(t => depths[t]).DefaultIfEmpty(0).Max();
            }).DefaultIfEmpty(0).Max();
        }

        private bool RequiresLogic(string t)
        {
            if (IsKeyItem(t))
                return true;
            return false;
        }

        private bool IsAllowed(string old, string rep)
        {
            return true;
        }
        private bool IsKeyItem(string t, bool orig = true)
        {
            DataStoreDB3<DataStoreRTreasurebox> db = orig ? treasuresOrig : treasures;
            return db[t].s11ItemResourceId_string.StartsWith("key") || db[t].s11ItemResourceId_string.StartsWith("opt") || db[t].s11ItemResourceId_string.StartsWith("frg");
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

        private bool IsHintable(string rep)
        {
            if (IsKeyItem(rep))
                return true;
            return false;
        }

        private List<string> GetLocationsAvailable(Dictionary<string, int> items)
        {
            HistoriaCruxRando cruxRando = randomizers.Get<HistoriaCruxRando>("Historia Crux");
            List<string> list = new List<string>();
            list.Add("start");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
                list.Add("h_hm_AD0003");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
                list.Add("h_bj_AD0005");
            int oldCount = -1;

            while (list.Count != oldCount)
            {
                oldCount = list.Count;
                int wildsNeeded = cruxRando.GetWildsNeeded(list);
                int gravitonsHeld = items.Keys.Where(i => i.StartsWith("frg_cmn_gvtn")).Select(i => items[i]).Sum();

                cruxRando.gateData.Values.Where(g =>
                {
                    if (!list.Contains(g.Location))
                        return false;
                    string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                    if (!cruxRando.placement.ContainsKey(nextLocation))
                        return false;
                    if (g.Traits.Contains("Wild") && (items.ContainsKey("opt_silver") ? items["opt_silver"] : 0) < wildsNeeded)
                        return false;
                    if (g.Traits.Contains("Graviton") && gravitonsHeld < 5)
                        return false;
                    if (!g.ItemRequirements.IsValid(items))
                        return false;
                    return true;
                }).ForEach(g =>
                {
                    string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                    if (!list.Contains(cruxRando.placement[nextLocation]))
                    {
                        list.Add(cruxRando.placement[nextLocation]);
                    }
                });
            }

            // Unlock Void after Ch 2
            if (list.Contains("h_gh_AD0010") && list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000"))
                list.Add("h_sp_NA0001");

            int gravitons = items.Keys.Where(i => i.StartsWith("frg_cmn_gvtn")).Select(i => items[i]).Sum();
            // Unlock Dying World/Bodhum 700 after Academia 4XX and Graviton
            if (list.Contains("h_aa_AD0400") && gravitons >= 5)
            {
                list.Add("h_dd_AD0700");
                list.Add("h_hm_AD0700");
                list.Add("h_zz_NA0950");
            }

            // Unlock Serendipity after Yaschas 1X and Sunleth 300
            if (list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && list.Contains("h_gh_AD0010") && list.Contains("h_cl_NA0000"))
                list.Add("h_cs_NA0000");

            return list;
        }

        private void SaveHints()
        {
            HistoriaCruxRando cruxRando = randomizers.Get<HistoriaCruxRando>("Historia Crux");
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            TextRando textRando = randomizers.Get<TextRando>("Text");

            if (FF13_2Flags.Items.Treasures.FlagEnabled)
            {
                hintData.Values.ForEach(h =>
                {
                    textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId_string] = "";
                    h.Areas.ForEach(a =>
                    {
                        if (hintsNotesSharedCount[a] > 0)
                        {
                            textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId_string] += $"{cruxRando.areaData[a].Name} has {hintsNotesUniqueCount[a]} unique important checks and {hintsNotesSharedCount[a]} shared with other time periods.";
                        }
                        else
                        {
                            textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId_string] += $"{cruxRando.areaData[a].Name} has {hintsNotesUniqueCount[a]} unique important checks.";
                        }
                    });
                });
            }
        }

        public override void Save()
        {
            SaveHints();
            treasures.SaveDB3(@"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        }

        public override HTMLPage GetDocumentation()
        {
            HistoriaCruxRando cruxRando = randomizers.Get<HistoriaCruxRando>("Historia Crux");
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("", (new string[] { "Location", "New Contents" }).ToList(), (new int[] { 60, 40 }).ToList(), treasureData.Values.Select(t =>
            {
                string itemID = treasures[t.ID].s11ItemResourceId_string;
                string name = GetItemName(itemID);
                string location = $"{string.Join("/", treasureData[t.ID].Locations.Select(s => cruxRando.areaData[s].Name))} - {treasureData[t.ID].Name}";
                return new string[] { location, $"{name} x {treasures[t.ID].iItemCount}" }.ToList();
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
            else if(itemID.StartsWith("frg"))
            {
                name = textRando.mainSysUS[fragments[itemID].sNameStringId_string];
                if (name.Contains("{End}"))
                    name = name.Substring(0, name.IndexOf("{End}"));
            }
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
            public string Name { get; set; }
            public List<string> Locations { get; set; }
            public int MogLevel { get; set; }
            public List<string> RequiredLocations { get; set; }
            public ItemReq Requirements { get; set; }
            public TreasureData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Locations = row[2].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                MogLevel = int.Parse(row[3]);
                RequiredLocations = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = ItemReq.Parse(row[5]);
            }

            public bool IsValid(Dictionary<string, int> items, TreasureRando treasureRando)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }
        }

        public class HintData
        {
            public string ID { get; set; }
            public List<string> Areas { get; set; }
            public HintData(string[] row)
            {
                ID = row[0];
                Areas = row[1].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
        }
    }
}
