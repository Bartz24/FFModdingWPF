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
            abilities.LoadDB3("LR", @"\db\resident\bt_ability.wdb");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            LRFlags.Other.Abilities.SetRand();

            IEnumerable<string> keys = abilities.Keys.Where(k => k.StartsWith("ti") && k != "ti800_00" && k != "ti820_00" && k != "ti900_00");
            keys.ForEach(k1 => abilities.Swap(k1, keys.ElementAt(RandomNum.RandInt(0, keys.Count() - 1))));

            RandomNum.ClearRand();
        }

        public override void Save()
        {
            abilities.SaveDB3(@"\db\resident\bt_ability.wdb");
        }
    }
}
