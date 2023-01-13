using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class EquipRando : Randomizer
    {
        public DataStoreDB3<DataStoreItemWeapon> itemWeapons = new DataStoreDB3<DataStoreItemWeapon>();
        public DataStoreDB3<DataStoreItem> items = new DataStoreDB3<DataStoreItem>();
        public DataStoreDB3<DataStoreItem> itemsOrig = new DataStoreDB3<DataStoreItem>();
        public DataStoreDB3<DataStoreBtAutoAbility> autoAbilities = new DataStoreDB3<DataStoreBtAutoAbility>();
        public DataStoreDB3<DataStoreRPassiveAbility> passiveAbilities = new DataStoreDB3<DataStoreRPassiveAbility>();
        public DataStoreDB3<DataStoreRItemAbi> itemAbilities = new DataStoreDB3<DataStoreRItemAbi>();
        public DataStoreDB3<DataStoreRItemAbi> itemAbilitiesOrig = new DataStoreDB3<DataStoreRItemAbi>();
        public DataStoreDB3<DataStoreRBtUpgrade> upgrades = new DataStoreDB3<DataStoreRBtUpgrade>();
        Dictionary<string, AbilityData> abilityData = new Dictionary<string, AbilityData>();

        Dictionary<string, PassiveData> passiveData = new Dictionary<string, PassiveData>();

        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Equip Data...", 0, 100);
            itemWeapons.LoadDB3("LR", @"\db\resident\item_weapon.wdb");
            Randomizers.SetUIProgress("Loading Equip Data...", 10, 100);
            items.LoadDB3("LR", @"\db\resident\item.wdb");
            Randomizers.SetUIProgress("Loading Equip Data...", 20, 100);
            itemsOrig.LoadDB3("LR", @"\db\resident\item.wdb");
            Randomizers.SetUIProgress("Loading Equip Data...", 30, 100);
            autoAbilities.LoadDB3("LR", @"\db\resident\bt_auto_ability.wdb");
            Randomizers.SetUIProgress("Loading Equip Data...", 40, 100);
            itemAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            Randomizers.SetUIProgress("Loading Equip Data...", 60, 100);
            itemAbilitiesOrig.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            Randomizers.SetUIProgress("Loading Equip Data...", 70, 100);
            passiveAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_pasv_ablty.wdb", false);
            Randomizers.SetUIProgress("Loading Equip Data...", 80, 100);
            upgrades.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb", false);

            FileHelpers.ReadCSVFile(@"data\passives.csv", row =>
            {
                PassiveData p = new PassiveData(row);
                passiveData.Add(p.ID, p);
            }, FileHelpers.CSVFileHeader.HasHeader);

            FileHelpers.ReadCSVFile(@"data\abilities.csv", row =>
            {
                AbilityData a = new AbilityData(row);
                abilityData.Add(a.ID, a);
            }, FileHelpers.CSVFileHeader.HasHeader);

            /*
            items.InsertCopyAlphabetical("key_b_20", "key_r_kanki");
            items["key_r_kanki"].sItemNameStringId_string = "$m_001";
            items["key_r_kanki"].sHelpStringId_string = "$m_001_ac000";
            items["key_r_kanki"].u16SortAllByKCategory = 100;
            items["key_r_kanki"].u16SortCategoryByCategory = 150;
            */
            items.InsertCopyAlphabetical("key_b_20", "key_r_multi");
            items["key_r_multi"].sItemNameStringId_string = "$m_001_ac100";
            items["key_r_multi"].sHelpStringId_string = "$m_002";
            items["key_r_multi"].u16SortAllByKCategory = 101;
            items["key_r_multi"].u16SortCategoryByCategory = 151;

            Randomizers.SetUIProgress("Loading Equip Data...", 90, 100);
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);

            upgrades.Values.Where(u => u.i16AtbSpdLimit >= 32768).ForEach(u => u.i16AtbSpdLimit -= 65536);
            upgrades.Values.Where(u => u.i16BrkBonusLimit >= 32768).ForEach(u => u.i16BrkBonusLimit -= 65536);

            autoAbilities.Values.Where(a => a.i16AutoAblArgInt0 >= 32768).ForEach(a => a.i16AutoAblArgInt0 -= 65536);
            autoAbilities.Values.Where(a => a.i16AutoAblArgInt1 >= 32768).ForEach(a => a.i16AutoAblArgInt1 -= 65536);

            itemAbilities.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);
            itemAbilitiesOrig.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Equip Data...", 0, 100);
            if (LRFlags.StatsAbilities.EquipStats.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipStats.SetRand();
                RandomizeStats();

                // Clear vanilla upgrades as they don't matter
                upgrades.Clear();

                RandomizeUpgrades();
                RandomNum.ClearRand();

                itemWeapons.Values.Where(w => !upgrades.Keys.Contains(w.sUpgradeId_string)).ForEach(w => w.sUpgradeId_string = "");
            }

            Randomizers.SetUIProgress("Randomizing Equip Data...", 40, 100);
            if (LRFlags.StatsAbilities.GarbAbilities.FlagEnabled)
            {
                LRFlags.StatsAbilities.GarbAbilities.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Equip Data...", 70, 100);
            if (LRFlags.StatsAbilities.EquipPassives.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipPassives.SetRand();
                RandomizePassives();
                RandomizeUpgradePassives();
                RandomNum.ClearRand();
            }

            itemWeapons.Values.Where(a => a.u4AccessoryPos > 0 && items.Keys.Contains(a.name)).ForEach(a =>
            {
                items[a.name].uPurchasePrice = 50000;
                items[a.name].u1OnlyOne = 1;
            });
        }

        private void RandomizeAbilities()
        {
            foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
            {
                string forceWeaponType = "";
                if (garb.name == "cos_ba00")
                    forceWeaponType = "wea_ea08";
                if (garb.name == "cos_ca00")
                    forceWeaponType = "wea_ca00";

                do
                {
                    garb.sCosAbilityCir_string = RandomizeAbility(garb.sCosAbilityCir_string, forceWeaponType == "" ? -1 : (itemWeapons[forceWeaponType].i16AttackModVal > itemWeapons[forceWeaponType].i16MagicModVal ? 26 : 27));
                    garb.sCosAbilityCro_string = RandomizeAbility(garb.sCosAbilityCro_string, -1);
                    garb.sCosAbilityTri_string = RandomizeAbility(garb.sCosAbilityTri_string, -1);
                    garb.sCosAbilitySqu_string = RandomizeAbility(garb.sCosAbilitySqu_string, -1);
                } while (new string[] { garb.sCosAbilityCir_string, garb.sCosAbilityCro_string, garb.sCosAbilityTri_string, garb.sCosAbilitySqu_string }.Distinct().Where(x => x.StartsWith("abi_")).GroupBy(x => itemAbilities[x].sAbilityId_string).Any(g => g.Count() > 1));
            }
        }

        private string RandomizeAbility(string name, int forceType)
        {
            AbilityRando abilityRando = Randomizers.Get<AbilityRando>();
            if (name != "")
            {
                List<string> possible = GetGarbAbilities(forceType);
                string newAbility = possible.ElementAt(RandomNum.RandInt(0, possible.Count() - 1));
                if (name.StartsWith("abi_") && !name.EndsWith("zz99"))
                {
                    string origAbility = itemAbilities[name].sAbilityId_string;
                    itemAbilities[name].sAbilityId_string = newAbility;
                    items[name].sItemNameStringId_string = "";
                    items[name].sHelpStringId_string = "";
                    items[name].sScriptId_string = "";
                    items[name].u8MenuIcon = abilityData[newAbility].MenuIcon;

                    if (itemAbilities[name].u4Lv < 1)
                        itemAbilities[name].u4Lv = 1;

                    int origATB = abilityData[origAbility].ATBCost;
                    float origATBMult = (abilityData[origAbility].ATBCost - itemAbilities[name].i8AtbDec) / (float)origATB;
                    int newATB = abilityData[newAbility].ATBCost;
                    itemAbilities[name].i8AtbDec = -(int)(Math.Ceiling(newATB * origATBMult) - abilityData[newAbility].ATBCost);

                    int origExpectedPower = abilityData[origAbility].BasePower + (abilityRando.abilityGrowths[origAbility].GetPowMin(itemAbilities[name].u4Lv, abilityData[origAbility].HitMultiplier) + abilityRando.abilityGrowths[origAbility].GetPowMax(itemAbilities[name].u4Lv, abilityData[origAbility].HitMultiplier)) / 2;
                    float origMult = (abilityData[origAbility].BasePower + itemAbilities[name].iPower * abilityData[origAbility].HitMultiplier) / (float)origExpectedPower;
                    int newExpectedPower = abilityData[newAbility].BasePower + (abilityRando.abilityGrowths[newAbility].GetPowMin(itemAbilities[name].u4Lv, abilityData[newAbility].HitMultiplier) + abilityRando.abilityGrowths[newAbility].GetPowMax(itemAbilities[name].u4Lv, abilityData[newAbility].HitMultiplier)) / 2;
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
            List<string> list = itemsOrig.Values.Where(i => IsAbility(i) && (forceType == -1 || i.u8MenuIcon == forceType)).Select(i => i.name).ToList();

            return list.Select(s => itemsOrig[s]).GroupBy(i => i.sScriptId_string).Select(g => g.First()).ToList();
        }

        public bool IsAbility(string item)
        {
            return items.Keys.Contains(item) && IsAbility(items[item]);
        }

        public bool IsAbility(DataStoreItem item)
        {
            return item.u8ItemCategory == (int)ItemCategory.Ability && !item.name.StartsWith("abi_");
        }

        private void RandomizeStats()
        {
            foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
            {
                (int, int)[] bounds = {
                    (-75, 100),
                    (0, 100)
                };
                float[] weights = new float[] { 3, 1 };
                int[] zeros = new int[] { 50, 5 };
                int[] negs = new int[] { 50, 0 };
                StatPoints statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { garb.i16AtbModVal, garb.i16AtbStartModVal });

                garb.i16AtbModVal = statPoints[0];
                garb.i16AtbStartModVal = statPoints[1];

                /*
                garb.i16AtbModVal = 100;
                garb.i16AtbStartModVal = 100;
                */

                if (items.Keys.Contains(garb.name) && items[garb.name].uPurchasePrice == 0)
                {
                    items[garb.name].uPurchasePrice = 50000;
                }
            }
            foreach (DataStoreItemWeapon weapon in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Weapon && w.u4AccessoryPos == 0))
            {
                bool starting = weapon.name == "wea_ea08" || weapon.name == "wea_ca00";

                StatPoints statPoints;
                do
                {
                    (int, int)[] bounds = {
                        (-2000, 5000),
                        (-2000, 5000),
                        (-5000, 20000),
                        (-25, 50),
                        (-90, 75)
                    };
                    float[] weights = new float[] { 2, 2, 3, 6, 4 };
                    int[] zeros = new int[] { 10, 10, 85, 60, 80 };
                    int[] negs = new int[] { 15, 15, 40, 10, 5 };
                    statPoints = new StatPoints(bounds, weights, zeros, negs);
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
                bool starting = shield.name == "shi_ea08" || shield.name == "shi_ca00";

                StatPoints statPoints;
                do
                {
                    (int, int)[] bounds = {
                        (-2000, 5000),
                        (-2000, 5000),
                        (-5000, 20000),
                        (-25, 50),
                        (0, 800)
                    };
                    float[] weights = new float[] { 6, 6, 2, 4, 4 };
                    int[] zeros = new int[] { 90, 90, 40, 30, 20 };
                    int[] negs = new int[] { 30, 30, 15, 5, 0 };
                    statPoints = new StatPoints(bounds, weights, zeros, negs);
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
                    50000,
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

                DataStoreRBtUpgrade[] shieldUpgrades = GetAndRegisterBlankUpgrades(shield.name);
                if (shield.u16UpgradeLimit < 10)
                    shield.u16UpgradeLimit = 10;
                int upgradesRemaining = shield.u16UpgradeLimit * RandomNum.RandInt(105, 150) / 100;

                while (upgradesRemaining > 0)
                {
                    int type = upgradesRemaining > 125 ? 0 : (upgradesRemaining > 25 ? 1 : 2);

                    int next = RandomNum.SelectRandomWeighted(Enumerable.Range(0, baseStats.Length).ToList(), i =>
                    {
                        int weight = 0;
                        if (currentStats[i] + inc[i][type] > bounds[i] || (i < 2 && baseStats[i] <= 0))
                        {
                            weight = 0;
                        }
                        else if (baseStats[i] < 0)
                        {
                            weight = 5 + type * 10;
                        }
                        else
                        {
                            weight = (100 - Math.Abs(bounds[i] / 2 - currentStats[i]) * 100 / bounds[i]) + 1;
                        }
                        if (i >= 2)
                            weight *= 4;
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
                if (currentStats[2] > 15000 || currentStats[3] > 40 || currentStats[4] > 500)
                    mats = mats.TakeLast(shieldUpgrades.Length).ToArray();
                else
                    mats = mats.Take(shieldUpgrades.Length).ToArray();

                for (int type = 0; type < shieldUpgrades.Length; type++)
                {
                    DataStoreRBtUpgrade upgrade = shieldUpgrades[type];
                    if (currentStats[0] > baseStats[0])
                    {
                        upgrade.sPhyAtkItemId_string = mats[type];
                        upgrade.uPhyAtkGil = gilVals[type];
                        upgrade.i16PhyAtkLimit += baseStats[0];
                        upgrade.u8PhyAtkItemCount = 1;
                    }
                    if (currentStats[1] > baseStats[1])
                    {
                        upgrade.sMagAtkItemId_string = mats[type];
                        upgrade.uMagAtkGil = gilVals[type];
                        upgrade.i16MagAtkLimit += baseStats[1];
                        upgrade.u8MagAtkItemCount = 1;
                    }
                    if (currentStats[2] > baseStats[2])
                    {
                        upgrade.sMaxHpItemId_string = mats[type];
                        upgrade.uMaxHpGil = gilVals[type];
                        upgrade.i16MaxHpLimit += baseStats[2];
                        upgrade.u8MaxHpItemCount = 1;
                    }
                    if (currentStats[3] > baseStats[3])
                    {
                        upgrade.sAtbSpdItemId_string = mats[type];
                        upgrade.uAtbSpdGil = gilVals[type];
                        upgrade.i16AtbSpdLimit += baseStats[3];
                        upgrade.u8AtbSpdItemCount = 1;
                    }
                    if (currentStats[4] > baseStats[4])
                    {
                        upgrade.sGuardItemId_string = mats[type];
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
                    50000,
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

                DataStoreRBtUpgrade[] weaponUpgrades = GetAndRegisterBlankUpgrades(weapon.name);
                if (weapon.u16UpgradeLimit < 10)
                    weapon.u16UpgradeLimit = 10;
                int upgradesRemaining = weapon.u16UpgradeLimit * RandomNum.RandInt(105, 150) / 100;

                while (upgradesRemaining > 0)
                {
                    int type = upgradesRemaining > 125 ? 0 : (upgradesRemaining > 25 ? 1 : 2);

                    int next = RandomNum.SelectRandomWeighted(Enumerable.Range(0, baseStats.Length).ToList(), i =>
                    {
                        int weight = 0;
                        if (currentStats[i] + inc[i][type] > bounds[i] || (i >= 2 && baseStats[i] <= 0))
                        {
                            weight = 0;
                        }
                        else if (baseStats[i] < 0)
                        {
                            weight = 5 + type * 10;
                        }
                        else
                        {
                            weight = (100 - Math.Abs(bounds[i] / 2 - currentStats[i]) * 100 / bounds[i]) + 1;
                        }
                        if (i < 2)
                            weight *= 4;
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
                if (currentStats[0] > 4000 || currentStats[1] > 4000)
                    mats = mats.TakeLast(weaponUpgrades.Length).ToArray();
                else
                    mats = mats.Take(weaponUpgrades.Length).ToArray();

                for (int type = 0; type < weaponUpgrades.Length; type++)
                {
                    DataStoreRBtUpgrade upgrade = weaponUpgrades[type];
                    if (currentStats[0] > baseStats[0])
                    {
                        upgrade.sPhyAtkItemId_string = mats[type];
                        upgrade.uPhyAtkGil = gilVals[type];
                        upgrade.i16PhyAtkLimit += baseStats[0];
                        upgrade.u8PhyAtkItemCount = 1;
                    }
                    if (currentStats[1] > baseStats[1])
                    {
                        upgrade.sMagAtkItemId_string = mats[type];
                        upgrade.uMagAtkGil = gilVals[type];
                        upgrade.i16MagAtkLimit += baseStats[1];
                        upgrade.u8MagAtkItemCount = 1;
                    }
                    if (currentStats[2] > baseStats[2])
                    {
                        upgrade.sMaxHpItemId_string = mats[type];
                        upgrade.uMaxHpGil = gilVals[type];
                        upgrade.i16MaxHpLimit += baseStats[2];
                        upgrade.u8MaxHpItemCount = 1;
                    }
                    if (currentStats[3] > baseStats[3])
                    {
                        upgrade.sAtbSpdItemId_string = mats[type];
                        upgrade.uAtbSpdGil = gilVals[type];
                        upgrade.i16AtbSpdLimit += baseStats[3];
                        upgrade.u8AtbSpdItemCount = 1;
                    }
                    if (currentStats[4] > baseStats[4])
                    {
                        upgrade.sBrkBonusItemId_string = mats[type];
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
                DataStoreRBtUpgrade upgrade = new DataStoreRBtUpgrade();
                foreach (var property in typeof(DataStoreRBtUpgrade).GetProperties())
                {
                    if (property.PropertyType == typeof(string) && upgrade.GetPropValue<string>(property.Name) == null)
                        upgrade.SetPropValue(property.Name, "");
                }
                    
                upgrade.name = $"{name}_{i}";
                if (i < 2)
                {
                    upgrade.sNextId_string = $"{name}_{i + 1}";
                }
                upgrade.u2Rank = i;

                upgrades.Add(upgrade, 104);
                if (i == 0)
                {
                    itemWeapons[name].sUpgradeId_string = upgrade.name;
                }

                return upgrade;
            }).ToArray();
        }

        private void RandomizePassives()
        {
            List<DataStoreBtAutoAbility> filteredAbilities = GetFilteredAbilities();
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility_string != ""))
            {
                equip.sAbility_string = filteredAbilities.ElementAt(RandomNum.RandInt(0, filteredAbilities.Count - 1)).name;
            }
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility2_string != ""))
            {
                IEnumerable<DataStoreBtAutoAbility> enumerable = filteredAbilities.Where(a => a.name != equip.sAbility_string);
                equip.sAbility2_string = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1)).name;
            }
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility3_string != ""))
            {
                IEnumerable<DataStoreBtAutoAbility> enumerable = filteredAbilities.Where(a => a.name != equip.sAbility_string && a.name != equip.sAbility2_string);
                equip.sAbility3_string = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1)).name;
            }

            foreach (DataStoreItemWeapon garb in itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Costume))
            {
                RandomizeGarbPassive(garb.sCosAbilityCir_string);
                RandomizeGarbPassive(garb.sCosAbilityCro_string);
                RandomizeGarbPassive(garb.sCosAbilityTri_string);
                RandomizeGarbPassive(garb.sCosAbilitySqu_string);
            }
        }

        private void RandomizeUpgradePassives()
        {
            int[] gilVals = { 20, 50, 100 };
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => upgrades.Keys.Contains(w.sUpgradeId_string)))
            {
                DataStoreRBtUpgrade next = upgrades[equip.sUpgradeId_string];
                List<DataStoreRBtUpgrade> nextUpgrades = new List<DataStoreRBtUpgrade>();
                do
                {
                    nextUpgrades.Add(next);
                    next = upgrades.Keys.Contains(next.sNextId_string) ? upgrades[next.sNextId_string] : null;
                } while (next != null);

                if (equip.sAbility_string != "" && passiveData[equip.sAbility_string].UpgradeInto.Count > 0)
                {
                    List<(string, int)> abiVals = GetRandomPassiveUpgrades(equip.sAbility_string, nextUpgrades.Count);
                    for(int i = 0; i < nextUpgrades.Count; i++)
                    {
                        nextUpgrades[i].i16Abi1Limit = abiVals[i].Item2;
                        nextUpgrades[i].uAbi1Gil = gilVals[i];
                        nextUpgrades[i].u8Abi1ItemCount = 1;
                        nextUpgrades[i].sAbi1Id_string = abiVals[i].Item1;
                        nextUpgrades[i].sAbi1ItemId_string = GetMaterialForUpgrade(nextUpgrades[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < nextUpgrades.Count; i++)
                    {
                        nextUpgrades[i].i16Abi1Limit = 0;
                        nextUpgrades[i].uAbi1Gil = 0;
                        nextUpgrades[i].u8Abi1ItemCount = 0;
                        nextUpgrades[i].sAbi1Id_string = "";
                        nextUpgrades[i].sAbi1ItemId_string = "";
                    }
                }

                if (equip.sAbility2_string != "" && passiveData[equip.sAbility2_string].UpgradeInto.Count > 0)
                {
                    List<(string, int)> abiVals = GetRandomPassiveUpgrades(equip.sAbility2_string, nextUpgrades.Count);
                    for (int i = 0; i < nextUpgrades.Count; i++)
                    {
                        nextUpgrades[i].i16Abi2Limit = abiVals[i].Item2;
                        nextUpgrades[i].uAbi2Gil = gilVals[i];
                        nextUpgrades[i].u8Abi2ItemCount = 1;
                        nextUpgrades[i].sAbi2Id_string = abiVals[i].Item1;
                        nextUpgrades[i].sAbi2ItemId_string = GetMaterialForUpgrade(nextUpgrades[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < nextUpgrades.Count; i++)
                    {
                        nextUpgrades[i].i16Abi2Limit = 0;
                        nextUpgrades[i].uAbi2Gil = 0;
                        nextUpgrades[i].u8Abi2ItemCount = 0;
                        nextUpgrades[i].sAbi2Id_string = "";
                        nextUpgrades[i].sAbi2ItemId_string = "";
                    }
                }
            }
        }

        private List<(string, int)> GetRandomPassiveUpgrades(string start, int count)
        {
            List<string> upgrades = new List<string>();
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
                StatValues s = new StatValues(g.Count());
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
            string[] mats = { upgrade.sAbi1ItemId_string, upgrade.sAbi2ItemId_string, upgrade.sAtbSpdItemId_string, upgrade.sBrkBonusItemId_string, upgrade.sGuardItemId_string, upgrade.sMagAtkItemId_string, upgrade.sMaxHpItemId_string, upgrade.sPhyAtkItemId_string };
            return mats.First(s => s != "" && s.StartsWith("mat_cus"));
        }

        private void RandomizeGarbPassive(string name)
        {
            if (name.StartsWith("abi_"))
            {
                if (RandomNum.RandInt(0, 99) < 15)
                {
                    List<DataStoreBtAutoAbility> filteredAbilities = GetFilteredAbilities();
                    itemAbilities[name].sPasvAbility_string = filteredAbilities.ElementAt(RandomNum.RandInt(0, filteredAbilities.Count - 1)).name;
                }
                else
                    itemAbilities[name].sPasvAbility_string = "";
            }
        }

        public List<DataStoreBtAutoAbility> GetFilteredAbilities()
        {
            return autoAbilities.Values.Where(a => passiveData.Keys.Contains(a.name)).ToList();
        }

        public override void Save()
        {
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal < 0).ForEach(w => w.i16AtbSpeedModVal += 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal < 0).ForEach(w => w.i16MagicModVal += 65536);

            upgrades.Values.Where(u => u.i16AtbSpdLimit < 0).ForEach(u => u.i16AtbSpdLimit += 65536);
            upgrades.Values.Where(u => u.i16BrkBonusLimit < 0).ForEach(u => u.i16BrkBonusLimit += 65536);

            itemAbilities.Values.Where(i => i.i8AtbDec < 0).ForEach(i => i.i8AtbDec += 256);

            Randomizers.SetUIProgress("Saving Equip Data...", 0, 100);
            itemWeapons.SaveDB3(@"\db\resident\item_weapon.wdb");
            Randomizers.SetUIProgress("Saving Equip Data...", 20, 100);
            items.SaveDB3(@"\db\resident\item.wdb");
            Randomizers.SetUIProgress("Saving Equip Data...", 40, 100);
            itemAbilities.SaveDB3(@"\db\resident\_wdbpack.bin\r_item_abi.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_item_abi.wdb");
            Randomizers.SetUIProgress("Saving Equip Data...", 80, 100);
            autoAbilities.DeleteDB3(@"\db\resident\bt_auto_ability.db3");
            passiveAbilities.DeleteDB3(@"\db\resident\_wdbpack.bin\r_pasv_ablty.db3");
            Randomizers.SetUIProgress("Saving Equip Data...", 90, 100);
            upgrades.SaveDB3(@"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_bt_upgrade.wdb");
            TempSaveFix();
        }

        private void TempSaveFix()
        {
            byte[] data = File.ReadAllBytes(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb");

            uint startUpgradeData = data.ReadUInt(0xE0);
            if (data.ReadUInt(0x100) - startUpgradeData == 0x64)
            {
                List<DataStoreRBtUpgrade> values = upgrades.Values.ToList();
                for (int i = 0; i < values.Count; i++)
                {
                    data.SetUInt(0xE0 + 0x20 * i, (uint)(startUpgradeData + 0x68 * i));
                    data.SetUInt((int)startUpgradeData + 0x68 * i, (uint)values[i].sNextId_pointer);
                    byte[] missingBytes = new byte[4];
                    missingBytes.SetUInt(0, (uint)values[i].u8Abi2ItemCount);
                    data = data.SubArray(0, (int)startUpgradeData + 0x68 * i + 0x64).Concat(missingBytes).Concat(data.SubArray((int)startUpgradeData + 0x68 * i + 0x64, data.Length - ((int)startUpgradeData + 0x68 * i + 0x64)));
                }

                File.WriteAllBytes(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_bt_upgrade.wdb", data);
            }
        }

        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);

            HTMLPage page = new HTMLPage("Equipment", "template/documentation.html");

            page.HTMLElements.Add(new Table("Garbs", (new string[] { "Name", "Maximum ATB", "Default ATB", "Locked Abilities", "Passives" }).ToList(), (new int[] { 15, 10, 10, 30, 35 }).ToList(), itemWeapons.Values.Where(g => g.u4WeaponKind == (int)WeaponKind.Costume && items.Keys.Contains(g.name)).Select(g =>
            {
                string name = GetItemName(g.name);
                List<string> passiveNames = GetEquipPassivesDocs(g);
                List<string> abilityNames = new List<string>();
                if (g.sCosAbilityCir_string != "")
                    abilityNames.Add(GetAbilityName(g.sCosAbilityCir_string));
                if (g.sCosAbilityCro_string != "")
                    abilityNames.Add(GetAbilityName(g.sCosAbilityCro_string));
                if (g.sCosAbilitySqu_string != "")
                    abilityNames.Add(GetAbilityName(g.sCosAbilitySqu_string));
                if (g.sCosAbilityTri_string != "")
                    abilityNames.Add(GetAbilityName(g.sCosAbilityTri_string));


                return new string[] { name, g.i16AtbModVal.ToString(), g.i16AtbStartModVal.ToString(), string.Join(", ", abilityNames), string.Join(", ", passiveNames) }.ToList();
            }).ToList()));

            page.HTMLElements.Add(new Table("Weapons", (new string[] { "Name", "Strength", "Magic", "HP", "ATB Speed", "Stagger Power", "Passives" }).ToList(), (new int[] { 15, 10, 10, 10, 10, 10, 35 }).ToList(), itemWeapons.Values.Where(w => w.u4WeaponKind == (int)WeaponKind.Weapon && w.u4AccessoryPos == 0 && items.Keys.Contains(w.name)).Select(w =>
            {
                string name = GetItemName(w.name);
                List<string> passiveNames = GetEquipPassivesDocs(w);

                return new string[] { name, w.i16AttackModVal.ToString(), w.i16MagicModVal.ToString(), w.i16HpModVal.ToString(), w.i16AtbSpeedModVal.ToString(), w.iBreakBonus.ToString(), string.Join(", ", passiveNames) }.ToList();
            }).ToList()));

            page.HTMLElements.Add(new Table("Shields", (new string[] { "Name", "Strength", "Magic", "HP", "ATB Speed", "Guard Defense", "Passives" }).ToList(), (new int[] { 15, 10, 10, 10, 10, 10, 35 }).ToList(), itemWeapons.Values.Where(s => s.u4WeaponKind == (int)WeaponKind.Shield && items.Keys.Contains(s.name)).Select(s =>
            {
                string name = GetItemName(s.name);
                List<string> passiveNames = GetEquipPassivesDocs(s);

                return new string[] { name, s.i16AttackModVal.ToString(), s.i16MagicModVal.ToString(), s.i16HpModVal.ToString(), s.i16AtbSpeedModVal.ToString(), s.iGuardModVal.ToString(), string.Join(", ", passiveNames) }.ToList();
            }).ToList()));

            page.HTMLElements.Add(new Table("Accessories", (new string[] { "Name", "Passives" }).ToList(), (new int[] { 15, 85 }).ToList(), itemWeapons.Values.Where(s => s.u4AccessoryPos > 0 && items.Keys.Contains(s.name)).Select(s =>
            {
                string name = GetItemName(s.name);
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
            List<string> passiveNames = new List<string>();
            if (w.sAbility_string != "")
                passiveNames.Add(GetPassiveName(w.sAbility_string));
            if (w.sAbility2_string != "")
                passiveNames.Add(GetPassiveName(w.sAbility2_string));
            if (w.sAbility3_string != "")
                passiveNames.Add(GetPassiveName(w.sAbility3_string));
            if (w.sCosAbilityCir_string != "" && itemAbilities.Keys.Contains(w.sCosAbilityCir_string) && itemAbilities[w.sCosAbilityCir_string].sPasvAbility_string != "")
                passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityCir_string].sPasvAbility_string));
            if (w.sCosAbilityCro_string != "" && itemAbilities.Keys.Contains(w.sCosAbilityCro_string) && itemAbilities[w.sCosAbilityCro_string].sPasvAbility_string != "")
                passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityCro_string].sPasvAbility_string));
            if (w.sCosAbilityTri_string != "" && itemAbilities.Keys.Contains(w.sCosAbilityTri_string) && itemAbilities[w.sCosAbilityTri_string].sPasvAbility_string != "")
                passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilityTri_string].sPasvAbility_string));
            if (w.sCosAbilitySqu_string != "" && itemAbilities.Keys.Contains(w.sCosAbilitySqu_string) && itemAbilities[w.sCosAbilitySqu_string].sPasvAbility_string != "")
                passiveNames.Add(GetPassiveName(itemAbilities[w.sCosAbilitySqu_string].sPasvAbility_string));
            return passiveNames;
        }

        private string GetItemName(string itemID)
        {
            TextRando textRando = Randomizers.Get<TextRando>();
            string name = textRando.mainSysUS[items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
                name = name.Substring(0, name.IndexOf("{End}"));

            return name;
        }

        private string GetPassiveName(string passiveID)
        {
            TextRando textRando = Randomizers.Get<TextRando>();
            string name = "";
            if (autoAbilities[passiveID].sStringResId_string != "" && textRando.mainSysUS.Keys.Contains(autoAbilities[passiveID].sStringResId_string))
                name = textRando.mainSysUS[autoAbilities[passiveID].sStringResId_string];
            else if (autoAbilities[passiveID].sAutoAblArgStr0_string != "")
                name = textRando.mainSysUS[passiveAbilities[autoAbilities[passiveID].sAutoAblArgStr0_string].sStringResId_string];
            if (name.Contains("{End}"))
                name = name.Substring(0, name.IndexOf("{End}"));
            name = name.Replace("{VarF7 64}", autoAbilities[passiveID].i16AutoAblArgInt0.ToString());
            name = name.Replace("{VarF7 65}", autoAbilities[passiveID].i16AutoAblArgInt1.ToString());
            name = name.Replace("+-", "-");

            return name;
        }

        private string GetAbilityName(string abilityID)
        {
            TextRando textRando = Randomizers.Get<TextRando>();
            AbilityRando abilityRando = Randomizers.Get<AbilityRando>();
            string name = "";
            if (abilityRando.abilities.Keys.Contains(abilityID))
            {
                name = textRando.mainSysUS[abilityRando.abilities[abilityID].sStringResId_string];
                name += " Lv. " + abilityRando.abilities[abilityID].u4Lv;
            }
            else if (itemAbilities[abilityID].sAbilityId_string != "" && abilityRando.abilities.Keys.Contains(itemAbilities[abilityID].sAbilityId_string))
            {
                name = textRando.mainSysUS[abilityRando.abilities[itemAbilities[abilityID].sAbilityId_string].sStringResId_string];
                name += " Lv. " + itemAbilities[abilityID].u4Lv;
            }

            return name;
        }

        public class AbilityData : CSVDataRow
        {
            [RowIndex(0)]
            public string ID { get; set; }
            [RowIndex(1)]
            public int BasePower { get; set; }
            [RowIndex(2)]
            public int HitMultiplier { get; set; }
            [RowIndex(3)]
            public int ATBCost { get; set; }
            [RowIndex(4)]
            public int MenuIcon { get; set; }
            public AbilityData(string[] row) : base(row)
            {
            }
        }

        public class PassiveData : CSVDataRow
        {
            [RowIndex(0)]
            public string ID { get; set; }

            [RowIndex(1)]
            public string Name { get; set; }

            [RowIndex(2)]
            public List<string> UpgradeInto { get; set; }
            public PassiveData(string[] row) : base(row)
            {
            }
        }
    }
}
