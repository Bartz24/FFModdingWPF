using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using static FF13Rando.Enums;

namespace FF13Rando
{
    public class EquipRando : Randomizer
    {
        public DataStoreWDB<DataStoreItem> items = new DataStoreWDB<DataStoreItem>();
        public DataStoreWDB<DataStoreEquip> equip = new DataStoreWDB<DataStoreEquip>();

        public Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();
        public Dictionary<string, PassiveData> passiveData = new Dictionary<string, PassiveData>();

        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Equip/Item Data...", -1, 100);
            items.LoadWDB("13", @"\db\resident\item.wdb");
            equip.LoadWDB("13", @"\db\resident\item_weapon.wdb");

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

            FileHelpers.ReadCSVFile(@"data\passives.csv", row =>
            {
                PassiveData p = new PassiveData(row);
                passiveData.Add(p.Name, p);
            }, FileHelpers.CSVFileHeader.HasHeader);

            itemData.Values.Where(i => i.OverrideBuy != -1).ForEach(i => items[i.ID].u16BuyPrice = (uint)i.OverrideBuy);
            equip.Values.Where(e => e.ID.StartsWith("wea_") && e.ID.EndsWith("_000")).ForEach(e =>
            {
                e.u8StrengthIncrease = 40;
                e.u8MagicIncrease = 40;
            });
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

            RandomizeEquipmentPassivesAndStats();
        }

