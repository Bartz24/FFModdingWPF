using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
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
        Dictionary<string, AbilityData> abilityData = new Dictionary<string, AbilityData>();

        List<string> passives = new List<string>();

        public EquipRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Equip Data...", 0, 100);
            itemWeapons.LoadDB3("LR", @"\db\resident\item_weapon.wdb");
            Randomizers.SetProgressFunc("Loading Equip Data...", 10, 100);
            items.LoadDB3("LR", @"\db\resident\item.wdb");
            Randomizers.SetProgressFunc("Loading Equip Data...", 30, 100);
            itemsOrig.LoadDB3("LR", @"\db\resident\item.wdb");
            Randomizers.SetProgressFunc("Loading Equip Data...", 40, 100);
            autoAbilities.LoadDB3("LR", @"\db\resident\bt_auto_ability.wdb");
            Randomizers.SetProgressFunc("Loading Equip Data...", 50, 100);
            itemAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            Randomizers.SetProgressFunc("Loading Equip Data...", 70, 100);
            itemAbilitiesOrig.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            Randomizers.SetProgressFunc("Loading Equip Data...", 80, 100);
            passiveAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_pasv_ablty.wdb", false);

            FileHelpers.ReadCSVFile(@"data\passives.csv", row =>
            {
                passives.Add(row[0]);
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

            Randomizers.SetProgressFunc("Loading Equip Data...", 90, 100);
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);

            autoAbilities.Values.Where(a => a.i16AutoAblArgInt0 >= 32768).ForEach(a => a.i16AutoAblArgInt0 -= 65536);
            autoAbilities.Values.Where(a => a.i16AutoAblArgInt1 >= 32768).ForEach(a => a.i16AutoAblArgInt1 -= 65536);

            itemAbilities.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);
            itemAbilitiesOrig.Values.Where(i => i.i8AtbDec >= 128).ForEach(i => i.i8AtbDec -= 256);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Equip Data...", 0, 100);
            if (LRFlags.StatsAbilities.EquipStats.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipStats.SetRand();
                RandomizeStats();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Equip Data...", 40, 100);
            if (LRFlags.StatsAbilities.GarbAbilities.FlagEnabled)
            {
                LRFlags.StatsAbilities.GarbAbilities.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Equip Data...", 70, 100);
            if (LRFlags.StatsAbilities.EquipPassives.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipPassives.SetRand();
                RandomizePassives();
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
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                    new Tuple<int, int>(-75, 100),
                    new Tuple<int, int>(0, 100)
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
                    Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(-2000, 5000),
                        new Tuple<int, int>(-2000, 5000),
                        new Tuple<int, int>(-5000, 20000),
                        new Tuple<int, int>(-25, 50),
                        new Tuple<int, int>(-90, 100)
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
                    Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(-2000, 5000),
                        new Tuple<int, int>(-2000, 5000),
                        new Tuple<int, int>(-5000, 20000),
                        new Tuple<int, int>(-25, 50),
                        new Tuple<int, int>(0, 800)
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
            return autoAbilities.Values.Where(a => passives.Contains(a.name)).ToList();
        }

        public override void Save()
        {
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal < 0).ForEach(w => w.i16AtbSpeedModVal += 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal < 0).ForEach(w => w.i16MagicModVal += 65536);

            itemAbilities.Values.Where(i => i.i8AtbDec < 0).ForEach(i => i.i8AtbDec += 256);

            Randomizers.SetProgressFunc("Saving Equip Data...", 0, 100);
            itemWeapons.SaveDB3(@"\db\resident\item_weapon.wdb");
            Randomizers.SetProgressFunc("Saving Equip Data...", 20, 100);
            items.SaveDB3(@"\db\resident\item.wdb");
            Randomizers.SetProgressFunc("Saving Equip Data...", 40, 100);
            itemAbilities.SaveDB3(@"\db\resident\_wdbpack.bin\r_item_abi.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_item_abi.wdb");
            Randomizers.SetProgressFunc("Saving Equip Data...", 80, 100);
            autoAbilities.DeleteDB3(@"\db\resident\bt_auto_ability.db3");
            passiveAbilities.DeleteDB3(@"\db\resident\_wdbpack.bin\r_pasv_ablty.db3");
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

        public class AbilityData
        {
            public string ID { get; set; }
            public int BasePower { get; set; }
            public int HitMultiplier { get; set; }
            public int ATBCost { get; set; }
            public int MenuIcon { get; set; }
            public AbilityData(string[] row)
            {
                ID = row[0];
                BasePower = int.Parse(row[1]);
                HitMultiplier = int.Parse(row[2]);
                ATBCost = int.Parse(row[3]);
                MenuIcon = int.Parse(row[4]);
            }
        }
    }
}
