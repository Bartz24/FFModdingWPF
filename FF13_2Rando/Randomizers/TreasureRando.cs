using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using FF13_2Rando.Logic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace FF13_2Rando;

public partial class TreasureRando : Randomizer
{
    public DataStoreWDB<DataStoreRTreasurebox> treasuresOrig = new();
    public DataStoreWDB<DataStoreRTreasurebox> treasures = new();
    public DataStoreWDB<DataStoreSearchItem> searchOrig = new();
    public DataStoreWDB<DataStoreSearchItem> search = new();

    public DataStoreWDB<DataStoreRFragment> fragments = new();
    private readonly Dictionary<string, HintData> hintData = new();
    private readonly Dictionary<string, FF13_2ItemLocation> ItemLocations = new();
    private readonly Dictionary<string, List<string>> hintsMain = new();
    private readonly Dictionary<string, int> hintsNotesUniqueCount = new();
    private readonly Dictionary<string, int> hintsNotesSharedCount = new();

    public FF13_2ItemPlacer ItemPlacer { get; set; }
    private bool usingBackup = false;

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Treasure Data...");
        treasuresOrig.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        treasures.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        searchOrig.LoadDB3(Generator, "13-2", @"\db\resident\searchitem.wdb");
        search.LoadDB3(Generator, "13-2", @"\db\resident\searchitem.wdb");
        fragments.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_fragment.wdb", false);

        ItemLocations.Clear();

