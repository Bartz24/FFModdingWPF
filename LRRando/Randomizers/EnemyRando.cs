using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LRRando
{
    public class EnemyRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtCharaSpec> enemies = new DataStoreDB3<DataStoreBtCharaSpec>();

        public EnemyRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetID()
        {
            return "Enemies";
        }

        public override void Load()
        {
            string path = Nova.GetNovaFile("LR", @"db\resident\bt_chara_spec.wdb", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_chara_spec.wdb";
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
        }
        public override void Randomize(Action<int> progressSetter)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>("Equip");
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");

            if (LRFlags.Enemies.BhuniPlus.FlagEnabled)
                treasureRando.treasures["ran_bhuni_p"].s11ItemResourceId_string = "true";


            string[] types = new string[] { "drop0", "drop1", "dropCnd0", "dropCnd1", "dropCnd2" };
            List<string> matDrops = new List<string>();
            RemoveMatDrop(equipRando, matDrops);
            List<string> abiDrops = new List<string>();
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
                        SetDrop(baseE, type, newBaseItem);
                    enemies.Values.Where(e => e.sBaseBtSpec_string == baseE.name).ForEach(e =>
                    {
                        string item = GetDrop(e, type);
                        string newItem = null;
                        if (item == baseItem)
                            newItem = newBaseItem;
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
                            SetDrop(e, type, newItem);
                    });

                }
            });
        }

        private static void RemoveMatDrop(EquipRando equipRando, List<string> list)
        {
            LRFlags.Enemies.MatDrops.SetRand();
            if (list.Count > 0)
                list.RemoveAt(0);
            if (list.Count == 0)
                list.AddRange(equipRando.items.Keys.Where(s => s.StartsWith("mat_z")).ToList().Shuffle());
            RandomNum.ClearRand();
        }

        private static void RemoveAbiDrop(EquipRando equipRando, List<string> list)
        {
            LRFlags.Enemies.AbiDrops.SetRand();
            if (list.Count > 0)
                list.RemoveAt(0);
            if (list.Count == 0)
                list.AddRange(equipRando.GetAbilities(-1).Shuffle().Select(i => i.name));
            RandomNum.ClearRand();
        }

        private string GetDrop(DataStoreBtCharaSpec e, string type)
        {
            switch (type)
            {
                case "drop0":
                    return e.s10DropItem0_string;
                case "drop1":
                    return e.s10DropItem1_string;
                case "dropCnd0":
                    return e.sDropCndItem0_string;
                case "dropCnd1":
                    return e.sDropCndItem1_string;
                case "dropCnd2":
                    return e.sDropCndItem2_string;
            }
            return null;
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
            string outPath = SetupData.OutputFolder + @"\db\resident\bt_chara_spec.wdb";
            enemies.Save(outPath, SetupData.Paths["Nova"]);
        }
    }
}
