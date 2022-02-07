using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
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

        Dictionary<string, string> finalPlacement = new Dictionary<string, string>();

        public Dictionary<string, string> BattleDrops = new Dictionary<string, string>();
        public Dictionary<string, string> OrigBattleDrops = new Dictionary<string, string>();

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
            AddTreasure("tre_key_l_kimo", "key_kimochi", 1, "");
            AddTreasure("tre_key_l_kagi", "key_l_kagi", 1, "");
            AddTreasure("tre_key_l_kish", "key_l_kishin", 1, "");

            AddTreasure("trd_niku", "key_niku", 1, "");
            AddTreasure("trd_ninjin", "key_ninjin", 1, "");
            AddTreasure("trd_ticket", "key_y_ticket", 1, "");
            AddTreasure("trd_soulcd", "key_soulcd", 1, "");

            AddTreasure("tre_drp_hunnu", "key_s_hunnu", 1, "");
            AddTreasure("tre_drp_keisan", "key_d_keisan", 1, "");
            AddTreasure("tre_drp_kaban", "key_y_kaban", 1, "");
            AddTreasure("tre_drp_bashira", "key_y_bashira", 1, "");

            AddTreasure("ran_rando_id", "false", 1, "");
            AddTreasure("ran_bhuni_p", "false", 1, "");
            AddTreasure("ran_multi", "rando_multi_item", 9999, "");

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

                if (!LRFlags.Items.KeyMain.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("MainKey")).ToList();
                if (!LRFlags.Items.KeySide.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("SideKey")).ToList();
                if (!LRFlags.Items.KeyCoP.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("CoPKey")).ToList();

                if (!LRFlags.Items.EPLearns.Enabled)
                    keys = keys.Where(k => !treasureData[k].Traits.Contains("EP")).ToList();
                if (!LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled)
                    keys = keys.Where(t => treasuresOrig[t].s11ItemResourceId_string != "ti830_00").ToList();
                if (!LRFlags.StatsAbilities.EPAbilitiesChrono.Enabled)
                    keys = keys.Where(t => treasuresOrig[t].s11ItemResourceId_string != "ti840_00").ToList();
                if (!LRFlags.StatsAbilities.EPAbilitiesTp.Enabled)
                    keys = keys.Where(t => treasuresOrig[t].s11ItemResourceId_string != "ti810_00").ToList();

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

                        int max = treasureData.Keys.Where(t => treasureData[t].Location == loc && !treasureData[t].Traits.Contains("Missable")).Count();
                        long val = (long)(100 * Math.Pow(1 - (hintsNotesCount[loc] / (float)max), 4));

                        if (loc.Contains("CoP"))
                            val = (long)(val * copMult);
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
                sameKeys.ForEach(k =>
                {
                    placement.Add(k, k);
                    keys.Add(k);
                });

                finalPlacement = placement;

                // Same treasures take priority
                keys = keys.OrderBy(k => !treasureData[k].Traits.Contains("Same")).ToList();

                foreach (string key in keys)
                {
                    string repKey = placement[key];
                    treasures[key].s11ItemResourceId_string = treasuresOrig[repKey].s11ItemResourceId_string;
                    treasures[key].iItemCount = treasuresOrig[repKey].iItemCount;
                    bool isSame = treasureData[key].Traits.Contains("Same");
                    if (RandomEquip.Contains(treasures[key].s11ItemResourceId_string))
                    {
                        Func<string, string, bool> sameCheck = (rep, orig) =>
                        {
                            if (rep.StartsWith("cos") && orig.StartsWith("cos") && rep != "cos_ba08" && rep != "cos_ca08")
                                return true;
                            if (rep.StartsWith("wea") && orig.StartsWith("wea"))
                                return true;
                            if (rep.StartsWith("shi") && orig.StartsWith("shi"))
                                return true;
                            if (rep.StartsWith("e") && orig.StartsWith("e") && rep.Length == 4 && orig.Length == 4)
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

                if (LRFlags.Items.IDCardBuy.Enabled)
                    treasures["ran_rando_id"].s11ItemResourceId_string = "true";
            }
        }

        private Tuple<bool, Dictionary<string, string>> GetImportantPlacement(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> hintsCountRem, List<string> locations, List<string> important, int initialCount)
        {
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
                // Only key items and EP abilities are affected by location/depth logic
                if (IsKeyItem(rep) || IsEPAbility(rep))
                {
                    List<string> nextLocations = new List<string>();
                    nextLocations.AddRange(hintsCountRem.Keys.Where(l => !IsHintable(rep) || (hintsCountRem[l] > 0 && IsHintable(rep))).ToList().Shuffle());
                    // If there are no more locations with available spots, just add to any location
                    nextLocations.AddRange(hintsCountRem.Keys.Where(l => !nextLocations.Contains(l)).ToList().Shuffle());

                    foreach (string loc in nextLocations)
                    {
                        List<string> possible = locations.Where(t => !soFar.ContainsKey(t) && treasureData[t].IsValid(items, this) && treasureData[t].Location == loc && IsAllowed(t, rep)).ToList();
                        while (possible.Count > 0)
                        {
                            Tuple<string, int> nextPlacement = SelectNext(soFar, depths, items, possible, rep);
                            string next = nextPlacement.Item1;
                            int depth = nextPlacement.Item2;
                            string hint = null;
                            if (LRFlags.Other.HintsMain.FlagEnabled)
                                hint = AddHint(soFar, depths, items, next, rep, depth);
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
            if (LRFlags.Items.KeyDepth.SelectedValue == LRFlags.Items.KeyDepth.Values[LRFlags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, soFar, depths, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
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
            int reqsMax = GetReqsMaxDepth(soFar, depths, treasureData[location].Requirements);

            int minItems = 8;
            int keyItemsFound = soFar.Where(p => IsKeyItem(p.Value)).Count();
            // Early day/easier checks have higher "depths" as minItems is low to start chains
            float diffModifier = Math.Min(minItems, keyItemsFound) / (float)minItems;
            int maxDifficulty = treasureData.Values.Select(t => t.Difficulty).Max();
            int diffValue = (int)(diffModifier * treasureData[location].Difficulty + (1 - diffModifier) * (maxDifficulty - treasureData[location].Difficulty));

            int val = reqsMax + 1 + diffValue;
            return RandomNum.RandInt(Math.Max(reqsMax + 1, val - 2), val + 2);
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
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.KeyPlacement.IndexOfCurrentValue() < LRFlags.Items.KeyPlacement.IndexOf("CoP"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Grindy"))
            {
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.KeyPlacement.IndexOfCurrentValue() < LRFlags.Items.KeyPlacement.IndexOf("Grindy"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Quest"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("it"))
                    return false;
                if (IsKeyItem(rep) && !IsKeyItem(old) && LRFlags.Items.KeyPlacement.IndexOfCurrentValue() < LRFlags.Items.KeyPlacement.IndexOf("Quests"))
                    return false;
            }
            if (treasureData[old].Traits.Contains("Battle"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string.StartsWith("it"))
                    return false;
                if (treasuresOrig[rep].iItemCount > 1)
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string == "")
                    return false;
            }
            if (treasureData[old].Traits.Contains("Trade"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (treasuresOrig[rep].iItemCount > 1)
                    return false;
                if (treasuresOrig[rep].s11ItemResourceId_string == "")
                    return false;
            }

            if (IsKeyItem(rep) && !treasureData[rep].Traits.Contains("Pilgrim") && (!IsKeyItem(old) || treasureData[old].Traits.Contains("Pilgrim")) && LRFlags.Items.KeyPlacement.IndexOfCurrentValue() < LRFlags.Items.KeyPlacement.IndexOf("Treasures"))
                return false;
            if ((!IsKeyItem(rep) || treasureData[rep].Traits.Contains("Pilgrim")) && IsKeyItem(old) && !treasureData[old].Traits.Contains("Pilgrim") && LRFlags.Items.KeyPlacement.IndexOfCurrentValue() < LRFlags.Items.KeyPlacement.IndexOf("Treasures"))
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

        private bool IsMainKeyItem(string t)
        {         
            return treasureData[t].Traits.Contains("MainKey");
        }

        private bool IsSideKeyItem(string t)
        {
            return treasureData[t].Traits.Contains("SideKey");
        }

        private bool IsCoPKeyItem(string t)
        {
            return treasureData[t].Traits.Contains("CoPKey");
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

        private string AddHint(Dictionary<string, string> soFar, Dictionary<string, int> depths, Dictionary<string, int> items, string old, string rep, int itemDepth)
        {
            if (IsHintable(rep))
            {
                List<HintData> possible = hintData.Values.Where(h =>
                {
                    if (!h.Requirements.IsValid(items))
                        return false;
                    if (h.Requirements.GetPossibleRequirements().Contains(rep))
                        return false;
                    return true;
                }).ToList().Shuffle().OrderByDescending(h =>
                {
                    if (LRFlags.Other.HintsDepth.Enabled)
                    {
                        int hintDepth = GetReqsMaxDepth(soFar, depths, h.Requirements);
                        if (hintDepth > itemDepth)
                            return false;
                    }
                    return true;
                }).ThenBy(h => hintsMain[h.ID].Count).ToList();

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
            SaveHints();
            SetAndClearBattleDrops();
            treasures.SaveDB3(@"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        }

        private void SetAndClearBattleDrops()
        {
            BattleDrops.Clear();
            OrigBattleDrops.Clear();
            AddAndClearBattleDrop("btsc04902", "tre_drp_hunnu");
            AddAndClearBattleDrop("btsc04900", "tre_drp_keisan");
            AddAndClearBattleDrop("btsc02902", "tre_drp_kaban");
            AddAndClearBattleDrop("btsc02952", "tre_drp_bashira");

            AddAndClearBattleDrop(new string[] { "btsc07800", "btsc07801" }, "tre_acc_a_9060");
            AddAndClearBattleDrop(new string[] { "btsc01800", "btsc01801" }, "tre_wea_oa00");
            AddAndClearBattleDrop(new string[] { "btsc06800", "btsc06801" }, "tre_wea_oa02");
            AddAndClearBattleDrop(new string[] { "btsc05900", "btsc05901", "btsc05902" }, "tre_acc_a_9050");
            AddAndClearBattleDrop("btsc11900", "tre_acc_a_9210");

            AddAndClearBattleDrop("btsc10020", "tre_acc_a_9000");
            AddAndClearBattleDrop("btsc10002", "tre_acc_a_9010");
            AddAndClearBattleDrop("btsc10032", "tre_acc_a_9020");
            AddAndClearBattleDrop("btsc10027", "tre_acc_a_9030");
            AddAndClearBattleDrop("btsc10003", "tre_acc_a_9040");
            AddAndClearBattleDrop("btsc10030", "tre_acc_a_9070");
            AddAndClearBattleDrop("btsc10029", "tre_acc_a_9080");
            AddAndClearBattleDrop("btsc10025", "tre_acc_a_9090");
            AddAndClearBattleDrop("btsc10026", "tre_acc_a_9100");
            AddAndClearBattleDrop("btsc10022", "tre_acc_a_9110");
            AddAndClearBattleDrop("btsc10019", "tre_acc_a_9120");
            AddAndClearBattleDrop("btsc10005", "tre_acc_a_9130");
            AddAndClearBattleDrop("btsc10004", "tre_acc_a_9140");
            AddAndClearBattleDrop("btsc10021", "tre_acc_a_9150");
            AddAndClearBattleDrop("btsc10031", "tre_acc_a_9180");
            AddAndClearBattleDrop("btsc10035", "tre_acc_a_9190");
            AddAndClearBattleDrop("btsc10009", "tre_acc_a_9220");
            AddAndClearBattleDrop("btsc10024", "tre_acc_b_9000");
            AddAndClearBattleDrop("btsc10015", "tre_acc_b_9010");
            AddAndClearBattleDrop("btsc10007", "tre_acc_b_9020");
            AddAndClearBattleDrop("btsc10033", "tre_acc_b_9030");
            AddAndClearBattleDrop("btsc10008", "tre_acc_b_9040");
            AddAndClearBattleDrop("btsc10014", "tre_acc_b_9050");
            AddAndClearBattleDrop("btsc10018", "tre_acc_b_9080");
            AddAndClearBattleDrop("btsc10023", "tre_acc_b_9090");
            AddAndClearBattleDrop("btsc10001", "tre_wea_oa05");
            AddAndClearBattleDrop("btsc10006", "tre_wea_oa07");
            AddAndClearBattleDrop("btsc10017", "tre_wea_oa13");
            AddAndClearBattleDrop("btsc10016", "tre_acc_b_9070");
            AddAndClearBattleDrop("btsc10028", "tre_acc_a_9160");
            AddAndClearBattleDrop("btsc10000", "tre_acc_a_9170");
        }

        private void AddAndClearBattleDrop(string btsc, string treasure)
        {
            AddAndClearBattleDrop(new string[] { btsc }, treasure);
        }

        private void AddAndClearBattleDrop(string[] btscs, string treasure)
        {
            btscs.ForEach(btsc =>
            {
                BattleDrops.Add(btsc, treasures[treasure].s11ItemResourceId_string);
            });
            OrigBattleDrops.Add(treasure, treasures[treasure].s11ItemResourceId_string);

            treasures[treasure].s11ItemResourceId_string = "";
            treasures[treasure].iItemCount = 0;
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
                        string type = "Other";
                        if (IsMainKeyItem(finalPlacement[t.name]))
                            type = "a Story Key Item";
                        if (IsSideKeyItem(finalPlacement[t.name]))
                            type = "a Side Key Item";
                        if (IsCoPKeyItem(finalPlacement[t.name]))
                            type = "a CoP Key Item";
                        if (treasureData[finalPlacement[t.name]].Traits.Contains("Pilgrim"))
                            type = "Pilgrim's Crux";
                        if (IsEPAbility(t.name, false))
                            type = "an EP Ability";

                        return $"{treasureData[t.name].Name} has {type}";
                    }
                case 2:
                    {
                        return $"{treasureData[t.name].Location} has {GetItemName(t.s11ItemResourceId_string)}";
                    }
                case 3:
                    {
                        return $"{treasureData[t.name].Name} has ?????";
                    }
            }
        }

        public override HTMLPage GetDocumentation()
        {
            OrigBattleDrops.Keys.ForEach(name =>
            {
                treasures[name].s11ItemResourceId_string = OrigBattleDrops[name];
                treasures[name].iItemCount = 1;
            });

            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Location (for Hints)" }).ToList(), (new int[] { 40, 30, 30 }).ToList(), treasureData.Values.Select(t =>
            {
                string itemID = treasures[t.ID].s11ItemResourceId_string;
                string name = GetItemName(itemID);
                return new string[] { t.Name, $"{name} x {treasures[t.ID].iItemCount}", t.Location }.ToList();
            }).ToList()));

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
            {
                TextRando textRando = randomizers.Get<TextRando>("Text");

                page.HTMLElements.Add(new Table("Hints", (new string[] { "Main Quest", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintsMain.Keys.Select(h =>
                {
                    return new string[] { hintData[h].Name, textRando.mainSysUS["$" + h].Replace("{Text NewLine}", "\n") }.ToList();
                }).ToList()));
            }
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

            public bool IsValid(Dictionary<string, int> items, TreasureRando treasureRando)
            {
                if (Traits.Contains("EP") && !HasEP(items, treasureRando))
                    return false;

                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public bool HasEP(Dictionary<string, int> items, TreasureRando treasureRando)
            {
                QuestRando questRando = treasureRando.randomizers.Get<QuestRando>("Quests");

                foreach (DataStoreRQuest quest in questRando.questRewards.Values.Where(q => q.iMaxGp > 0))
                {
                    if (quest.name == "qst_027" && treasureRando.treasureData["tre_qst_027"].IsValid(items, treasureRando)) // Peace and Quiet, Kupo
                        return true;
                    if (quest.name == "qst_028" && treasureRando.treasureData["tre_qst_028"].IsValid(items, treasureRando)) // Saving an Angel
                        return true;
                    if (quest.name == "qst_046" && treasureRando.treasureData["tre_qst_046"].IsValid(items, treasureRando)) // Adonis's Audition
                        return true;
                    if (quest.name == "qst_062" && treasureRando.treasureData["tre_qst_062"].IsValid(items, treasureRando)) // Fighting Actress
                        return true;
                    if (quest.name == "qst_9000" && treasureRando.hintData["fl_mnlx_005e"].Requirements.IsValid(items)) // 1-5
                        return true;
                    if (quest.name == "qst_9010" && treasureRando.hintData["fl_mnyu_004e"].Requirements.IsValid(items)) // 2-3
                        return true;
                    if (quest.name == "qst_9020" && treasureRando.hintData["fl_mndd_005e"].Requirements.IsValid(items)) // 4-5
                        return true;
                    if (quest.name == "qst_9030" && treasureRando.hintData["fl_mnwl_003e"].Requirements.IsValid(items)) // 3-3
                        return true;
                    /*if (quest.name == "qst_9040" && treasureData["tre_qst_027_2"].Requirements.IsValid(items)) // Ereshkigal (Missable)
                        return true;*/
                    if (quest.name == "qst_9050" && treasureRando.hintData["fl_mnsz_001e"].Requirements.IsValid(items))
                        return true;
                }
                return false;
            }
        }

        public class HintData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public ItemReq Requirements { get; set; }
            public HintData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Requirements = ItemReq.Parse(row[2]);
            }
        }
    }
}