        Dictionary<string, TreasureData> treasureData = new();
        FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
        {
            TreasureData t = new(Generator, row);
            treasureData.Add(t.ID, t);
        }, FileHelpers.CSVFileHeader.HasHeader);
        treasureData.ForEach(p => ItemLocations.Add(p.Key, p.Value));

        Dictionary<string, SearchItemData> searchData = new();
        FileHelpers.ReadCSVFile(@"data\searchItems.csv", row =>
        {
            SearchItemData s = new(Generator, row);
            searchData.Add(s.ID, s);
        }, FileHelpers.CSVFileHeader.HasHeader);
        searchData.ForEach(p => ItemLocations.Add(p.Key, p.Value));

        FileHelpers.ReadCSVFile(@"data\fakeChecks.csv", row =>
        {
            string[] fakeItems = row[7].Split('|');
            for (int i = 0; i < fakeItems.Length; i++)
            {
                string fakeItem = fakeItems[i];
                int amount = 1;
                if (fakeItem.Contains("*"))
                {
                    amount = int.Parse(fakeItem.Split('*')[1]);
                    fakeItem = fakeItem.Split('*')[0];
                }

                FF13_2FakeItemLocation f = new(Generator, row, fakeItem, amount);
                f.ID = f.ID + ":" + i;
                ItemLocations.Add(f.ID, f);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        hintData.Clear();
        FileHelpers.ReadCSVFile(@"data\hints.csv", row =>
        {
            HintData h = new(row);
            hintData.Add(h.ID, h);
        }, FileHelpers.CSVFileHeader.HasHeader);

        AddTreasure("ran_init_cp", "", 0, "");
        AddTreasure("frg_cmn_hmaa001", "frg_cmn_hmaa001", 1, "");
        AddTreasure("frg_cmn_hmaa002", "frg_cmn_hmaa002", 1, "");
        AddTreasure("key_s_neck", "key_s_neck", 1, "");
        AddTreasure("key_l_knife", "key_l_knife", 1, "key_opt_silver");
        // Just grant wild artefacts here for now for clearance purposes?
        // This adds to the pool so now you have so many. so so many.
        AddTreasure("key_opt_silver", "opt_silver", 10, "");

        // Remove repeatable gil moogle throws
        search.Values.ForEach(s =>
        {
            for (int i = 0; i < 8; i++)
            {
                if (s.GetItem(i) == "" && s.GetMax(i) == 0 && s.GetRandom(i) > 0)
                {
                    s.SetRandom(i, 0);
                }
            }
        });

        List<string> hintsNotesLocations = hintData.Values.SelectMany(h => h.Areas).ToList();
    }

    public void AddTreasure(string newName, string item, int count, string next)
    {
        AddTreasure(treasuresOrig, newName, item, count, next);
        AddTreasure(treasures, newName, item, count, next);
    }

    private void AddTreasure(DataStoreWDB<DataStoreRTreasurebox> database, string newName, string item, int count, string next)
    {
        database.Copy(database.Keys[0], newName);
        database[newName].s11ItemResourceId = item;
        database[newName].s8NextTreasureBoxResourceId = next;
        database[newName].iItemCount = count;
    }

    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Treasure Data...");
        if (FF13_2Flags.Items.Treasures.FlagEnabled)
        {
            FF13_2Flags.Items.Treasures.SetRand();
            HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();

            if(cruxRando.rootLocation == null)
            {
                throw new Exception("Cannot randomised due to split root");
            }

            // Scan through fake locations, find gate open locations, update area to be the incoming side of the link rather than outgoing so it updates properly
            foreach(var loc in ItemLocations)
            {
                if (loc.Value.Traits.Contains("Gate"))
                {
                    var gateId = loc.Value.ID.Split(":")[0];
                    var target = cruxRando.gateTable[gateId].sOpenHistoria1;
                    var source = cruxRando.gateTable[gateId].sArea;
                    loc.Value.Areas = new() { source };
                    Generator.Logger.LogDebug($"Updating gate location with id {gateId} (links {source} -> {target}) to have an area of {source}");
                }
            }



            AreaGraph areaGraph = new(Generator);
            areaGraph.Areas = cruxRando.areaData.ToDictionary(kvp => kvp.Value.ID, kvp => new Area([kvp.Value.ID]));
            areaGraph.Connections = cruxRando.gateTable.Values.Select(v =>
            {
                // All connections in the crux are one-way so no need to construct reverse links
                // Ensure any hard item requirements are fulfilled as well as the outgoing link id for artefact tracking
                List<ItemReq> reqs = new();
                // If the fake check location has been setup, include it in logic (should all be there now from fake checks)
                if (ItemLocations.ContainsKey(v.record+":0"))
                {
                    reqs.Add(new AmountItemReq(v.record, 1));
                }
                // If the link has a known requirement, add it
                if (cruxRando.gateData.ContainsKey(v.record))
                {
                    reqs.Add(cruxRando.gateData[v.record].ItemRequirements);
                }

                // TODO: Wild artefact requirements
                // Increase amount of required wild artefacts based on depth? 
                // Just grant wild artefacts up front for now at starting time?

                ItemReq finalReq;
                if(reqs.Count == 0)
                {
                    finalReq = new BoolItemReq(true);
                } else if (reqs.Count == 1)
                {
                    finalReq = reqs[0];
                } else
                {
                    finalReq = new AndItemReq(reqs);
                }

                // TODO: traits: DLC, paradox end, etc?
                // TODO: difficulty, based on depth?
                return new AreaConnection(Generator, v.record, v.sArea, v.sOpenHistoria1.Substring(0,v.sOpenHistoria1.Length - 2), finalReq, new(), 1);
            }).Append(
                // Ensure initial actually connects to something...
                // TODO: Add DLC checks if enabled
                new AreaConnection(Generator, "Initial", "Initial", cruxRando.rootLocation, new BoolItemReq(true), new(), 0)
                ).ToList();

            // Make sure this got built up correctly...
            areaGraph.VerifyIntegrity();

            // Sphere calc isn't working, step through and figure out why...
            // Looks like area opening isn't working (despite fake checks now existing?)

            ItemPlacer = new(Generator, areaGraph);
            ItemPlacer.Replacements = ItemLocations.Values.ToHashSet();
            ItemPlacer.PossibleLocations = ItemLocations.Values.ToHashSet();
            ItemPlacer.PlaceItems();
            ItemPlacer.ApplyToGameData();

            RandomNum.ClearRand();

        }

        if (FF13_2Flags.Stats.InitCP.FlagEnabled)
        {
            treasures["ran_init_cp"].iItemCount = FF13_2Flags.Stats.InitCPAmount.Value;
        }
    }

    private void SaveHints()
    {
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();

        if (FF13_2Flags.Items.Treasures.FlagEnabled)
        {
            hintData.Values.ForEach(h =>
            {
                // TODO: what should hints be now?
                // textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId] = "";
                // Ignore hints for now
                //h.Areas.ForEach(a =>
                //{
                //    if (hintsNotesSharedCount[a] > 0)
                //    {
                //        textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId] += $"{cruxRando.areaData[a].Name} has {hintsNotesUniqueCount[a]} unique important checks and {hintsNotesSharedCount[a]} shared with other time periods.";
                //    }
                //    else
                //    {
                //        textRando.mainSysUS[equipRando.items[h.ID].sHelpStringId] += $"{cruxRando.areaData[a].Name} has {hintsNotesUniqueCount[a]} unique important checks.";
                //    }
                //});
            });
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Treasure Data...");
        SaveHints();
        treasures.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        search.SaveDB3(Generator, @"\db\resident\searchitem.wdb");
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        HTMLPage page = new("Item Locations", "template/documentation.html");

        // TODO: add sphere depth to locations
        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents","Sphere" }).ToList(), (new int[] { 45, 45,10 }).ToList(), ItemLocations.Values
            .Where(v => v is not FF13_2FakeItemLocation).Select(t =>
        {
            string itemID = ItemLocations[t.ID].GetItem(false).Value.Item1;
            string name = GetItemName(itemID);
            string reqsDisplay = t.Requirements.GetDisplay(GetItemName);
            if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
            {
                reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
            }

            string location = $"{string.Join("/", ItemLocations[t.ID].Areas.Select(s => cruxRando.areaData[s].Name))} - {ItemLocations[t.ID].Name}";

            TableCellMultiple nameCell = new(new List<string>());
            nameCell.Elements.Add($"<div style=\"margin-right: auto\">{location}</div>");
            if (reqsDisplay != ItemReq.TRUE.GetDisplay() || t.MogLevel > 0)
            {
                string disp = "";
                if (reqsDisplay != ItemReq.TRUE.GetDisplay())
                {
                    disp += "Requires: " + reqsDisplay;
                    if (t.MogLevel > 0)
                    {
                        disp += "<br>";
                    }
                }

                if (t.MogLevel > 0)
                {
                    disp += "Mog Level: " + GetMogLevelRequiredText(t.MogLevel);
                }

                nameCell.Elements.Add(new IconTooltip("common/images/lock_white_48dp.svg", disp).ToString());
            }

            return (new object[] { nameCell, $"{name} x {ItemLocations[t.ID].GetItem(false).Value.Item2}", ItemPlacer.SphereCalculator.Spheres.ContainsKey(t) ? ItemPlacer.SphereCalculator.Spheres[t] : "N/A" }).ToList();
        }).ToList(), "itemlocations"));

        pages.Add("item_locations", page);
        return pages;
    }

    private string GetItemName(string itemID)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if (itemID == "")
        {
            name = "Gil";
        }
        else if (itemID.StartsWith("frg"))
        {
            name = textRando.mainSysUS[fragments[itemID].sNameStringId];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }
        else
        {
            name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }

        return name;
    }

    private string GetMogLevelRequiredText(int level)
    {
        return level switch
        {
            0 => "0 - None",
            1 => "1 - Moogle Hunt",
            2 => "2 - Moogle Throw",
            3 => "3 - Advanced Moogle Hunt",
            _ => level.ToString(),
        };
    }
}