        private void RandomizeEquipmentPassivesAndStats()
        {
            Dictionary<PassiveData, int> passiveDistribution = equip.Values.Select(e => GetEquipPassive(e)).GroupBy(p => p).ToDictionary(g => g.Key, g => g.Count());
            HashSet<DataStoreEquip> equipHitPassiveCap = new HashSet<DataStoreEquip>();

            List<DataStoreEquip> equipsSorted = equip.Values.OrderBy(e => GetEquipUpgradeDepth(e)).ToList();

            equipsSorted.ForEach(e =>
            {
                bool isAccessory = itemData[e.ID].Category == "Accessory";
                if (FF13Flags.Equip.RandEquipPassives.FlagEnabled)
                {
                    FF13Flags.Equip.RandEquipPassives.SetRand();
                    PassiveData original = GetEquipPassive(e);

                    if (original == null)
                        throw new Exception("Unknown passive found");

                    // Use a random parent for equipment that can have multiple parents
                    DataStoreEquip parent = GetRandomEquipParent(e);
                    PassiveData parentPassive = parent == null ? null : GetEquipPassive(parent);

                    PassiveData newPassive;
                    if (FF13Flags.Equip.EquipSamePassiveCategory.SelectedIndex == 3)
                    {
                        newPassive = RandomNum.SelectRandomWeighted(passiveData.Values.ToList(), p => isAccessory && p.Name == "None" ? 0 : passiveDistribution[p]);
                    }
                    else if (parent == null || (FF13Flags.Equip.EquipSamePassiveCategory.SelectedIndex == 2 || FF13Flags.Equip.EquipSamePassiveCategory.SelectedIndex == 1 && isAccessory) && equipHitPassiveCap.Contains(parent))
                    {
                        newPassive = RandomNum.SelectRandomWeighted(passiveData.Values.ToList(), p => isAccessory && p.Name == "None" || p == parentPassive ? 0 : 1);
                    }
                    else
                    {
                        newPassive = parentPassive;
                        // Upgrade in same category if exists
                        if (!string.IsNullOrEmpty(newPassive.Upgrade))
                        {
                            newPassive = passiveData[newPassive.Upgrade];
                        }
                    }

                    int strMagAvg = (int)((original.StrengthMult > 0 ? e.i8StrengthInitial / original.StrengthMult : 0) + (original.MagicMult > 0 ? e.i8MagicInitial / original.MagicMult : 0)) / 2;
                    int strMagIncreaseAvg = (int)((original.StrengthMult > 0 ? e.u8StrengthIncrease / original.StrengthMult : 0) + (original.MagicMult > 0 ? e.u8MagicIncrease / original.MagicMult : 0)) / 2;

                    e.i8StrengthInitial = (short)Math.Ceiling(strMagAvg * newPassive.StrengthMult);
                    e.u8StrengthIncrease = (ushort)Math.Ceiling(strMagIncreaseAvg * newPassive.StrengthMult);
                    e.i8MagicInitial = (short)Math.Ceiling(strMagAvg * newPassive.MagicMult);
                    e.u8MagicIncrease = (ushort)Math.Ceiling(strMagIncreaseAvg * newPassive.MagicMult);

                    e.sPassive_string = newPassive.ID;
                    e.sPassiveDisplayName_string = newPassive.DisplayNameID;
                    e.sHelpDisplay_string = newPassive.HelpID;
                    e.u1StatType1 = (byte)newPassive.StatType1;
                    e.u8StatType2 = (ushort)newPassive.StatType2;
                    e.i8StatInitial = (short)CalculateInitialFromRank(newPassive, items[e.ID].Rank, newPassive.StatInitial == newPassive.MaxValue ? newPassive.MaxValue : int.MinValue);
                    int maxTarget = (int)Math.Min(CalculateInitialFromRank(newPassive, items[e.ID].Rank + (2f + 0.01f * e.u1MaxLevel), e.i8StatInitial), newPassive.MaxValue);
                    e.u8StatIncrease = (ushort)((maxTarget - e.i8StatInitial) / (e.u1MaxLevel - 1));
                    if (e.u8StatIncrease == 0 && e.i8StatInitial < newPassive.MaxValue)
                    {
                        if ((e.u1MaxLevel - 1) + e.i8StatInitial < newPassive.MaxValue)
                        {
                            e.u8StatIncrease = 1;
                        }
                        else
                        {
                            e.i8StatInitial = (short)RandomNum.RandInt(e.i8StatInitial, maxTarget);
                            equipHitPassiveCap.Add(e);
                        }
                    }
                    else if (newPassive.StatInitial == newPassive.MaxValue && string.IsNullOrEmpty(newPassive.Upgrade))
                    {
                        equipHitPassiveCap.Add(e);
                    }
                    RandomNum.ClearRand();
                }

                if (FF13Flags.Equip.RandEquipStats.FlagEnabled)
                {
                    FF13Flags.Equip.RandEquipStats.SetRand();
                    if (e.i8StrengthInitial > 0 && e.i8MagicInitial > 0)
                    {
                        // Weapons only have one parent (for now, but this should still work if it goes random)
                        DataStoreEquip parent = GetRandomEquipParent(e);
                        PassiveData passive = GetEquipPassive(e);
                        PassiveData parentPassive = parent == null ? null : GetEquipPassive(parent);
                        int[] oldStats = new int[] { e.i8StrengthInitial, e.i8MagicInitial };

                        // If no parent or new passive, use new ratio for str/mag
                        if (parent == null || GetRandomPassiveRoot(passive) != GetRandomPassiveRoot(parentPassive))
                        {
                            (int, int)[] bounds =
                                {
                                (0, 99999),
                                (0, 99999)
                            };
                            float[] weights = { passive.StrengthMult, passive.MagicMult };
                            int[] chances = { 1, 1 };
                            int[] zeros = { 5, 5 };
                            int[] negs = { 0, 0 };
                            StatPoints statPoints = new StatPoints(bounds, weights, chances, zeros, negs, 0.5f);
                            statPoints.Randomize(oldStats);

                            e.i8StrengthInitial = (short)statPoints[0];
                            e.i8MagicInitial = (short)statPoints[1];
                        }
                        // Otherwise use previous ratio
                        else
                        {
                            int strMagAvg = (e.i8StrengthInitial + e.i8MagicInitial) / 2;
                            int strMagParentAvg = (parent.i8StrengthInitial + parent.i8MagicInitial) / 2;

                            e.i8StrengthInitial = (short)Math.Ceiling((double)strMagAvg * parent.i8StrengthInitial / strMagParentAvg);
                            e.i8MagicInitial = (short)Math.Ceiling((double)strMagAvg * parent.i8MagicInitial / strMagParentAvg);
                        }

                        e.u8StrengthIncrease = (ushort)Math.Ceiling(e.u8StrengthIncrease * (float)e.i8StrengthInitial / oldStats[0]);
                        e.u8MagicIncrease = (ushort)Math.Ceiling(e.u8MagicIncrease * (float)e.i8MagicInitial / oldStats[1]);
                    }
                    RandomNum.ClearRand();
                }

                if (FF13Flags.Equip.RandEquipSynthGroup.FlagEnabled)
                {
                    FF13Flags.Equip.RandEquipSynthGroup.SetRand();
                    items[e.ID].SynthesisGroup = (byte)RandomNum.SelectRandom(((SynthesisGroup[])Enum.GetValues(typeof(SynthesisGroup))).ToList());
                    RandomNum.ClearRand();
                }
            });
        }

        private double CalculateInitialFromRank(PassiveData newPassive, float rank, int min)
        {
            double center = newPassive.StatInitial * Math.Pow(newPassive.RankConstB, rank - 1) + newPassive.RankConstP * (rank - 1);
            return RandomNum.RandInt((int)Math.Max(Math.Round(center - Math.Abs(center) * 0.25), min), (int)Math.Min(Math.Round(center + Math.Abs(center) * 0.25), newPassive.MaxValue));
        }

        private PassiveData GetEquipPassive(DataStoreEquip e)
        {
            return passiveData.Values.Where(p =>
            {
                return p.ID == e.sPassive_string && p.DisplayNameID == e.sPassiveDisplayName_string && p.StatType1 == e.u1StatType1 && p.StatType2 == e.u8StatType2;
            }).FirstOrDefault();
        }

