using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class AbilityRando : Randomizer
    {
        public DataStoreDB3<DataStoreBtAbility> abilities = new DataStoreDB3<DataStoreBtAbility>();
        public DataStoreDB3<DataStoreRBtAbiGrow> abilityGrowths = new DataStoreDB3<DataStoreRBtAbiGrow>();

        public AbilityRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Abilities...";
        }
        public override string GetID()
        {
            return "Abilities";
        }

        public override void Load()
        {
            abilities.LoadDB3("LR", @"\db\resident\bt_ability.wdb");
            abilityGrowths.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_bt_abi_grow.wdb", false);
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            treasureRando.AddTreasure("ini_ba_abi", "", 1, "");
            treasureRando.AddTreasure("ini_ca_abi", "", 1, "");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");

            if (LRFlags.StatsAbilities.EPAbilities.FlagEnabled)
            {
                LRFlags.StatsAbilities.EPAbilities.SetRand();

                IEnumerable<DataStoreRTreasurebox> keys = treasureRando.treasures.Values.Where(t => t.s11ItemResourceId_string.StartsWith("ti") || t.s11ItemResourceId_string == "at900_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled)
                    keys = keys.Where(t => t.s11ItemResourceId_string != "ti830_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesChrono.Enabled)
                    keys = keys.Where(t => t.s11ItemResourceId_string != "ti840_00");
                if (!LRFlags.StatsAbilities.EPAbilitiesTp.Enabled)
                    keys = keys.Where(t => t.s11ItemResourceId_string != "ti810_00");

                keys.ToList().Shuffle((t1, t2) => {
                    string value = t1.s11ItemResourceId_string;
                    t1.s11ItemResourceId_string = t2.s11ItemResourceId_string;
                    t2.s11ItemResourceId_string = value;
                });

                if (LRFlags.StatsAbilities.NerfOC.FlagEnabled)
                {
                    abilities["ti900_00"].i17AtbCount = 5 * 2000;
                }

                RandomNum.ClearRand();
            }

            if (LRFlags.StatsAbilities.EPCosts.FlagEnabled)
            {
                LRFlags.StatsAbilities.EPCosts.SetRand();

                int min0 = 0;
                int min1 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 0 : 1;
                int min2 = LRFlags.StatsAbilities.EPCostsZero.Enabled ? 1 : 2;

                abilities["ti000_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti000_00"].i17AtbCount / 2000) * 2000;
                abilities["ti020_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti020_00"].i17AtbCount / 2000) * 2000;
                abilities["ti030_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti030_00"].i17AtbCount / 2000) * 2000;
                abilities["ti500_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti500_00"].i17AtbCount / 2000) * 2000;
                abilities["ti600_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti600_00"].i17AtbCount / 2000) * 2000;
                abilities["ti810_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti810_00"].i17AtbCount / 2000) * 2000;
                abilities["ti830_00"].i17AtbCount = RandomEPCost(min0, 5, abilities["ti830_00"].i17AtbCount / 2000) * 2000;
                abilities["ti840_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["ti840_00"].i17AtbCount / 2000) * 2000;
                abilities["ti900_00"].i17AtbCount = RandomEPCost(min2, 5, abilities["ti900_00"].i17AtbCount / 2000) * 2000;
                abilities["at900_00"].i17AtbCount = RandomEPCost(min1, 5, abilities["at900_00"].i17AtbCount / 2000) * 2000;

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
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            List<DataStoreItem> enumerable = equipRando.GetAbilities(-1).Where(a => a.name.EndsWith("_00")).ToList();
            DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1));

            treasureRando.treasures[name].s11ItemResourceId_string = random.name;
        }

        public override void Save()
        {
            abilities.SaveDB3(@"\db\resident\bt_ability.wdb");
            //abilities.DeleteDB3(@"\db\resident\bt_ability.db3");
            abilityGrowths.DeleteDB3(@"\db\resident\_wdbpack.bin\r_bt_abi_grow.db3");
        }
    }
}
