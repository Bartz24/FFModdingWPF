using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;
using System.Windows.Input;
using static LRRando.EquipRando;

namespace LRRando;

public partial class TreasureRando : Randomizer
{
    public DataStoreWDB<DataStoreRTreasurebox> treasuresOrig = new();
    public DataStoreWDB<DataStoreRTreasurebox> treasures = new();
    public Dictionary<string, ItemLocation> ItemLocations = new();
    public Dictionary<string, HintData> hintData = new();

    public Dictionary<string, string> BattleDrops = new();
    public Dictionary<string, string> OrigBattleDrops = new();
    public LRItemPlacer ItemPlacer { get; set; }
    public LRHintPlacer HintPlacer { get; set; }
    public AreaGraph AreaGraph { get; set; }

    private bool hintsSaved = false;

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 0, 100);
        treasuresOrig.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 10, 100);
        treasures.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);

        FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
        {
            TreasureLocation t = new(Generator, row, this);
            ItemLocations.Add(t.ID, t);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\battleDrops.csv", row =>
        {
            BattleDropLocation b = new(Generator, row, this);
            ItemLocations.Add(b.ID, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\fakeChecks.csv", row =>
        {
            string[] fakeItems = row[6].Split('|');
            for (int i = 0; i < fakeItems.Length; i++)
            {
                string fakeItem = fakeItems[i];
                int amount = 1;
                if (fakeItem.Contains("*"))
                {
                    amount = int.Parse(fakeItem.Split('*')[1]);
                    fakeItem = fakeItem.Split('*')[0];
                }

                FakeLocation f = new(Generator, row, fakeItem, amount);
                f.ID = f.ID + ":" + i;
                ItemLocations.Add(f.ID, f);
            }
        }, FileHelpers.CSVFileHeader.HasHeader);

        hintData.Clear();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        FileHelpers.ReadCSVFile(@"data\hints.csv", row =>
        {
            HintData h = new(row);
            hintData.Add(h.ID, h);

            string[] fakeData = new string[] { h.Area, h.Name + " Hint", "_hint_" + h.ID, "", "", "0" };
            FakeLocation fake = new(Generator, fakeData, h.Name + " Hint");
            fake.Requirements = h.Requirements;
            treasureRando.ItemLocations.Add(fake.ID, fake);

            h.FakeLocationLink = fake.ID;
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
        AddTreasure("ran_mfind", "rando_findstring", 32658, "");
        AddTreasure("ran_multi", RandoFlags.Mode == RandoFlags.SeedMode.Archipelago ? "rando_multi_item" : "disabled", 32500, "");

        treasures.Swap("tre_box_p_000", "tre_box_p_003");
        treasuresOrig.Swap("tre_box_p_000", "tre_box_p_003");
        treasures.Swap("tre_box_p_001", "tre_box_p_200");
        treasuresOrig.Swap("tre_box_p_001", "tre_box_p_200");
        treasures.Swap("tre_box_p_002", "tre_box_p_201");
        treasuresOrig.Swap("tre_box_p_002", "tre_box_p_201");

        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 80, 100);
        LRFlags.Items.Treasures.SetRand();
        List<string> locations = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().Shuffle();
        RandomNum.ClearRand();

        AreaGraph = new(Generator);
        AreaGraph.ReadFromCSVs(@"data\areas.csv", @"data\areaConnections.csv");
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
        database[newName].s10NextTreasureBoxResourceId = next;
        database[newName].iItemCount = count;
    }

    public override void Randomize()
    {
        RandoUI.SetUIProgressDeterminate("Randomizing Treasure Data...", 0, 100);
        if (LRFlags.Items.Treasures.FlagEnabled)
        {
            LRFlags.Items.Treasures.SetRand();

            ItemPlacer = new(Generator, AreaGraph);
            ItemPlacer.Replacements = ItemLocations.Values.ToHashSet();
            ItemPlacer.PossibleLocations = ItemLocations.Values.ToHashSet();
            ItemPlacer.PlaceItems();
            ItemPlacer.ApplyToGameData();

            HintPlacer = new(Generator, ItemPlacer, hintData.Keys.ToHashSet());
            HintPlacer.PlaceHints();

            RandomNum.ClearRand();

            HandleIDCardBuyOption();
        }
    }

    protected void HandleIDCardBuyOption()
    {
        if (LRFlags.Items.IDCardBuy.Enabled)
        {
            treasures["ran_rando_id"].s11ItemResourceId = "true";
        }
    }

    public List<string> GetRandomizableEquip()
    {
        static bool isEquip(string s)
        {
            return (s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi") || s.StartsWith("acc")) && s != "cos_fa00";
        }

        List<string> list = new();
        list.AddRange(treasuresOrig.Values.Where(t => isEquip(t.s11ItemResourceId)).Select(t => t.s11ItemResourceId));

        return list;
    }

    public List<string> GetAdornments()
    {
        static bool isAdorn(string s)
        {
            return s.StartsWith("e");
        }

        List<string> list = new();
        list.AddRange(treasuresOrig.Values.Where(t => isAdorn(t.s11ItemResourceId)).Select(t => t.s11ItemResourceId));

        return list;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Treasure Data...");
        SaveHints();
        SetAndClearBattleDrops();
        treasures.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_treasurebox.wdb");
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
            BattleDrops.Add(btsc, treasures[treasure].s11ItemResourceId);
        });
        OrigBattleDrops.Add(treasure, treasures[treasure].s11ItemResourceId);

        treasures[treasure].s11ItemResourceId = "";
        treasures[treasure].iItemCount = 0;
    }

    private void SaveHints()
    {
        if (hintsSaved)
        {
            return;
        }
        hintsSaved = true;
        EquipRando equipRando = Generator.Get<EquipRando>();
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        TextRando textRando = Generator.Get<TextRando>();

        if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
        {
            Dictionary<string, string> bossHintLocations = new();
            if (LRFlags.Other.HintsBosses.SelectedIndex > 0)
            {
                // This is the last main quest hint location before the boss (if it has one) for each quest
                int[] mainQuestCount = [1, 4, 2, 2, 4, 6];
                string[] originalBosses = ["Aeronite", "Noel", "Snow", "Caius", "Grendel", "Ereshkigal"];
                for (var i = 0; i < mainQuestCount.Length; i++)
                {
                    // For each main quest, identify what kind of hint type we're doing:
                    var hintType = LRFlags.Other.HintsBosses.SelectedIndex;
                    LRFlags.Other.HintsMain.SetRand();
                    if(hintType == 5)
                    {
                        hintType = RandomNum.RandInt(1, 4);
                    }

                    var hintIdx = 1;
                    if (hintType % 2 == 0)
                    {
                        hintIdx = RandomNum.RandInt(1, mainQuestCount[i]);
                    }

                    RandomNum.ClearRand();
                    var name = $"{i}-{hintIdx}";

                    BattleRando battleRando = Generator.Get<BattleRando>();
                    var shuffled = battleRando.RandomisedBosses();

                    var original = originalBosses[i];

                    var hintText = "";

                    switch ((hintType+1)/2)
                    {
                        case 1:
                            // Source
                            var source = shuffled[original];
                            hintText = $"{original} location has {source}";
                            break;
                        case 2:
                            // Target
                            var target = shuffled.Where(kv => kv.Value == original).Select(kv => kv.Key).FirstOrDefault();
                            hintText = $"{original} is in {target}'s location";
                            break;
                    }

                    bossHintLocations.Add(name, hintText);
                }
            }

            hintData.Keys.ForEach(h =>
            {
                List<string> lines = new();
                if (HintPlacer?.Hints[h].Count > 0)
                {
                    lines = HintPlacer.Hints[h].Select(l => HintPlacer.GetHintText(l)).ToList();
                }

                // Add in boss hint here.
                var quest = hintData[h].Name;

                if (bossHintLocations.ContainsKey(quest))
                {
                    lines.Add(bossHintLocations[quest]);
                }

                if (lines.Count == 0)
                {
                    lines.Add("There is nothing left to hint.");
                }
                

                textRando.mainSysUS["$" + h] = string.Join("{Text NewLine}{Text NewLine}", lines);
            });
        }
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        SaveHints();
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();
        OrigBattleDrops.Keys.ForEach(name =>
        {
            treasures[name].s11ItemResourceId = OrigBattleDrops[name];
            treasures[name].iItemCount = 1;
        });

        HTMLPage page = new("Item Locations", "template/documentation.html");

        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Sphere" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), ItemLocations.Values.Where(l => l is not FakeLocation).Select(t =>
        {
            string itemID = ItemLocations[t.ID].GetItem(false).Value.Item1;
            string name = equipRando.GetItemName(itemID);
            string reqsDisplay = t.Requirements.GetDisplay(equipRando.GetItemName);
            if (reqsDisplay.StartsWith("(") && reqsDisplay.EndsWith(")"))
            {
                reqsDisplay = reqsDisplay.Substring(1, reqsDisplay.Length - 2);
            }

            TableCellMultiple nameCell = new(new List<string>());
            nameCell.Elements.Add($"<div style=\"margin-right: auto\">{t.Areas[0]} - {t.Name}</div>");
            if (!string.IsNullOrEmpty(t.LocationImagePath))
            {
                nameCell.Elements.Add(new IconTooltip("common/images/map_white_48dp.svg", $"<img src='common/images/locations/{t.LocationImagePath}.jpg' height='200px'/>").ToString());
            }

            if (reqsDisplay != ItemReq.TRUE.GetDisplay())
            {
                nameCell.Elements.Add(new IconTooltip("common/images/lock_white_48dp.svg", "Requires: " + reqsDisplay).ToString());
            }

            string item = $"{name} x {ItemLocations[t.ID].GetItem(false).Value.Item2}";
            return (new object[] { nameCell, item, ItemPlacer.SphereCalculator.Spheres.ContainsKey(t) ? ItemPlacer.SphereCalculator.Spheres[t] : "N/A" }).ToList();
        }).ToList(), "itemlocations"));
        pages.Add("item_locations", page);

        if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled && HintPlacer != null)
        {
            HTMLPage hintsPage = new("Hints", "template/documentation.html");

            if (LRFlags.Other.HintsMain.FlagEnabled)
            {
                hintsPage.HTMLElements.Add(new Table("Main Quest Hints", (new string[] { "Main Quest", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintData.Keys.Select(h =>
                {
                    return new string[] { hintData[h].Name, textRando.mainSysUS["$" + h].Replace("{Text NewLine}", "\n") }.ToList();
                }).ToList()));
            }

            pages.Add("hints", hintsPage);
        }

        return pages;
    }
}