        private PassiveData GetRandomPassiveRoot(PassiveData p)
        {
            PassiveData parent = passiveData.Values.Where(p2 => p2.Upgrade == p.Name).Shuffle().DefaultIfEmpty(p).First();
            if (p == parent)
                return p;
            
            return GetRandomPassiveRoot(parent);
        }

        private DataStoreEquip GetRandomEquipParent(DataStoreEquip e)
        {
            return equip.Values.Where(e2 => e2.sUpgradeInto_string == e.ID).Shuffle().FirstOrDefault();
        }

        private int GetEquipUpgradeDepth(DataStoreEquip e)
        {
            int maxDepth = 0;

            int newDepth = equip.Values
                .Where(e2 => e2.sUpgradeInto_string == e.ID)
                .Select(e2 => GetEquipUpgradeDepth(e2) + 1)
                .DefaultIfEmpty(0)
                .Max();

            return Math.Max(newDepth, maxDepth);
        }

        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();

            HTMLPage page = new HTMLPage("Equipment", "template/documentation.html");

            page.HTMLElements.Add(new Table("Weapons", (new string[] { "Name", "Strength", "Magic", "Passive", "Synthesis Group" }).ToList(), (new int[] { 20, 20, 20, 20, 20 }).ToList(), equip.Values.Where(w => itemData[w.ID].Category == "Weapon").Select(w =>
            {
                return GenerateRowContents(w);
            }).ToList()));

            page.HTMLElements.Add(new Table("Accessories", (new string[] { "Name", "Passive", "Synthesis Group" }).ToList(), (new int[] { 34, 33, 33 }).ToList(), equip.Values.Where(a => itemData[a.ID].Category == "Accessory").Select(a =>
            {
                return GenerateRowContents(a);
            }).ToList()));

            pages.Add("equipment", page);
            return pages;

            List<string> GenerateRowContents(DataStoreEquip e)
            {
                string name = itemData[e.ID].Name;
                string strength = $"{e.i8StrengthInitial}";
                if (e.u8StrengthIncrease > 0)
                {
                    strength += $"-{e.i8StrengthInitial + e.u8StrengthIncrease * (e.u1MaxLevel - 1)} (+{e.u8StrengthIncrease}/Lv)";
                }
                string magic = $"{e.i8MagicInitial}";
                if (e.u8MagicIncrease > 0)
                {
                    magic += $"-{e.i8MagicInitial + e.u8MagicIncrease * (e.u1MaxLevel - 1)} (+{e.u8MagicIncrease}/Lv)";
                }
                string passive = GetEquipPassive(e).Name;
                if (passive.Contains("X"))
                {
                    string statRange = e.i8StatInitial.ToString();
                    if (e.u8StatIncrease > 0)
                    {
                        statRange += $"-{e.i8StatInitial + e.u8StatIncrease * (e.u1MaxLevel - 1)}";
                        passive += $" (+{e.u8StatIncrease}/Lv)";
                    }
                    passive = passive.Replace("X", statRange);
                }

                if (itemData[e.ID].Category == "Accessory")
                    return new string[] { name, passive, ((SynthesisGroup)(items[e.ID].SynthesisGroup)).ToString().SeparateWords() }.ToList();
                else
                    return new string[] { name, strength, magic, passive, ((SynthesisGroup)(items[e.ID].SynthesisGroup)).ToString().SeparateWords() }.ToList();
            }
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Equip/Item Data...", -1, 100);
            items.SaveWDB(@"\db\resident\item.wdb");
            equip.SaveWDB(@"\db\resident\item_weapon.wdb");

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

        public class PassiveData : CSVDataRow
        {
            [RowIndex(0)]
            public string Name { get; set; }
            [RowIndex(1)]
            public string ID { get; set; }
            [RowIndex(2)]
            public float StrengthMult { get; set; }
            [RowIndex(3)]
            public float MagicMult { get; set; }
            [RowIndex(4)]
            public string DisplayNameID { get; set; }
            [RowIndex(5)]
            public string HelpID { get; set; }
            [RowIndex(6)]
            public int StatInitial { get; set; }
            [RowIndex(7)]
            public int StatType1 { get; set; }
            [RowIndex(8)]
            public int StatType2 { get; set; }
            [RowIndex(9)]
            public int MaxValue { get; set; }

            // b and p are used as follows:
            // stat = initial * b ^ (rank - 1) + p * (rank - 1)
            [RowIndex(10)]
            public float RankConstB { get; set; }
            [RowIndex(11)]
            public float RankConstP { get; set; }
            [RowIndex(12)]
            public string Upgrade { get; set; }
            public PassiveData(string[] row) : base(row)
            {
            }
        }
    }
}
