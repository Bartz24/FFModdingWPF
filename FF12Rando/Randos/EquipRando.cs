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

public class EquipRando : Randomizer
{
    public Dictionary<string, ItemData> itemData = new();
    public Dictionary<string, AugmentData> augmentData = new();
    public DataStoreBPEquip equip;
    public EquipRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Item/Equip Data...", 0, -1);
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
        Generator.SetUIProgress("Randomizing Item/Equip Data...", 0, -1);
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
            float[] weights = { weapon.Category is EquipCategory.Gun or EquipCategory.Measure ? 4 : 1, 3 / 4f, 1 / 10f, 1, 1, 1, 1, 2 };
            int[] chances = { 70, 8, 3, 3, 4, 4, 3, 1 };
            int[] zeros = { 0, 70, 97, 97, 90, 90, 90, 90 };
            int[] negs = { 0, 0, 0, 0, 0, 0, 0, 0 };

            if (FF12Flags.Stats.EquipHiddenStats.Enabled)
            {
                (int, int)[] hiddenBounds = {
                    (0, 100),
                    (0, 100),
                    (RandomNum.RandInt(RandomNum.RandInt(-100, -40), -20), -10)
                };
                float[] hiddenWeights = { 2, 2, 4 };
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
            float[] weights = { 1, 2, 1 / 10f, 1, 1, 1, 1, 2 };
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
                (0, 50),
                (0, 50),
                (0, 5000),
                (0, 500),
                (0, 99),
                (0, 99),
                (0, 99),
                (0, 99)
            };
            float[] weights = { 2, 2, 1 / 10f, 1, 1, 1, 1, 2 };
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
            float[] weights = { 1, 1, 1 / 10f, 1.2f, 1.2f, 1.2f, 1.2f, 2 };
            int[] chances = { 30, 30, 10, 10, 10, 10, 10, 10 };
            int[] zeros = { 50, 50, 70, 70, 70, 70, 70, 97 };
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
            float[] weights = { 1, 1, 1 / 10f, 1, 1, 1, 1, 2 };
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
            StatPoints statPoints;
            (int, int)[] bounds = {
                (-1, 3),
                (-1, 3),
                (-1, 3),
                (-1, 3),
                (-1, 3),
                (-1, 3),
                (-1, 3),
                (-1, 3)
            };
            float[] weights = { 1, 1, 1, 1, 1, 1, 1, 1 };
            int[] chances = { 1, 1, 1, 1, 1, 1, 1, 1 };
            int[] zeros = { 90, 90, 90, 90, 90, 90, 90, 90 };
            int[] negs = { 10, 10, 10, 10, 10, 10, 10, 10 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);

            List<Element> elements = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();

            statPoints.Randomize(elements.Select(e => GetElementLevel(e, attribute)).ToArray());

            attribute.ElementsAbsorb = 0;
            attribute.ElementsHalf = 0;
            attribute.ElementsImmune = 0;
            attribute.ElementsWeak = 0;

            elements.ForEach(e => SetElementLevel(e, statPoints[elements.IndexOf(e)], attribute));

