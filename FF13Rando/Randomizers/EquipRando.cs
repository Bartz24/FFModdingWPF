using Bartz24.Data;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando
{
    public class EquipRando : Randomizer
    {
        public DataStoreWDB<DataStoreItem> items = new DataStoreWDB<DataStoreItem>();
        public Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();

        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Equip/Item Data...", -1, 100);
            items.LoadWDB("13", @"\db\resident\item.wdb");

            string[] chars = { "lig", "fan", "hop", "saz", "sno", "van" };
            string[] roles = { "com", "rav", "sen", "syn", "sab", "med" };

            foreach (string c in chars)
            {
                foreach (string r in roles)
                {
                    string name = $"rol_{c}_{r}";
                    items.Copy("key_c_shiva", name);
                }
            }
            items.Copy("key_c_shiva", "cry_stage");

            for (int i = 1; i <= 13; i++)
            {
                items.Copy("key_receiver", "chap_prog_" + i.ToString("00"));
                items.Copy("key_receiver", "chap_comp_" + i.ToString("00"));
            }

            FileHelpers.ReadCSVFile(@"data\items.csv", row =>
            {
                ItemData i = new ItemData(row);
                i.SortIndex = itemData.Count;
                itemData.Add(i.ID, i);
            }, FileHelpers.CSVFileHeader.HasHeader);

            itemData.Values.Where(i => i.OverrideBuy != -1).ForEach(i => items[i.ID].u16BuyPrice = (uint)i.OverrideBuy);
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Equip/Item Data...", -1, 100);
            TextRando textRando = Randomizers.Get<TextRando>();
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            string[] chars = { "lig", "fan", "hop", "saz", "sno", "van" };
            string[] charNames = { "Lightning", "Fang", "Hope", "Sazh", "Snow", "Vanille" };
            string[] roles = { "com", "rav", "sen", "syn", "sab", "med" };
            string[] roleNames = { "Commando", "Ravager", "Sentinel", "Synergist", "Saboteur", "Medic" };

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

            string chapterProgress = newNames[0];
            newNames.RemoveAt(0);
            textRando.mainSysUS[chapterProgress] = "Used for tracking in the rando to determine current progress in each chapter.";
            string chapterComplete = newNames[0];
            newNames.RemoveAt(0);
            textRando.mainSysUS[chapterComplete] = "Used for tracking in the rando to determine completed chapters.";
            for (int i = 1; i <= 13; i++)
            {
                items["chap_prog_" + i.ToString("00")].sItemNameStringId_string = newNames[0];
                newNames.RemoveAt(0);
                items["chap_prog_" + i.ToString("00")].sHelpStringId_string = chapterProgress;
                textRando.mainSysUS[items["chap_prog_" + i.ToString("00")].sItemNameStringId_string] = "Chapter " + i + " Progress{End}";

                items["chap_comp_" + i.ToString("00")].sItemNameStringId_string = newNames[0];
                newNames.RemoveAt(0);
                items["chap_comp_" + i.ToString("00")].sHelpStringId_string = chapterComplete;
                textRando.mainSysUS[items["chap_comp_" + i.ToString("00")].sItemNameStringId_string] = "Chapter " + i + " Completed{End}";
            }
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Equip/Item Data...", -1, 100);
            items.SaveWDB(@"\db\resident\item.wdb");

        }

        private string GetItemName(string itemID)
        {
            TextRando textRando = Randomizers.Get<TextRando>();
            string name = textRando.mainSysUS[items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
                name = name.Substring(0, name.IndexOf("{End}"));

            return name;
        }

        public class ItemData : CSVDataRow
        {
            [RowIndex(0)]
            public string ID { get; set; }
            [RowIndex(1)]
            public string Name { get; set; }
            [RowIndex(2)]
            public string Category { get; set; }
            [RowIndex(3)]
            public int Rank { get; set; }
            [RowIndex(4)]
            public string DefaultShop { get; set; }
            [RowIndex(5)]
            public List<string> Traits { get; set; }
            public int SortIndex { get; set; }
            [RowIndex(6)]
            public int OverrideBuy { get; set; }
            public ItemData(string[] row) : base(row)
            {
            }
        }
    }
}
