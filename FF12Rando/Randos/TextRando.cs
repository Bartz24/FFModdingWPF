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
    public class TextRando : Randomizer
    {
        public DataStoreBinText TextAbilities = new DataStoreBinText();
        public DataStoreBinText TextEquipment = new DataStoreBinText();
        public DataStoreBinText TextKeyItems = new DataStoreBinText();
        public DataStoreBinText TextLoot = new DataStoreBinText();
        public TextRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetProgressMessage()
        {
            return "Randomizing Text...";
        }
        public override string GetID()
        {
            return "Text";
        }

        public override void Load()
        {
            TextAbilities.Load("data\\text\\abilities.txt");
            TextEquipment.Load("data\\text\\equipment.txt");
            TextKeyItems.Load("data\\text\\keyitems.txt");
            TextLoot.Load("data\\text\\loot.txt");
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        public override void Save()
        {
        }
    }
}
