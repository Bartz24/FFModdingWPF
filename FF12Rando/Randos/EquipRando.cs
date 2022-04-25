using Bartz24.Docs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;
using Bartz24.RandoWPF;
using Bartz24.FF12;

namespace FF12Rando
{
    public class EquipRando : Randomizer
    {
        public Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();
        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetProgressMessage()
        {
            return "Randomizing Equip...";
        }
        public override string GetID()
        {
            return "Equip";
        }

        public override void Load()
        {
            FileExtensions.ReadCSVFile(@"data\items.csv", row =>
            {
                ItemData i = new ItemData(row);
                itemData.Add(i.ID, i);
            }, true);
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        public override void Save()
        {
        }
        public class ItemData
        {
            public string Name { get; set; }
            public int IntID { get; set; }
            public string ID { get; set; }
            public int Rank { get; set; }
            public int IntUpgrade { get; set; }
            public string Upgrade { get; set; }
            public ItemData(string[] row)
            {
                Name = row[0];
                IntID = Convert.ToInt32(row[1], 16);
                ID = row[1];
                Rank = int.Parse(row[2]);
                Upgrade = row[3];
                if (!string.IsNullOrEmpty(Upgrade))
                    IntUpgrade = Convert.ToInt32(row[3], 16);
            }
        }
    }
}
