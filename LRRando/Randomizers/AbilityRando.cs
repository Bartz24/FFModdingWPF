using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LRRando;

public class AbilityRando : Randomizer
{
    public DataStoreDB3<DataStoreBtAbility> abilities = new();
    public DataStoreDB3<DataStoreRBtAbiGrow> abilityGrowths = new();

    public AbilityRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Ability Data...", 0, 100);
        abilities.LoadDB3("LR", @"\db\resident\bt_ability.wdb");
        Randomizers.SetUIProgress("Loading Ability Data...", 50, 100);
        abilityGrowths.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb", false);
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
        Randomizers.SetUIProgress("Loading Ability Data...", 80, 100);
        treasureRando.AddTreasure("ini_ba_abi", "", 1, "");
        treasureRando.AddTreasure("ini_ca_abi", "", 1, "");
    }
    public override void Randomize()
    {
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();

        Randomizers.SetUIProgress("Randomizing Ability Data...", 0, 100);
        if (LRFlags.StatsAbilities.EPAbilities.FlagEnabled)
        {
            LRFlags.StatsAbilities.EPAbilities.SetRand();

            IEnumerable<ItemLocation> keys = treasureRando.itemLocations.Values.Where(t => treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Value.Item1.StartsWith("ti") || treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Value.Item1 == "at900_00");

            keys = keys.Where(t => LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(treasureRando.PlacementAlgo.Logic.GetLocationItem(t.ID, false).Value.Item1));

            keys.ToList().Shuffle((t1, t2) =>
            {
                string value = treasureRando.PlacementAlgo.Logic.GetLocationItem(t1.ID, false).Value.Item1;
                treasureRando.PlacementAlgo.Logic.SetLocationItem(t1.ID, treasureRando.PlacementAlgo.Logic.GetLocationItem(t2.ID, false).Value.Item1, 1);
                treasureRando.PlacementAlgo.Logic.SetLocationItem(t2.ID, value, 1);
            });

            if (LRFlags.StatsAbilities.NerfOC.FlagEnabled)
            {
                abilities["ti900_00"].i17AtbCount = 5 * 2000;
            }

            RandomNum.ClearRand();
        }

        Randomizers.SetUIProgress("Randomizing Ability Data...", 50, 100);
        if (LRFlags.StatsAbilities.EPCosts.FlagEnabled)
        {
            LRFlags.StatsAbilities.EPCosts.SetRand();

            int min1 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 0 : 1;
            int min2 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 1 : 2;
            int max = LRFlags.StatsAbilities.EPCostMax.Value;

            int[] minValues = { min2, min2, min1, min2, min1, min2, min1, min2, min1 };
            string[] abilityIds = { "ti000_00", "ti020_00", "ti030_00", "ti500_00", "ti600_00", "ti810_00", "ti840_00", "ti900_00", "at900_00" };

            for (int i = 0; i < abilityIds.Length; i++)
            {
                string abilityId = abilityIds[i];
                int min = minValues[i];
                abilities[abilityId].i17AtbCount = RandomEPCost(min, max, abilities[abilityId].i17AtbCount / 2000) * 2000;
            }

            RandomNum.ClearRand();
        }

        Randomizers.SetUIProgress("Randomizing Ability Data...", 80, 100);
        if (LRFlags.StatsAbilities.AbilityPassives.FlagEnabled)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>();
            LRFlags.StatsAbilities.AbilityPassives.SetRand();

            abilityGrowths.Values.ForEach(abi =>
            {
                // Get all properties of the abi object with the name "sPasvAbility[0-9]*_string"
                PropertyInfo[] pasvAbilityProperties = abi.GetType().GetProperties()
                    .Where(p => Regex.IsMatch(p.Name, "sPasvAbility[0-9]*_string")).ToArray();

                // Set the value of each property to a random ability name
                foreach (PropertyInfo pasvAbilityProp in pasvAbilityProperties)
                {
                    string propertyValue = pasvAbilityProp.GetValue(abi) as string;
                    if (!string.IsNullOrEmpty(propertyValue) && !propertyValue.Equals("0"))
                    {
                        pasvAbilityProp.SetValue(abi, RandomNum.SelectRandom(equipRando.GetFilteredAbilities().Select(p => p.name).ToList()));
                    }
                }
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
        DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count - 1));

        treasureRando.treasures[name].s11ItemResourceId_string = random.name;
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Ability Data...", 0, 100);
        abilities.SaveDB3(@"\db\resident\bt_ability.wdb");
        Randomizers.SetUIProgress("Saving Ability Data...", 50, 100);
        abilityGrowths.SaveDB3(@"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb");
        SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_bt_abi_grow.wdb");
    }
}
