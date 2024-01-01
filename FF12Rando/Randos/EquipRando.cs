using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public partial class EquipRando : Randomizer
{
    public Dictionary<string, ItemData> itemData = new();
    public Dictionary<string, AugmentData> augmentData = new();
    public DataStoreBPEquip equip;
    public EquipRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Item/Equip Data...");
        FileHelpers.ReadCSVFile(@"data\items.csv", row =>
        {
            ItemData i = new(row);
            itemData.Add(i.ID, i);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\augments.csv", row =>
        {
            AugmentData a = new(row);
            augmentData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);

        equip = new DataStoreBPEquip();
        equip.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_013.bin"));
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Item/Equip Data...");
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
            (int, int)[] bounds = {
                (1, 255),
                (0, 50),
                (0, 5000),
                (0, 500),
                (0, 99),
                (0, 99),
                (0, 99),
                (0, 99)
            };
            int atkCost = weapon.Category is EquipCategory.Gun or EquipCategory.Measure || weapon.IntID == 0x10C1
                ? 4 : 
                weapon.Category is EquipCategory.Pole 
                ? 2 :
                1;
            float[] weights = { atkCost, 0.6f, 0.1f, 0.4f, 1, 1, 1, 2 };
            int[] chances = { 70, 30, 3, 3, 4, 4, 3, 1 };
            int[] zeros = { 0, 70, 97, 97, 90, 90, 90, 90 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };

            if (FF12Flags.Stats.EquipHiddenStats.Enabled)
            {
                (int, int)[] hiddenBounds = {
                    (0, 100),
                    (0, 100),
                    (RandomNum.RandInt(RandomNum.RandInt(-100, -40), -20), -10)
                };
                float[] hiddenWeights = { 0.8f, 0.8f, 4 };
                int[] hiddenChances = { 30, 30, 200 };
                int[] hiddenZeros = { 10, 5, 0 };
                int[] hiddenNegs = { 0, 0, 100 };

                bounds = bounds.Concat(hiddenBounds);
                chances = chances.Concat(hiddenChances);
                weights = weights.Concat(hiddenWeights);
                zeros = zeros.Concat(hiddenZeros);
                negs = negs.Concat(hiddenNegs);
            }

            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);

            int[] stats = { weapon.AttackPower, weapon.Evade, attribute.HP, attribute.MP, attribute.Strength, attribute.MagickPower, attribute.Vitality, attribute.Speed };

            if (FF12Flags.Stats.EquipHiddenStats.Enabled)
            {
                int[] newStats = { weapon.KnockbackChance, weapon.ComboChance, -weapon.ChargeTime };
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
            (int, int)[] bounds = {
                (1, 10),
                (0, 10),
                (0, 500),
                (0, 50),
                (0, 9),
                (0, 9),
                (0, 9),
                (0, 9)
            };
            float[] weights = { 1, 2, 0.1f, 0.4f, 1, 1, 1, 2 };
            int[] chances = { 90, 1, 2, 2, 3, 3, 2, 1 };
            int[] zeros = { 0, 95, 99, 99, 95, 95, 95, 95 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
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
            (int, int)[] bounds = {
                (0, 70),
                (0, 70),
                (0, 5000),
                (0, 500),
                (0, 99),
                (0, 99),
                (0, 99),
                (0, 99)
            };
            float[] weights = { 2, 2, 0.1f, 0.4f, 1, 1, 1, 2 };
            int[] chances = { 30, 30, 10, 10, 3, 3, 5, 5 };
            int[] zeros = { 50, 50, 90, 95, 97, 97, 90, 90 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
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
            (int, int)[] bounds = {
                (0, 120),
                (0, 120),
                (0, 5000),
                (0, 500),
                (0, 99),
                (0, 99),
                (0, 99),
                (0, 99)
            };

            int defZeroChance, mResZeroChance;
            int type = RandomNum.RandInt(0, 99);
            if (type < 40)
            {
                defZeroChance = 0;
                mResZeroChance = 100;
            }
            else if (type < 80)
            {
                defZeroChance = 100;
                mResZeroChance = 0;
            }
            else if (type < 93)
            {
                defZeroChance = 0;
                mResZeroChance = 0;
            }
            else
            {
                defZeroChance = 100;
                mResZeroChance = 100;
            }
            bool singleStat = defZeroChance + mResZeroChance == 100;

            float[] weights = { 1.3f, 1.3f, 1 / 10f, 0.4f, 0.8f, 0.8f, 0.8f, 2 };
            int[] chances = { 30, 30, singleStat ? 20 : 10, singleStat ? 20 : 10, singleStat ? 20 : 10, singleStat ? 20 : 10, singleStat ? 20 : 10, 10 };
            int[] zeros = { defZeroChance, mResZeroChance, singleStat ? 60 : 80, singleStat ? 60 : 80, singleStat ? 60 : 80, singleStat ? 60 : 80, singleStat ? 60 : 80, singleStat ? 90 : 97 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
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
            (int, int)[] bounds = {
                (0, 120),
                (0, 120),
                (0, 5000),
                (0, 500),
                (0, 99),
                (0, 99),
                (0, 99),
                (0, 99)
            };
            float[] weights = { 1, 1, 1 / 10f, 0.4f, 0.8f, 0.8f, 0.8f, 2 };
            int[] chances = { 5, 5, 20, 20, 20, 20, 20, 20 };
            int[] zeros = { 95, 95, 90, 90, 95, 95, 95, 90 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
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
            int count = GetRandomElementCount(weapon.Elements, 1);
            List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
            weapon.Elements = 0;
            newElem.Shuffle().Take(count).ForEach(e => weapon.Elements |= e);
        }

        foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(a => a is DataStoreAmmo))
        {
            int count = GetRandomElementCount(ammo.Elements, 1);
            List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
            ammo.Elements = 0;
            newElem.Shuffle().Take(count).ForEach(e => ammo.Elements |= e);
        }

        foreach (DataStoreAttribute attribute in equip.AttributeDataList)
        {
            attribute.ElementsWeak = GetNewElementVal(attribute.ElementsWeak, 95);
            attribute.ElementsHalf = GetNewElementVal(attribute.ElementsHalf, 97);
            attribute.ElementsImmune = GetNewElementVal(attribute.ElementsImmune, 98);
            attribute.ElementsAbsorb = GetNewElementVal(attribute.ElementsAbsorb, 99);
            attribute.ElementsBoost = GetNewElementVal(attribute.ElementsBoost, 95);

            // Remove duplicate elements in higher up tiers
            attribute.ElementsHalf &= ~attribute.ElementsWeak;
            attribute.ElementsImmune &= ~(attribute.ElementsWeak | attribute.ElementsHalf);
            attribute.ElementsAbsorb &= ~(attribute.ElementsWeak | attribute.ElementsHalf | attribute.ElementsImmune);
        }
    }

    private Element GetNewElementVal(Element element, int sameChance)
    {
        int count = GetRandomElementCount(element, sameChance);
        List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
        element = 0;
        newElem.Shuffle().Take(count).ForEach(e => element |= e);
        return element;
    }

    private int GetRandomElementCount(Element elem, int max = 8, int sameChance = 80)
    {
        int count = elem.EnumToList().Count;

        for (int i = 0; i < 2; i++)
        {
            if (RandomNum.RandInt(0, 99) < sameChance)
            {
                return count;
            }

            count = RandomNum.RandIntBounds(0, max, count, 1);
        }

        return count;
    }

    private int GetElementLevel(Element element, DataStoreAttribute attribute)
    {
        return attribute.ElementsAbsorb.HasFlag(element)
            ? 3
            : attribute.ElementsImmune.HasFlag(element)
            ? 2
            : attribute.ElementsHalf.HasFlag(element) ? 1 : attribute.ElementsWeak.HasFlag(element) ? -1 : 0;
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
            if (RandomNum.RandInt(0, 99) < (armor.Category is EquipCategory.Accessory or EquipCategory.AccessoryCrown ? 80 : 20))
            {
                armor.AugmentOffset = (byte)augmentData.Values.Where(a => a.Traits.Contains("Equip")).Shuffle().First().IntID;
            }
            else
            {
                armor.AugmentOffset = 0xFF; // None
            }
        }
    }

    private void RandomizeStatusEffects()
    {
        foreach (DataStoreWeapon weapon in equip.EquipDataList.Where(w => w is DataStoreWeapon))
        {
            int count = GetRandomStatsEffectCount(weapon.StatusEffects, 90);
            List<Status> newStatus = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            weapon.StatusEffects = 0;

            if (count > 0 && weapon.StatusChance == 0)
            {
                weapon.StatusChance = (byte)(RandomNum.RandInt(1, 4) * 5);
            }
            else if (count == 0)
            {
                weapon.StatusChance = 0;
            }

            for (int i = 0; i < count; i++)
            {
                if (newStatus.Count == 0)
                {
                    break;
                }

                Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightOnHit);
                newStatus.Remove(next);
                weapon.StatusEffects |= next;
            }
        }

        foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(w => w is DataStoreAmmo))
        {
            int count = GetRandomStatsEffectCount(ammo.StatusEffects, 70);
            List<Status> newStatus = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            ammo.StatusEffects = 0;

            if (count > 0 && ammo.StatusChance == 0)
            {
                ammo.StatusChance = (byte)(RandomNum.RandInt(1, 4) * 5);
            }
            else if (count == 0)
            {
                ammo.StatusChance = 0;
            }

            for (int i = 0; i < count; i++)
            {
                if (newStatus.Count == 0)
                {
                    break;
                }

                Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightOnHit);
                newStatus.Remove(next);
                ammo.StatusEffects |= next;
            }
        }

        for (int id = 0; id < equip.AttributeDataList.Count; id++)
        {
            DataStoreAttribute attribute = equip.AttributeDataList[id];
            int countEquip = GetRandomStatsEffectCount(attribute.StatusEffectsOnEquip, 95);
            List<Status> newStatus = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            attribute.StatusEffectsOnEquip = 0;

            for (int i = 0; i < countEquip; i++)
            {
                if (newStatus.Count == 0)
                {
                    break;
                }

                Status next = RandomNum.SelectRandomWeighted(newStatus, s => StatusEffectWeightOnEquip(s, id));
                newStatus.Remove(next);
                attribute.StatusEffectsOnEquip |= next;
            }

            int countImmune = GetRandomStatsEffectCount(attribute.StatusEffectsImmune, 98);
            attribute.StatusEffectsImmune = 0;
            for (int i = 0; i < countImmune; i++)
            {
                if (newStatus.Count == 0)
                {
                    break;
                }

                Status next = RandomNum.SelectRandomWeighted(newStatus, StatusEffectWeightImmune);
                newStatus.Remove(next);
                attribute.StatusEffectsImmune |= next;
            }
        }
    }

    private int GetRandomStatsEffectCount(Status status, int sameChance)
    {
        int count = status.EnumToList().Count;

        for (int i = 0; i < 3; i++)
        {
            if (RandomNum.RandInt(0, 99) < sameChance)
            {
                return count;
            }

            count = RandomNum.RandIntBounds(0, 32, count, 1);
        }

        return count;
    }

    private long StatusEffectWeightOnHit(Status status)
    {
        return status is Status.Stone or Status.CriticalHP
            ? 0
            : status is Status.Death or Status.Petrify or Status.Stop or Status.Doom or Status.Disable or Status.Disease or Status.Bubble or Status.XZone
            ? 1
            : status is Status.Lure or Status.Protect or Status.Shell or Status.Haste or Status.Bravery or Status.Faith or Status.Reflect or Status.Berserk
            ? 3
            : status is Status.Sleep or Status.Confuse or Status.Sap or Status.Reverse or Status.Immobilize or Status.Libra
            ? 4
            : 10;
    }

    private long StatusEffectWeightOnEquip(Status status, int id)
    {
        if (id is 0 or 0x183)
        {
            if (status is Status.Immobilize or Status.Confuse or Status.Sleep or Status.Disable or Status.Doom or Status.Stop or Status.Petrify or Status.Berserk)
            {
                return 0;
            }
        }

        return status is Status.Death or Status.Stone or Status.CriticalHP or Status.XZone or Status.Doom or Status.Petrify
            ? 0
            : status is Status.Reverse
            ? 1
            : status is Status.Stop or Status.Disease
            ? 2
            : status is Status.Immobilize or Status.Confuse or Status.Sleep or Status.Disable or Status.Bubble
            ? 6
            : status is Status.Sap or Status.Libra ? 8 : 20;
    }

    private long StatusEffectWeightImmune(Status status)
    {
        return status is Status.Death or Status.Stone or Status.CriticalHP
            ? 0
            : status is Status.Lure or Status.Protect or Status.Shell or Status.Haste or Status.Bravery or Status.Faith or Status.Reflect or Status.Berserk or Status.Bubble
            ? 1
            : status is Status.Sap or Status.Libra
            ? 2
            : status is Status.Immobilize or Status.Confuse or Status.Sleep or Status.Disable or Status.Petrify or Status.Doom or Status.Stop or Status.Reverse or Status.Disease or Status.XZone
            ? 4
            : 10;
    }

    private void GenerateDescriptionsFile()
    {
        string scriptFolder = $"{SetupData.Paths["12"]}\\x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig";
        if (!File.Exists(Path.Combine(scriptFolder, "us.lua.before_rando")))
        {
            File.Move(Path.Combine(scriptFolder, "us.lua"), Path.Combine(scriptFolder, "us.lua.before_rando"), true);
        }

        List<string> linesPage1 = new()
        {
            "local function config(contents)",
            "  local inventory = {",
        };

        List<string> linesPage2 = new()
        {
            "local function config(contents)",
            "  local inventory = {",
        };

        bool onePage = true;
        foreach (DataStoreWeapon weapon in equip.EquipDataList.Where(w => w is DataStoreWeapon))
        {
            DataStoreAttribute attribute = equip.AttributeDataList[(int)weapon.AttributeOffset];

            string info = "{" + GetEquipId(weapon);

            string displayPage1 = "";

            displayPage1 += $"Attack: {weapon.AttackPower}\\t";
            displayPage1 += $"Evade: {weapon.Evade}\\t";
            if (weapon.Elements.EnumToList().Count > 0 || weapon.StatusEffects.EnumToList().Count > 0 && weapon.StatusChance > 0)
            {
                List<string> onhit = new()
                {
                    GetElementsDisplay(weapon.Elements.EnumToList(), true),
                    weapon.StatusChance == 0 ? "" : $"{weapon.StatusChance}% {GetStatusDisplay(weapon.StatusEffects.EnumToList())}"
                };
                displayPage1 += $"On Hit: {string.Join(", ", onhit.Where(s => !string.IsNullOrEmpty(s)))}";
            }

            if (!onePage)
            {
                string displayPage2 = "";
                displayPage2 += displayPage1;

                // Page 1 only
                displayPage1 += "[1/2]";
                displayPage1 += "\\n";
                displayPage1 += $"Knockback: {weapon.KnockbackChance}%\\t";
                displayPage1 += $"Combo: {weapon.ComboChance}%\\t";
                displayPage1 += $"CT: {weapon.ChargeTime}\\t";
                displayPage1 += "\\n";
                displayPage1 += GetAttributeStatDisplay(attribute);

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");

                // Page 2 only
                displayPage2 += "[2/2]";
                displayPage2 += GetElementAttributeDisplayLine(attribute);
                displayPage2 += GetStatusDisplayLine(attribute);
                displayPage2 += $"\\nLicense Needed: TODO";

                linesPage2.Add($"    {info + ", \"" + displayPage2 + "\"}"},");
            }
            else
            {
                displayPage1 += "\\n";
                displayPage1 += $"Knockback: {weapon.KnockbackChance}%\\t";
                displayPage1 += $"Combo: {weapon.ComboChance}%\\t";
                displayPage1 += $"CT: {weapon.ChargeTime}\\t";
                if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAttributeStatDisplay(attribute);
                }

                displayPage1 += GetElementAttributeDisplayLine(attribute);
                displayPage1 += GetStatusDisplayLine(attribute);

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
                linesPage2.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
            }
        }

        foreach (DataStoreShield shield in equip.EquipDataList.Where(s => s is DataStoreShield))
        {
            DataStoreAttribute attribute = equip.AttributeDataList[(int)shield.AttributeOffset];

            string info = "{" + GetEquipId(shield);

            string displayPage1 = "";

            displayPage1 += $"Evade: {shield.Evade}\\t";
            displayPage1 += $"Magick Evade: {shield.MagickEvade}\\t";

            if (!onePage)
            {
                string displayPage2 = "";
                displayPage2 += displayPage1;

                // Page 1 only
                displayPage1 += "[1/2]";
                if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAttributeStatDisplay(attribute);
                }

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");

                // Page 2 only
                displayPage2 += "[2/2]";
                displayPage2 += GetElementAttributeDisplayLine(attribute);
                displayPage2 += GetStatusDisplayLine(attribute);
                displayPage2 += $"\\nLicense Needed: TODO";

                linesPage2.Add($"    {info + ", \"" + displayPage2 + "\"}"},");
            }
            else
            {
                if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAttributeStatDisplay(attribute);
                }

                displayPage1 += GetElementAttributeDisplayLine(attribute);
                displayPage1 += GetStatusDisplayLine(attribute);

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
                linesPage2.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
            }
        }

        foreach (DataStoreAmmo ammo in equip.EquipDataList.Where(w => w is DataStoreAmmo))
        {
            DataStoreAttribute attribute = equip.AttributeDataList[(int)ammo.AttributeOffset];

            string info = "{" + GetEquipId(ammo);

            string displayAll = "";

            displayAll += $"Attack: {ammo.AttackPower}\\t";

            if (ammo.Evade > 0)
            {
                displayAll += $"Evade: {ammo.Evade}\\t";
            }

            if (ammo.Elements.EnumToList().Count > 0 || ammo.StatusEffects.EnumToList().Count > 0 && ammo.StatusChance > 0)
            {
                List<string> onhit = new()
                {
                    GetElementsDisplay(ammo.Elements.EnumToList(), true),
                    ammo.StatusChance == 0 ? "" : $"{ammo.StatusChance}% {GetStatusDisplay(ammo.StatusEffects.EnumToList())}"
                };
                displayAll += $"On Hit: {string.Join(", ", onhit.Where(s => !string.IsNullOrEmpty(s)))}";
            }

            if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
            {
                displayAll += "\\n";
                displayAll += GetAttributeStatDisplay(attribute);
            }

            displayAll += GetElementAttributeDisplayLine(attribute);
            displayAll += GetStatusDisplayLine(attribute);

            linesPage1.Add($"    {info + ", \"" + displayAll + "\"}"},");
            linesPage2.Add($"    {info + ", \"" + displayAll + "\"}"},");
        }

        foreach (DataStoreArmor armor in equip.EquipDataList.Where(a => a is DataStoreArmor && a.Category != EquipCategory.Accessory && a.Category != EquipCategory.AccessoryCrown))
        {
            DataStoreAttribute attribute = equip.AttributeDataList[(int)armor.AttributeOffset];

            string info = "{" + GetEquipId(armor);

            string displayPage1 = "";

            displayPage1 += $"Defense: {armor.Defense}\\t";
            displayPage1 += $"Magick Resist: {armor.MagickResist}\\t";

            if (!onePage)
            {
                string displayPage2 = "";
                displayPage2 += displayPage1;

                // Page 1 only
                displayPage1 += "[1/2]";
                if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAttributeStatDisplay(attribute);
                }

                if (armor.AugmentOffset != 0xFF)
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAugmentDescription(armor);
                }

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");

                // Page 2 only
                displayPage2 += "[2/2]";
                displayPage2 += GetElementAttributeDisplayLine(attribute);
                displayPage2 += GetStatusDisplayLine(attribute);
                displayPage2 += $"\\nLicense Needed: TODO";

                linesPage2.Add($"    {info + ", \"" + displayPage2 + "\"}"},");
            }
            else
            {
                displayPage1 += GetAttributeStatDisplay(attribute);

                if (armor.AugmentOffset != 0xFF)
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAugmentDescription(armor);
                }

                displayPage1 += GetElementAttributeDisplayLine(attribute);
                displayPage1 += GetStatusDisplayLine(attribute);

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
                linesPage2.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
            }
        }

        foreach (DataStoreArmor armor in equip.EquipDataList.Where(a => a is DataStoreArmor && (a.Category == EquipCategory.Accessory || a.Category == EquipCategory.AccessoryCrown)))
        {
            DataStoreAttribute attribute = equip.AttributeDataList[(int)armor.AttributeOffset];

            string info = "{" + GetEquipId(armor);

            string displayPage1 = "";

            if (armor.Defense > 0)
            {
                displayPage1 += $"Defense: {armor.Defense}\\t";
            }

            if (armor.MagickResist > 0)
            {
                displayPage1 += $"Magick Resist: {armor.MagickResist}\\t";
            }

            if (!onePage)
            {
                string displayPage2 = "";
                displayPage2 += displayPage1;

                // Page 1 only
                displayPage1 += "[1/2]";
                if (!string.IsNullOrEmpty(GetAttributeStatDisplay(attribute)))
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAttributeStatDisplay(attribute);
                }

                if (armor.AugmentOffset != 0xFF)
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAugmentDescription(armor);
                }

                if (displayPage1.StartsWith("\\n"))
                {
                    displayPage1 = displayPage1.Substring(2);
                }

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");

                // Page 2 only
                displayPage2 += "[2/2]";
                displayPage2 += GetElementAttributeDisplayLine(attribute);
                displayPage2 += GetStatusDisplayLine(attribute);
                displayPage2 += $"\\nLicense Needed: TODO";

                if (displayPage2.StartsWith("\\n"))
                {
                    displayPage2 = displayPage2.Substring(2);
                }

                linesPage2.Add($"    {info + ", \"" + displayPage2 + "\"}"},");
            }
            else
            {
                displayPage1 += GetAttributeStatDisplay(attribute);

                if (armor.AugmentOffset != 0xFF)
                {
                    displayPage1 += "\\n";
                    displayPage1 += GetAugmentDescription(armor);
                }

                displayPage1 += GetElementAttributeDisplayLine(attribute);
                displayPage1 += GetStatusDisplayLine(attribute);

                if (displayPage1.StartsWith("\\n"))
                {
                    displayPage1 = displayPage1.Substring(2);
                }

                linesPage1.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
                linesPage2.Add($"    {info + ", \"" + displayPage1 + "\"}"},");
            }
        }

        List<string> footer = new()
        {
            "  }",
            "",
            "  return inventory",
            "end",
            "",
            "return config"
        };

        linesPage1.AddRange(footer);
        linesPage2.AddRange(footer);

        File.WriteAllLines($"{scriptFolder}\\us.lua", linesPage1);
        File.WriteAllLines($"{scriptFolder}\\us.lua.page1", linesPage1);
        File.WriteAllLines($"{scriptFolder}\\us.lua.page2", linesPage2);
    }

    private string GetAugmentDescription(DataStoreArmor armor, int maxLength = 20)
    {
        string desc = augmentData.Values.First(a => a.IntID == armor.AugmentOffset).Description;
        if (desc.Length > maxLength)
        {
            desc.Insert(maxLength, "\\n");
        }

        return desc;
    }

    private static string GetAttributeStatDisplay(DataStoreAttribute attribute, string separator = "\\t")
    {
        string display = "";
        if (attribute.HP > 0)
        {
            display += $"HP + {attribute.HP}{separator}";
        }

        if (attribute.MP > 0)
        {
            display += $"MP + {attribute.MP}{separator}";
        }

        if (attribute.Strength > 0)
        {
            display += $"STR + {attribute.Strength}{separator}";
        }

        if (attribute.MagickPower > 0)
        {
            display += $"MAG + {attribute.MagickPower}{separator}";
        }

        if (attribute.Vitality > 0)
        {
            display += $"VIT + {attribute.Vitality}{separator}";
        }

        if (attribute.Speed > 0)
        {
            display += $"SPD + {attribute.Speed}{separator}";
        }

        return display;
    }

    private string GetElementAttributeDisplayLine(DataStoreAttribute attribute, string newLine = "\\n", bool useInGameFormat = true, string separator = "\\t")
    {
        string display = "";
        if (attribute.ElementsBoost.EnumToList().Count > 0 || attribute.ElementsAbsorb.EnumToList().Count > 0 || attribute.ElementsImmune.EnumToList().Count > 0 || attribute.ElementsHalf.EnumToList().Count > 0 || attribute.ElementsWeak.EnumToList().Count > 0)
        {
            display += newLine;

            if (attribute.ElementsBoost.EnumToList().Count > 0)
            {
                display += $"Boost: {GetElementsDisplay(attribute.ElementsBoost.EnumToList(), useInGameFormat)}{separator}";
            }

            if (attribute.ElementsAbsorb.EnumToList().Count > 0)
            {
                display += $"Absorb: {GetElementsDisplay(attribute.ElementsAbsorb.EnumToList(), useInGameFormat)}{separator}";
            }

            if (attribute.ElementsImmune.EnumToList().Count > 0)
            {
                display += $"Immune: {GetElementsDisplay(attribute.ElementsImmune.EnumToList(), useInGameFormat)}{separator}";
            }

            if (attribute.ElementsHalf.EnumToList().Count > 0)
            {
                display += $"Half: {GetElementsDisplay(attribute.ElementsHalf.EnumToList(), useInGameFormat)}{separator}";
            }

            if (attribute.ElementsWeak.EnumToList().Count > 0)
            {
                display += $"Weak: {GetElementsDisplay(attribute.ElementsWeak.EnumToList(), useInGameFormat)}{separator}";
            }
        }

        return display;
    }

    private string GetStatusDisplayLine(DataStoreAttribute attribute, string newLine = "\\n", string separator = "\\t", string join = ",")
    {
        string display = "";
        if (attribute.StatusEffectsOnEquip.EnumToList().Count > 0 || attribute.StatusEffectsImmune.EnumToList().Count > 0)
        {
            display += newLine;

            if (attribute.StatusEffectsOnEquip.EnumToList().Count > 0)
            {
                display += $"On Equip: {GetStatusDisplay(attribute.StatusEffectsOnEquip.EnumToList(), join)}{separator}";
            }

            if (attribute.StatusEffectsImmune.EnumToList().Count > 0)
            {
                display += $"Immune: {GetStatusDisplay(attribute.StatusEffectsImmune.EnumToList(), join)}{separator}";
            }
        }

        return display;
    }

    private string GetElementsDisplay(List<Element> elements, bool useInGameFormat)
    {
        if (useInGameFormat)
        {
            string elems = string.Join("", elements.Select(e => e switch
            {
                Element.Fire => "{icon:10}",
                Element.Ice => "{icon:11}",
                Element.Lightning => "{icon:12}",
                Element.Water => "{icon:13}",
                Element.Wind => "{icon:14}",
                Element.Earth => "{icon:15}",
                Element.Holy => "{icon:16}",
                Element.Dark => "{icon:17}",
                _ => ""
            }));
            if (!string.IsNullOrEmpty(elems))
            {
                elems = "{vpos:-2}" + elems + "{vpos:0}";
            }
            return elems;
        }
        else
        {
            return string.Join(", ", elements.Select(e => e.ToString()));
        }
    }

    private string GetStatusDisplay(List<Status> statuses, string join = ",")
    {
        return string.Join(join, statuses.Select(s => s switch
        {
            Status.Death => "KO",
            Status.CriticalHP => "Critical HP",
            Status.XZone => "X-Zone",
            _ => s.ToString()
        }));
    }

    private int GetEquipId(DataStoreEquip equipment)
    {
        return equip.EquipDataList.IndexOf(equipment) + 4096;
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        Dictionary<string, HTMLPage> pages = new();

        HTMLPage page = new("Equipment", "template/documentation.html");

        page.HTMLElements.Add(new Table("Weapons", (new string[] { "Name", "Attack Power", "Evade", "Knockback %", "Combo %", "Charge Time (CT)", "Other Stats", "Element Effects", "Status Effects" }).ToList(), (new int[] { 10, 10, 10, 8, 8, 8, 10, 20, 20 }).ToList(), equip.EquipDataList.Where(w => w is DataStoreWeapon).Select(e =>
        {
            DataStoreWeapon weapon = e as DataStoreWeapon;

            string name = treasureRando.GetItemName(weapon.ID);

            string onHitElem = GetElementsDisplay(weapon.Elements.EnumToList(), false);
            if (!string.IsNullOrEmpty(onHitElem))
            {
                onHitElem = $"On Hit: {onHitElem}";
            }

            string onHitStatus = "";
            if (weapon.StatusChance > 0 && weapon.StatusEffects.EnumToList().Count > 0)
            {             
                onHitStatus = $"On Hit: {weapon.StatusChance}% " + GetStatusDisplay(weapon.StatusEffects.EnumToList(), ", ");
            }

            DataStoreAttribute attribute = equip.AttributeDataList[(int)weapon.AttributeOffset];
            string elemDisplay = onHitElem + GetElementAttributeDisplayLine(attribute, string.IsNullOrEmpty(onHitElem) ? "" : "\n", false, "\n");
            string statusDisplay = onHitStatus + GetStatusDisplayLine(attribute, string.IsNullOrEmpty(onHitStatus) ? "" : "\n", "\n", ", ");

            return new string[] { name, weapon.AttackPower.ToString(), weapon.Evade.ToString(), weapon.KnockbackChance.ToString(), weapon.ComboChance.ToString(), weapon.ChargeTime.ToString(), GetAttributeStatDisplay(attribute," "), elemDisplay, statusDisplay }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Shields", (new string[] { "Name", "Evade", "Magick Evade", "Other Stats", "Element Effects", "Status Effects" }).ToList(), (new int[] { 10, 10, 10, 10, 20, 20 }).ToList(), equip.EquipDataList.Where(w => w is DataStoreShield).Select(e =>
        {
            DataStoreShield shield = e as DataStoreShield;

            string name = treasureRando.GetItemName(shield.ID);

            DataStoreAttribute attribute = equip.AttributeDataList[(int)shield.AttributeOffset];
            return new string[] { name, shield.Evade.ToString(), shield.MagickEvade.ToString(), GetAttributeStatDisplay(attribute, " "), GetElementAttributeDisplayLine(attribute, "", false, "\n"), GetStatusDisplayLine(attribute, "", "\n", ", ") }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Ammo", (new string[] { "Name", "Attack Power", "Evade", "Other Stats", "Element Effects", "Status Effects" }).ToList(), (new int[] { 10, 10, 10, 10, 20, 20 }).ToList(), equip.EquipDataList.Where(w => w is DataStoreAmmo).Select(e =>
        {
            DataStoreAmmo ammo = e as DataStoreAmmo;

            string name = treasureRando.GetItemName(ammo.ID);

            string onHitElem = GetElementsDisplay(ammo.Elements.EnumToList(), false);
            if (!string.IsNullOrEmpty(onHitElem))
            {
                onHitElem = $"On Hit: {onHitElem}";
            }

            string onHitStatus = "";
            if (ammo.StatusChance > 0 && ammo.StatusEffects.EnumToList().Count > 0)
            {
                onHitStatus = $"On Hit: {ammo.StatusChance}% " + GetStatusDisplay(ammo.StatusEffects.EnumToList(), ", ");
            }

            DataStoreAttribute attribute = equip.AttributeDataList[(int)ammo.AttributeOffset];
            string elemDisplay = onHitElem + GetElementAttributeDisplayLine(attribute, string.IsNullOrEmpty(onHitElem) ? "" : "\n", false, "\n");
            string statusDisplay = onHitStatus + GetStatusDisplayLine(attribute, string.IsNullOrEmpty(onHitStatus) ? "" : "\n", "\n", ", ");

            return new string[] { name, ammo.AttackPower.ToString(), ammo.Evade.ToString(), GetAttributeStatDisplay(attribute, " "), elemDisplay, statusDisplay }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Armor", (new string[] { "Name", "Defense", "Magick Resist", "Other Stats", "Element Effects", "Status Effects", "Passive" }).ToList(), (new int[] { 10, 10, 10, 10, 20, 20, 20 }).ToList(), equip.EquipDataList.Where(a => a is DataStoreArmor && a.Category != EquipCategory.Accessory && a.Category != EquipCategory.AccessoryCrown).Select(e =>
        {
            DataStoreArmor armor = e as DataStoreArmor;

            string name = treasureRando.GetItemName(armor.ID);

            DataStoreAttribute attribute = equip.AttributeDataList[(int)armor.AttributeOffset];
            return new string[] { name, armor.Defense.ToString(), armor.MagickResist.ToString(), GetAttributeStatDisplay(attribute, " "), GetElementAttributeDisplayLine(attribute, "", false, "\n"), GetStatusDisplayLine(attribute, "", "\n", ", "), GetAugmentDescription(armor, int.MaxValue) }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Accessories", (new string[] { "Name", "Defense", "Magick Resist", "Other Stats", "Element Effects", "Status Effects", "Passive" }).ToList(), (new int[] { 10, 10, 10, 10, 20, 20, 20 }).ToList(), equip.EquipDataList.Where(a => a is DataStoreArmor && (a.Category == EquipCategory.Accessory || a.Category == EquipCategory.AccessoryCrown)).Select(e =>
        {
            DataStoreArmor accessory = e as DataStoreArmor;

            string name = treasureRando.GetItemName(accessory.ID);

            DataStoreAttribute attribute = equip.AttributeDataList[(int)accessory.AttributeOffset];
            return new string[] { name, accessory.Defense.ToString(), accessory.MagickResist.ToString(), GetAttributeStatDisplay(attribute, " "), GetElementAttributeDisplayLine(attribute, "", false, "\n"), GetStatusDisplayLine(attribute, "", "\n", ", "), GetAugmentDescription(accessory, int.MaxValue) }.ToList();
        }).ToList()));

        pages.Add("equipment", page);

        return pages;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Item/Equip Data...");
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_013.bin", equip.Data);
        if (FF12SeedGenerator.DescriptiveInstalled())
        {
            GenerateDescriptionsFile();
        }
    }
}
