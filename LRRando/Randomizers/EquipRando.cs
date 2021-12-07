using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class EquipRando : Randomizer
    {
        public DataStoreDB3<DataStoreItemWeapon> itemWeapons = new DataStoreDB3<DataStoreItemWeapon>();
        public DataStoreDB3<DataStoreItem> items = new DataStoreDB3<DataStoreItem>();
        public DataStoreDB3<DataStoreItem> itemsOrig = new DataStoreDB3<DataStoreItem>();
        public DataStoreDB3<DataStoreBtAutoAbility> autoAbilities = new DataStoreDB3<DataStoreBtAutoAbility>();
        public DataStoreDB3<DataStoreRItemAbi> itemAbilities = new DataStoreDB3<DataStoreRItemAbi>();
        public DataStoreDB3<DataStoreRItemAbi> itemAbilitiesOrig = new DataStoreDB3<DataStoreRItemAbi>();
        Dictionary<string, AbilityData> abilityData = new Dictionary<string, AbilityData>();

        List<string> passives = new List<string>();

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
            itemWeapons.LoadDB3("LR", @"\db\resident\item_weapon.wdb");
            items.LoadDB3("LR", @"\db\resident\item.wdb");
            itemsOrig.LoadDB3("LR", @"\db\resident\item.wdb");
            autoAbilities.LoadDB3("LR", @"\db\resident\bt_auto_ability.wdb");
            itemAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            itemAbilitiesOrig.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            passives = File.ReadAllLines(@"data\passives.csv").ToList();
            abilityData = File.ReadAllLines(@"data\abilities.csv").Select(s => new AbilityData(s.Split(","))).ToDictionary(a => a.ID, e => e);

            items.InsertCopyAlphabetical("key_b_20", "key_r_kanki");
            items["key_r_kanki"].sItemNameStringId_string = "$m_001";
            items["key_r_kanki"].sHelpStringId_string = "$m_001_ac000";
            items["key_r_kanki"].u16SortAllByKCategory = 100;
            items["key_r_kanki"].u16SortCategoryByCategory = 150;
        }
        public override void Randomize(Action<int> progressSetter)
        {
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal >= 32768).ForEach(w => w.i16MagicModVal -= 65536);
            if (LRFlags.StatsAbilities.EquipStats.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipStats.SetRand();
                RandomizeStats();
                RandomNum.ClearRand();
            }

            if (LRFlags.StatsAbilities.GarbAbilities.FlagEnabled)
            {
                LRFlags.StatsAbilities.GarbAbilities.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            if (LRFlags.StatsAbilities.EquipPassives.FlagEnabled)
            {
                LRFlags.StatsAbilities.EquipPassives.SetRand();
                RandomizePassives();
                RandomNum.ClearRand();
            }

            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal < 0).ForEach(w => w.i16AtbSpeedModVal += 65536);
            itemWeapons.Values.Where(w => w.i16MagicModVal < 0).ForEach(w => w.i16MagicModVal += 65536);
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
            AbilityRando abilityRando = randomizers.Get<AbilityRando>("Abilities");
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
                    itemAbilities[name].i8AtbDec = -(int)((newATB * origATBMult) - abilityData[newAbility].ATBCost);

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
                StatPoints statPoints = new StatPoints(bounds, weights, zeros);
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
                    statPoints = new StatPoints(bounds, weights, zeros);
                    statPoints.Randomize(new int[] { weapon.i16AttackModVal, weapon.i16MagicModVal, weapon.i16HpModVal, weapon.i16AtbSpeedModVal, weapon.iBreakBonus });
                }
                while (starting && statPoints[0] < 50 && statPoints[1] < 50);

                weapon.i16AttackModVal = statPoints[0];
                weapon.i16MagicModVal = statPoints[1];
                weapon.i16HpModVal = statPoints[2];
                weapon.i16AtbSpeedModVal = statPoints[3];
                weapon.iBreakBonus = statPoints[4];

                /*
                weapon.i16AttackModVal = 10000;
                weapon.i16MagicModVal = 10000;
                weapon.i16HpModVal = 30000;
                weapon.i16AtbSpeedModVal = 100;
                */
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
                    statPoints = new StatPoints(bounds, weights, zeros);
                    statPoints.Randomize(new int[] { shield.i16AttackModVal, shield.i16MagicModVal, shield.i16HpModVal, shield.i16AtbSpeedModVal, shield.iGuardModVal });
                }
                while (starting && (statPoints[0] < 0 || statPoints[1] < 0));

                shield.i16AttackModVal = statPoints[0];
                shield.i16MagicModVal = statPoints[1];
                shield.i16HpModVal = statPoints[2];
                shield.i16AtbSpeedModVal = statPoints[3];
                shield.iGuardModVal = statPoints[4];

                /*
                shield.iGuardModVal = 3000;
                */
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

        private List<DataStoreBtAutoAbility> GetFilteredAbilities()
        {
            return autoAbilities.Values.Where(a => passives.Contains(a.name)).ToList();
        }

        public override void Save()
        {
            itemWeapons.SaveDB3(@"\db\resident\item_weapon.wdb");
            items.SaveDB3(@"\db\resident\item.wdb");
            itemAbilities.SaveDB3(@"\db\resident\_wdbpack.bin\r_item_abi.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_item_abi.wdb");
            autoAbilities.DeleteDB3(@"\db\resident\bt_auto_ability.db3");
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
