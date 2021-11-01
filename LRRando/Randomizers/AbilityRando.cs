using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
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
        DataStoreDB3<DataStoreBtAbility> abilities = new DataStoreDB3<DataStoreBtAbility>();

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
            //abilities.LoadDB3("LR", @"\db\resident\bt_ability.wdb");
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            treasureRando.AddTreasure("ini_ba_abi", "", 1, "");
            treasureRando.AddTreasure("ini_ca_abi", "", 1, "");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");

            if (LRFlags.Other.EPAbilities.FlagEnabled)
            {
                LRFlags.Other.EPAbilities.SetRand();

                IEnumerable<DataStoreRTreasurebox> keys = treasureRando.treasures.Values.Where(t => t.s11ItemResourceId_string.StartsWith("ti") || t.s11ItemResourceId_string == "at900_00");
                if (!LRFlags.Other.EPAbilitiesEscape.FlagEnabled)
                    keys = keys.Where(t => t.s11ItemResourceId_string != "ti830_00");

                keys.ToList().Shuffle((t1, t2) => {
                    string value = t1.s11ItemResourceId_string;
                    t1.s11ItemResourceId_string = t2.s11ItemResourceId_string;
                    t2.s11ItemResourceId_string = value;
                });

                RandomNum.ClearRand();
            }

            LRFlags.Other.EPAbilities.SetRand();
            RandomizeInitAbility("ini_ba_abi");
            RandomizeInitAbility("ini_ca_abi");
            RandomNum.ClearRand();
        }

        private void RandomizeInitAbility(string name)
        {
            TreasureRando treasureRando = randomizers.Get<TreasureRando>("Treasures");
            EquipRando equipRando = randomizers.Get<EquipRando>("Equip");
            IEnumerable<DataStoreItem> enumerable = equipRando.GetAbilities(name, -1).Where(a => a.name.EndsWith("_00"));
            DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1));

            treasureRando.treasures[name].s11ItemResourceId_string = random.name;
        }

        public override void Save()
        {
            //abilities.SaveDB3(@"\db\resident\bt_ability.wdb");
        }
    }
}
