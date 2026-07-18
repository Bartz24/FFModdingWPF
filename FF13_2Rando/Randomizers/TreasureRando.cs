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
    public readonly Dictionary<string, FF13_2ItemLocation> ItemLocations = new();
    private readonly Dictionary<string, List<string>> hintsMain = new();
    private readonly Dictionary<string, int> hintsNotesUniqueCount = new();
    private readonly Dictionary<string, int> hintsNotesSharedCount = new();

    private Dictionary<string, DataStoreWDB<DataStoreZoneScript>> zoneScriptTables = new();

    public FF13_2ItemPlacer ItemPlacer { get; set; }
    private bool usingBackup = false;

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Treasure Data...");
        treasuresOrig.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        treasures.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        searchOrig.LoadWDB(Generator, "13-2", @"\db\resident\searchitem.wdb");
        search.LoadWDB(Generator, "13-2", @"\db\resident\searchitem.wdb");
        fragments.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_fragment.wdb", false);
        eventFlagsOrig.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_eventflag.wdb", false);
        eventFlags.LoadWDB(Generator, "13-2", @"\db\resident\_wdbpack.bin\r_eventflag.wdb", false);

        treasures.BitsPerOffset = 16;
        treasuresOrig.BitsPerOffset = 16;
        // May be needed on search item or event flags - do some research

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
        if (FF13_2Flags.Stats.InitCP.FlagEnabled)
        {
            treasures["ran_init_cp"].iItemCount = FF13_2Flags.Stats.InitCPAmount.Value;
        }

        if (FF13_2Flags.Items.ReplaceWildArtefacts.Enabled)
        {
            AddTreasure("ran_init_silver", "opt_silver", 0, "", false);
        }
        else
        {
            AddTreasure("ran_init_silver", "opt_silver", 10, "", false);
        }

        AddTreasure("ran_init_shop", "key_shop_level", FF13_2Flags.Items.InitialShopLevel.Value, "", false);

        // Must be applied IN THIS ORDER
        AddTreasure("ran_mfind", "rando_findstrin", 32658, "", false);
        AddTreasure("ran_multi", RandoFlags.Mode == RandoFlags.SeedMode.Archipelago ? "rando_multiitem" : "disabled", 32500, "", false);

        // AddTreasure("victory", "key_r_victory", 1, "");

        // Mog level items
        AddTreasure("mog_level_1", "key_mog_level", 1, "");
        AddTreasure("mog_level_2", "key_mog_level", 1, "");
        AddTreasure("mog_level_3", "key_mog_level", 1, "");

        // TODO: does this need adjusting based on initial shop level count?
        // Shop levels (TODO: flag to disable)
        AddTreasure("shop_level_01", "key_shop_level", 1, "");
        AddTreasure("shop_level_02", "key_shop_level", 1, "");
        AddTreasure("shop_level_03", "key_shop_level", 1, "");
        AddTreasure("shop_level_04", "key_shop_level", 1, "");
        AddTreasure("shop_level_05", "key_shop_level", 1, "");
        AddTreasure("shop_level_06", "key_shop_level", 1, "");
        AddTreasure("shop_level_07", "key_shop_level", 1, "");
        AddTreasure("shop_level_08", "key_shop_level", 1, "");
        AddTreasure("shop_level_09", "key_shop_level", 1, "");
        AddTreasure("shop_level_10", "key_shop_level", 1, "");
        AddTreasure("shop_level_11", "key_shop_level", 1, "");

        for(int i = 1; i < 12; i++)
        {
            // Replace "spent" shop levels with items for junk placement stuff
            if(FF13_2Flags.Items.InitialShopLevel.Value >= i)
            {
                treasures[string.Format("zshop_level_{0:D2}", i)].s11ItemResourceId = "it_potion";
                treasuresOrig[string.Format("zshop_level_{0:D2}", i)].s11ItemResourceId = "it_potion";
            }
        }

        // TODO: length restriction stuff

        // Other assorted key items etc.
        AddTreasure("frgcmn_hmaa001", "frg_cmn_hmaa001", 0, "");
        AddTreasure("frgcmn_hmaa002", "frg_cmn_hmaa002", 0, "");
        AddTreasure("key_s_neck", "key_s_neck", 1, "");
        AddTreasure("key_l_knife", "key_l_knife", 1, "");
        AddTreasure("key_tissue", "key_tissue", 1, "");
        // Already has a chest??
        // AddTreasure("key_wep_sozai", "key_wep_sozai", 1, "");
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
        //AddTreasure("key_access_la", "key_access_la", 1, ""); //skip
        //AddTreasure("key_access_52", "key_access_52", 1, ""); //skip
        AddTreasure("key_access_13", "key_access_13", 1, "");
        AddTreasure("tmap_gt", "tmap_gt", 1, "");
        AddTreasure("tmap_ac", "tmap_ac", 1, "");
        AddTreasure("frgcmn_acea012", "frg_cmn_acea012", 0, "");
        // AddTreasure("just_one_gil", "", 1, ""); - what is this even doing?
        //AddTreasure("key_casino_prz", "key_casino_prz", 1, "", false); // skip - vanilla flag?
        //AddTreasure("key_chaos_cly", "key_chaos_cly", 1, "", false); // skip - vanilla flag?
        //AddTreasure("key_casino_dice", "key_casino_dice", 1, "", false); // skip - vanilla flag?
        AddTreasure("tmap_cs", "tmap_cs", 1, "");
        AddTreasure("cs_chip_00", "cs_chip_00", 1, ""); // Skip flag - tied to above
        AddTreasure("frgcmn_vpba001", "frg_cmn_vpba001", 0, "");
        AddTreasure("tmap_vp", "tmap_vp", 1, "");
        AddTreasure("frgcmn_vpca001", "frg_cmn_vpca001", 0, "");
        AddTreasure("key_acdmycom", "key_acdmycom", 1, "");
        AddTreasure("tmap_bj", "tmap_bj", 1, "");
        AddTreasure("key_behi_fang", "key_behi_fang", 1, "");
        AddTreasure("frgpzl_bjaa001", "frg_pzl_bjaa001", 1, "");

        // flg_bjaa_01_010

        // Fragment experimenting
        AddTreasure("frgcmn_acfa002", "frg_cmn_acfa002", 0, "");
        AddTreasure("frgcmn_pdxe001", "frg_cmn_pdxe001", 0, "");
        AddTreasure("frgcmn_pdxe002", "frg_cmn_pdxe002", 0, "");
        AddTreasure("frgcmn_pdxe003", "frg_cmn_pdxe003", 0, "");
        AddTreasure("frgcmn_pdxe004", "frg_cmn_pdxe004", 0, "");
        AddTreasure("frgcmn_pdxe005", "frg_cmn_pdxe005", 0, "");
        AddTreasure("frgcmn_pdxe006", "frg_cmn_pdxe006", 0, "");
        AddTreasure("frgcmn_pdxe007", "frg_cmn_pdxe007", 0, "");
        AddTreasure("frgcmn_pdxe008", "frg_cmn_pdxe008", 0, "");
        AddTreasure("frgcmn_bjaa001", "frg_cmn_bjaa001", 0, "");
        AddTreasure("frgcmn_gyaa003", "frg_cmn_gyaa003", 0, "");
        AddTreasure("frgcmn_snda002", "frg_cmn_snda002", 0, "");
        AddTreasure("frgcmn_spza001", "frg_cmn_spza001", 0, "");
        AddTreasure("frgcmn_spza002", "frg_cmn_spza002", 0, "");
        AddTreasure("frgcmn_spza003", "frg_cmn_spza003", 0, "");
        AddTreasure("frgcmn_spza004", "frg_cmn_spza004", 0, "");
        AddTreasure("frgcmn_spza005", "frg_cmn_spza005", 0, "");
        AddTreasure("frgcmn_clza001", "frg_cmn_clza001", 0, "");
        AddTreasure("frgcmn_gdza003", "frg_cmn_gdza003", 0, "");
        AddTreasure("frgcmn_gdza004", "frg_cmn_gdza004", 0, "");
        AddTreasure("frgcmn_gdza005", "frg_cmn_gdza005", 0, "");
        AddTreasure("frgcmn_gdza006", "frg_cmn_gdza006", 0, "");
        AddTreasure("frgcmn_gdza007", "frg_cmn_gdza007", 0, "");
        AddTreasure("frgcmn_gtca001", "frg_cmn_gtca001", 0, "");
        AddTreasure("frgcmn_acfa001", "frg_cmn_acfa001", 0, "");
        AddTreasure("frgcmn_vpca005", "frg_cmn_vpca005", 0, "");
        AddTreasure("frgcmn_snea001", "frg_cmn_snea001", 0, "");
        AddTreasure("frgcmn_snea002", "frg_cmn_snea002", 0, "");
        AddTreasure("frgcmn_snea003", "frg_cmn_snea003", 0, "");
        AddTreasure("frgcmn_snea004", "frg_cmn_snea004", 0, "");
        AddTreasure("frgcmn_snea005", "frg_cmn_snea005", 0, "");
        AddTreasure("frgcmn_snea006", "frg_cmn_snea006", 0, "");
        //AddTreasure("frg_itm_bjba001", "frg_itm_bjba001", 1, "");

        AddTreasure("frgcmn_acea001", "frg_cmn_acea001", 0, "");
        AddTreasure("frgcmn_acea002", "frg_cmn_acea002", 0, "");
        AddTreasure("frgcmn_acea003", "frg_cmn_acea003", 0, "");
        AddTreasure("frgcmn_acea004", "frg_cmn_acea004", 0, "");
        AddTreasure("frgcmn_acea005", "frg_cmn_acea005", 0, "");
        AddTreasure("frgcmn_acea006", "frg_cmn_acea006", 0, "");
        AddTreasure("frgcmn_acea007", "frg_cmn_acea007", 0, "");
        AddTreasure("frgcmn_acea008", "frg_cmn_acea008", 0, "");
        AddTreasure("frgcmn_acea009", "frg_cmn_acea009", 0, "");
        AddTreasure("frgcmn_acea010", "frg_cmn_acea010", 0, "");
        AddTreasure("frgcmn_acea011", "frg_cmn_acea011", 0, "");
        AddTreasure("frgcmn_hmha001", "frg_cmn_hmha001", 0, "");
        AddTreasure("frgcmn_hmha002", "frg_cmn_hmha002", 0, "");
        AddTreasure("frgcmn_hmha003", "frg_cmn_hmha003", 0, "");
        AddTreasure("frgcmn_ddha001", "frg_cmn_ddha001", 0, "");

        // Artefact experimenting
        AddTreasure("opt_aaea02_sp", "opt_aaea02_sp", 1, "");
        AddTreasure("opt_acea01_gt", "opt_acea01_gt", 1, "");
        AddTreasure("opt_gtca01_aa", "opt_gtca01_aa", 1, "");
        AddTreasure("opt_gwca01_gh", "opt_gwca01_gh", 1, "");
        AddTreasure("opt_gyaa01_gw", "opt_gyaa01_gw", 1, "");
        AddTreasure("opt_hmaa01_bj", "opt_hmaa01_bj", 1, "");

        // Fragment skills
        AddTreasure("privilege01", "privilege01", 1, "");
        AddTreasure("privilege02", "privilege02", 1, "");
        AddTreasure("privilege03", "privilege03", 1, "");
        AddTreasure("privilege04", "privilege04", 1, "");
        AddTreasure("privilege05", "privilege05", 1, "");
        AddTreasure("privilege06", "privilege06", 1, "");
        AddTreasure("privilege08", "privilege08", 1, "");
        AddTreasure("privilege10", "privilege10", 1, "");
        AddTreasure("privilege11", "privilege11", 1, "");
        AddTreasure("privilege12", "privilege12", 1, "");
        AddTreasure("privilege14", "privilege14", 1, "");
        AddTreasure("privilege15", "privilege15", 1, "");
        AddTreasure("privilege18", "privilege18", 1, "");

        /**
         * Meta-treasures used for "dynamic" scripting
         */

        int puzzle0maxStage = 10;
        int puzzle1maxStage = 10;
        int puzzle2maxStage = 10;
        int puzzle2maxClock = 5;
        int puzzle2time = 1;

        if (FF13_2Flags.Other.PuzzleQol.FlagEnabled)
        {
            puzzle0maxStage = FF13_2Flags.Other.Puzzle0StageCount.Value;
            puzzle1maxStage = FF13_2Flags.Other.Puzzle1StageCount.Value;
            puzzle2maxStage = FF13_2Flags.Other.Puzzle2StageCount.Value;
            puzzle2maxClock = FF13_2Flags.Other.Puzzle2MaxSize.Value;
            // this is 0 index but the script expect 1 index
            puzzle2time = FF13_2Flags.Other.Puzzle2TimeBehaviour.SelectedIndex + 1;
        }

        //Maximum number of stages allowed for puzzles
        AddTreasure("ran_puz0_maxstg", "", puzzle0maxStage, "", false, true);
        AddTreasure("ran_puz1_maxstg", "", puzzle1maxStage, "", false, true);
        AddTreasure("ran_puz2_maxstg", "", puzzle2maxStage, "", false, true);
        // Cap on number of clocks for clock puzzles
        AddTreasure("ran_puz2_max", "", puzzle2maxClock, "", false, true);
        // Time behaviour for clock puzzles (0 = unlimited, 1 = vanilla, 2 = 2x)
        AddTreasure("ran_puz2_time", "", puzzle2time, "", false, true);

        var wincodition = setupWinCondition();
        // Win condition (0 = no special, 1 = fragments, 2 = areas?)
        AddTreasure("ran_win_cond", "", wincodition.Item1, "", false, true);
        // Number of fragments/areas
        AddTreasure("ran_win_con_ct", "", wincodition.Item2, "", false, true);
        // 1 = require final bosses. 2 = don't
        AddTreasure("ran_win_cond_fb", "", wincodition.Item3, "", false, true);

        if(wincodition.Item1 == 1)
        {
            ItemLocations["final_boss_access:0"].Requirements = new TraitAmountItemReq("Fragment", wincodition.Item2);
        }

        if (FF13_2Flags.Items.ReplaceWildArtefacts.Enabled)
        {
            replaceWildArtefactsWithCustom();
        }

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

        // TODO Testing
        //treasures.BitsPerOffset = 16;
        for (int i = 0; i < 800; i++)
        {
            AddTreasure($"zabc_t{i:000}", $"item_{i:000}", 1, "");
        }
    }

    protected virtual (int, int, int) setupWinCondition()
    {
        int winCondition = 0;
        int winConditionCount = 0;
        int winConditionRequireFinal = 1;

        if (FF13_2Flags.Other.WinCondition.FlagEnabled)
        {
            winCondition = FF13_2Flags.Other.WinConditionType.SelectedIndex;
            winConditionCount = FF13_2Flags.Other.WinConditionFragCount.Value;
            winConditionRequireFinal = FF13_2Flags.Other.WinConditionRequireFinalBosses.Enabled ? 1 : 2;
        }
        return (winCondition, winConditionCount, winConditionRequireFinal);
    }

    private string[] wildZoneNums = [];

    private void replaceWildArtefactsWithCustom()
    {
        Dictionary<string, string> zoneToArtefactMap = new Dictionary<string, string>();
        // setup fake items (equip rando)
        // replace vanilla wild artefact treasures accordingly
        treasures["tre_bjaa_opts1"].s11ItemResourceId = "opt_bjaa03_bj";
        zoneToArtefactMap.Add("00020", "opt_bjaa03_bj");
        treasures["tre_bjda_opts1"].s11ItemResourceId = "opt_bjba01_gy";
        zoneToArtefactMap.Add("00023", "opt_bjba01_gy");
        treasures["tre_gyba_opts1"].s11ItemResourceId = "opt_gyba01_sn";
        zoneToArtefactMap.Add("00031", "opt_bjba01_gy");
        treasures["tre_gwca_opt01"].s11ItemResourceId = "opt_gwda01_gw";
        zoneToArtefactMap.Add("00053", "opt_gwda01_gw");
        treasures["tre_snda_opts1"].s11ItemResourceId = "opt_acea02_gy";
        zoneToArtefactMap.Add("00114", "opt_acea02_gy");
        treasures["tre_gdza_opts1"].s11ItemResourceId = "opt_gdaa01_vp";
        zoneToArtefactMap.Add("00080", "opt_gdaa01_vp");
        treasures["tre_aaea_opts1"].s11ItemResourceId = "opt_aaea03_vp";
        zoneToArtefactMap.Add("00204", "opt_aaea03_vp");
        treasures["tre_gtca_opts1"].s11ItemResourceId = "opt_gtca02_gw";
        zoneToArtefactMap.Add("00092", "opt_gtca02_gw");
        treasures["tre_csza_002"].s11ItemResourceId = "opt_ghaa01_gt";
        zoneToArtefactMap.Add("00180", "opt_ghaa01_gt");
        treasures["tre_ddha_opts1"].s11ItemResourceId = "opt_ddha01_bj";
        zoneToArtefactMap.Add("00157", "opt_ddha01_bj");

        wildZoneNums = zoneToArtefactMap.Keys.ToArray();
        treasuresOrig["tre_bjaa_opts1"].s11ItemResourceId = "opt_bjaa03_bj";
        treasuresOrig["tre_bjda_opts1"].s11ItemResourceId = "opt_bjba01_gy";
        treasuresOrig["tre_gyba_opts1"].s11ItemResourceId = "opt_gyba01_sn";
        treasuresOrig["tre_gwca_opt01"].s11ItemResourceId = "opt_gwda01_gw";
        treasuresOrig["tre_snda_opts1"].s11ItemResourceId = "opt_acea02_gy";
        treasuresOrig["tre_gdza_opts1"].s11ItemResourceId = "opt_gdaa01_vp";
        treasuresOrig["tre_aaea_opts1"].s11ItemResourceId = "opt_aaea03_vp";
        treasuresOrig["tre_gtca_opts1"].s11ItemResourceId = "opt_gtca02_gw";
        treasuresOrig["tre_csza_002"].s11ItemResourceId = "opt_ghaa01_gt";
        treasuresOrig["tre_ddha_opts1"].s11ItemResourceId = "opt_ddha01_bj";
        // update requirements on wild gates accordingly for each treasure (replace opt_silver requirement with item
        ItemLocations["hs_aaea01_vp:0"].Requirements = new AmountItemReq("opt_aaea03_vp", 1);
        ItemLocations["hs_acea02_gy:0"].Requirements = new AmountItemReq("opt_acea02_gy", 1);
        ItemLocations["hs_bjaa03_bj:0"].Requirements = new AndItemReq([new AmountItemReq("opt_bjaa03_bj", 1), new AmountItemReq("key_lockjail", 1)]);
        ItemLocations["hs_bjda01_gy:0"].Requirements = new AmountItemReq("opt_bjba01_gy", 1);
        ItemLocations["hs_ddha02_bj:0"].Requirements = new AmountItemReq("opt_ddha01_bj", 1);
        ItemLocations["hs_gdza01_vp:0"].Requirements = new AndItemReq([new AmountItemReq("opt_gdaa01_vp", 1), new AmountItemReq("boss_faeryl", 1)]);
        ItemLocations["hs_ghaa02_gt:0"].Requirements = new AmountItemReq("opt_ghaa01_gt", 1);
        ItemLocations["hs_gtca02_gw:0"].Requirements = new AndItemReq([new AmountItemReq("opt_gtca02_gw", 1), new AmountItemReq("key_access_la", 1)]);
        ItemLocations["hs_gwda01_gw:0"].Requirements = new AmountItemReq("opt_gwda01_gw", 1);
        ItemLocations["hs_gyba01_sn:0"].Requirements = new AmountItemReq("opt_gyba01_sn", 1);
        // update gate table with custom artefact name
        HistoriaCruxRando hisRand = Generator.Get<HistoriaCruxRando>();
        hisRand.gateTable["hs_aaea01_vp"].sOopartsName = "opt_aaea03_vp";
        hisRand.gateTable["hs_aaea01_vp"].sGateRelationItem0 = "opt_aaea03_vp";
        hisRand.gateTable["hs_acea02_gy"].sOopartsName = "opt_acea02_gy";
        hisRand.gateTable["hs_acea02_gy"].sGateRelationItem0 = "opt_acea02_gy";
        hisRand.gateTable["hs_bjaa03_bj"].sOopartsName = "opt_bjaa03_bj";
        hisRand.gateTable["hs_bjaa03_bj"].sGateRelationItem0 = "opt_bjaa03_bj";
        hisRand.gateTable["hs_bjda01_gy"].sOopartsName = "opt_bjba01_gy";
        hisRand.gateTable["hs_bjda01_gy"].sGateRelationItem0 = "opt_bjba01_gy";
        hisRand.gateTable["hs_ddha02_bj"].sOopartsName = "opt_ddha01_bj";
        hisRand.gateTable["hs_ddha02_bj"].sGateRelationItem0 = "opt_ddha01_bj";
        hisRand.gateTable["hs_gdza01_vp"].sOopartsName = "opt_gdaa01_vp";
        hisRand.gateTable["hs_gdza01_vp"].sGateRelationItem0 = "opt_gdaa01_vp";
        hisRand.gateTable["hs_ghaa02_gt"].sOopartsName = "opt_ghaa01_gt";
        hisRand.gateTable["hs_ghaa02_gt"].sGateRelationItem0 = "opt_ghaa01_gt";
        hisRand.gateTable["hs_gtca02_gw"].sOopartsName = "opt_gtca02_gw";
        hisRand.gateTable["hs_gtca02_gw"].sGateRelationItem0 = "opt_gtca02_gw";
        hisRand.gateTable["hs_gwda01_gw"].sOopartsName = "opt_gwda01_gw";
        hisRand.gateTable["hs_gwda01_gw"].sGateRelationItem0 = "opt_gwda01_gw";
        hisRand.gateTable["hs_gyba01_sn"].sOopartsName = "opt_gyba01_sn";
        hisRand.gateTable["hs_gyba01_sn"].sGateRelationItem0 = "opt_gyba01_sn";


        // change zone script table to point at sfRandoGateBase / sfRandoGateFailBase and add custom item name as argument
        foreach(string zoneNum in wildZoneNums)
        {
            DataStoreWDB<DataStoreZoneScript> zoneScripts = new DataStoreWDB<DataStoreZoneScript>();
            zoneScripts.LoadWDB(Generator, "13-2", $@"\db\script\script{zoneNum}.wdb");
            foreach(var scriptEntryKey in zoneScripts.Keys)
            {
                var scriptBody = zoneScripts[scriptEntryKey];
                if (scriptBody.sClassName == "cmn/common")
                {
                    if (scriptBody.sMethodName == "sfSilverGateBase")
                    {
                        scriptBody.sMethodName = "sfRandoGateBase";
                        scriptBody.iAdditionalStringArgCount = 1;
                        // Relevant artefact item goes here...
                        scriptBody.sAdditionalStringArg0 = zoneToArtefactMap[zoneNum];
                    } else if (scriptBody.sMethodName == "sfSilverGateFailBase")
                    {
                        scriptBody.sMethodName = "sfRandoGateFailBase";
                        scriptBody.iAdditionalStringArgCount = 1;
                        // Relevant artefact item goes here...
                        scriptBody.sAdditionalStringArg0 = zoneToArtefactMap[zoneNum];
                    }
                }
            }
            zoneScriptTables.Add(zoneNum, zoneScripts);
        }

        // custom items also need text entries for their names? (text rando)
    }

    public int GetMaxFlagIndex(DataStoreWDB<DataStoreREventFlag> store)
    {
        // Set 6000 as base index to space apart from existing flags.
        // Even if we assume its 14bit for some wild reason that still gives us up to 8192, loads of headroom.
        return Math.Max(6000, store.Values.Max(r => r.iFlagIndex));
    }

    private string[] reservedRandoTreasures = new string[] { "ran_init_cp", "ran_mfind", "ran_multi" };

    public void AddTreasure(string newName, string item, int count, string next, bool addFlag = true, bool isMeta = false)
    {
        string modifiedName = addFlag ? "z" + newName : newName;
        if (!ItemLocations.ContainsKey(modifiedName) && !reservedRandoTreasures.Contains(newName) && !isMeta)
        {
            //throw new Exception($"Identified newly added treasure {modifiedName} without data entry!");
        }
        if(modifiedName.Length > 15)
        {
            throw new Exception($"Max name length is 15! {modifiedName}");
        }
        if(item.Length > 15)
        {
            throw new Exception($"Max item length is 15! {item}");
        }
        AddTreasure(treasuresOrig, modifiedName, item, count, next);
        AddTreasure(treasures, modifiedName, item, count, next);
        if (addFlag)
        {
            AddFlag(eventFlags, modifiedName, GetMaxFlagIndex(eventFlags) + 1);
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

            cruxRando.CalculateAreaSpheres(ItemPlacer.SphereCalculator.Spheres.ToDictionary(kvp => kvp.Key.ID, kvp => kvp.Value));


            RandomNum.ClearRand();
        }
    }

    protected virtual void SaveHints()
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
        treasures.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
        eventFlags.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_eventflag.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_eventflag.wdb");
        search.SaveWDB(Generator, @"\db\resident\searchitem.wdb");
        foreach(var entry in zoneScriptTables)
        {
            var zoneNum = entry.Key;
            var scriptDb = entry.Value;
            scriptDb.SaveWDB(Generator, $@"\db\script\script{zoneNum}.wdb");
        }
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
