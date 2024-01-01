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
        RandoUI.SetUIProgressIndeterminate("Loading Ability Data...");
        abilities.LoadDB3(Generator, "LR", @"\db\resident\bt_ability.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Ability Data...", 50, 100);
        abilityGrowths.LoadDB3(Generator, "LR", @"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb", false);
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        RandoUI.SetUIProgressDeterminate("Loading Ability Data...", 80, 100);
        treasureRando.AddTreasure("ini_ba_abi", "", 1, "");
        treasureRando.AddTreasure("ini_ca_abi", "", 1, "");
    }
    public override void Randomize()
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        RandoUI.SetUIProgressIndeterminate("Randomizing Ability Data...");
        if (LRFlags.StatsAbilities.EPAbilities.FlagEnabled)
        {
            LRFlags.StatsAbilities.EPAbilities.SetRand();

            IEnumerable<ItemLocation> keys = treasureRando.ItemLocations.Values.Where(t => t.GetItem(false).Value.Item1.StartsWith("ti") || t.GetItem(false).Value.Item1 == "at900_00");

            keys = keys.Where(t => LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(t.GetItem(false).Value.Item1));

            keys.ToList().Shuffle((t1, t2) =>
            {
                string value = treasureRando.ItemLocations[t1.ID].GetItem(false).Value.Item1;
                treasureRando.ItemLocations[t1.ID].SetItem(treasureRando.ItemLocations[t2.ID].GetItem(false).Value.Item1, 1);
                treasureRando.ItemLocations[t2.ID].SetItem(value, 1);
            });

            if (LRFlags.StatsAbilities.NerfOC.FlagEnabled)
            {
                abilities["ti900_00"].i17AtbCount = 5 * 2000;
            }

            RandomNum.ClearRand();
        }

        RandoUI.SetUIProgressDeterminate("Randomizing Ability Data...", 50, 100);
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

        RandoUI.SetUIProgressDeterminate("Randomizing Ability Data...", 80, 100);
        if (LRFlags.StatsAbilities.AbilityPassives.FlagEnabled)
        {
            EquipRando equipRando = Generator.Get<EquipRando>();
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
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        EquipRando equipRando = Generator.Get<EquipRando>();
        List<DataStoreItem> enumerable = equipRando.GetAbilities(-1).Where(a => a.name.EndsWith("_00")).ToList();
        DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count - 1));

        treasureRando.treasures[name].s11ItemResourceId_string = random.name;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Ability Data...");
        abilities.SaveDB3(Generator, @"\db\resident\bt_ability.wdb");
        RandoUI.SetUIProgressDeterminate("Saving Ability Data...", 50, 100);
        abilityGrowths.SaveDB3(Generator, @"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_bt_abi_grow.wdb");
    }
}
