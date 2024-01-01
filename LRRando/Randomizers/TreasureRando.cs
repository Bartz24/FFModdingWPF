using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using LRRando;
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
    public DataStoreDB3<DataStoreRTreasurebox> treasuresOrig = new();
    public DataStoreDB3<DataStoreRTreasurebox> treasures = new();
    public Dictionary<string, ItemLocation> ItemLocations = new();
    public Dictionary<string, HintData> hintData = new();
    public Dictionary<string, List<string>> hintsMain = new();
    private readonly Dictionary<string, string> hintsNotesLocations = new();
    private readonly Dictionary<string, int> hintsNotesCount = new();

    public Dictionary<string, string> BattleDrops = new();
    public Dictionary<string, string> OrigBattleDrops = new();

    private ItemPlacementAlgorithm<ItemLocation> placementAlgoNormal;
    private ItemPlacementAlgorithm<ItemLocation> placementAlgoBackup;
    private bool usingBackup = false;

    public ItemPlacementAlgorithm<ItemLocation> PlacementAlgo => usingBackup ? placementAlgoBackup : placementAlgoNormal;

    public TreasureRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 0, 100);
        treasuresOrig.LoadDB3(Generator, "LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);
        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 10, 100);
        treasures.LoadDB3(Generator, "LR", @"\db\resident\_wdbpack.bin\r_treasurebox.wdb", false);

        FileHelpers.ReadCSVFile(@"data\treasures.csv", row =>
        {
            TreasureData t = new(Generator, row, this);
            ItemLocations.Add(t.ID, t);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\battleDrops.csv", row =>
        {
            BattleDropData b = new(Generator, row, this);
            ItemLocations.Add(b.ID, b);
        }, FileHelpers.CSVFileHeader.HasHeader);

        hintsMain.Clear();
        hintData.Clear();
        FileHelpers.ReadCSVFile(@"data\hints.csv", row =>
        {
            HintData h = new(row);
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

        RandoUI.SetUIProgressDeterminate("Loading Treasure Data...", 80, 100);
        LRFlags.Items.Treasures.SetRand();
        List<string> locations = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().Shuffle();
        List<string> libraItems = hintsNotesLocations.Keys.Shuffle();
        for (int i = 0; i < locations.Count; i++)
        {
            hintsNotesLocations[libraItems[i]] = locations[i];
        }
        RandomNum.ClearRand();

        placementAlgoNormal = new AssumedItemPlacementAlgorithm<ItemLocation>(ItemLocations, locations, Generator, 3);
        placementAlgoNormal.Logic = new LRItemPlacementLogic(placementAlgoNormal, Generator);

        placementAlgoBackup = new ItemPlacementAlgorithm<ItemLocation>(ItemLocations, locations, Generator, -1);
        placementAlgoBackup.Logic = new LRItemPlacementLogic(placementAlgoBackup, Generator);
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

    public override void Randomize()
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        ShopRando shopRando = Generator.Get<ShopRando>();

        RandoUI.SetUIProgressDeterminate("Randomizing Treasure Data...", 0, 100);
        if (LRFlags.Items.Treasures.FlagEnabled)
        {
            LRFlags.Items.Treasures.SetRand();

            List<string> keys = ItemLocations.Keys.Shuffle();

            Dictionary<string, double> areaMults = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToDictionary(s => s, _ => RandomNum.RandInt(10, 200) * 0.01d);
            if (!placementAlgoNormal.Randomize(new List<string>(), areaMults))
            {
                usingBackup = true;
                placementAlgoBackup.Randomize(new List<string>(), areaMults);
            }

            // Same treasures take priority
            keys = keys.OrderBy(k => !ItemLocations[k].Traits.Contains("Same")).ToList();

            static bool sameCheck(string rep, string orig)
            {
                if (rep.StartsWith("cos") && orig.StartsWith("cos") && rep != "cos_ba08" && rep != "cos_ca08")
                {
                    return true;
                }

                if (rep.StartsWith("wea") && orig.StartsWith("wea") && rep != "wea_ea00")
                {
                    return true;
                }

                return rep.StartsWith("shi") && orig.StartsWith("shi") && rep != "shi_ea00"
                    || (rep.StartsWith("acc") && orig.StartsWith("acc"))
                    || (rep.StartsWith("e") && orig.StartsWith("e") && rep.Length == 4 && orig.Length == 4);
            }

            RandoUI.SetUIProgressDeterminate("Randomizing Treasure Data...", 40, 100);
            ItemLocations.Values.Where(t => equipRando.itemData.ContainsKey(ItemLocations[t.ID].GetItem(false).Value.Item1) && !equipRando.itemData[ItemLocations[t.ID].GetItem(false).Value.Item1].Traits.Contains("Key")).ForEach(t =>
            {
                (string, int) orig = ItemLocations[t.ID].GetItem(false).Value;


                ItemData item = equipRando.itemData.GetValueOrDefault(ItemLocations[t.ID].GetItem(false).Value.Item1);
                if (item?.Category == "Adornment")
                {
                    string next;
                    int count;
                    if (item.Traits.Contains("Remove"))
                    {
                        // Replace removed adornments with equipment
                        next = RandomNum.SelectRandom(equipRando.RemainingEquip);
                        equipRando.RemainingEquip.Remove(next);
                        count = 1;
                    }
                    else
                    {
                        next = RandomNum.SelectRandom(equipRando.itemData.Values.Where(i => i.Category == "Material")).ID;
                        // Rank [1, 10] -> [5, 1] count
                        count = (int)Math.Max((double)RandomNum.RandInt(100, 200) / 100.0 * Math.Pow(1.2, -(equipRando.itemData[next].Rank + 5)), 1);
                    }

                    ItemLocations[t.ID].SetItem(next, count);
                    return;
                }

                string repItem = null;
                do
                {
                    string category = equipRando.itemData[orig.Item1].Category;
                    if (LRFlags.Items.ReplaceAny.Enabled && !t.Traits.Contains("Same"))
                    {
                        // Do not include adornments (shops only)
                        category = equipRando.itemData.Values.Select(i => i.Category).Where(c => c != "Adornment").Distinct().Shuffle().First();
                    }

                    int rankRange = LRFlags.Items.ReplaceRank.Value;
                    IEnumerable<ItemData> possible = equipRando.itemData.Values.Where(i =>
                        i.Category == category &&
                        i.Rank >= equipRando.itemData[orig.Item1].Rank - rankRange &&
                        i.Rank <= equipRando.itemData[orig.Item1].Rank + rankRange &&
                        (!t.Traits.Contains("Same") || sameCheck(i.ID, ItemLocations[t.ID].GetItem(true).Value.Item1)) &&
                        !i.Traits.Contains("Key"));

                    if (category == "Weapon" || category == "Shield" || category == "Garb" || category == "Accessory")
                    {
                        possible = possible.Where(i => equipRando.RemainingEquip.Contains(i.ID));
                    }

                    if (possible.Count() == 0)
                    {
                        continue;
                    }

                    repItem = possible.Shuffle().Select(i => i.ID).First();

                    if (category == "Weapon" || category == "Shield" || category == "Garb" || category == "Accessory")
                    {
                        equipRando.RemainingEquip.Remove(repItem);
                    }
                } while (repItem == null);

                equipRando.RemainingEquip.Remove(repItem);

                int repCount = equipRando.itemData[repItem].Category == "Garb" || equipRando.itemData[repItem].Category == "Accessory" ? 1 : orig.Item2;
                ItemLocations[t.ID].SetItem(repItem, repCount);
            });

            RandoUI.SetUIProgressIndeterminate("Randomizing Treasure Data...");
            foreach (string key in keys)
            {
                if (equipRando.items.Keys.Contains(ItemLocations[key].GetItem(false).Value.Item1) && equipRando.IsAbility(equipRando.items[ItemLocations[key].GetItem(false).Value.Item1]))
                {
                    int lv = int.Parse(ItemLocations[key].GetItem(false).Value.Item1.Substring(ItemLocations[key].GetItem(false).Value.Item1.Length - 2)) / 10 + 1;
                    lv = RandomNum.RandInt(Math.Max(lv - 2, 1), Math.Min(lv + 2, 5));
                    string next = equipRando.GetAbilities(-1).Shuffle().First().sScriptId_string;
                    ItemLocations[key].SetItem($"{next}_{((lv - 1) * 10).ToString("00")}", 1);
                }
            }

            ItemLocations.Values.Where(t => ItemLocations[t.ID].GetItem(false).Value.Item1 == "" || equipRando.itemData.ContainsKey(ItemLocations[t.ID].GetItem(false).Value.Item1)).ForEach(t =>
            {
                (string, int) orig = ItemLocations[t.ID].GetItem(false).Value;
                if (orig.Item2 > 0)
                {
                    ItemLocations[t.ID].SetItem(orig.Item1, RandomNum.RandInt((int)Math.Round(Math.Max(1, orig.Item2 * 0.75)), (int)Math.Round(orig.Item2 * 1.25)));
                }
            });

            RandomNum.ClearRand();

            RandoUI.SetUIProgressIndeterminate("Randomizing Treasure Data...");
            if (LRFlags.Other.HintsNotes.FlagEnabled)
            {
                // Update hints again to reflect actual numbers
                hintsNotesLocations.Keys.Where(note => hintsNotesLocations[note] != null).ForEach(note =>
                    {
                        int locationCount = ItemLocations.Keys.Where(t => PlacementAlgo.Placement.ContainsKey(t) && ItemLocations[t].Areas[0] == hintsNotesLocations[note] && PlacementAlgo.Logic.IsHintable(PlacementAlgo.Placement[t])).Count();
                        hintsNotesCount[hintsNotesLocations[note]] = locationCount;
                    });
            }

            if (LRFlags.Items.IDCardBuy.Enabled)
            {
                treasures["ran_rando_id"].s11ItemResourceId_string = "true";
            }
        }
    }
    public bool IsImportantKeyItem(string location)
    {
        return !IsPilgrimKeyItem(location) && IsKeyItem(location);
    }

    public bool IsEPAbility(string t, bool orig = true)
    {
        return ItemLocations[t].GetItem(orig).Value.Item1.StartsWith("ti") || ItemLocations[t].GetItem(orig).Value.Item1 == "at900_00";
    }

    public bool IsKeyItem(string t, bool orig = true)
    {
        return LRFlags.Items.KeyItems.DictValues.Keys.Contains(ItemLocations[t].GetItem(orig).Value.Item1);
    }

    public bool IsPilgrimKeyItem(string t, bool orig = true)
    {
        return ItemLocations[t].GetItem(orig).Value.Item1 == "key_d_key";
    }

    public List<string> GetRandomizableEquip()
    {
        static bool isEquip(string s)
        {
            return (s.StartsWith("cos") || s.StartsWith("wea") || s.StartsWith("shi") || s.StartsWith("acc")) && s != "cos_fa00";
        }

        List<string> list = new();
        list.AddRange(treasuresOrig.Values.Where(t => isEquip(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

        return list;
    }

    public List<string> GetAdornments()
    {
        static bool isAdorn(string s)
        {
            return s.StartsWith("e");
        }

        List<string> list = new();
        list.AddRange(treasuresOrig.Values.Where(t => isAdorn(t.s11ItemResourceId_string)).Select(t => t.s11ItemResourceId_string));

        return list;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Treasure Data...");
        SaveHints();
        SetAndClearBattleDrops();
        treasures.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_treasurebox.wdb");
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
            BattleDrops.Add(btsc, treasures[treasure].s11ItemResourceId_string);
        });
        OrigBattleDrops.Add(treasure, treasures[treasure].s11ItemResourceId_string);

        treasures[treasure].s11ItemResourceId_string = "";
        treasures[treasure].iItemCount = 0;
    }

    private void SaveHints()
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        TextRando textRando = Generator.Get<TextRando>();

        if (LRFlags.Items.Treasures.FlagEnabled && LRFlags.Other.HintsMain.FlagEnabled)
        {
            hintsMain.Keys.ForEach(h =>
            {
                textRando.mainSysUS["$" + h] = string.Join("{Text NewLine}{Text NewLine}", hintsMain[h].Select(t => ItemLocations[t]).Select(t => GetHintText(t)));
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
                    return $"{t.Name} has {GetItemName(ItemLocations[t.ID].GetItem(false).Value.Item1)}";
                }
            case 1:
                {
                    string type = "Other";
                    if (IsKeyItem(PlacementAlgo.Placement[t.ID]))
                    {
                        type = "a Key Item";
                    }

                    if (IsPilgrimKeyItem(PlacementAlgo.Placement[t.ID]))
                    {
                        type = "Pilgrim's Crux";
                    }

                    if (IsEPAbility(t.ID, false))
                    {
                        type = "an EP Ability";
                    }

                    return $"{t.Name} has {type}";
                }
            case 2:
                {
                    return $"{t.Areas[0]} has {GetItemName(ItemLocations[t.ID].GetItem(false).Value.Item1)}";
                }
            case 3:
                {
                    return $"{t.Name} has ?????";
                }
        }
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();
        OrigBattleDrops.Keys.ForEach(name =>
        {
            treasures[name].s11ItemResourceId_string = OrigBattleDrops[name];
            treasures[name].iItemCount = 1;
        });

        HTMLPage page = new("Item Locations", "template/documentation.html");

        page.HTMLElements.Add(new Table("Item Locations", (new string[] { "Name", "New Contents", "Difficulty" }).ToList(), (new int[] { 45, 45, 10 }).ToList(), ItemLocations.Values.Select(t =>
        {
            string itemID = ItemLocations[t.ID].GetItem(false).Value.Item1;
            string name = GetItemName(itemID);
            string reqsDisplay = t.Requirements.GetDisplay(GetItemName);
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

            return (new object[] { nameCell, $"{name} x {ItemLocations[t.ID].GetItem(false).Value.Item2}", $"{t.BaseDifficulty}" }).ToList();
        }).ToList(), "itemlocations"));
        pages.Add("item_locations", page);

        if (LRFlags.Items.Treasures.FlagEnabled && (LRFlags.Other.HintsMain.FlagEnabled || LRFlags.Other.HintsNotes.FlagEnabled))
        {
            HTMLPage hintsPage = new("Hints", "template/documentation.html");

            if (LRFlags.Other.HintsMain.FlagEnabled)
            {
                hintsPage.HTMLElements.Add(new Table("Main Quest Hints", (new string[] { "Main Quest", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintsMain.Keys.Select(h =>
                {
                    return new string[] { hintData[h].Name, textRando.mainSysUS["$" + h].Replace("{Text NewLine}", "\n") }.ToList();
                }).ToList()));
            }

            if (LRFlags.Other.HintsNotes.FlagEnabled)
            {
                hintsPage.HTMLElements.Add(new Table("Libra Note Hints", (new string[] { "Libra Note", "Hint" }).ToList(), (new int[] { 20, 80 }).ToList(), hintsNotesLocations.Keys.Where(note => hintsNotesLocations[note] != null).Select(i =>
                {
                    return new string[] { GetItemName(i), textRando.mainSysUS[equipRando.items[i].sHelpStringId_string] }.ToList();
                }).ToList()));
            }

            pages.Add("hints", hintsPage);
        }

        return pages;
    }

    private string GetItemName(string itemID)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if (itemID == "")
        {
            name = "Gil";
        }
        else if (abilityRando.abilities.Keys.Contains(itemID))
        {
            name = textRando.mainSysUS[abilityRando.abilities[itemID].sStringResId_string];
        }
        else
        {
            name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }

        return name;
    }
}