            int count = GetRandomElementCount(attribute.ElementsBoost);
            List<Element> newElem = Enum.GetValues(typeof(Element)).Cast<Element>().ToList();
            attribute.ElementsBoost = 0;
            newElem.Shuffle().Take(count).ForEach(e => attribute.ElementsBoost |= e);
        }
    }

    private int GetRandomElementCount(Element elem, int max = 8)
    {
        int baseCount = elem.EnumToList().Count;
        if (RandomNum.RandInt(0, 99) < 70)
        {
            return baseCount;
        }
        int round1 = RandomNum.RandIntBounds(0, max, baseCount, 1);
        return RandomNum.RandIntBounds(0, max, round1, 1);
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
            if (RandomNum.RandInt(0, 99) < (armor.Category is EquipCategory.Accessory or EquipCategory.AccessoryCrown ? 95 : 20))
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
            int count = GetRandomStatsEffectCount(weapon.StatusEffects);
            List<Status> newStatus = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            weapon.StatusEffects = 0;

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
            int count = GetRandomStatsEffectCount(ammo.StatusEffects);
            List<Status> newStatus = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            ammo.StatusEffects = 0;

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
            int countEquip = GetRandomStatsEffectCount(attribute.StatusEffectsOnEquip);
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

            int countImmune = GetRandomStatsEffectCount(attribute.StatusEffectsImmune);
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

    private int GetRandomStatsEffectCount(Status status)
    {
        int baseCount = status.EnumToList().Count;
        if (RandomNum.RandInt(0, 99) < 60)
        {
            return baseCount;
        }
        int round1 = RandomNum.RandIntBounds(0, 32, baseCount, 1);
        int round2 = RandomNum.RandIntBounds(0, 32, round1, 1);
        return RandomNum.RandIntBounds(0, 32, round2, 1);
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

        return status is Status.Death or Status.Stone or Status.CriticalHP or Status.XZone
            ? 0
            : status is Status.Petrify or Status.Doom or Status.Stop or Status.Reverse or Status.Disease
            ? 1
            : status is Status.Immobilize or Status.Confuse or Status.Sleep or Status.Disable or Status.Bubble
            ? 3
            : status is Status.Sap or Status.Libra ? 4 : 10;
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
                    GetElementsDisplay(weapon.Elements.EnumToList()),
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
                    GetElementsDisplay(ammo.Elements.EnumToList()),
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

    private string GetAugmentDescription(DataStoreArmor armor)
    {
        string desc = augmentData.Values.First(a => a.IntID == armor.AugmentOffset).Description;
        if (desc.Length > 20)
        {
            desc.Insert(20, "\\n");
        }

        return desc;
    }

    private static string GetAttributeStatDisplay(DataStoreAttribute attribute)
    {
        string display = "";
        if (attribute.HP > 0)
        {
            display += $"HP + {attribute.HP}\\t";
        }

        if (attribute.MP > 0)
        {
            display += $"MP + {attribute.MP}\\t";
        }

        if (attribute.Strength > 0)
        {
            display += $"STR + {attribute.Strength}\\t";
        }

        if (attribute.MagickPower > 0)
        {
            display += $"MAG + {attribute.MagickPower}\\t";
        }

        if (attribute.Vitality > 0)
        {
            display += $"VIT + {attribute.Vitality}\\t";
        }

        if (attribute.Speed > 0)
        {
            display += $"SPD + {attribute.Speed}\\t";
        }

        return display;
    }

    private string GetElementAttributeDisplayLine(DataStoreAttribute attribute)
    {
        string display = "";
        if (attribute.ElementsBoost.EnumToList().Count > 0 || attribute.ElementsAbsorb.EnumToList().Count > 0 || attribute.ElementsImmune.EnumToList().Count > 0 || attribute.ElementsHalf.EnumToList().Count > 0 || attribute.ElementsWeak.EnumToList().Count > 0)
        {
            display += "\\n";
            if (attribute.ElementsBoost.EnumToList().Count > 0)
            {
                display += $"Boost: {GetElementsDisplay(attribute.ElementsBoost.EnumToList())}\\t";
            }

            if (attribute.ElementsAbsorb.EnumToList().Count > 0)
            {
                display += $"Absorb: {GetElementsDisplay(attribute.ElementsAbsorb.EnumToList())}\\t";
            }

            if (attribute.ElementsImmune.EnumToList().Count > 0)
            {
                display += $"Immune: {GetElementsDisplay(attribute.ElementsImmune.EnumToList())}\\t";
            }

            if (attribute.ElementsHalf.EnumToList().Count > 0)
            {
                display += $"Half: {GetElementsDisplay(attribute.ElementsHalf.EnumToList())}\\t";
            }

            if (attribute.ElementsWeak.EnumToList().Count > 0)
            {
                display += $"Weak: {GetElementsDisplay(attribute.ElementsWeak.EnumToList())}\\t";
            }
        }

        return display;
    }

    private string GetStatusDisplayLine(DataStoreAttribute attribute)
    {
        string display = "";
        if (attribute.StatusEffectsOnEquip.EnumToList().Count > 0 || attribute.StatusEffectsImmune.EnumToList().Count > 0)
        {
            display += "\\n";
            if (attribute.StatusEffectsOnEquip.EnumToList().Count > 0)
            {
                display += $"On Equip: {GetStatusDisplay(attribute.StatusEffectsOnEquip.EnumToList())}\\t";
            }

            if (attribute.StatusEffectsImmune.EnumToList().Count > 0)
            {
                display += $"Immune: {GetStatusDisplay(attribute.StatusEffectsImmune.EnumToList())}\\t";
            }
        }

        return display;
    }

    private string GetElementsDisplay(List<Element> elements)
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

    private string GetStatusDisplay(List<Status> statuses)
    {
        return string.Join(",", statuses.Select(s => s switch
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

    public override void Save()
    {
        Generator.SetUIProgress("Saving Item/Equip Data...", 0, -1);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_013.bin", equip.Data);
        if (FF12SeedGenerator.DescriptiveInstalled())
        {
            GenerateDescriptionsFile();
        }
    }
    public class ItemData : CSVDataRow
    {
        [RowIndex(0)]
        public string Name { get; set; }
        [RowIndex(1), FieldTypeOverride(FieldType.HexInt)]
        public int IntID { get; set; }
        [RowIndex(1)]
        public string ID { get; set; }
        [RowIndex(2)]
        public int Rank { get; set; }
        [RowIndex(3), FieldTypeOverride(FieldType.HexInt)]
        public int IntUpgrade { get; set; }
        [RowIndex(3)]
        public string Upgrade { get; set; }
        public ItemData(string[] row) : base(row)
        {
        }
    }
    public class AugmentData : CSVDataRow
    {
        [RowIndex(0)]
        public string Name { get; set; }
        [RowIndex(1)]
        public int IntID { get; set; }
        [RowIndex(1)]
        public string ID { get; set; }
        [RowIndex(2)]
        public string Description { get; set; }
        [RowIndex(3)]
        public List<string> Traits { get; set; }
        public AugmentData(string[] row) : base(row)
        {
        }
    }
}
