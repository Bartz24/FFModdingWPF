using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace FF13_2Rando
{
    public class EquipRando : Randomizer
    {
        public DataStoreDB3<DataStoreItem> items = new DataStoreDB3<DataStoreItem>();

        public EquipRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Equipment...";
        }
        public override string GetID()
        {
            return "Equip";
        }

        public override void Load()
        {
            items.LoadDB3("13-2", @"\db\resident\item.wdb");
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }
        public override void Save()
        {
            items.SaveDB3(@"\db\resident\item.wdb");
        }

        private string GetItemName(string itemID)
        {
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            string name = textRando.mainSysUS[items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
                name = name.Substring(0, name.IndexOf("{End}"));

            return name;
        }
    }
}
