using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13_2Rando
{
    public class TreasureRando : Randomizer
    {
        public DataStoreDB3<DataStoreRTreasurebox> treasuresOrig = new DataStoreDB3<DataStoreRTreasurebox>();
        public DataStoreDB3<DataStoreRTreasurebox> treasures = new DataStoreDB3<DataStoreRTreasurebox>();
        public DataStoreDB3<DataStoreSearchItem> searchOrig = new DataStoreDB3<DataStoreSearchItem>();
        public DataStoreDB3<DataStoreSearchItem> search = new DataStoreDB3<DataStoreSearchItem>();

        public DataStoreDB3<DataStoreRFragment> fragments = new DataStoreDB3<DataStoreRFragment>();
        Dictionary<string, HintData> hintData = new Dictionary<string, HintData>();

        Dictionary<string, FF13_2ItemLocation> itemLocations = new Dictionary<string, FF13_2ItemLocation>();

        Dictionary<string, List<string>> hintsMain = new Dictionary<string, List<string>>();
        Dictionary<string, int> hintsNotesUniqueCount = new Dictionary<string, int>();
        Dictionary<string, int> hintsNotesSharedCount = new Dictionary<string, int>();

        FF13_2AssumedItemPlacementAlgorithm placementAlgo;
        FF13_2ItemPlacementAlgorithm placementAlgoBackup;
        private bool usingBackup = false;

        public ItemPlacementAlgorithm<FF13_2ItemLocation> PlacementAlgo { get => usingBackup ? placementAlgoBackup : placementAlgo; }

        public TreasureRando(RandomizerManager randomizers) : base(randomizers) { }

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
            searchOrig.LoadDB3("13-2", @"\db\resident\searchitem.wdb");
            search.LoadDB3("13-2", @"\db\resident\searchitem.wdb");
            fragments.LoadDB3("13-2", @"\db\resident\_wdbpack.bin\r_fragment.wdb", false);

            itemLocations.Clear();

            Dictionary<string, TreasureData> treasureData = new Dictionary<string, TreasureData>();
            FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
            {
                TreasureData t = new TreasureData(row);
                treasureData.Add(t.ID, t);
            }, FileHelpers.CSVFileHeader.HasHeader);
            treasureData.ForEach(p => itemLocations.Add(p.Key, p.Value));

            Dictionary<string, SearchItemData> searchData = new Dictionary<string, SearchItemData>();
            FileHelpers.ReadCSVFile(@"data\searchItems.csv", row =>
            {
                SearchItemData s = new SearchItemData(row);
                searchData.Add(s.ID, s);
            }, FileHelpers.CSVFileHeader.HasHeader);
            searchData.ForEach(p => itemLocations.Add(p.Key, p.Value));

            hintData.Clear();
            FileHelpers.ReadCSVFile(@"data\hints.csv", row =>
            {
                HintData h = new HintData(row);
                hintData.Add(h.ID, h);
            }, FileHelpers.CSVFileHeader.HasHeader);

            AddTreasure("ran_init_cp", "", 0, "");
            AddTreasure("frg_cmn_hmaa001", "frg_cmn_hmaa001", 1, "");
            AddTreasure("frg_cmn_hmaa002", "frg_cmn_hmaa002", 1, "");
            AddTreasure("key_s_neck", "key_s_neck", 1, "");
            AddTreasure("key_l_knife", "key_l_knife", 1, "");

            // Remove repeatable gil moogle throws
            search.Values.ForEach(s =>
            {
                for (int i = 0; i < 8; i++)
                {
                    if (s.GetItem(i) == "" && s.GetMax(i) == 0 && s.GetRandom(i) > 0)
                        s.SetRandom(i, 0);
                }
            });

            List<string> hintsNotesLocations = hintData.Values.SelectMany(h => h.Areas).ToList();

            placementAlgo = new FF13_2AssumedItemPlacementAlgorithm(itemLocations, hintsNotesLocations, Randomizers, 10);
            placementAlgoBackup = new FF13_2ItemPlacementAlgorithm(itemLocations, hintsNotesLocations, Randomizers, -1);
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

                if (!placementAlgo.Randomize(new List<string>()))
                {
                    usingBackup = true;
                    placementAlgoBackup.Randomize(new List<string>());
                }

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

            if (FF13_2Flags.Stats.InitCP.FlagEnabled)
            {
                treasures["ran_init_cp"].iItemCount = FF13_2Flags.Stats.InitCPAmount.Value;
            }
        }

        private void SaveHints()
        {
            HistoriaCruxRando cruxRando = Randomizers.Get<HistoriaCruxRando>("Historia Crux");
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            TextRando textRando = Randomizers.Get<TextRando>("Text");

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
            search.SaveDB3(@"\db\resident\searchitem.wdb");
        }

        public override HTMLPage GetDocumentation()
        {
            HistoriaCruxRando cruxRando = Randomizers.Get<HistoriaCruxRando>("Historia Crux");
            HTMLPage page = new HTMLPage("Item Locations", "template/documentation.html");

            page.HTMLElements.Add(new Button("document.getElementById(\"itemlocations\").classList.toggle(\"hide3\")", null, "Hide/Show Requirements"));

            page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Requirements", "Mog Level Required" }).ToList(), (new int[] { 30, 25, 30, 15 }).ToList(), itemLocations.Values.Select(t =>
            {
                string itemID = PlacementAlgo.GetLocationItem(t.ID, false).Item1;
                string name = GetItemName(itemID);
                string reqsDisplay = t.Requirements.GetDisplay(GetItemName);
                if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
                    reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
                string location = $"{string.Join("/", itemLocations[t.ID].Areas.Select(s => cruxRando.areaData[s].Name))} - {itemLocations[t.ID].Name}";
                return (new string[] { location, $"{name} x {PlacementAlgo.GetLocationItem(t.ID, false).Item2}", reqsDisplay, GetMogLevelRequiredText(t.MogLevel) }).ToList();
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
            else if (itemID.StartsWith("frg"))
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

        private string GetMogLevelRequiredText(int level)
        {
            switch (level)
            {
                case 0:
                    return "0 - None";
                case 1:
                    return "1 - Moogle Hunt";
                case 2:
                    return "2 - Moogle Throw";
                case 3:
                    return "3 - Advanced Moogle Hunt";
                default:
                    return level.ToString();
            }
        }

        public class TreasureData : FF13_2ItemLocation
        {
            public override string ID { get; }
            public override string Name { get; }
            public override int MogLevel { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override List<string> RequiredAreas { get; }

            public override int Difficulty => throw new NotImplementedException();

            public TreasureData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Areas = row[2].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                MogLevel = int.Parse(row[3]);
                RequiredAreas = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = ItemReq.Parse(row[5]);
                Traits = row[6].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
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

        public class SearchItemData : FF13_2ItemLocation
        {
            public override string ID { get; }
            public int Index { get; }
            public override string Name { get; }
            public override int MogLevel { get; }
            public override ItemReq Requirements { get; }
            public override List<string> Traits { get; }
            public override List<string> Areas { get; }
            public override List<string> RequiredAreas { get; }

            public override int Difficulty => throw new NotImplementedException();

            public SearchItemData(string[] row)
            {
                ID = row[0] + ":" + row[1];
                Index = int.Parse(row[1]);
                Name = row[2];
                Areas = row[3].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                MogLevel = 2;
                RequiredAreas = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = ItemReq.Parse(row[5]);
                Traits = row[6].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            public override bool IsValid(Dictionary<string, int> items)
            {
                if (!Requirements.IsValid(items))
                    return false;
                return true;
            }

            public override void SetData(dynamic obj, string newItem, int newCount)
            {
                DataStoreSearchItem s = (DataStoreSearchItem)obj;
                s.SetItem(Index, newItem);
                s.SetCount(Index, newCount);
                s.SetMax(Index, 1);
            }

            public override Tuple<string, int> GetData(dynamic obj)
            {
                DataStoreSearchItem s = (DataStoreSearchItem)obj;
                int count = s.GetCount(Index);
                int max = s.GetMax(Index);
                return new Tuple<string, int>(s.GetItem(Index), max == 0 ? count : (count * max));
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
