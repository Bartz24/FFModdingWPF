using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class EquipRando : Randomizer
    {
        public DataStoreWDB<DataStoreItem> items = new DataStoreWDB<DataStoreItem>();

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
            items.LoadWDB("13", @"\db\resident\item.wdb");

            string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            string[] roles = new string[] { "com", "rav", "sen", "syn", "sab", "med" };

            foreach (string c in chars)
            {
                foreach (string r in roles)
                {
                    string name = $"rol_{c}_{r}";
                    items.Copy("key_c_shiva", name);
                }
            }
            items.Copy("key_c_shiva", "cry_stage");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
            string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            string[] charNames = new string[] { "Lightning", "Fang", "Hope", "Sazh", "Snow", "Vanille" };
            string[] roles = new string[] { "com", "rav", "sen", "syn", "sab", "med" };
            string[] roleNames = new string[] { "Commando", "Ravager", "Sentinel", "Synergist", "Saboteur", "Medic" };

            List<string> newNames = textRando.mainSysUS.Keys.Where(s => textRando.mainSysUS[s] == "Attack" && s.StartsWith("$m")).ToList();

            foreach (string c in chars)
            {
                foreach (string r in roles)
                {
                    string name = $"rol_{c}_{r}";
                    items[name].sItemNameStringId_string = newNames[0];
                    items[name].sHelpStringId_string = "$mb_000_00eh";
                    newNames.RemoveAt(0);
                    textRando.mainSysUS[items[name].sItemNameStringId_string] = $"{charNames[chars.ToList().IndexOf(c)]}'s {roleNames[roles.ToList().IndexOf(r)]} Role" + "{End}";
                }
            }
            items["cry_stage"].sItemNameStringId_string = newNames[0];
            items["cry_stage"].sHelpStringId_string = "$mb_000_00eh";
            newNames.RemoveAt(0);
            textRando.mainSysUS[items["cry_stage"].sItemNameStringId_string] = "Crystarium Expansion{End}{Many}Crystarium Expansions{End}{Article}a{End}";
        }

        public override void Save()
        {
            items.SaveWDB(@"\db\resident\item.wdb");

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
