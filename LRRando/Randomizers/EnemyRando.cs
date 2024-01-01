using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using LRRando;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;

public class EnemyRando : Randomizer
{
    public DataStoreDB3<DataStoreBtCharaSpec> enemies = new();

    public EnemyRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Enemy Data...");
        string path = Nova.GetNovaFile("LR", @"db\resident\bt_chara_spec.wdb", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
        string outPath = Generator.DataOutFolder + @"\db\resident\bt_chara_spec.wdb";
        FileHelpers.CopyFile(path, outPath);
        enemies.Load("LR", outPath, SetupData.Paths["Nova"]);
        enemies["m375"].fBrkLoopTime3 = 1203982208;
        enemies["m375_break1"].fBrkLoopTime3 = 1203982208;
        enemies["m375_break2"].fBrkLoopTime3 = 1203982208;
        enemies["m375_break3"].fBrkLoopTime3 = 1203982208;
        enemies["m375_break4"].fBrkLoopTime3 = 1203982208;
        enemies["m375"].s8Ability18_string = "m375_ac900";
        enemies["m375_break1"].s8Ability18_string = "m375_ac900";
        enemies["m375_break2"].s8Ability18_string = "m375_ac900";
        enemies["m375_break3"].s8Ability18_string = "m375_ac900";
        enemies["m375_break4"].s8Ability18_string = "m375_ac900";

        enemies["m330"].u16DropGil = 4000;
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Enemy Data...");
        EquipRando equipRando = Generator.Get<EquipRando>();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        if (LRFlags.Enemies.BhuniPlus.FlagEnabled)
        {
            treasureRando.treasures["ran_bhuni_p"].s11ItemResourceId_string = "true";
        }

        string[] types = { "drop0", "drop1", "dropCnd0", "dropCnd1", "dropCnd2" };
        List<string> matDrops = new();
        RemoveMatDrop(equipRando, matDrops);
        List<string> abiDrops = new();
        RemoveAbiDrop(equipRando, abiDrops);
        enemies.Values.Where(e => e.sBaseBtSpec_string == "").ForEach(baseE =>
        {
            foreach (string type in types)
            {
                string baseItem = GetDrop(baseE, type);
                string newBaseItem = null;
                if (LRFlags.Enemies.MatDrops.FlagEnabled && baseItem.StartsWith("mat_z"))
                {
                    newBaseItem = matDrops.First();
                    RemoveMatDrop(equipRando, matDrops);
                }

                if (LRFlags.Enemies.AbiDrops.FlagEnabled && equipRando.IsAbility(baseItem))
                {
                    string lv = baseItem.Substring(baseItem.Length - 3);
                    string next = equipRando.items[abiDrops.First()].sScriptId_string;
                    newBaseItem = next + lv;
                    RemoveAbiDrop(equipRando, abiDrops);
                }

                if (newBaseItem != null)
                {
                    SetDrop(baseE, type, newBaseItem);
                }

                enemies.Values.Where(e => e.sBaseBtSpec_string == baseE.name).ForEach(e =>
                {
                    string item = GetDrop(e, type);
                    string newItem = null;
                    if (item == baseItem)
                    {
                        newItem = newBaseItem;
                    }
                    else if (LRFlags.Enemies.MatDrops.FlagEnabled && baseItem.StartsWith("mat_z"))
                    {
                        newItem = matDrops.First();
                        RemoveMatDrop(equipRando, matDrops);
                    }
                    else if (LRFlags.Enemies.AbiDrops.FlagEnabled && equipRando.IsAbility(item))
                    {
                        string lv = item.Substring(item.Length - 3);
                        string next = abiDrops.First();
                        newItem = next + lv;
                        RemoveAbiDrop(equipRando, abiDrops);
                    }

                    if (newItem != null)
                    {
                        SetDrop(e, type, newItem);
                    }
                });

            }
        });
    }

    private static void RemoveMatDrop(EquipRando equipRando, List<string> list)
    {
        LRFlags.Enemies.MatDrops.SetRand();
        if (list.Count > 0)
        {
            list.RemoveAt(0);
        }

        if (list.Count == 0)
        {
            list.AddRange(equipRando.items.Keys.Where(s => s.StartsWith("mat_z")).Shuffle());
        }

        RandomNum.ClearRand();
    }

    private static void RemoveAbiDrop(EquipRando equipRando, List<string> list)
    {
        LRFlags.Enemies.AbiDrops.SetRand();
        if (list.Count > 0)
        {
            list.RemoveAt(0);
        }

        if (list.Count == 0)
        {
            list.AddRange(equipRando.GetAbilities(-1).Shuffle().Select(i => i.name));
        }

        RandomNum.ClearRand();
    }

    private string GetDrop(DataStoreBtCharaSpec e, string type)
    {
        return type switch
        {
            "drop0" => e.s10DropItem0_string,
            "drop1" => e.s10DropItem1_string,
            "dropCnd0" => e.sDropCndItem0_string,
            "dropCnd1" => e.sDropCndItem1_string,
            "dropCnd2" => e.sDropCndItem2_string,
            _ => null,
        };
    }

    private void SetDrop(DataStoreBtCharaSpec e, string type, string item)
    {
        switch (type)
        {
            case "drop0":
                e.s10DropItem0_string = item;
                return;
            case "drop1":
                e.s10DropItem1_string = item;
                return;
            case "dropCnd0":
                e.sDropCndItem0_string = item;
                return;
            case "dropCnd1":
                e.sDropCndItem1_string = item;
                return;
            case "dropCnd2":
                e.sDropCndItem2_string = item;
                return;
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Enemy Data...");
        string outPath = Generator.DataOutFolder + @"\db\resident\bt_chara_spec.wdb";
        enemies.Save(outPath, SetupData.Paths["Nova"]);
    }
}
