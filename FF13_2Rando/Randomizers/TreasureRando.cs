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

namespace FF13_2Rando;

public partial class TreasureRando : Randomizer
{
    public DataStoreWDB<DataStoreRTreasurebox> treasuresOrig = new();
    public DataStoreWDB<DataStoreRTreasurebox> treasures = new();
    public DataStoreWDB<DataStoreSearchItem> searchOrig = new();
    public DataStoreWDB<DataStoreSearchItem> search = new();

    public DataStoreWDB<DataStoreREventFlag> eventFlagsOrig = new();
    public DataStoreWDB<DataStoreREventFlag> eventFlags = new();

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
        eventFlagsOrig.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_eventflag.wdb", false);
        eventFlags.LoadDB3(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_eventflag.wdb", false);

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

        List<EventFlagData> extraFlags = new List<EventFlagData>();
        FileHelpers.ReadCSVFile(@"data\eventflags.csv", row =>
        {
            EventFlagData e = new(row);
            extraFlags.Add(e);
        }, FileHelpers.CSVFileHeader.HasHeader);

        foreach (var flag in extraFlags)
        {
            AddFlag(eventFlags, flag.ID, GetMaxFlagIndex(eventFlags) + 1);
        }

        // TODO: modifications to also add flags to r_eventflag for treasures being added
        // This then also allows us to modify scripts in such a way that we can check if a treasure is granted
        // Will also need to allow for extra arbitrary event flags for other things (like side quest checks), so might need to maintain a separate csv for that to combine it together.

        // Initial treasures don't need flags as they aren't referenced again in the scripts.
        AddTreasure("ran_init_cp", "", 0, "", false);
        AddTreasure("ran_init_silver", "opt_silver", 10, "", false);

        // Mog level items
        AddTreasure("mog_level_1", "key_mog_level", 1, "");
        AddTreasure("mog_level_2", "key_mog_level", 1, "");
        AddTreasure("mog_level_3", "key_mog_level", 1, "");

        // Other assorted key items etc.
        AddTreasure("frg_cmn_hmaa001", "frg_cmn_hmaa001", 1, "");
        AddTreasure("frg_cmn_hmaa002", "frg_cmn_hmaa002", 1, "");
        AddTreasure("key_s_neck", "key_s_neck", 1, "");
        AddTreasure("key_l_knife", "key_l_knife", 1, "");
        AddTreasure("key_tissue", "key_tissue", 1, "");
        AddTreasure("key_wep_sozai", "key_wep_sozai", 1, "");
        AddTreasure("key_mon_data", "key_mon_data", 1, "");
        AddTreasure("key_kansoku", "key_kansoku", 1, "");
        AddTreasure("key_f_colonel", "key_f_colonel", 1, "");
        AddTreasure("key_f_message", "key_f_message", 1, "");
        AddTreasure("key_yukimi", "key_yukimi", 1, "");
        AddTreasure("tmap_gy", "tmap_gy", 1, "");
        AddTreasure("key_sone_info", "key_sone_info", 1, "");
        AddTreasure("key_y_baggage", "key_y_baggage", 1, "");
        AddTreasure("key_f_proof", "key_f_proof", 1, "");
        AddTreasure("tmap_sn", "tmap_sn", 1, "");
        AddTreasure("opt_snda01_cl", "opt_snda01_cl", 1, "");
        AddTreasure("opt_snda02_gd", "opt_snda02_gd", 1, "");
        AddTreasure("key_gowa_wool", "key_gowa_wool", 1, "");
        AddTreasure("key_nuku_wool", "key_nuku_wool", 1, "");
        AddTreasure("key_moko_wool", "key_moko_wool", 1, "");
        AddTreasure("tmap_gd", "tmap_gd", 1, "");
        AddTreasure("key_access_50", "key_access_50", 1, "");
        AddTreasure("key_access_la", "key_access_la", 1, ""); //skip
        AddTreasure("key_access_52", "key_access_52", 1, ""); //skip
        AddTreasure("key_access_13", "key_access_13", 1, "");
        AddTreasure("tmap_gt", "tmap_gt", 1, "");
        AddTreasure("tmap_ac", "tmap_ac", 1, "");
        AddTreasure("frg_cmn_acea012", "frg_cmn_acea012", 1, "");
        // AddTreasure("just_one_gil", "", 1, ""); - what is this even doing?
        AddTreasure("key_casino_prz", "key_casino_prz", 1, "", false); // skip - vanilla flag?
        AddTreasure("key_chaos_cly", "key_chaos_cly", 1, "", false); // skip - vanilla flag?
        AddTreasure("key_casino_dice", "key_casino_dice", 1, "", false); // skip - vanilla flag?
        AddTreasure("tmap_cs", "tmap_cs", 1, "");
        AddTreasure("cs_chip_00", "cs_chip_00", 1, "", false); // Skip flag - tied to above
        AddTreasure("frg_cmn_vpba001", "frg_cmn_vpba001", 1, "");
        AddTreasure("tmap_vp", "tmap_vp", 1, "");
        AddTreasure("frg_cmn_vpca001", "frg_cmn_vpca001", 1, "");
        AddTreasure("key_acdmycom", "key_acdmycom", 1, "");
        AddTreasure("tmap_bj", "tmap_bj", 1, "");
        AddTreasure("key_behi_fang", "key_behi_fang", 1, "");
        AddTreasure("frg_pzl_bjaa001", "frg_pzl_bjaa001", 1, "");

        // Fragment experimenting
        AddTreasure("frg_cmn_acfa002", "frg_cmn_acfa002", 1, "");
        AddTreasure("frg_cmn_pdxe001", "frg_cmn_pdxe001", 1, "");
        AddTreasure("frg_cmn_pdxe002", "frg_cmn_pdxe002", 1, "");
        AddTreasure("frg_cmn_pdxe003", "frg_cmn_pdxe003", 1, "");
        AddTreasure("frg_cmn_pdxe004", "frg_cmn_pdxe004", 1, "");
        AddTreasure("frg_cmn_pdxe005", "frg_cmn_pdxe005", 1, "");
        AddTreasure("frg_cmn_pdxe006", "frg_cmn_pdxe006", 1, "");
        AddTreasure("frg_cmn_pdxe007", "frg_cmn_pdxe007", 1, "");
        AddTreasure("frg_cmn_pdxe008", "frg_cmn_pdxe008", 1, "");
        AddTreasure("frg_cmn_bjaa001", "frg_cmn_bjaa001", 1, "");
        AddTreasure("frg_cmn_gyaa003", "frg_cmn_gyaa003", 1, "");
        AddTreasure("frg_cmn_snda002", "frg_cmn_snda002", 1, "");
        AddTreasure("frg_cmn_spza001", "frg_cmn_spza001", 1, "");
        AddTreasure("frg_cmn_spza002", "frg_cmn_spza002", 1, "");
        AddTreasure("frg_cmn_spza003", "frg_cmn_spza003", 1, "");
        AddTreasure("frg_cmn_spza004", "frg_cmn_spza004", 1, "");
        AddTreasure("frg_cmn_spza005", "frg_cmn_spza005", 1, "");
        AddTreasure("frg_cmn_clza001", "frg_cmn_clza001", 1, "");
        AddTreasure("frg_cmn_gdza003", "frg_cmn_gdza003", 1, "");
        AddTreasure("frg_cmn_gdza004", "frg_cmn_gdza004", 1, "");
        AddTreasure("frg_cmn_gdza005", "frg_cmn_gdza005", 1, "");
        AddTreasure("frg_cmn_gdza006", "frg_cmn_gdza006", 1, "");
        AddTreasure("frg_cmn_gdza007", "frg_cmn_gdza007", 1, "");
        AddTreasure("frg_cmn_gtca001", "frg_cmn_gtca001", 1, "");
        AddTreasure("frg_cmn_acfa001", "frg_cmn_acfa001", 1, "");
        AddTreasure("frg_cmn_vpca005", "frg_cmn_vpca005", 1, "");
        AddTreasure("frg_cmn_snea001", "frg_cmn_snea001", 1, "");
        AddTreasure("frg_cmn_snea002", "frg_cmn_snea002", 1, "");
        AddTreasure("frg_cmn_snea003", "frg_cmn_snea003", 1, "");
        AddTreasure("frg_cmn_snea004", "frg_cmn_snea004", 1, "");
        AddTreasure("frg_cmn_snea005", "frg_cmn_snea005", 1, "");
        AddTreasure("frg_cmn_snea006", "frg_cmn_snea006", 1, "");
        //AddTreasure("frg_itm_bjba001", "frg_itm_bjba001", 1, "");

        // Artefact experimenting
        AddTreasure("opt_aaea02_sp", "opt_aaea02_sp", 1, "");
        AddTreasure("opt_acea01_gt", "opt_acea01_gt", 1, "");
        AddTreasure("opt_gtca01_aa", "opt_gtca01_aa", 1, "");
        AddTreasure("opt_gwca01_gh", "opt_gwca01_gh", 1, "");
        AddTreasure("opt_gyaa01_gw", "opt_gyaa01_gw", 1, "");
        AddTreasure("opt_hmaa01_bj", "opt_hmaa01_bj", 1, "");

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

    public int GetMaxFlagIndex(DataStoreWDB<DataStoreREventFlag> store)
    {
        // Set 6000 as base index to space apart from existing flags.
        // Even if we assume its 14bit for some wild reason that still gives us up to 8192, loads of headroom.
        return Math.Max(6000, store.Values.Max(r => r.iFlagIndex));
    }

    public void AddTreasure(string newName, string item, int count, string next, bool addFlag = true)
    {
        AddTreasure(treasuresOrig, newName, item, count, next);
        AddTreasure(treasures, newName, item, count, next);
        if (addFlag)
        {
            AddFlag(eventFlags, newName, GetMaxFlagIndex(eventFlags) + 1);
        }
    }

    private void AddFlag(DataStoreWDB<DataStoreREventFlag> database, string name, int id)
    {
        database.Copy(database.Keys[0], name);
        database[name].iFlagIndex = id;
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

            if (cruxRando.rootLocation == null)
            {
                throw new Exception("Cannot randomised due to split root");
            }

            // Scan through fake locations, find gate open locations, update area to be the incoming side of the link rather than outgoing so it updates properly
            foreach (var loc in ItemLocations)
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
                if (ItemLocations.ContainsKey(v.record + ":0"))
                {
                    reqs.Add(new AmountItemReq(v.record, 1));
                }
                // If the link has known requirement, add it
                if (cruxRando.gateData.ContainsKey(v.record))
                {
                    reqs.Add(cruxRando.gateData[v.record].ItemRequirements);

                    if (cruxRando.gateData[v.record].MinMogLevel > 0)
                    {
                        reqs.Add(new AmountItemReq("key_mog_level", cruxRando.gateData[v.record].MinMogLevel));
                    }
                }

                // TODO: Wild artefact requirements
                // Increase amount of required wild artefacts based on depth? 
                // Just grant wild artefacts up front for now at starting time?

                ItemReq finalReq;
                if (reqs.Count == 0)
                {
                    finalReq = new BoolItemReq(true);
                }
                else if (reqs.Count == 1)
                {
                    finalReq = reqs[0];
                }
                else
                {
                    finalReq = new AndItemReq(reqs);
                }

                // TODO: traits: DLC, paradox end, etc?
                // TODO: difficulty, based on depth?
                return new AreaConnection(Generator, v.record, v.sArea, v.sOpenHistoria1.Substring(0, v.sOpenHistoria1.Length - 2), finalReq, new(), 1);
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
            ItemPlacer.Replacements = ItemLocations.Values.ToOrderedSet();
            ItemPlacer.PossibleLocations = ItemLocations.Values.ToOrderedSet();
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
            List<string> gravitonCoreNames = new() { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta" };
            for (var i = 1; i < 8; i++)
            {
                // Graviton core location hints
                var gravitonCoreItemId = $"frg_cmn_gvtn00{i}";
                var gravitonCoreHintTextId = $"$cap_core_0{i}_p1";

                var areaName = "Unknown Location";
                var dateText = "cannot resolve a fixed date";
                var dateTextFixedPrefix = "puts the date at ";
                var accessText = "";
                var accessTextPrefix = " According to our calculations, such a gate exists within ";
                var indexName = gravitonCoreNames[i - 1];
                // TODO: this isn't working currently
                var gravitonCoreRandoLocation = ItemLocations.Where(kvp => kvp.Value.GetItem(false).Value.Item == gravitonCoreItemId).Select(kvp => kvp.Value).FirstOrDefault();
                if (gravitonCoreRandoLocation != null)
                {
                    var treasureArea = gravitonCoreRandoLocation.Areas;
                    var area = treasureArea[0];
                    var areaSplit = area.Split("_");
                    var areaPrefix = areaSplit[1];
                    // Only add the date text if we know exactly where it will end up.
                    if (treasureArea.Count == 1)
                    {
                        var areaTimeMarker = areaSplit[2];
                        if (areaTimeMarker.StartsWith("NA"))
                        {
                            dateText = dateTextFixedPrefix + "??? AF";
                        }
                        else if (HistoriaCruxConstants.DATE_SPECIAL_CASES.ContainsKey(area))
                        {
                            dateText = dateTextFixedPrefix + HistoriaCruxConstants.DATE_SPECIAL_CASES[area] + " AF";
                        }
                        else
                        {
                            dateText = dateTextFixedPrefix + areaTimeMarker.Substring(3) + " AF";
                        }
                        var parent = cruxRando.shuffledNodes[area].parent;
                        while (parent != null && parent.name.Contains("_zz_"))
                        {
                            parent = parent.parent;
                        }
                        if (parent != null)
                        {
                            var parentPrefix = parent.name.Split("_")[1];
                            accessText = accessTextPrefix + HistoriaCruxConstants.AREA_PREFIX_LOOKUP[parentPrefix] + ".";
                        }
                    }
                    areaName = HistoriaCruxConstants.AREA_PREFIX_LOOKUP[treasureArea[0].Split("_")[1]];
                }

                var updatedText = $$"""Graviton Core readings have been detected somewhere in the area of {Color Yellow}{{areaName}}{Color White}.{Text NewLine}{Text NewLine}"""
                    + $$"""Resonance imaging {{dateText}}. To recover the object, you will need to find a Time Gate that connects to this time period.{{accessText}}{Text NewLine}{Text NewLine}"""
                    + $$"""We have designated the target {Color IceBlue}Graviton Core {{indexName}}{Color White}. Travel the timeline and bring it back.""";

                textRando.mainSysUS[gravitonCoreHintTextId] = updatedText;
            }
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Treasure Data...");
        SaveHints();
        treasures.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        eventFlags.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_eventflag.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_eventflag.wdb");
        search.SaveDB3(Generator, @"\db\resident\searchitem.wdb");
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        HTMLPage page = new("Item Locations", "template/documentation.html");

        // TODO: add sphere depth to locations
        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Sphere" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), ItemLocations.Values
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
            var sphere = ItemPlacer != null && ItemPlacer.SphereCalculator.Spheres.ContainsKey(t) ? ItemPlacer.SphereCalculator.Spheres[t].ToString() : "N/A";
            return (new object[] { nameCell, $"{name} x {ItemLocations[t.ID].GetItem(false).Value.Item2}", sphere }).ToList();
        }).ToList(), "itemlocations"));

        pages.Add("item_locations", page);
        return pages;
    }

    private string GetItemName(string itemID)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        BattleRando battleRando = Generator.Get<BattleRando>();
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if (itemID == "")
        {
            name = "Gil";
        }
        else if (itemID.StartsWith("frg") && fragments.Keys.Contains(itemID))
        {
            name = textRando.mainSysUS[fragments[itemID].sNameStringId];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }
        else
        {
            try
            {
                name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId];
                if (name.Contains("{End}"))
                {
                    name = name.Substring(0, name.IndexOf("{End}"));
                }
            }
            catch
            {
                Generator.Logger.LogDebug($"Cannot resolve proper name for item with id {itemID}");
                name = itemID;
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
