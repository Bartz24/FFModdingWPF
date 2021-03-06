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
        public DataStoreDB3<DataStoreRTreasurebox> treasuresOrig = new DataStoreDB3<DataStoreRTreasurebox>();
        public DataStoreDB3<DataStoreRTreasurebox> treasures = new DataStoreDB3<DataStoreRTreasurebox>();
        public Dictionary<string, ItemLocation> itemLocations = new Dictionary<string, ItemLocation>();
        public Dictionary<string, HintData> hintData = new Dictionary<string, HintData>();
        public Dictionary<string, List<string>> hintsMain = new Dictionary<string, List<string>>();
        Dictionary<string, string> hintsNotesLocations = new Dictionary<string, string>();
        Dictionary<string, int> hintsNotesCount = new Dictionary<string, int>();

        public Dictionary<string, string> BattleDrops = new Dictionary<string, string>();
        public Dictionary<string, string> OrigBattleDrops = new Dictionary<string, string>();

        public List<string> RandomEquip = new List<string>();
        public List<string> RemainingEquip = new List<string>();
        public List<string> RandomAdorn = new List<string>();
        public List<string> RemainingAdorn = new List<string>();

        private LRAssumedItemPlacementAlgorithm placementAlgo;
        private LRItemPlacementAlgorithm placementAlgoBackup;
        private bool usingBackup = false;

        public ItemPlacementAlgorithm<ItemLocation> PlacementAlgo { get => usingBackup ? placementAlgoBackup : placementAlgo; }

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

            FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
            {
                TreasureData t = new TreasureData(row, this);
                itemLocations.Add(t.ID, t);
            }, FileHelpers.CSVFileHeader.HasHeader);

            FileHelpers.ReadCSVFile(@"data\battleDrops.csv", row =>
            {
                BattleDropData b = new BattleDropData(row, this);
                itemLocations.Add(b.ID, b);
            }, FileHelpers.CSVFileHeader.HasHeader);

            hintsMain.Clear();
            hintData.Clear();
            FileHelpers.ReadCSVFile(@"data\hints.csv", row =>
            {
                HintData h = new HintData(row);
                hintData.Add(h.ID, h);
                hintsMain.Add(h.ID, new List<string>());
            }, FileHelpers.CSVFileHeader.HasHeader);

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
            AddTreasure("tre_d_base", "key_d_base", 1, "");
            AddTreasure("tre_d_wing", "key_d_wing", 1, "");
            AddTreasure("tre_d_top", "key_d_top", 1, "");
            AddTreasure("tre_libra_m375", "libra_m375", 1, "");
            AddTreasure("tre_w_tane", "key_w_tane", 5, "");
            AddTreasure("tre_w_tamago", "key_w_tamago", 1, "");
            AddTreasure("tre_w_moji1", "key_w_moji1", 1, "");
            AddTreasure("tre_w_moji2", "key_w_moji2", 1, "");
            AddTreasure("tre_w_buhin1", "key_w_buhin1", 1, "");
            AddTreasure("tre_w_buhin2", "key_w_buhin2", 1, "");
            AddTreasure("tre_w_buhin3", "key_w_buhin3", 1, "");
            AddTreasure("tre_w_data", "key_w_data", 1, "");
            AddTreasure("tre_w_apple1", "key_w_apple", 1, "");
            AddTreasure("tre_w_apple2", "key_w_apple", 1, "");
            AddTreasure("tre_w_apple3", "key_w_apple", 1, "");
            AddTreasure("tre_y_letter", "key_y_letter", 1, "");
            AddTreasure("tre_y_recipe", "key_y_recipe", 1, "");
            AddTreasure("tre_y_cream", "key_y_cream", 1, "");
            AddTreasure("tre_libra_m330", "libra_m330", 1, "");
            AddTreasure("tre_s_hiai", "key_s_hiai", 1, "");
            AddTreasure("tre_wea_ea00", "wea_ea00", 1, "");
            AddTreasure("tre_shi_ea00", "shi_ea00", 1, "");

            AddTreasure("trd_niku", "key_niku", 1, "");
            AddTreasure("trd_ninjin", "key_ninjin", 1, "");
            AddTreasure("trd_ticket", "key_y_ticket", 1, "");
            AddTreasure("trd_soulcd", "key_soulcd", 1, "");

            AddTreasure("ran_rando_id", "false", 1, "");
            AddTreasure("ran_bhuni_p", "false", 1, "");
            AddTreasure("ran_multi", "rando_multi_item", 9999, "");

            treasures.Swap("tre_box_p_000", "tre_box_p_003");
            treasuresOrig.Swap("tre_box_p_000", "tre_box_p_003");
            treasures.Swap("tre_box_p_001", "tre_box_p_200");
            treasuresOrig.Swap("tre_box_p_001", "tre_box_p_200");
            treasures.Swap("tre_box_p_002", "tre_box_p_201");
            treasuresOrig.Swap("tre_box_p_002", "tre_box_p_201");

            hintsNotesLocations.Clear();
            hintsNotesCount.Clear();
            treasuresOrig.Keys.Where(k => treasuresOrig[k].s11ItemResourceId_string.StartsWith("libra")).ForEach(k => hintsNotesLocations.Add(treasuresOrig[k].s11ItemResourceId_string, null));

            LRFlags.Items.Treasures.SetRand();
            List<string> locations = itemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList().Shuffle().ToList();
            List<string> libraItems = hintsNotesLocations.Keys.ToList().Shuffle().ToList();
            for (int i = 0; i < locations.Count; i++)
            {
                hintsNotesLocations[libraItems[i]] = locations[i];
            }
            RandomNum.ClearRand();

            placementAlgo = new LRAssumedItemPlacementAlgorithm(itemLocations, locations, Randomizers, 10);
            placementAlgoBackup = new LRItemPlacementAlgorithm(itemLocations, locations, Randomizers);
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
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            ShopRando shopRando = Randomizers.Get<ShopRando>("Shops");

            if (LRFlags.Items.Treasures.FlagEnabled)
            {
                LRFlags.Items.Treasures.SetRand();

                RandomEquip = GetRandomizableEquip();
                RandomEquip.AddRange(shopRando.GetRandomizableEquip());
                RandomEquip = RandomEquip.Distinct().ToList();
                RemainingEquip = new List<string>(RandomEquip);

                RandomAdorn = GetAdornments();
                RandomAdorn.AddRange(shopRando.GetAdornments());
                RandomAdorn = RandomAdorn.Distinct().ToList();
                RemainingAdorn = new List<string>(RandomAdorn);

                List<string> keys = itemLocations.Keys.ToList().Shuffle().ToList();

                if(!placementAlgo.Randomize(new List<string>()))
                {
                    usingBackup = true;
                    placementAlgoBackup.Randomize(new List<string>());
                }

                // Same treasures take priority
                keys = keys.OrderBy(k => !itemLocations[k].Traits.Contains("Same")).ToList();

                foreach (string key in keys)
                {
                    bool isSame = itemLocations[key].Traits.Contains("Same");
                    Func<string, string, bool> sameCheck = (rep, orig) =>
                    {
                        if (rep.StartsWith("cos") && orig.StartsWith("cos") && rep != "cos_ba08" && rep != "cos_ca08")
                            return true;
                        if (rep.StartsWith("wea") && orig.StartsWith("wea") && rep != "wea_ea00")
                            return true;
                        if (rep.StartsWith("shi") && orig.StartsWith("shi") && rep != "shi_ea00")
                            return true;
                        if (rep.StartsWith("acc") && orig.StartsWith("acc"))
                            return true;
                        if (rep.StartsWith("e") && orig.StartsWith("e") && rep.Length == 4 && orig.Length == 4)
                            return true;
                        return false;
                    };
                    if (RandomEquip.Contains(PlacementAlgo.GetLocationItem(key, false).Item1))
                    {
                        string next = RemainingEquip.Where(s => !isSame || sameCheck(s, PlacementAlgo.GetLocationItem(key, false).Item1)).ToList().Shuffle().First();
                        RemainingEquip.Remove(next);
                        PlacementAlgo.SetLocationItem(key, next, 1);
                    }
                    if (RandomAdorn.Contains(PlacementAlgo.GetLocationItem(key, false).Item1))
                    {
                        string next = RemainingAdorn.Where(s => !isSame || sameCheck(s, PlacementAlgo.GetLocationItem(key, false).Item1)).ToList().Shuffle().First();
                        RemainingAdorn.Remove(next);
                        PlacementAlgo.SetLocationItem(key, next, 1);
                    }

                    if (equipRando.items.Keys.Contains(PlacementAlgo.GetLocationItem(key, false).Item1) && equipRando.IsAbility(equipRando.items[PlacementAlgo.GetLocationItem(key, false).Item1]))
                    {
                        string lv = PlacementAlgo.GetLocationItem(key, false).Item1.Substring(PlacementAlgo.GetLocationItem(key, false).Item1.Length - 3);
                        string next = equipRando.GetAbilities(-1).Shuffle().First().sScriptId_string;
                        PlacementAlgo.SetLocationItem(key, next + lv, 1);
                    }
                }

                RandomNum.ClearRand();

                if (LRFlags.Other.HintsNotes.FlagEnabled)
                {
                    // Update hints again to reflect actual numbers
                    hintsNotesLocations.Keys.Where(note => hintsNotesLocations[note] != null).ForEach(note =>
                        {
                            int locationCount = itemLocations.Keys.Where(t => PlacementAlgo.Placement.ContainsKey(t) && itemLocations[t].Areas[0] == hintsNotesLocations[note] && PlacementAlgo.IsHintable(PlacementAlgo.Placement[t])).Count();
                            hintsNotesCount[hintsNotesLocations[note]] = locationCount;
                        });
                }

                if (LRFlags.Items.IDCardBuy.Enabled)
                    treasures["ran_rando_id"].s11ItemResourceId_string = "true";
            }
        }
        public bool IsImportantKeyItem(string location)
        {
            return IsMainKeyItem(location) || IsSideKeyItem(location) || IsCoPKeyItem(location);
        }

        public bool IsEPAbility(string t, bool orig = true)
        {
            return PlacementAlgo.GetLocationItem(t, orig).Item1.StartsWith("ti") || PlacementAlgo.GetLocationItem(t, orig).Item1 == "at900_00";
        }

        public bool IsMainKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("MainKey");
        }

        public bool IsSideKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("SideKey");
        }

        public bool IsCoPKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("CoPKey");
        }

        public bool IsPilgrimKeyItem(string t)
        {
            return PlacementAlgo.ItemLocations[t].Traits.Contains("Pilgrim");
        }

        public List<string> GetRandomizableEquip()
        {
            Func<string, bool> isEquip = s => (s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi") || s.StartsWith("acc")) && s != "cos_fa00";
            List<string> list = new List<string>();
            list.AddRange(treasuresOrig.Values.Where(t => isEquip(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

            return list;
        }

        public List<string> GetAdornments()
        {
            Func<string, bool> isAdorn = s => s.StartsWith("e");
            List<string> list = new List<string>();
            list.AddRange(treasuresOrig.Values.Where(t => isAdorn(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

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
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = Randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = Randomizers.Get<TextRando>("Text");

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
            {
                hintsMain.Keys.ForEach(h =>
                {
                    textRando.mainSysUS["$" + h] = string.Join("{Text NewLine}{Text NewLine}", hintsMain[h].Select(t => itemLocations[t]).Select(t => GetHintText(t)));
                });
            }

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsNotes.FlagEnabled)
            {
                hintsNotesLocations.Keys.Where(note => hintsNotesLocations[note] != null).ForEach(i =>
                {
                    textRando.mainSysUS[equipRando.items[i].sHelpStringId_string] = $"{hintsNotesLocations[i]} has {hintsNotesCount[hintsNotesLocations[i]]} important checks.";
                });
            }
        }

        private string GetHintText(ItemLocation t)
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
                        return $"{t.Name} has {GetItemName(PlacementAlgo.GetLocationItem(t.ID, false).Item1)}";
                    }
                case 1:
                    {
                        string type = "Other";
                        if (IsMainKeyItem(PlacementAlgo.Placement[t.ID]))
                            type = "a Story Key Item";
                        if (IsSideKeyItem(PlacementAlgo.Placement[t.ID]))
                            type = "a Side Key Item";
                        if (IsCoPKeyItem(PlacementAlgo.Placement[t.ID]))
                            type = "a CoP Key Item";
                        if (IsPilgrimKeyItem(PlacementAlgo.Placement[t.ID]))
                            type = "Pilgrim's Crux";
                        if (IsEPAbility(t.ID, false))
                            type = "an EP Ability";

                        return $"{t.Name} has {type}";
                    }
                case 2:
                    {
                        return $"{t.Areas[0]} has {GetItemName(PlacementAlgo.GetLocationItem(t.ID, false).Item1)}";
                    }
                case 3:
                    {
                        return $"{t.Name} has ?????";
                    }
            }
        }

        public override HTMLPage GetDocumentation()
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            OrigBattleDrops.Keys.ForEach(name =>
            {
                treasures[name].s11ItemResourceId_string = OrigBattleDrops[name];
                treasures[name].iItemCount = 1;
            });

            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Button("document.getElementById(\"itemlocations\").classList.toggle(\"hide4\")", null, "Hide/Show Requirements"));

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Location", "Requirements", "'Difficulty'" }).ToList(), (new int[] { 30, 20, 10, 35, 5 }).ToList(), itemLocations.Values.Select(t =>
            {
                string itemID = PlacementAlgo.GetLocationItem(t.ID, false).Item1;
                string name = GetItemName(itemID);
                string reqsDisplay = t.Requirements.GetDisplay(GetItemName);
                if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
                    reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
                return (new string[] { t.Name, $"{name} x {PlacementAlgo.GetLocationItem(t.ID, false).Item2}", t.Areas[0], reqsDisplay, $"{t.Difficulty}" }).ToList();
            }).ToList(), "itemlocations"));

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
            {
                page.HTMLElements.Add(new Table("Main Quest Hints", (new string[] { "Main Quest", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintsMain.Keys.Select(h =>
                {
                    return new string[] { hintData[h].Name, textRando.mainSysUS["$" + h].Replace("{Text NewLine}", "\n") }.ToList();
                }).ToList()));
            }

            if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsNotes.FlagEnabled)
            {
                page.HTMLElements.Add(new Table("Libra Note Hints", (new string[] { "Libra Note", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintsNotesLocations.Keys.Where(note => hintsNotesLocations[note] != null).Select(i =>
                {
                    return new string[] { GetItemName(i), textRando.mainSysUS[equipRando.items[i].sHelpStringId_string] }.ToList();
                }).ToList()));
            }

            return page;
        }

        private string GetItemName(string itemID)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            AbilityRando abilityRando = Randomizers.Get<AbilityRando>("Abilities");
            TextRando textRando = Randomizers.Get<TextRando>("Text");
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

        public class TreasureData : ItemLocation
        {
            public override string ID { get; }
            public override string Name { get; }
            public override int Difficulty { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }

            private TreasureRando rando;

            public TreasureData(string[] row, TreasureRando treasureRando)
            {
                ID = row[0];
                Areas = new List<string>() { row[1] };
                Name = row[2];
                Traits = row[3].Split("|").ToList();
                Requirements = ItemReq.Parse(row[4]);
                Difficulty = int.Parse(row[5]);

                rando = treasureRando;
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (Traits.Contains("EP") && !HasEP(items))
                    return false;

                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public bool HasEP(Dictionary<string, int> items)
            {
                QuestRando questRando = rando.Randomizers.Get<QuestRando>("Quests");

                foreach (DataStoreRQuest quest in questRando.questRewards.Values.Where(q => q.iMaxGp > 0))
                {
                    if (quest.name == "qst_027" && rando.itemLocations["tre_qst_027"].IsValid(items)) // Peace and Quiet, Kupo
                        return true;
                    if (quest.name == "qst_028" && rando.itemLocations["tre_qst_028"].IsValid(items)) // Saving an Angel
                        return true;
                    if (quest.name == "qst_046" && rando.itemLocations["tre_qst_046"].IsValid(items)) // Adonis's Audition
                        return true;
                    if (quest.name == "qst_062" && rando.itemLocations["tre_qst_062"].IsValid(items)) // Fighting Actress
                        return true;
                    if (quest.name == "qst_9000" && rando.hintData["fl_mnlx_005e"].Requirements.IsValid(items)) // 1-5
                        return true;
                    if (quest.name == "qst_9010" && rando.hintData["fl_mnyu_004e"].Requirements.IsValid(items)) // 2-3
                        return true;
                    if (quest.name == "qst_9020" && rando.hintData["fl_mndd_005e"].Requirements.IsValid(items)) // 4-5
                        return true;
                    if (quest.name == "qst_9030" && rando.hintData["fl_mnwl_003e"].Requirements.IsValid(items)) // 3-3
                        return true;
                    if (quest.name == "qst_9040" && rando.itemLocations["tre_qst_027_2"].Requirements.IsValid(items)) // Ereshkigal
                        return true;
                    if (quest.name == "qst_9050" && rando.hintData["fl_mnsz_001e"].Requirements.IsValid(items))
                        return true;
                }
                return false;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreRTreasurebox t = (DataStoreRTreasurebox)obj;
                t.s11ItemResourceId_string = newItem;
                t.iItemCount = newCount;
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreRTreasurebox t = (DataStoreRTreasurebox)obj;
                return new Tuple<string, int>(t.s11ItemResourceId_string, t.iItemCount);
            }
        }

        public class BattleDropData : ItemLocation
        {
            public override string ID { get; }
            public override string Name { get; }
            public override int Difficulty { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }

            private TreasureRando rando;

            public BattleDropData(string[] row, TreasureRando treasureRando)
            {
                ID = row[0];
                Areas = new List<string>() { row[1] };
                Name = row[2];
                Traits = row[3].Split("|").ToList();
                Traits.Add("Battle");
                Requirements = ItemReq.Parse(row[4]);
                Difficulty = int.Parse(row[5]);

                rando = treasureRando;
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                return Requirements.IsValid(items);
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreBtScene b = (DataStoreBtScene)obj;
                b.sDropItem0_string = newItem;
                b.u16DropProb0 = 10000;
                b.u8NumDrop0 = newCount;
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreBtScene b = (DataStoreBtScene)obj;
                return new Tuple<string, int>(b.sDropItem0_string, b.u8NumDrop0);
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
