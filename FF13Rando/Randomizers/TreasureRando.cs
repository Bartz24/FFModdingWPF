using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando
{
    public class TreasureRando : Randomizer
    {
        public DataStoreWDB<DataStoreTreasurebox> treasuresOrig = new DataStoreWDB<DataStoreTreasurebox>();
        public DataStoreWDB<DataStoreTreasurebox> treasures = new DataStoreWDB<DataStoreTreasurebox>();

        Dictionary<string, HintData> hintData = new Dictionary<string, HintData>();

        public Dictionary<string, FF13ItemLocation> itemLocations = new Dictionary<string, FF13ItemLocation>();

        Dictionary<string, List<string>> hintsMain = new Dictionary<string, List<string>>();
        Dictionary<string, int> hintsNotesUniqueCount = new Dictionary<string, int>();
        Dictionary<string, int> hintsNotesSharedCount = new Dictionary<string, int>();

        FF13AssumedItemPlacementAlgorithm placementAlgoNormal;
        FF13ItemPlacementAlgorithm placementAlgoBackup;
        private bool usingBackup = false;

        public ItemPlacementAlgorithm<FF13ItemLocation> PlacementAlgo { get => usingBackup ? placementAlgoBackup : placementAlgoNormal; }

        public TreasureRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetID()
        {
            return "Treasures";
        }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Treasure Data...", -1, 100);
            treasuresOrig.LoadWDB("13", @"\db\resident\treasurebox.wdb");
            treasures.LoadWDB("13", @"\db\resident\treasurebox.wdb");

            itemLocations.Clear();

            FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
            {
                TreasureData t = new TreasureData(row);
                itemLocations.Add(t.ID, t);
            }, FileHelpers.CSVFileHeader.HasHeader);

            FileHelpers.ReadCSVFile(@"data\battledrops.csv", row =>
            {
                BattleData b = new BattleData(row);
                itemLocations.Add(b.ID, b);
            }, FileHelpers.CSVFileHeader.HasHeader);

            FileHelpers.ReadCSVFile(@"data\enemydrops.csv", row =>
            {
                EnemyData e = new EnemyData(row);
                itemLocations.Add(e.ID, e);
            }, FileHelpers.CSVFileHeader.HasHeader);

            hintData.Clear();

            string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            string[] roles = new string[] { "com", "rav", "sen", "syn", "sab", "med" };
            int[] hp = new int[] { 200, 390, 170, 220, 350, 350 };
            int[] str = new int[] { 15, 28, 8, 12, 17, 23 };
            int[] mag = new int[] { 15, 22, 20, 12, 7, 35 };

            foreach (string c in chars)
            {
                foreach (string r in roles)
                {
                    AddTreasure($"z_ran_{c}_{r}", $"rol_{c}_{r}", 1);
                }
                AddTreasure($"z_ini_{c}_hp", "", hp[chars.ToList().IndexOf(c)]);
                AddTreasure($"z_ini_{c}_str", "", str[chars.ToList().IndexOf(c)]);
                AddTreasure($"z_ini_{c}_mag", "", mag[chars.ToList().IndexOf(c)]);
                AddTreasure($"z_ini_{c}_wea", $"wea_{c}_001", 1);
            }

            AddTreasure("z_ini_potion", "it_potion", 5);
            AddTreasure("z_ini_gil", "", 200);
            AddTreasure("z_ini_shop", "key_shop_00", 1);

            AddTreasure("z_ran_stg_03", "cry_stage", 1);
            AddTreasure("z_ran_stg_04", "cry_stage", 1);
            AddTreasure("z_ran_stg_05", "cry_stage", 1);
            AddTreasure("z_ran_stg_06", "cry_stage", 1);
            AddTreasure("z_ran_stg_07", "cry_stage", 1);
            AddTreasure("z_ran_stg_09", "cry_stage", 1);
            AddTreasure("z_ran_stg_10", "cry_stage", 1);
            AddTreasure("z_ran_stg_11", "cry_stage", 1);
            AddTreasure("z_ran_stg_13", "cry_stage", 1);

            AddTreasure("z_shp_03_06", "key_shop_06", 1);
            AddTreasure("z_shp_04_06", "key_shop_06", 1);
            AddTreasure("z_shp_05_00", "key_shop_00", 1);
            AddTreasure("z_shp_05_06", "key_shop_06", 1);
            AddTreasure("z_shp_06_02", "key_shop_02", 1);
            AddTreasure("z_shp_06_07", "key_shop_07", 1);
            AddTreasure("z_shp_07_00", "key_shop_00", 1);
            AddTreasure("z_shp_07_06", "key_shop_06", 1);
            AddTreasure("z_shp_07_12", "key_shop_12", 1);
            AddTreasure("z_shp_08_07", "key_shop_07", 1);
            AddTreasure("z_shp_08_10", "key_shop_10", 1);
            AddTreasure("z_shp_09_00", "key_shop_00", 1);
            AddTreasure("z_shp_09_06", "key_shop_06", 1);
            AddTreasure("z_shp_09_12", "key_shop_12", 1);
            AddTreasure("z_shp_10_00", "key_shop_00", 1);
            AddTreasure("z_shp_10_03", "key_shop_03", 1);
            AddTreasure("z_shp_10_06", "key_shop_06", 1);
            AddTreasure("z_shp_10_07", "key_shop_07", 1);
            AddTreasure("z_shp_10_10", "key_shop_10", 1);
            AddTreasure("z_shp_10_11", "key_shop_11", 1);
            AddTreasure("z_shp_11_00", "key_shop_00", 1);
            AddTreasure("z_shp_11_06", "key_shop_06", 1);
            AddTreasure("z_shp_11_07", "key_shop_07", 1);
            AddTreasure("z_shp_11_08", "key_shop_08", 1);
            AddTreasure("z_shp_11_10", "key_shop_10", 1);
            AddTreasure("z_shp_11_11", "key_shop_11", 1);
            AddTreasure("z_shp_11_02b", "key_shop_02", 1);
            AddTreasure("z_shp_11_03b", "key_shop_03", 1);
            AddTreasure("z_shp_11_06b", "key_shop_06", 1);
            AddTreasure("z_shp_11_12b", "key_shop_12", 1);
            AddTreasure("z_shp_12_03", "key_shop_03", 1);
            AddTreasure("z_shp_12_06", "key_shop_06", 1);
            AddTreasure("z_shp_13_03", "key_shop_03", 1);

            List<string> hintsNotesLocations = hintData.Values.SelectMany(h => h.Areas).ToList();
            List<string> locations = itemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList();

            placementAlgoNormal = new FF13AssumedItemPlacementAlgorithm(itemLocations, locations, Randomizers, 3);
            placementAlgoNormal.SetProgressFunc = Randomizers.SetProgressFunc;
            placementAlgoBackup = new FF13ItemPlacementAlgorithm(itemLocations, locations, Randomizers, -1);
            placementAlgoBackup.SetProgressFunc = Randomizers.SetProgressFunc;
        }

        public void AddTreasure(string newName, string item, int count)
        {
            AddTreasure(treasuresOrig, newName, item, count);
            AddTreasure(treasures, newName, item, count);
        }

        private void AddTreasure(DataStoreWDB<DataStoreTreasurebox> database, string newName, string item, int count)
        {
            database.Copy(database.Keys[0], newName);
            database[newName].sItemResourceId_string = item;
            database[newName].iItemCount = (uint)count;
        }

        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Treasure Data...", -0, 100);
            if (FF13Flags.Items.Treasures.FlagEnabled)
            {
                FF13Flags.Items.Treasures.SetRand();

                Dictionary<string, double> areaMults = itemLocations.Values.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);
                placementAlgoNormal.SetAreaMults(areaMults);
                placementAlgoBackup.SetAreaMults(areaMults);
                if (!placementAlgoNormal.Randomize(new List<string>()))
                {
                    usingBackup = true;
                    placementAlgoBackup.Randomize(placementAlgoBackup.GetNewAreasAvailable(new Dictionary<string, int>(), new List<string>()));
                }
                Randomizers.SetProgressFunc("Randomizing Treasure Data...", 60, 100);

                // Update hints again to reflect actual numbers
                PlacementAlgo.HintsByLocation.ForEach(l =>
                {
                    int uniqueCount = itemLocations.Keys.Where(t => PlacementAlgo.Placement.ContainsKey(t) && itemLocations[t].Areas.Count == 1 && itemLocations[t].Areas[0] == l && PlacementAlgo.IsHintable(PlacementAlgo.Placement[t])).Count();
                    hintsNotesUniqueCount.Add(l, uniqueCount);

                    int sharedCount = itemLocations.Keys.Where(t => PlacementAlgo.Placement.ContainsKey(t) && itemLocations[t].Areas.Count > 1 && itemLocations[t].Areas.Contains(l) && PlacementAlgo.IsHintable(PlacementAlgo.Placement[t])).Count();
                    hintsNotesSharedCount.Add(l, sharedCount);
                });

                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Treasure Data...", 80, 100);
            if (FF13Flags.Items.ShuffleRoles.FlagEnabled)
            {
                FF13Flags.Items.ShuffleRoles.SetRand();
                string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
                string[] roles = new string[] { "com", "rav", "sen", "syn", "sab", "med" };

                foreach (string c in chars)
                {
                    string first = c == "saz" ? "rav" : RandomNum.SelectRandomWeighted(new List<string>() { "com", "rav" }, _ => 1);
                    List<string> rolesRemaining = roles.Where(r => r != first).ToList().Shuffle().ToList();
                    PlacementAlgo.SetLocationItem(itemLocations.Values.First(t => t.ID.StartsWith("z_ran_" + c) && itemLocations[t.ID].Traits.Contains("Same")).ID, $"rol_{c}_{first}", 1);

                    itemLocations.Values.Where(t => PlacementAlgo.GetLocationItem(t.ID, false).Item1.StartsWith("rol_" + c) && !itemLocations[t.ID].Traits.Contains("Same")).ForEach(t =>
                    {
                        PlacementAlgo.SetLocationItem(t.ID, $"rol_{c}_{rolesRemaining[0]}", 1);
                        rolesRemaining.RemoveAt(0);
                    });
                }

                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Treasure Data...", 90, 100);
            if (FF13Flags.Items.ShuffleShops.FlagEnabled)
            {
                FF13Flags.Items.ShuffleShops.SetRand();

                itemLocations.Values.Where(t => IsShop(t.ID, false)).ToList().Shuffle((l1, l2) =>
                {
                    Tuple<string, int> temp = PlacementAlgo.GetLocationItem(l1.ID, false);
                    PlacementAlgo.SetLocationItem(l1.ID, PlacementAlgo.GetLocationItem(l2.ID, false).Item1, 1);
                    PlacementAlgo.SetLocationItem(l2.ID, temp.Item1, 1);
                });

                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Treasure Data...", 95, 100);
            if (FF13Flags.Items.StartingEquip.FlagEnabled)
            {
                FF13Flags.Items.StartingEquip.SetRand();

                string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
                chars.ForEach(c => ShuffleEquip($"wea_{c}_"));

                RandomNum.ClearRand();
            }
        }

        private void ShuffleEquip(string prefix)
        {
            itemLocations.Values.Where(t => PlacementAlgo.GetLocationItem(t.ID, false).Item1.StartsWith(prefix)).ToList().Shuffle((l1, l2) =>
            {
                Tuple<string, int> temp = PlacementAlgo.GetLocationItem(l1.ID, false);
                PlacementAlgo.SetLocationItem(l1.ID, PlacementAlgo.GetLocationItem(l2.ID, false).Item1, 1);
                PlacementAlgo.SetLocationItem(l2.ID, temp.Item1, 1);
            });
        }
        public bool IsRepeatableAllowed(string location)
        {
            return IsInitRole(location) || IsOtherRole(location) || IsEidolon(location) || PlacementAlgo.GetLocationItem(location).Item1 == "key_ctool" || IsGysahlReins(location);
        }

        public bool IsImportantKeyItem(string location)
        {
            return IsInitRole(location) || IsOtherRole(location) || IsStage(location) || IsEidolon(location) || IsShop(location) || IsGysahlReins(location);
        }
        public bool IsInitRole(string t)
        {
            return PlacementAlgo.GetLocationItem(t, true).Item1.StartsWith("rol") && itemLocations[t].Areas.Contains("Initial");
        }

        public bool IsOtherRole(string t)
        {
            return PlacementAlgo.GetLocationItem(t, true).Item1.StartsWith("rol") && !itemLocations[t].Areas.Contains("Initial");
        }

        public bool IsStage(string t, bool orig = true)
        {
            return PlacementAlgo.GetLocationItem(t, orig).Item1 == "cry_stage";
        }

        public bool IsEidolon(string t, bool orig = true)
        {
            return itemLocations[t].Traits.Contains("Eidolon");
        }
        public bool IsShop(string t, bool orig = true)
        {
            return PlacementAlgo.GetLocationItem(t, orig).Item1.StartsWith("key_shop") || PlacementAlgo.GetLocationItem(t, orig).Item1 == "key_ctool";
        }
        public bool IsGysahlReins(string t, bool orig = true)
        {
            return PlacementAlgo.GetLocationItem(t, orig).Item1 == "key_field_00";
        }

        private void SaveHints()
        {
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Treasure Data...", -1, 100);
            SaveHints();
            treasures.SaveWDB(@"\db\resident\treasurebox.wdb");
        }

        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Location", "Requirements" }).ToList(), (new int[] { 30, 25, 15, 30 }).ToList(), itemLocations.Values.Select(t =>
            {
                string itemID = PlacementAlgo.GetLocationItem(t.ID, false).Item1;
                string name = GetItemName(itemID);
                string reqsDisplay = t.Requirements.GetDisplay(GetItemName);
                if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
                    reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
                string location = $"{itemLocations[t.ID].Name}";
                return (new string[] { location, $"{name} x {PlacementAlgo.GetLocationItem(t.ID, false).Item2}", t.Areas[0], reqsDisplay }).ToList();
            }).ToList(), "itemlocations"));

            return page;
        }

        private string GetItemName(string itemID)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            TextRando textRando = Randomizers.Get<TextRando>("Text");
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

        public class TreasureData : FF13ItemLocation
        {
            public override string ID { get; }
            public override string Name { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override List<string> Characters { get; }
            public override int Difficulty { get; }

            public TreasureData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Areas = new List<string>() { row[2] };
                Requirements = ItemReq.Parse(row[3]);
                Traits = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Difficulty = int.Parse(row[5]);
                Characters = FF13RandoHelpers.ParseReqCharas(row[6]);
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreTreasurebox t = (DataStoreTreasurebox)obj;
                t.sItemResourceId_string = newItem;
                t.iItemCount = (uint)newCount;
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreTreasurebox t = (DataStoreTreasurebox)obj;
                return new Tuple<string, int>(t.sItemResourceId_string, (int)t.iItemCount);
            }
        }

        public class EnemyData : FF13ItemLocation
        {
            public override string ID { get; }
            public int Index { get; }
            public override string Name { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override List<string> Characters { get; }
            public List<string> LinkedIDs { get; }
            public override int Difficulty { get; }

            public EnemyData(string[] row)
            {
                ID = row[0];
                Index = int.Parse(row[1]);
                Name = row[2];
                Areas = new List<string>() { row[3] };
                Requirements = ItemReq.Parse(row[4]);
                Traits = row[5].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                LinkedIDs = row[6].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Difficulty = int.Parse(row[7]);
                Characters = FF13RandoHelpers.ParseReqCharas(row[8]);
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreBtCharaSpec s = (DataStoreBtCharaSpec)obj;
                if (Index == 0)
                    s.sDropItem0_string = newItem;
                else
                    s.sDropItem1_string = newItem;

                if (s.u8NumDrop > 0)
                    s.u8NumDrop = (byte)newCount;
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreBtCharaSpec s = (DataStoreBtCharaSpec)obj;
                return new Tuple<string, int>(Index == 0 ? s.sDropItem0_string : s.sDropItem1_string, s.u8NumDrop);
            }
        }

        public class BattleData : FF13ItemLocation
        {
            public override string ID { get; }
            public override string Name { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override List<string> Characters { get; }
            public override int Difficulty { get; }

            public BattleData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Areas = new List<string>() { row[2] };
                Requirements = ItemReq.Parse(row[3]);
                Traits = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Difficulty = int.Parse(row[5]);
                Characters = FF13RandoHelpers.ParseReqCharas(row[6]);
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreBtScene s = (DataStoreBtScene)obj;
                s.sDrop100Id_string = newItem;
                s.u8NumDrop100 = (byte)newCount;
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreBtScene s = (DataStoreBtScene)obj;
                return new Tuple<string, int>(s.sDrop100Id_string, s.u8NumDrop100);
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
