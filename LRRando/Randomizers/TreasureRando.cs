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
            AddTreasure("tre_cos_fa00", "cos_fa00", 1, "");
            AddTreasure("tre_cos_la00", "cos_la00", 1, "");
            AddTreasure("tre_wea_da00", "wea_da00", 1, "");
            AddTreasure("tre_key_b_20", "key_b_20", 1, "");
            AddTreasure("tre_key_yasai_t", "key_w_yasai_t", 1, "");

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

            if (LRFlags.Items.Treasures.FlagEnabled)
            {
                LRFlags.Items.Treasures.SetRand();

                RandomEquip = GetRandomizableEquip();
                RandomEquip.AddRange(shopRando.GetRandomizableEquip());
                RandomEquip = RandomEquip.Distinct().ToList();
                RemainingEquip = new List<string>(RandomEquip);

                List<string> keys = treasureData.Keys.ToList().Shuffle().ToList();

                if (!LRFlags.Items.Pilgrims.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("Pilgrim")).ToList();

                if (LRFlags.Items.Key.SelectedValue == "None")
                    keys = keys.Where(k => !IsKeyItem(k) || treasureData[k].Traits.Contains("Pilgrim")).ToList();

                if (!LRFlags.Items.EPLearns.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("EP")).ToList();

                List<string> sameKeys = keys.Where(k => treasureData[k].Traits.Contains("Same")).ToList();
                keys = keys.Where(k => !treasureData[k].Traits.Contains("Same")).ToList();


                List<string> locations = treasureData.Values.Select(t => t.Location).Distinct().ToList().Shuffle().ToList();
                hintsNotesLocations.Keys.ForEach(h =>
                {
                    hintsNotesLocations[h] = locations[0];
                    locations.RemoveAt(0);
                });

                hintsNotesLocations.Values.ForEach(l =>
                {
                    hintsNotesCount.Add(l, 0);
                });

                for (int i = 0; i < keys.Where(t => IsHintable(t)).Count(); i++)
                {
                    List<string> randomZeros = new List<string>();
                    for (int j = 0; j < 10; j++)
                    {
                        if (RandomNum.RandInt(0, 99) < 10)
                            randomZeros.Add(hintsNotesCount.Keys.Where(l => !randomZeros.Contains(l)).ToList().Shuffle().First());
                    }
                    Func<string, long> weight = loc => {
                        if (randomZeros.Contains(loc))
                            return 0;

                        int max = treasureData.Keys.Where(t => treasureData[t].Location == loc).Count();
                        return (long)(100 * Math.Pow(1 - (hintsNotesCount[loc] / (float)max), 2));
                    };
                    string next = RandomNum.SelectRandomWeighted(hintsNotesCount.Keys.ToList(), weight);
                    hintsNotesCount[next]++;
                }

                Dictionary<string, int> depths = new Dictionary<string, int>();
                Dictionary<string, int> hintsRem = hintsNotesCount.ToDictionary(p => p.Key, p => p.Value);
                Dictionary<string, string> placement = GetImportantPlacement(new Dictionary<string, string>(), depths, hintsRem, keys, keys.Where(t => IsImportant(t, true)).ToList(), 0).Item2;
                List<string> remainingKeys = keys.Where(k => !placement.ContainsKey(k)).ToList();
                List<string> remainingPlacements = keys.Where(k => !placement.ContainsValue(k)).ToList();
                placement = GetImportantPlacement(placement, depths, hintsRem, remainingKeys, remainingPlacements.Where(t => IsImportant(t, false)).ToList(), placement.Count).Item2;


                List<string> newKeys = keys.Where(k => !placement.ContainsValue(k)).ToList().Shuffle().ToList();
                foreach (string k in keys.Where(k => !placement.ContainsKey(k)))
                {
                    placement.Add(k, newKeys[0]);
                    newKeys.RemoveAt(0);
                }
                sameKeys.ForEach(k =>
                {
                    placement.Add(k, k);
                    keys.Add(k);
                });

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
                    // Update hints again to reflect actual numbers
                    hintsNotesLocations.Values.ForEach(l =>
                    {
                        int locationCount = treasureData.Keys.Where(t => placement.ContainsKey(t) && treasureData[t].Location == l && IsHintable(placement[t])).Count();
                        hintsNotesCount[l] = locationCount;
                    });
                }
            }
        }

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
                List<string> nextLocations = hintsCountRem.Keys.Where(l => !IsHintable(rep) || (hintsCountRem[l] > 0 && IsHintable(rep))).ToList().Shuffle().ToList();
                // If there are no more locations with available spots for key items, just add to any location
                nextLocations.AddRange(hintsCountRem.Keys.Where(l => !nextLocations.Contains(l)).ToList().Shuffle());
                foreach (string loc in nextLocations)
                {
                    List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].Requirements.IsValid(items) && treasureData[t].Location == loc && IsAllowed(t, rep)).ToList();
                    while (possible.Count > 0)
                    {
                        Tuple<string, int> nextPlacement = SelectNext(soFar, depths, items, possible);
                        string next = nextPlacement.Item1;
                        int depth = nextPlacement.Item2;
                        string hint = null;
                        if (LRFlags.Other.HintsMain.FlagEnabled)
                            hint = AddHint(soFar, next, rep);
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
            return new Tuple<bool, Dictionary<string, string>>(false, soFar);
        }

        private Tuple<string, int> SelectNext(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> items, List<string> possible)
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

        private bool IsImportant(string t, bool keysOnly)
        {
            if (treasureData[t].Traits.Contains("Same"))
                return false;
            if (IsKeyItem(t))
                return (keysOnly && !treasureData[t].Traits.Contains("Pilgrim")) || (!keysOnly && treasureData[t].Traits.Contains("Pilgrim"));
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("libra"))
                return !keysOnly;
            if (IsEPAbility(t))
                return !keysOnly;
            if (treasuresOrig[t].s11ItemResourceId_string.StartsWith("it"))
                return !keysOnly;
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

        private string AddHint(Dictionary<string, string> soFar, string old, string rep)
        {
            if (IsHintable(rep))
            {
                Dictionary<string, int> items = GetItemsAvailable(soFar);
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

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
            {
                hintsMain.Keys.ForEach(h =>
                {
                    textRando.mainSysUS["$" + h] = string.Join("{Text NewLine}{Text NewLine}", hintsMain[h].Select(t => treasures[t]).Select(t => GetHintText(t)));
                });
            }

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsNotes.FlagEnabled)
            {
                hintsNotesLocations.Keys.ForEach(i =>
                {
                    textRando.mainSysUS[equipRando.items[i].sHelpStringId_string] = $"{hintsNotesLocations[i]} has {hintsNotesCount[hintsNotesLocations[i]]} important checks.";
                });
            }
        }

        private string GetHintText(DataStoreRTreasurebox t)
        {
            int index = LRFlags.Other.HintsSpecific.Values.IndexOf(LRFlags.Other.HintsSpecific.SelectedValue);
            if (index == LRFlags.Other.HintsSpecific.Values.Count - 1)
            {
                LRFlags.Other.HintsMain.SetRand();
                index = RandomNum.RandInt(0, LRFlags.Other.HintsSpecific.Values.Count - 2);
                RandomNum.ClearRand();
            }
            switch (index)
            {
                case 0:
                default:
                    {
                        return $"{treasureData[t.name].Name} has {GetItemName(t.s11ItemResourceId_string)}";
                    }
                case 1:
                    {
                        string type = IsKeyItem(t.name, false) ? "a Key Item" : (IsEPAbility(t.name, false) ? "an EP Ability" : "Other");
                        return $"{treasureData[t.name].Name} has {type}";
                    }
                case 2:
                    {
                        return $"{treasureData[t.name].Location} has {GetItemName(t.s11ItemResourceId_string)}";
                    }
                case 3:
                    {
                        string type = IsKeyItem(t.name, false) ? "a Key Item" : (IsEPAbility(t.name, false) ? "an EP Ability" : "Other");
                        return $"{treasureData[t.name].Location} has {type}";
                    }
                case 4:
                    {
                        return $"{treasureData[t.name].Name} has ?????";
                    }
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
            public int Difficulty { get; set; }
            public TreasureData(string[] row)
            {
                ID = row[0];
                Location = row[1];
                Name = row[2];
                Traits = row[3].Split("|").ToList();
                Requirements = ItemReq.Parse(row[4]);
                Difficulty = int.Parse(row[5]);
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
