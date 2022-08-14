using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando
{
    public class EquipRando : Randomizer
    {
        public Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();
        public Dictionary<string, AugmentData> augmentData = new Dictionary<string, AugmentData>();
        public DataStoreBPEquip equip;
        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            FileHelpers.ReadCSVFile(@"data\items.csv", row =>
            {
                ItemData i = new ItemData(row);
                itemData.Add(i.ID, i);
            }, FileHelpers.CSVFileHeader.HasHeader);

            FileHelpers.ReadCSVFile(@"data\augments.csv", row =>
            {
                AugmentData a = new AugmentData(row);
                augmentData.Add(a.ID, a);
            }, FileHelpers.CSVFileHeader.HasHeader);

            equip = new DataStoreBPEquip();
            equip.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_013.bin"));
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (FF12Flags.Stats.EquipStats.FlagEnabled)
            {
                FF12Flags.Stats.EquipStats.SetRand();
                RandomizeStats();
                RandomNum.ClearRand();
            }

            if (FF12Flags.Stats.EquipElements.FlagEnabled)
            {
                FF12Flags.Stats.EquipElements.SetRand();
                RandomizeElements();
                RandomNum.ClearRand();
            }

            if (FF12Flags.Stats.EquipAugments.FlagEnabled)
            {
                FF12Flags.Stats.EquipAugments.SetRand();
                RandomizeAugments();
                RandomNum.ClearRand();
            }

            if (FF12Flags.Stats.EquipStatus.FlagEnabled)
            {
                FF12Flags.Stats.EquipStatus.SetRand();
                RandomizeStatusEffects();
                RandomNum.ClearRand();
            }
        }

        private void RandomizeStats()
        {
            foreach (DataStoreWeapon weapon in equip.EquipDataList.Where(w => w is DataStoreWeapon))
            {
                DataStoreAttribute attribute = equip.AttributeDataList[(int)weapon.AttributeOffset];
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(1, 255),
                        new Tuple<int, int>(0, 50),
                        new Tuple<int, int>(0, 5000),
                        new Tuple<int, int>(0, 500),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99)
                    };
                float[] weights = new float[] { weapon.Category == EquipCategory.Gun || weapon.Category == EquipCategory.Measure ? 4 : 1, 4, 2, 6, 4, 4, 8, 12 };
                int[] zeros = new int[] { 0, 90, 97, 97, 90, 90, 90, 90 };
                int[] negs = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                if (FF12Flags.Stats.EquipHiddenStats.Enabled)
                {
                    Tuple<int, int>[] hiddenBounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(0, 100),
                        new Tuple<int, int>(0, 100),
                        new Tuple<int, int>(RandomNum.RandInt(RandomNum.RandInt(-100, -40), -20), 0)
                    };
                    float[] hiddenWeights = new float[] { 4, 4, 1 };
                    int[] hiddenZeros = new int[] { 50, 5, 0 };
                    int[] hiddenNegs = new int[] { 0, 0, 100 };

                    bounds = bounds.Concat(hiddenBounds);
                    weights = weights.Concat(hiddenWeights);
                    zeros = zeros.Concat(hiddenZeros);
                    negs = negs.Concat(hiddenNegs);
                }

                statPoints = new StatPoints(bounds, weights, zeros, negs);

                int[] stats = new int[] { weapon.AttackPower, weapon.Evade, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed };

                if (FF12Flags.Stats.EquipHiddenStats.Enabled)
                {
                    int[] newStats = new int[] { weapon.KnockbackChance, weapon.ComboChance, -weapon.ChargeTime };
                    stats = stats.Concat(newStats);
                }

                statPoints.Randomize(stats);

                weapon.AttackPower = (byte)statPoints[0];
                weapon.Evade = (byte)statPoints[1];
                attribute.HP = (ushort)statPoints[2];
                attribute.MP = (ushort)statPoints[3];
                attribute.Strength = (byte)statPoints[4];
                attribute.MagickPower = (byte)statPoints[5];
                attribute.Vitality = (byte)statPoints[6];
                attribute.Speed = (byte)statPoints[7];

                if (FF12Flags.Stats.EquipHiddenStats.Enabled)
                {
                    weapon.KnockbackChance = (byte)statPoints[8];
                    weapon.ComboChance = (byte)statPoints[9];
                    weapon.ChargeTime = (byte)-statPoints[10];
                }
            }
            foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(a => a is DataStoreAmmo))
            {
                DataStoreAttribute attribute = equip.AttributeDataList[(int)ammo.AttributeOffset];
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(1, 10),
                        new Tuple<int, int>(0, 10),
                        new Tuple<int, int>(0, 500),
                        new Tuple<int, int>(0, 50),
                        new Tuple<int, int>(0, 9),
                        new Tuple<int, int>(0, 9),
                        new Tuple<int, int>(0, 9),
                        new Tuple<int, int>(0, 9)
                    };
                float[] weights = new float[] { 1, 4, 3, 8, 6, 6, 10, 15 };
                int[] zeros = new int[] { 0, 95, 99, 99, 95, 95, 95, 95 };
                int[] negs = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { ammo.AttackPower, ammo.Evade, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed });

                ammo.AttackPower = (byte)statPoints[0];
                ammo.Evade = (byte)statPoints[1];
                attribute.HP = (ushort)statPoints[2];
                attribute.MP = (ushort)statPoints[3];
                attribute.Strength = (byte)statPoints[4];
                attribute.MagickPower = (byte)statPoints[5];
                attribute.Vitality = (byte)statPoints[6];
                attribute.Speed = (byte)statPoints[7];
            }
            foreach (DataStoreShield shield in equip.EquipDataList.Where(s => s is DataStoreShield))
            {
                DataStoreAttribute attribute = equip.AttributeDataList[(int)shield.AttributeOffset];
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(0, 50),
                        new Tuple<int, int>(0, 50),
                        new Tuple<int, int>(0, 5000),
                        new Tuple<int, int>(0, 500),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99)
                    };
                float[] weights = new float[] { 4, 4, 1, 3, 8, 8, 6, 10 };
                int[] zeros = new int[] { 50, 50, 90, 95, 97, 97, 90, 90 };
                int[] negs = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { shield.Evade, shield.MagickEvade, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed });

                shield.Evade = (byte)statPoints[0];
                shield.MagickEvade = (byte)statPoints[1];
                attribute.HP = (ushort)statPoints[2];
                attribute.MP = (ushort)statPoints[3];
                attribute.Strength = (byte)statPoints[4];
                attribute.MagickPower = (byte)statPoints[5];
                attribute.Vitality = (byte)statPoints[6];
                attribute.Speed = (byte)statPoints[7];
            }
            foreach (DataStoreArmor armor in equip.EquipDataList.Where(a => a is DataStoreArmor && a.Category != EquipCategory.Accessory && a.Category != EquipCategory.AccessoryCrown))
            {
                DataStoreAttribute attribute = equip.AttributeDataList[(int)armor.AttributeOffset];
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(0, 120),
                        new Tuple<int, int>(0, 120),
                        new Tuple<int, int>(0, 5000),
                        new Tuple<int, int>(0, 500),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99)
                    };
                float[] weights = new float[] { 8, 8, 1, 3, 20, 20, 15, 15 };
                int[] zeros = new int[] { 50, 50, 90, 90, 90, 90, 90, 97 };
                int[] negs = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { armor.Defense, armor.MagickResist, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed });

                armor.Defense = (byte)statPoints[0];
                armor.MagickResist = (byte)statPoints[1];
                attribute.HP = (ushort)statPoints[2];
                attribute.MP = (ushort)statPoints[3];
                attribute.Strength = (byte)statPoints[4];
                attribute.MagickPower = (byte)statPoints[5];
                attribute.Vitality = (byte)statPoints[6];
                attribute.Speed = (byte)statPoints[7];
            }
            foreach (DataStoreArmor armor in equip.EquipDataList.Where(a => a is DataStoreArmor && (a.Category == EquipCategory.Accessory || a.Category == EquipCategory.AccessoryCrown)))
            {
                DataStoreAttribute attribute = equip.AttributeDataList[(int)armor.AttributeOffset];
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(0, 120),
                        new Tuple<int, int>(0, 120),
                        new Tuple<int, int>(0, 5000),
                        new Tuple<int, int>(0, 500),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99),
                        new Tuple<int, int>(0, 99)
                    };
                float[] weights = new float[] { 8, 8, 1, 3, 12, 12, 12, 15 };
                int[] zeros = new int[] { 95, 95, 90, 90, 95, 95, 95, 90 };
                int[] negs = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { armor.Defense, armor.MagickResist, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed });

                armor.Defense = (byte)statPoints[0];
                armor.MagickResist = (byte)statPoints[1];
                attribute.HP = (ushort)statPoints[2];
                attribute.MP = (ushort)statPoints[3];
                attribute.Strength = (byte)statPoints[4];
                attribute.MagickPower = (byte)statPoints[5];
                attribute.Vitality = (byte)statPoints[6];
                attribute.Speed = (byte)statPoints[7];
            }
        }

        private void RandomizeElements()
        {
            foreach (DataStoreWeapon weapon in equip.EquipDataList.Where(w => w is DataStoreWeapon))
            {
                int count = weapon.Elements.EnumToList().Count;
                List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
                weapon.Elements = 0;
                newElem.Shuffle().Take(count).ForEach(e => weapon.Elements |= e);
            }

            foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(a => a is DataStoreAmmo))
            {
                int count = ammo.Elements.EnumToList().Count;
                List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
                ammo.Elements = 0;
                newElem.Shuffle().Take(count).ForEach(e => ammo.Elements |= e);
            }

            foreach (DataStoreAttribute attribute in equip.AttributeDataList)
            {
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3),
                        new Tuple<int, int>(-1, 3)
                    };
                float[] weights = new float[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                int[] zeros = new int[] { 90, 90, 90, 90, 90, 90, 90, 90 };
                int[] negs = new int[] { 10, 10, 10, 10, 10, 10, 10, 10 };
                statPoints = new StatPoints(bounds, weights, zeros, negs);

                List<Element> elements = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();

                statPoints.Randomize(elements.Select(e => GetElementLevel(e, attribute)).ToArray());

                attribute.ElementsAbsorb = 0;
                attribute.ElementsHalf = 0;
                attribute.ElementsImmune = 0;
                attribute.ElementsWeak = 0;

                elements.ForEach(e => SetElementLevel(e, statPoints[elements.IndexOf(e)], attribute));

                int count = attribute.ElementsBoost.EnumToList().Count;
                List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
                attribute.ElementsBoost = 0;
                newElem.Shuffle().Take(count).ForEach(e => attribute.ElementsBoost |= e);
            }
        }

        private int GetElementLevel(Element element, DataStoreAttribute attribute)
        {
            if (attribute.ElementsAbsorb.HasFlag(element))
                return 3;
            if (attribute.ElementsImmune.HasFlag(element))
                return 2;
            if (attribute.ElementsHalf.HasFlag(element))
                return 1;
            if (attribute.ElementsWeak.HasFlag(element))
                return -1;
            return 0;
        }
        private void SetElementLevel(Element element, int level, DataStoreAttribute attribute)
        {
            switch (level)
            {
                case 3:
                    attribute.ElementsAbsorb |= element;
                    break;
                case 2:
                    attribute.ElementsImmune |= element;
                    break;
                case 1:
                    attribute.ElementsHalf |= element;
                    break;
                case -1:
                    attribute.ElementsWeak |= element;
                    break;
            }
        }

        private void RandomizeAugments()
        {
            foreach (DataStoreArmor armor in equip.EquipDataList.Where(a => a is DataStoreArmor))
            {
                armor.AugmentOffset = (byte)augmentData.Values.Where(a => a.Traits.Contains("Equip")).Shuffle().First().IntID;
            }
        }

        private void RandomizeStatusEffects()
        {
            foreach (DataStoreWeapon weapon in equip.EquipDataList.Where(w => w is DataStoreWeapon))
            {
                int count = weapon.StatusEffects.EnumToList().Count;
                List<Bartz24.FF12.Status> newStatus = Enum.GetValues(typeof(Bartz24.FF12.Status)).Cast<Bartz24.FF12.Status>().ToList();
                weapon.StatusEffects = 0;

                for (int i = 0; i < count; i++)
                {
                    Bartz24.FF12.Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightOnHit);
                    newStatus.Remove(next);
                    weapon.StatusEffects |= next;
                }
            }
            foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(w => w is DataStoreAmmo))
            {
                int count = ammo.StatusEffects.EnumToList().Count;
                List<Bartz24.FF12.Status> newStatus = Enum.GetValues(typeof(Bartz24.FF12.Status)).Cast<Bartz24.FF12.Status>().ToList();
                ammo.StatusEffects = 0;

                for (int i = 0; i < count; i++)
                {
                    Bartz24.FF12.Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightOnHit);
                    newStatus.Remove(next);
                    ammo.StatusEffects |= next;
                }
            }
            for (int id = 0; id < equip.AttributeDataList.Count; id++)
            {
                DataStoreAttribute attribute = equip.AttributeDataList[id];
                int countEquip = attribute.StatusEffectsOnEquip.EnumToList().Count;
                List<Bartz24.FF12.Status> newStatus = Enum.GetValues(typeof(Bartz24.FF12.Status)).Cast<Bartz24.FF12.Status>().ToList();
                attribute.StatusEffectsOnEquip = 0;

                for (int i = 0; i < countEquip; i++)
                {
                    Bartz24.FF12.Status next = RandomNum.SelectRandomWeighted(newStatus, s => StatusEffectWeightOnEquip(s, id));
                    newStatus.Remove(next);
                    attribute.StatusEffectsOnEquip |= next;
                }

                int countImmune = attribute.StatusEffectsImmune.EnumToList().Count;
                attribute.StatusEffectsImmune = 0;
                for (int i = 0; i < countImmune; i++)
                {
                    Bartz24.FF12.Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightImmune);
                    newStatus.Remove(next);
                    attribute.StatusEffectsImmune |= next;
                }
            }
        }

        private long StatusEffectWeightOnHit(Bartz24.FF12.Status status)
        {
            if (status == Bartz24.FF12.Status.Stone || status == Bartz24.FF12.Status.CriticalHP)
                return 0;
            if (status == Bartz24.FF12.Status.Death || status == Bartz24.FF12.Status.Petrify || status == Bartz24.FF12.Status.Stop || status == Bartz24.FF12.Status.Doom || status == Bartz24.FF12.Status.Disable || status == Bartz24.FF12.Status.Disease || status == Bartz24.FF12.Status.Bubble || status == Bartz24.FF12.Status.XZone)
                return 1;
            if (status == Bartz24.FF12.Status.Lure || status == Bartz24.FF12.Status.Protect || status == Bartz24.FF12.Status.Shell || status == Bartz24.FF12.Status.Haste || status == Bartz24.FF12.Status.Bravery || status == Bartz24.FF12.Status.Faith || status == Bartz24.FF12.Status.Reflect || status == Bartz24.FF12.Status.Berserk)
                return 3;
            if (status == Bartz24.FF12.Status.Sleep || status == Bartz24.FF12.Status.Confuse || status == Bartz24.FF12.Status.Sap || status == Bartz24.FF12.Status.Reverse || status == Bartz24.FF12.Status.Immobilize || status == Bartz24.FF12.Status.Libra)
                return 4;
            return 10;
        }

        private long StatusEffectWeightOnEquip(Bartz24.FF12.Status status, int id)
        {
            if (id == 0 || id == 0x183)
            {
                if (status == Bartz24.FF12.Status.Immobilize || status == Bartz24.FF12.Status.Confuse || status == Bartz24.FF12.Status.Sleep || status == Bartz24.FF12.Status.Disable || status == Bartz24.FF12.Status.Doom || status == Bartz24.FF12.Status.Stop || status == Bartz24.FF12.Status.Petrify || status == Bartz24.FF12.Status.Berserk)
                    return 0;
            }
            if (status == Bartz24.FF12.Status.Death || status == Bartz24.FF12.Status.Stone || status == Bartz24.FF12.Status.CriticalHP || status == Bartz24.FF12.Status.XZone)
                return 0;
            if (status == Bartz24.FF12.Status.Petrify || status == Bartz24.FF12.Status.Doom || status == Bartz24.FF12.Status.Stop || status == Bartz24.FF12.Status.Reverse || status == Bartz24.FF12.Status.Disease)
                return 1;
            if (status == Bartz24.FF12.Status.Immobilize || status == Bartz24.FF12.Status.Confuse || status == Bartz24.FF12.Status.Sleep || status == Bartz24.FF12.Status.Disable || status == Bartz24.FF12.Status.Bubble)
                return 3;
            if (status == Bartz24.FF12.Status.Sap || status == Bartz24.FF12.Status.Libra)
                return 4;
            return 10;
        }

        private long StatusEffectWeightImmune(Bartz24.FF12.Status status)
        {
            if (status == Bartz24.FF12.Status.Death || status == Bartz24.FF12.Status.Stone || status == Bartz24.FF12.Status.CriticalHP)
                return 0;
            if (status == Bartz24.FF12.Status.Lure || status == Bartz24.FF12.Status.Protect || status == Bartz24.FF12.Status.Shell || status == Bartz24.FF12.Status.Haste || status == Bartz24.FF12.Status.Bravery || status == Bartz24.FF12.Status.Faith || status == Bartz24.FF12.Status.Reflect || status == Bartz24.FF12.Status.Berserk || status == Bartz24.FF12.Status.Bubble)
                return 1;
            if (status == Bartz24.FF12.Status.Sap || status == Bartz24.FF12.Status.Libra)
                return 2;
            if (status == Bartz24.FF12.Status.Immobilize || status == Bartz24.FF12.Status.Confuse || status == Bartz24.FF12.Status.Sleep || status == Bartz24.FF12.Status.Disable || status == Bartz24.FF12.Status.Petrify || status == Bartz24.FF12.Status.Doom || status == Bartz24.FF12.Status.Stop || status == Bartz24.FF12.Status.Reverse || status == Bartz24.FF12.Status.Disease || status == Bartz24.FF12.Status.XZone)
                return 4;
            return 10;
        }

        public override void Save()
        {
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_013.bin", equip.Data);
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
        public class AugmentData
        {
            public string Name { get; set; }
            public int IntID { get; set; }
            public string ID { get; set; }
            public string Description { get; set; }
            public List<string> Traits { get; set; }
            public AugmentData(string[] row)
            {
                Name = row[0];
                IntID = Convert.ToInt32(row[1]);
                ID = row[1];
                Description = row[2];
                Traits = row[3].Split("|").Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
    }
}
