using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LRRando
{
    public class AbilityRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtAbility> abilities = new DataStoreDB3<DataStoreBtAbility>();
        public DataStoreDB3<DataStoreRBtAbiGrow> abilityGrowths = new DataStoreDB3<DataStoreRBtAbiGrow>();

        public AbilityRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Ability Data...", 0, 100);
            abilities.LoadDB3("LR", @"\db\resident\bt_ability.wdb");
            Randomizers.SetProgressFunc("Loading Ability Data...", 50, 100);
            abilityGrowths.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb", false);
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            Randomizers.SetProgressFunc("Loading Ability Data...", 80, 100);
            treasureRando.AddTreasure("ini_ba_abi", "", 1, "");
            treasureRando.AddTreasure("ini_ca_abi", "", 1, "");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();

            Randomizers.SetProgressFunc("Randomizing Ability Data...", 0, 100);
            if (LRFlags.StatsAbilities.EPAbilities.FlagEnabled)
            {
                LRFlags.StatsAbilities.EPAbilities.SetRand();

                IEnumerable<ItemLocation> keys = treasureRando.itemLocations.Values.Where(t => treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Item1.StartsWith("ti") || treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Item1 == "at900_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled)
                    keys = keys.Where(t => treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Item1 != "ti830_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesChrono.Enabled)
                    keys = keys.Where(t => treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Item1 != "ti840_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesTp.Enabled)
                    keys = keys.Where(t => treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Item1 != "ti810_00");

                keys.ToList().Shuffle((t1, t2) =>
                {
                    string value = treasureRando.PlacementAlgo.Logic.GetLocationItem(t1.ID, false).Item1;
                    treasureRando.PlacementAlgo.Logic.SetLocationItem(t1.ID, treasureRando.PlacementAlgo.Logic.GetLocationItem(t2.ID, false).Item1, 1);
                    treasureRando.PlacementAlgo.Logic.SetLocationItem(t2.ID, value, 1);
                });

                if (LRFlags.StatsAbilities.NerfOC.FlagEnabled)
                {
                    abilities["ti900_00"].i17AtbCount = 5 * 2000;
                }

                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Ability Data...", 50, 100);
            if (LRFlags.StatsAbilities.EPCosts.FlagEnabled)
            {
                LRFlags.StatsAbilities.EPCosts.SetRand();

                int min1 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 0 : 1;
                int min2 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 1 : 2;

                abilities["ti000_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti000_00"].i17AtbCount / 2000) * 2000;
                abilities["ti020_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti020_00"].i17AtbCount / 2000) * 2000;
                abilities["ti030_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti030_00"].i17AtbCount / 2000) * 2000;
                abilities["ti500_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti500_00"].i17AtbCount / 2000) * 2000;
                abilities["ti600_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti600_00"].i17AtbCount / 2000) * 2000;
                abilities["ti810_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti810_00"].i17AtbCount / 2000) * 2000;
                abilities["ti840_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti840_00"].i17AtbCount / 2000) * 2000;
                abilities["ti900_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti900_00"].i17AtbCount / 2000) * 2000;
                abilities["at900_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["at900_00"].i17AtbCount / 2000) * 2000;

                RandomNum.ClearRand();
            }


            Randomizers.SetProgressFunc("Randomizing Ability Data...", 80, 100);
            if (LRFlags.StatsAbilities.AbilityPassives.FlagEnabled)
            {
                EquipRando equipRando = Randomizers.Get<EquipRando>();
                LRFlags.StatsAbilities.AbilityPassives.SetRand();

                abilityGrowths.Values.ForEach(abi =>
                {
                    if (abi.sPasvAbility1_string != "" && abi.sPasvAbility1_string != "0")
                        abi.sPasvAbility1_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility2_string != "" && abi.sPasvAbility2_string != "0")
                        abi.sPasvAbility2_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility3_string != "" && abi.sPasvAbility3_string != "0")
                        abi.sPasvAbility3_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility4_string != "" && abi.sPasvAbility4_string != "0")
                        abi.sPasvAbility4_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility5_string != "" && abi.sPasvAbility5_string != "0")
                        abi.sPasvAbility5_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility6_string != "" && abi.sPasvAbility6_string != "0")
                        abi.sPasvAbility6_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility7_string != "" && abi.sPasvAbility7_string != "0")
                        abi.sPasvAbility7_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility8_string != "" && abi.sPasvAbility8_string != "0")
                        abi.sPasvAbility8_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility9_string != "" && abi.sPasvAbility9_string != "0")
                        abi.sPasvAbility9_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility10_string != "" && abi.sPasvAbility10_string != "0")
                        abi.sPasvAbility10_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility11_string != "" && abi.sPasvAbility11_string != "0")
                        abi.sPasvAbility11_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility12_string != "" && abi.sPasvAbility12_string != "0")
                        abi.sPasvAbility12_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility13_string != "" && abi.sPasvAbility13_string != "0")
                        abi.sPasvAbility13_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility14_string != "" && abi.sPasvAbility14_string != "0")
                        abi.sPasvAbility14_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility15_string != "" && abi.sPasvAbility15_string != "0")
                        abi.sPasvAbility15_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                    if (abi.sPasvAbility16_string != "" && abi.sPasvAbility16_string != "0")
                        abi.sPasvAbility16_string = RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList());
                });

                RandomNum.ClearRand();
            }

            LRFlags.StatsAbilities.EPAbilities.SetRand();
            RandomizeInitAbility("ini_ba_abi");
            RandomizeInitAbility("ini_ca_abi");
            RandomNum.ClearRand();
        }

        private int RandomEPCost(int absMin, int absMax, int val)
        {
            return RandomNum.RandInt(Math.Max(absMin, val - LRFlags.StatsAbilities.EPCostsRange.Value), Math.Min(val + LRFlags.StatsAbilities.EPCostsRange.Value, absMax));
        }

        private void RandomizeInitAbility(string name)
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            EquipRando equipRando = Randomizers.Get<EquipRando>();
            List<DataStoreItem> enumerable = equipRando.GetAbilities(-1).Where(a => a.name.EndsWith("_00")).ToList();
            DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1));

            treasureRando.treasures[name].s11ItemResourceId_string = random.name;
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Ability Data...", 0, 100);
            abilities.SaveDB3(@"\db\resident\bt_ability.wdb");
            Randomizers.SetProgressFunc("Saving Ability Data...", 50, 100);
            abilityGrowths.SaveDB3(@"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_bt_abi_grow.wdb");
        }
    }
}
