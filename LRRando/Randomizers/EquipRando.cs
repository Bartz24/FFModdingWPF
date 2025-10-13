using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando;

public partial class EquipRando : Randomizer
{
    public DataStoreWDB<DataStoreItemWeapon> itemWeapons = new();
    public DataStoreWDB<DataStoreItem> items = new();
    public DataStoreWDB<DataStoreItem> itemsOrig = new();
    public DataStoreWDB<DataStoreBtAutoAbility> autoAbilities = new();
    public DataStoreWDB<DataStoreRPassiveAbility> passiveAbilities = new();
    public DataStoreWDB<DataStoreRItemAbi> itemAbilities = new();
    public DataStoreWDB<DataStoreRItemAbi> itemAbilitiesOrig = new();
    public DataStoreWDB<DataStoreRBtUpgrade> upgrades = new();
    public readonly Dictionary<string, AbilityData> abilityData = new();
    public readonly Dictionary<string, PassiveData> passiveData = new();
    public readonly Dictionary<string, ItemData> itemData = new();

    public List<string> RemainingEquip = new();
    public List<string> RemainingAdorn = new();

    public EquipRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Equip Data...");
        itemWeapons.LoadWDB(Generator, "LR", @"\db\resident\item_weapon.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 10, 100);
        items.LoadWDB(Generator, "LR", @"\db\resident\item.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 20, 100);
        itemsOrig.LoadWDB(Generator, "LR", @"\db\resident\item.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 30, 100);
        autoAbilities.LoadWDB(Generator, "LR", @"\db\resident\bt_auto_ability.wdb");
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 40, 100);
        itemAbilities.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 60, 100);
        itemAbilitiesOrig.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 70, 100);
        passiveAbilities.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_pasv_ablty.wdb", false);
        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 80, 100);
        upgrades.LoadWDB(Generator, "LR", @"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb", false);

        FileHelpers.ReadCSVFile(@"data\passives.csv", row =>
        {
            PassiveData p = new(row);
            passiveData.Add(p.ID, p);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\abilities.csv", row =>
        {
            AbilityData a = new(row);
            abilityData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\items.csv", row =>
        {
            ItemData i = new(row);
            itemData.Add(i.ID, i);
        }, FileHelpers.CSVFileHeader.HasHeader);

        RandoUI.SetUIProgressDeterminate("Loading Equip Data...", 90, 100);
        itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
        itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);

        upgrades.Values.Where(u => u.i16AtbSpdLimit >= 32768).ForEach(u => u.i16AtbSpdLimit -= 65536);
        upgrades.Values.Where(u => u.i16BrkBonusLimit >= 32768).ForEach(u => u.i16BrkBonusLimit -= 65536);

        autoAbilities.Values.Where(a => a.i16AutoAblArgInt0 >= 32768).ForEach(a => a.i16AutoAblArgInt0 -= 65536);
        autoAbilities.Values.Where(a => a.i16AutoAblArgInt1 >= 32768).ForEach(a => a.i16AutoAblArgInt1 -= 65536);

        itemAbilities.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);
        itemAbilitiesOrig.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);

        itemData.Values.Where(i => i.OverrideBuyGil != -1).ForEach(i => items[i.ID].uPurchasePrice = i.OverrideBuyGil);
        itemData.Values.Where(i => i.OverrideBuyEP != -1).ForEach(i => items[i.ID].uGpCost = i.OverrideBuyEP * 1000);

        RemainingEquip = itemData.Values.Where(i => (i.Category == "Weapon" || i.Category == "Shield" || i.Category == "Garb" || i.Category == "Accessory") && !i.Traits.Contains("Key")).Select(i => i.ID).ToList();
        RemainingAdorn = itemData.Values.Where(i => i.Category == "Adornment" && !i.Traits.Contains("Remove")).Select(i => i.ID).ToList();
        FilterOutDLCItems();

        // Add rando victory item
        if (!items.Keys.Contains("key_r_victory"))
        {
            var apAdded = items.Copy("key_b_20", "key_r_victory");
            apAdded.sItemNameStringId = "$zzz_r_victory";
            apAdded.sHelpStringId = "$zzz_r_victoryh";
            apAdded.u16SortAllByKCategory = 101;
            apAdded.u16SortCategoryByCategory = 152;
        }
    }

    public virtual void FilterOutDLCItems()
    {
        if (!LRFlags.Items.IsIncludeDLCItems())
        {
            RemainingEquip.RemoveAll(i => itemData[i].Traits.Contains("DLC"));
            RemainingAdorn.RemoveAll(i => itemData[i].Traits.Contains("DLC"));
        }
    }

    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Equip Data...");
        if (LRFlags.StatsAbilities.EquipStats.FlagEnabled)
        {
            LRFlags.StatsAbilities.EquipStats.SetRand();
            RandomizeStats();

            // Clear vanilla upgrades as they don't matter
            upgrades.Clear();

            RandomizeUpgrades();
            RandomNum.ClearRand();

            itemWeapons.Values.Where(w => !upgrades.Keys.Contains(w.sUpgradeId)).ForEach(w => w.sUpgradeId = "");
        }

        RandoUI.SetUIProgressDeterminate("Randomizing Equip Data...", 40, 100);
        if (LRFlags.StatsAbilities.GarbAbilities.FlagEnabled)
        {
            LRFlags.StatsAbilities.GarbAbilities.SetRand();
            RandomizeAbilities();
            RandomNum.ClearRand();
        }

        RandoUI.SetUIProgressDeterminate("Randomizing Equip Data...", 70, 100);
        if (LRFlags.StatsAbilities.EquipPassives.FlagEnabled)
        {
            LRFlags.StatsAbilities.EquipPassives.SetRand();
            RandomizePassives();
            RandomizeUpgradePassives();
            RandomNum.ClearRand();
        }

        itemWeapons.Values.Where(a => a.u4AccessoryPos > 0 && items.Keys.Contains(a.record)).ForEach(a =>
        {
            items[a.record].uPurchasePrice = 50000;
            items[a.record].u1OnlyOne = 1;
        });
    }

    private void RandomizeAbilities()
    {
        foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
        {
            string forceWeaponType = "";
            if (garb.record == "cos_ba00")
            {
                forceWeaponType = "wea_ea08";
            }

            if (garb.record == "cos_ca00")
            {
                forceWeaponType = "wea_ca00";
            }

            do
            {
                garb.sCosAbilityCir = RandomizeAbility(garb.sCosAbilityCir, forceWeaponType == "" ? -1 : itemWeapons[forceWeaponType].i16AttackModVal > itemWeapons[forceWeaponType].i16MagicModVal ? 26 : 27);
                garb.sCosAbilityCro = RandomizeAbility(garb.sCosAbilityCro, -1);
                garb.sCosAbilityTri = RandomizeAbility(garb.sCosAbilityTri, -1);
                garb.sCosAbilitySqu = RandomizeAbility(garb.sCosAbilitySqu, -1);
            } while (new string[] { garb.sCosAbilityCir, garb.sCosAbilityCro, garb.sCosAbilityTri, garb.sCosAbilitySqu }.Distinct().Where(x => x.StartsWith("abi_")).GroupBy(x => itemAbilities[x].sAbilityId).Any(g => g.Count() > 1));
        }
    }

    private string RandomizeAbility(string name, int forceType)
    {
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        if (name != "")
        {
            List<string> possible = GetGarbAbilities(forceType);
            string newAbility = possible.ElementAt(RandomNum.RandInt(0, possible.Count - 1));
            if (name.StartsWith("abi_") && !name.EndsWith("zz99"))
            {
                string origAbility = itemAbilities[name].sAbilityId;
                itemAbilities[name].sAbilityId = newAbility;
                items[name].sItemNameStringId = "";
                items[name].sHelpStringId = "";
                items[name].sScriptId = "";
                items[name].u8MenuIcon = abilityData[newAbility].MenuIcon;

                if (itemAbilities[name].u4Lv < 1)
                {
                    itemAbilities[name].u4Lv = 1;
                }

                int origATB = abilityData[origAbility].ATBCost;
                float origATBMult = (abilityData[origAbility].ATBCost - itemAbilities[name].i8AtbDec) / (float)origATB;
                int newATB = abilityData[newAbility].ATBCost;
                itemAbilities[name].i8AtbDec = -(int)(Math.Ceiling(newATB * origATBMult) - abilityData[newAbility].ATBCost);

                int origExpectedPower = abilityData[origAbility].BasePower + ((abilityRando.abilityGrowths[origAbility].GetPowMin(itemAbilities[name].u4Lv, abilityData[origAbility].HitMultiplier) + abilityRando.abilityGrowths[origAbility].GetPowMax(itemAbilities[name].u4Lv, abilityData[origAbility].HitMultiplier)) / 2);
                float origMult = (abilityData[origAbility].BasePower + (itemAbilities[name].iPower * abilityData[origAbility].HitMultiplier)) / (float)origExpectedPower;
                int newExpectedPower = abilityData[newAbility].BasePower + ((abilityRando.abilityGrowths[newAbility].GetPowMin(itemAbilities[name].u4Lv, abilityData[newAbility].HitMultiplier) + abilityRando.abilityGrowths[newAbility].GetPowMax(itemAbilities[name].u4Lv, abilityData[newAbility].HitMultiplier)) / 2);
                itemAbilities[name].iPower = (int)((newExpectedPower * origMult) - abilityData[newAbility].BasePower) / abilityData[newAbility].HitMultiplier;
            }
            else
            {
                return newAbility;
            }
        }

        return name;
    }

    public List<string> GetGarbAbilities(int forceType)
    {
        return abilityData.Keys.Where(s => forceType == -1 || abilityData[s].MenuIcon == forceType).ToList();
    }

    public List<DataStoreItem> GetAbilities(int forceType)
    {
        List<string> list = itemsOrig.Values.Where(i => IsAbility(i) && (forceType == -1 || i.u8MenuIcon == forceType)).Select(i => i.record).ToList();

        return list.Select(s => itemsOrig[s]).GroupBy(i => i.sScriptId).Select(g => g.First()).ToList();
    }

    public bool IsAbility(string item)
    {
        return items.Keys.Contains(item) && IsAbility(items[item]);
    }

    public bool IsAbility(DataStoreItem item)
    {
        return item.u8ItemCategory == (int)ItemCategory.Ability && !item.record.StartsWith("abi_");
    }

    private void RandomizeStats()
    {
        foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
        {
            (int, int)[] bounds = {
                (-75, 100),
                (0, 100)
            };
            float[] weights = { 2, 1 };
            int[] chances = { 20, 80 };
            int[] zeros = { 50, 5 };
            int[] negs = { 50, 0 };
            StatPoints statPoints = new(bounds, weights, chances, zeros, negs);
            statPoints.Randomize(new int[] { garb.i16AtbModVal, garb.i16AtbStartModVal });

            garb.i16AtbModVal = statPoints[0];
            garb.i16AtbStartModVal = statPoints[1];

            /*
            garb.i16AtbModVal = 100;
            garb.i16AtbStartModVal = 100;
            */

            if (items.Keys.Contains(garb.record) && items[garb.record].uPurchasePrice == 0)
            {
                items[garb.record].uPurchasePrice = 50000;
            }
        }

        foreach (DataStoreItemWeapon weapon in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Weapon && w.u4AccessoryPos == 0))
        {
            bool starting = weapon.record is "wea_ea08" or "wea_ca00";

            StatPoints statPoints;
            do
            {
                (int, int)[] bounds = {
                    (-2000, 5000),
                    (-2000, 5000),
                    (-5000, 50000),
                    (-25, 50),
                    (-90, 75)
                };
                float[] weights = { 1, 1, 1 / 200f, 10, 5 };
                int[] chances = { 40, 40, 5, 5, 10 };
                int[] zeros = { 10, 10, 85, 60, 80 };
                int[] negs = { 15, 15, 40, 10, 5 };
                statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
                statPoints.Randomize(new int[] { weapon.i16AttackModVal, weapon.i16MagicModVal, weapon.i16HpModVal, weapon.i16AtbSpeedModVal, weapon.iBreakBonus });
            }
            while (starting && statPoints[0] < 50 && statPoints[1] < 50);

            weapon.i16AttackModVal = statPoints[0];
            weapon.i16MagicModVal = statPoints[1];
            weapon.i16HpModVal = statPoints[2];
            weapon.i16AtbSpeedModVal = statPoints[3];
            weapon.iBreakBonus = statPoints[4];

#if DEBUG
            /*weapon.i16AttackModVal = 10000;
            weapon.i16MagicModVal = 10000;
            weapon.i16HpModVal = 30000;
            weapon.i16AtbSpeedModVal = 100;*/
#endif

        }

        foreach (DataStoreItemWeapon shield in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Shield))
        {
            bool starting = shield.record is "shi_ea08" or "shi_ca00";

            StatPoints statPoints;
            do
            {
                (int, int)[] bounds = {
                    (-2000, 5000),
                    (-2000, 5000),
                    (-5000, 50000),
                    (-25, 50),
                    (0, 1000)
                };
                float[] weights = { 1, 1, 1 / 150f, 8, 8 };
                int[] chances = { 5, 5, 30, 30, 30 };
                int[] zeros = { 90, 90, 40, 30, 20 };
                int[] negs = { 30, 30, 15, 5, 0 };
                statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
                statPoints.Randomize(new int[] { shield.i16AttackModVal, shield.i16MagicModVal, shield.i16HpModVal, shield.i16AtbSpeedModVal, shield.iGuardModVal });
            }
            while (starting && (statPoints[0] < 0 || statPoints[1] < 0));

            shield.i16AttackModVal = statPoints[0];
            shield.i16MagicModVal = statPoints[1];
            shield.i16HpModVal = statPoints[2];
            shield.i16AtbSpeedModVal = statPoints[3];
            shield.iGuardModVal = statPoints[4];

#if DEBUG
            //shield.iGuardModVal = 3000;
#endif

        }
    }

    private void RandomizeUpgrades()
    {
        int[] gilVals = { 20, 50, 100 };
        // Shields first to be alphabetical order

        foreach (DataStoreItemWeapon shield in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Shield))
        {
            int[] bounds = {
                9999,
                9999,
                75000,
                100,
                1500 };

            int[] baseStats = {
                shield.i16AttackModVal,
                shield.i16MagicModVal,
                shield.i16HpModVal,
                shield.i16AtbSpeedModVal,
                shield.iGuardModVal };
            int[] currentStats = baseStats.ToArray();

            int[][] inc = {
                new int[] { 10, 25, 50 },
                new int[] { 10, 25, 50 },
                new int[] { 100, 100, 200 },
                new int[] { 1, 1, 1 },
                new int[] { 5, 5, 5 } };

            DataStoreRBtUpgrade[] shieldUpgrades = GetAndRegisterBlankUpgrades(shield.record);
            if (shield.u16UpgradeLimit < 10)
            {
                shield.u16UpgradeLimit = 10;
            }

            int upgradesRemaining = shield.u16UpgradeLimit * RandomNum.RandInt(105, 150) / 100;

            while (upgradesRemaining > 0)
            {
                int type = upgradesRemaining > 125 ? 0 : upgradesRemaining > 25 ? 1 : 2;

                int next = RandomNum.SelectRandomWeighted(Enumerable.Range(0, baseStats.Length).ToList(), i =>
                {
                    int weight = currentStats[i] + inc[i][type] > bounds[i] || (i < 2 && baseStats[i] <= 0)
                        ? 0
                        : baseStats[i] < 0 ? 5 + (type * 10) : 100 - (Math.Abs((bounds[i] / 2) - currentStats[i]) * 100 / bounds[i]) + 1;
                    if (i >= 2)
                    {
                        weight *= 4;
                    }

                    return weight;
                });

                currentStats[next] += inc[next][type];
                for (int t = type; t < 3; t++)
                {
                    switch (next)
                    {
                        case 0:
                            shieldUpgrades[t].i16PhyAtkLimit += inc[next][type];
                            break;
                        case 1:
                            shieldUpgrades[t].i16MagAtkLimit += inc[next][type];
                            break;
                        case 2:
                            shieldUpgrades[t].i16MaxHpLimit += inc[next][type];
                            break;
                        case 3:
                            shieldUpgrades[t].i16AtbSpdLimit += inc[next][type];
                            break;
                        case 4:
                            shieldUpgrades[t].i16GuardLimit += inc[next][type];
                            break;
                    }
                }

                upgradesRemaining--;
            }

            string[] mats =
            {
                "mat_cus_0_00",
                "mat_cus_0_02",
                currentStats[2] > currentStats[4] * 20 ? "mat_cus_0_05" : "mat_cus_0_06",
                "mat_cus_0_08"
            };
            mats = currentStats[2] > 15000 || currentStats[3] > 40 || currentStats[4] > 500
                ? mats.TakeLast(shieldUpgrades.Length).ToArray()
                : mats.Take(shieldUpgrades.Length).ToArray();

            for (int type = 0; type < shieldUpgrades.Length; type++)
            {
                DataStoreRBtUpgrade upgrade = shieldUpgrades[type];
                if (currentStats[0] > baseStats[0])
                {
                    upgrade.sPhyAtkItemId = mats[type];
                    upgrade.uPhyAtkGil = gilVals[type];
                    upgrade.i16PhyAtkLimit += baseStats[0];
                    upgrade.u8PhyAtkItemCount = 1;
                }

                if (currentStats[1] > baseStats[1])
                {
                    upgrade.sMagAtkItemId = mats[type];
                    upgrade.uMagAtkGil = gilVals[type];
                    upgrade.i16MagAtkLimit += baseStats[1];
                    upgrade.u8MagAtkItemCount = 1;
                }

                if (currentStats[2] > baseStats[2])
                {
                    upgrade.sMaxHpItemId = mats[type];
                    upgrade.uMaxHpGil = gilVals[type];
                    upgrade.i16MaxHpLimit += baseStats[2];
                    upgrade.u8MaxHpItemCount = 1;
                }

                if (currentStats[3] > baseStats[3])
                {
                    upgrade.sAtbSpdItemId = mats[type];
                    upgrade.uAtbSpdGil = gilVals[type];
                    upgrade.i16AtbSpdLimit += baseStats[3];
                    upgrade.u8AtbSpdItemCount = 1;
                }

                if (currentStats[4] > baseStats[4])
                {
                    upgrade.sGuardItemId = mats[type];
                    upgrade.uGuardGil = gilVals[type];
                    upgrade.i16GuardLimit += baseStats[4];
                    upgrade.u8GuardItemCount = 1;
                }
            }
        }

        foreach (DataStoreItemWeapon weapon in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Weapon && w.u4AccessoryPos == 0))
        {
            int[] bounds = {
                9999,
                9999,
                75000,
                100,
                150 };

            int[] baseStats = {
                weapon.i16AttackModVal,
                weapon.i16MagicModVal,
                weapon.i16HpModVal,
                weapon.i16AtbSpeedModVal,
                weapon.iBreakBonus };
            int[] currentStats = baseStats.ToArray();

            int[][] inc = {
                new int[] { 10, 25, 50 },
                new int[] { 10, 25, 50 },
                new int[] { 100, 100, 200 },
                new int[] { 1, 1, 1 },
                new int[] { 1, 1, 1 } };

            DataStoreRBtUpgrade[] weaponUpgrades = GetAndRegisterBlankUpgrades(weapon.record);
            if (weapon.u16UpgradeLimit < 10)
            {
                weapon.u16UpgradeLimit = 10;
            }

            int upgradesRemaining = weapon.u16UpgradeLimit * RandomNum.RandInt(105, 150) / 100;

            while (upgradesRemaining > 0)
            {
                int type = upgradesRemaining > 125 ? 0 : upgradesRemaining > 25 ? 1 : 2;

                int next = RandomNum.SelectRandomWeighted(Enumerable.Range(0, baseStats.Length).ToList(), i =>
                {
                    int weight = currentStats[i] + inc[i][type] > bounds[i] || (i >= 2 && baseStats[i] <= 0)
                        ? 0
                        : baseStats[i] < 0 ? 5 + (type * 10) : 100 - (Math.Abs((bounds[i] / 2) - currentStats[i]) * 100 / bounds[i]) + 1;
                    if (i < 2)
                    {
                        weight *= 4;
                    }

                    return weight;
                });

                currentStats[next] += inc[next][type];
                for (int t = type; t < 3; t++)
                {
                    switch (next)
                    {
                        case 0:
                            weaponUpgrades[t].i16PhyAtkLimit += inc[next][type];
                            break;
                        case 1:
                            weaponUpgrades[t].i16MagAtkLimit += inc[next][type];
                            break;
                        case 2:
                            weaponUpgrades[t].i16MaxHpLimit += inc[next][type];
                            break;
                        case 3:
                            weaponUpgrades[t].i16AtbSpdLimit += inc[next][type];
                            break;
                        case 4:
                            weaponUpgrades[t].i16BrkBonusLimit += inc[next][type];
                            break;
                    }
                }

                upgradesRemaining--;
            }

            string[] mats =
            {
                "mat_cus_0_00",
                "mat_cus_0_01",
                currentStats[1] > currentStats[0] * 1.25 ? "mat_cus_0_04" : "mat_cus_0_03",
                "mat_cus_0_07"
            };
            mats = currentStats[0] > 4000 || currentStats[1] > 4000
                ? mats.TakeLast(weaponUpgrades.Length).ToArray()
                : mats.Take(weaponUpgrades.Length).ToArray();

            for (int type = 0; type < weaponUpgrades.Length; type++)
            {
                DataStoreRBtUpgrade upgrade = weaponUpgrades[type];
                if (currentStats[0] > baseStats[0])
                {
                    upgrade.sPhyAtkItemId = mats[type];
                    upgrade.uPhyAtkGil = gilVals[type];
                    upgrade.i16PhyAtkLimit += baseStats[0];
                    upgrade.u8PhyAtkItemCount = 1;
                }

                if (currentStats[1] > baseStats[1])
                {
                    upgrade.sMagAtkItemId = mats[type];
                    upgrade.uMagAtkGil = gilVals[type];
                    upgrade.i16MagAtkLimit += baseStats[1];
                    upgrade.u8MagAtkItemCount = 1;
                }

                if (currentStats[2] > baseStats[2])
                {
                    upgrade.sMaxHpItemId = mats[type];
                    upgrade.uMaxHpGil = gilVals[type];
                    upgrade.i16MaxHpLimit += baseStats[2];
                    upgrade.u8MaxHpItemCount = 1;
                }

                if (currentStats[3] > baseStats[3])
                {
                    upgrade.sAtbSpdItemId = mats[type];
                    upgrade.uAtbSpdGil = gilVals[type];
                    upgrade.i16AtbSpdLimit += baseStats[3];
                    upgrade.u8AtbSpdItemCount = 1;
                }

                if (currentStats[4] > baseStats[4])
                {
                    upgrade.sBrkBonusItemId = mats[type];
                    upgrade.uBrkBonusGil = gilVals[type];
                    upgrade.i16BrkBonusLimit += baseStats[4];
                    upgrade.u8BrkBonusItemCount = 1;
                }
            }
        }
    }

    private DataStoreRBtUpgrade[] GetAndRegisterBlankUpgrades(string name)
    {
        return Enumerable.Range(0, 3).Select(i =>
        {
            DataStoreRBtUpgrade upgrade = new();
            foreach (System.Reflection.PropertyInfo property in typeof(DataStoreRBtUpgrade).GetProperties())
            {
                if (property.PropertyType == typeof(string) && upgrade.GetPropValue<string>(property.Name) == null)
                {
                    upgrade.SetPropValue(property.Name, "");
                }
            }

            upgrade.record = $"{name}_{i}";
            if (i < 2)
            {
                upgrade.sNextId = $"{name}_{i + 1}";
            }

            upgrade.u2Rank = i;

            upgrades.Add(upgrade);
            if (i == 0)
            {
                itemWeapons[name].sUpgradeId = upgrade.record;
            }

            return upgrade;
        }).ToArray();
    }

    private void RandomizePassives()
    {
        List<DataStoreBtAutoAbility> filteredAbilities = GetFilteredAbilities();
        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility != ""))
        {
            equip.sAbility = filteredAbilities.ElementAt(RandomNum.RandInt(0, filteredAbilities.Count - 1)).record;
        }

        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility2 != ""))
        {
            IEnumerable<DataStoreBtAutoAbility> enumerable = filteredAbilities.Where(a => a.record != equip.sAbility);
            equip.sAbility2 = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1)).record;
        }

        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility3 != ""))
        {
            IEnumerable<DataStoreBtAutoAbility> enumerable = filteredAbilities.Where(a => a.record != equip.sAbility && a.record != equip.sAbility2);
            equip.sAbility3 = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1)).record;
        }

        foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
        {
            RandomizeGarbPassive(garb.sCosAbilityCir);
            RandomizeGarbPassive(garb.sCosAbilityCro);
            RandomizeGarbPassive(garb.sCosAbilityTri);
            RandomizeGarbPassive(garb.sCosAbilitySqu);
        }
    }

    private void RandomizeUpgradePassives()
    {
        int[] gilVals = { 20, 50, 100 };
        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => upgrades.Keys.Contains(w.sUpgradeId)))
        {
            DataStoreRBtUpgrade next = upgrades[equip.sUpgradeId];
            List<DataStoreRBtUpgrade> nextUpgrades = new();
            do
            {
                nextUpgrades.Add(next);
                next = upgrades.Keys.Contains(next.sNextId) ? upgrades[next.sNextId] : null;
            } while (next != null);

            if (equip.sAbility != "" && passiveData[equip.sAbility].UpgradeInto.Count > 0)
            {
                List<(string, int)> abiVals = GetRandomPassiveUpgrades(equip.sAbility, nextUpgrades.Count);
                for (int i = 0; i < nextUpgrades.Count; i++)
                {
                    nextUpgrades[i].i16Abi1Limit = abiVals[i].Item2;
                    nextUpgrades[i].uAbi1Gil = gilVals[i];
                    nextUpgrades[i].u8Abi1ItemCount = 1;
                    nextUpgrades[i].sAbi1Id = abiVals[i].Item1;
                    nextUpgrades[i].sAbi1ItemId = GetMaterialForUpgrade(nextUpgrades[i]);
                }
            }
            else
            {
                for (int i = 0; i < nextUpgrades.Count; i++)
                {
                    nextUpgrades[i].i16Abi1Limit = 0;
                    nextUpgrades[i].uAbi1Gil = 0;
                    nextUpgrades[i].u8Abi1ItemCount = 0;
                    nextUpgrades[i].sAbi1Id = "";
                    nextUpgrades[i].sAbi1ItemId = "";
                }
            }

            if (equip.sAbility2 != "" && passiveData[equip.sAbility2].UpgradeInto.Count > 0)
            {
                List<(string, int)> abiVals = GetRandomPassiveUpgrades(equip.sAbility2, nextUpgrades.Count);
                for (int i = 0; i < nextUpgrades.Count; i++)
                {
                    nextUpgrades[i].i16Abi2Limit = abiVals[i].Item2;
                    nextUpgrades[i].uAbi2Gil = gilVals[i];
                    nextUpgrades[i].u8Abi2ItemCount = 1;
                    nextUpgrades[i].sAbi2Id = abiVals[i].Item1;
                    nextUpgrades[i].sAbi2ItemId = GetMaterialForUpgrade(nextUpgrades[i]);
                }
            }
            else
            {
                for (int i = 0; i < nextUpgrades.Count; i++)
                {
                    nextUpgrades[i].i16Abi2Limit = 0;
                    nextUpgrades[i].uAbi2Gil = 0;
                    nextUpgrades[i].u8Abi2ItemCount = 0;
                    nextUpgrades[i].sAbi2Id = "";
                    nextUpgrades[i].sAbi2ItemId = "";
                }
            }
        }
    }

    private List<(string, int)> GetRandomPassiveUpgrades(string start, int count)
    {
        List<string> upgrades = new();
        string current = start;
        for (int i = 0; i < count; i++)
        {
            if (passiveData[current].UpgradeInto.Count > 0)
            {
                current = RandomNum.SelectRandom(passiveData[current].UpgradeInto);
                upgrades.Add(current);
            }
        }

        while (upgrades.Count < count)
        {
            int index = RandomNum.RandInt(0, upgrades.Count - 1);
            upgrades.Insert(index, upgrades[index]);
        }

        Dictionary<string, List<int>> distribution = upgrades.GroupBy(s => s, s => s).ToDictionary(g => g.Key, g =>
        {
            StatValues s = new(g.Count());
            s.Randomize(Enumerable.Range(0, g.Count()).Select(_ => (0, 15)).ToArray(), 15);
            return s.Vals.ToList();
        });

        return upgrades.Select(s =>
        {
            int val = distribution[s][0];
            distribution[s].RemoveAt(0);
            return (s, val);
        }).ToList();
    }

    private string GetMaterialForUpgrade(DataStoreRBtUpgrade upgrade)
    {
        string[] mats = { upgrade.sAbi1ItemId, upgrade.sAbi2ItemId, upgrade.sAtbSpdItemId, upgrade.sBrkBonusItemId, upgrade.sGuardItemId, upgrade.sMagAtkItemId, upgrade.sMaxHpItemId, upgrade.sPhyAtkItemId };
        return mats.First(s => s != "" && s.StartsWith("mat_cus"));
    }

    private void RandomizeGarbPassive(string name)
    {
        if (name.StartsWith("abi_"))
        {
            if (RandomNum.RandInt(0, 99) < 15)
            {
                List<DataStoreBtAutoAbility> filteredAbilities = GetFilteredAbilities();
                itemAbilities[name].sPasvAbility = filteredAbilities.ElementAt(RandomNum.RandInt(0, filteredAbilities.Count - 1)).record;
            }
            else
            {
                itemAbilities[name].sPasvAbility = "";
            }
        }
    }

    public List<DataStoreBtAutoAbility> GetFilteredAbilities()
    {
        return autoAbilities.Values.Where(a => passiveData.Keys.Contains(a.record)).ToList();
    }

    public override void Save()
    {
        itemWeapons.Values.Where(w => w.i16AtbSpeedModVal < 0).ForEach(w => w.i16AtbSpeedModVal += 65536);
        itemWeapons.Values.Where(w => w.i16MagicModVal < 0).ForEach(w => w.i16MagicModVal += 65536);

        upgrades.Values.Where(u => u.i16AtbSpdLimit < 0).ForEach(u => u.i16AtbSpdLimit += 65536);
        upgrades.Values.Where(u => u.i16BrkBonusLimit < 0).ForEach(u => u.i16BrkBonusLimit += 65536);

        itemAbilities.Values.Where(i => i.i8AtbDec < 0).ForEach(i => i.i8AtbDec += 256);

        RandoUI.SetUIProgressIndeterminate("Saving Equip Data...");
        itemWeapons.SaveWDB(Generator, @"\db\resident\item_weapon.wdb");
        RandoUI.SetUIProgressDeterminate("Saving Equip Data...", 20, 100);
        items.SaveWDB(Generator, @"\db\resident\item.wdb");
        RandoUI.SetUIProgressDeterminate("Saving Equip Data...", 40, 100);
        itemAbilities.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_item_abi.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_item_abi.wdb");
        RandoUI.SetUIProgressDeterminate("Saving Equip Data...", 80, 100);
        autoAbilities.DeleteWDB(Generator, @"\db\resident\bt_auto_ability.db3");
        passiveAbilities.DeleteWDB(Generator, @"\db\resident\_wdbpack.bin\r_pasv_ablty.db3");
        RandoUI.SetUIProgressDeterminate("Saving Equip Data...", 90, 100);
        upgrades.SaveWDB(Generator, @"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb");
        SetupData.WPDTracking[Generator.DataOutFolder + @"\db\resident\wdbpack.bin"].Add("r_bt_upgrade.wdb");
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        Dictionary<string, HTMLPage> pages = base.GetDocumentation();
        itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
        itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);

        HTMLPage page = new("Equipment", "template/documentation.html");

        page.HTMLElements.Add(new Table("Garbs", (new string[] { "Name", "Maximum ATB", "Default ATB", "Locked Abilities", "Passives" }).ToList(), (new int[] { 15, 10, 10, 30, 35 }).ToList(), itemWeapons.Values.Where(g => g.u4WeaponKind == (int)WeaponKind.Costume && items.Keys.Contains(g.record)).Select(g =>
        {
            string name = GetItemName(g.record);
            List<string> passiveNames = GetEquipPassivesDocs(g);
            List<string> abilityNames = new();
            if (g.sCosAbilityCir != "")
            {
                abilityNames.Add(GetAbilityName(g.sCosAbilityCir));
            }

            if (g.sCosAbilityCro != "")
            {
                abilityNames.Add(GetAbilityName(g.sCosAbilityCro));
            }

            if (g.sCosAbilitySqu != "")
            {
                abilityNames.Add(GetAbilityName(g.sCosAbilitySqu));
            }

            if (g.sCosAbilityTri != "")
            {
                abilityNames.Add(GetAbilityName(g.sCosAbilityTri));
            }

            return new string[] { name, g.i16AtbModVal.ToString(), g.i16AtbStartModVal.ToString(), string.Join(", ", abilityNames), string.Join(", ", passiveNames) }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Weapons", (new string[] { "Name", "Strength", "Magic", "HP", "ATB Speed", "Stagger Power", "Passives" }).ToList(), (new int[] { 15, 10, 10, 10, 10, 10, 35 }).ToList(), itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Weapon && w.u4AccessoryPos == 0 && items.Keys.Contains(w.record)).Select(w =>
        {
            string name = GetItemName(w.record);
            List<string> passiveNames = GetEquipPassivesDocs(w);

            return new string[] { name, w.i16AttackModVal.ToString(), w.i16MagicModVal.ToString(), w.i16HpModVal.ToString(), w.i16AtbSpeedModVal.ToString(), w.iBreakBonus.ToString(), string.Join(", ", passiveNames) }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Shields", (new string[] { "Name", "Strength", "Magic", "HP", "ATB Speed", "Guard Defense", "Passives" }).ToList(), (new int[] { 15, 10, 10, 10, 10, 10, 35 }).ToList(), itemWeapons.Values.Where(s => s.u4WeaponKind == (int)WeaponKind.Shield && items.Keys.Contains(s.record)).Select(s =>
        {
            string name = GetItemName(s.record);
            List<string> passiveNames = GetEquipPassivesDocs(s);

            return new string[] { name, s.i16AttackModVal.ToString(), s.i16MagicModVal.ToString(), s.i16HpModVal.ToString(), s.i16AtbSpeedModVal.ToString(), s.iGuardModVal.ToString(), string.Join(", ", passiveNames) }.ToList();
        }).ToList()));

        page.HTMLElements.Add(new Table("Accessories", (new string[] { "Name", "Passives" }).ToList(), (new int[] { 15, 85 }).ToList(), itemWeapons.Values.Where(s => s.u4AccessoryPos > 0 && items.Keys.Contains(s.record)).Select(s =>
        {
            string name = GetItemName(s.record);
            List<string> passiveNames = GetEquipPassivesDocs(s);

            return new string[] { name, string.Join(", ", passiveNames) }.ToList();
        }).ToList()));

        itemWeapons.Values.Where(w => w.i16AtbSpeedModVal < 0).ForEach(w => w.i16AtbSpeedModVal += 65536);
        itemWeapons.Values.Where(w => w.i16MagicModVal < 0).ForEach(w => w.i16MagicModVal += 65536);

        pages.Add("equipment", page);
        return pages;
    }

    private List<string> GetEquipPassivesDocs(DataStoreItemWeapon w)
    {
        List<string> passiveNames = new();
        if (w.sAbility != "")
        {
            passiveNames.Add(GetPassiveName(w.sAbility));
        }

        if (w.sAbility2 != "")
        {
            passiveNames.Add(GetPassiveName(w.sAbility2));
        }

        if (w.sAbility3 != "")
        {
            passiveNames.Add(GetPassiveName(w.sAbility3));
        }

        if (w.sCosAbilityCir != "" && itemAbilities.Keys.Contains(w.sCosAbilityCir) && itemAbilities[w.sCosAbilityCir].sPasvAbility != "")
        {
            passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityCir].sPasvAbility));
        }

        if (w.sCosAbilityCro != "" && itemAbilities.Keys.Contains(w.sCosAbilityCro) && itemAbilities[w.sCosAbilityCro].sPasvAbility != "")
        {
            passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityCro].sPasvAbility));
        }

        if (w.sCosAbilityTri != "" && itemAbilities.Keys.Contains(w.sCosAbilityTri) && itemAbilities[w.sCosAbilityTri].sPasvAbility != "")
        {
            passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityTri].sPasvAbility));
        }

        if (w.sCosAbilitySqu != "" && itemAbilities.Keys.Contains(w.sCosAbilitySqu) && itemAbilities[w.sCosAbilitySqu].sPasvAbility != "")
        {
            passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilitySqu].sPasvAbility));
        }

        return passiveNames;
    }

    public string GetItemName(string itemID)
    {
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if (itemID == "")
        {
            name = "Gil";
        }
        else if (abilityRando.abilities.Keys.Contains(itemID))
        {
            name = textRando.mainSysUS[abilityRando.abilities[itemID].sStringResId];
        }
        else if (items.Keys.Contains(itemID) && textRando.mainSysUS.Keys.Contains(items[itemID].sItemNameStringId))
        {
            name = textRando.mainSysUS[items[itemID].sItemNameStringId];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }
        else
        {
            name = itemID;
        }

        return name;
    }

    private string GetPassiveName(string passiveID)
    {
        TextRando textRando = Generator.Get<TextRando>();
        string name = "";
        if (autoAbilities[passiveID].sStringResId != "" && textRando.mainSysUS.Keys.Contains(autoAbilities[passiveID].sStringResId))
        {
            name = textRando.mainSysUS[autoAbilities[passiveID].sStringResId];
        }
        else if (autoAbilities[passiveID].sAutoAblArgStr0 != "")
        {
            name = textRando.mainSysUS[passiveAbilities[autoAbilities[passiveID].sAutoAblArgStr0].sStringResId];
        }

        if (name.Contains("{End}"))
        {
            name = name.Substring(0, name.IndexOf("{End}"));
        }

        name = name.Replace("{VarF7 64}", autoAbilities[passiveID].i16AutoAblArgInt0.ToString());
        name = name.Replace("{VarF7 65}", autoAbilities[passiveID].i16AutoAblArgInt1.ToString());
        name = name.Replace("+-", "-");

        return name;
    }

    private string GetAbilityName(string abilityID)
    {
        TextRando textRando = Generator.Get<TextRando>();
        AbilityRando abilityRando = Generator.Get<AbilityRando>();
        string name = "";
        if (abilityRando.abilities.Keys.Contains(abilityID))
        {
            name = textRando.mainSysUS[abilityRando.abilities[abilityID].sStringResId];
            name += " Lv. " + abilityRando.abilities[abilityID].u4Lv;
        }
        else if (itemAbilities[abilityID].sAbilityId != "" && abilityRando.abilities.Keys.Contains(itemAbilities[abilityID].sAbilityId))
        {
            name = textRando.mainSysUS[abilityRando.abilities[itemAbilities[abilityID].sAbilityId].sStringResId];
            name += " Lv. " + itemAbilities[abilityID].u4Lv;
        }

        return name;
    }
}
