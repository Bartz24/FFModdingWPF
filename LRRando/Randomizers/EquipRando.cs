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
        public DataStoreDB3<DataStoreBtAutoAbility> autoAbilities = new DataStoreDB3<DataStoreBtAutoAbility>();
        public DataStoreDB3<DataStoreRItemAbi> itemAbilities = new DataStoreDB3<DataStoreRItemAbi>();

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
            autoAbilities.LoadDB3("LR", @"\db\resident\bt_auto_ability.wdb");
            itemAbilities.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_item_abi.wdb", false);
            passives = File.ReadAllLines(@"data\passives.csv").ToList();
        }
        public override void Randomize(Action<int> progressSetter)
        {
            itemWeapons.Values.Where(w => w.i16AtbSpeedModVal >= 32768).ForEach(w => w.i16AtbSpeedModVal -= 65536);
            if (LRFlags.Other.EquipStats.FlagEnabled)
            {
                LRFlags.Other.EquipStats.SetRand();
                RandomizeStats();
                RandomNum.ClearRand();
            }

            if (LRFlags.Other.GarbAbilities.FlagEnabled)
            {
                LRFlags.Other.GarbAbilities.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            if (LRFlags.Other.EquipPassives.FlagEnabled)
            {
                LRFlags.Other.EquipPassives.SetRand();
                RandomizePassives();
                RandomNum.ClearRand();
            }

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
            if (name != "")
            {
                IEnumerable<DataStoreItem> enumerable = GetAbilities(name, forceType);
                DataStoreItem random = enumerable.ElementAt(RandomNum.RandInt(0, enumerable.Count() - 1));
                if (name.StartsWith("abi_"))
                {
                    itemAbilities[name].sAbilityId_string = random.sScriptId_string;
                    items[name].sItemNameStringId_string = "";
                    items[name].sHelpStringId_string = "";
                    items[name].sScriptId_string = "";
                    items[name].u8MenuIcon = random.u8MenuIcon;
                }
                else
                {
                    return random.sScriptId_string;
                }
                if (itemAbilities[name].u4Lv < 1)
                {
                    itemAbilities[name].u4Lv = 1;
                    itemAbilities[name].iPower = 15;
                }
            }
            return name;
        }

        public IEnumerable<DataStoreItem> GetAbilities(string name, int forceType)
        {
            return items.Values.Where(i => IsAbility(i) && (forceType == -1 || i.u8MenuIcon == forceType));
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

                if(items.Keys.Contains(garb.name) && items[garb.name].uPurchasePrice == 0)
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
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                    new Tuple<int, int>(-2000, 5000),
                    new Tuple<int, int>(-2000, 5000),
                    new Tuple<int, int>(-5000, 20000),
                    new Tuple<int, int>(-25, 50),
                    new Tuple<int, int>(0, 800)
                };
                float[] weights = new float[] { 6, 6, 2, 4, 4 };
                int[] zeros = new int[] { 90, 90, 40, 30, 20 };
                StatPoints statPoints = new StatPoints(bounds, weights, zeros);
                statPoints.Randomize(new int[] { shield.i16AttackModVal, shield.i16MagicModVal, shield.i16HpModVal, shield.i16AtbSpeedModVal, shield.iGuardModVal });

                shield.i16AttackModVal = statPoints[0];
                shield.i16MagicModVal = statPoints[1];
                shield.i16HpModVal = statPoints[2];
                shield.i16AtbSpeedModVal = statPoints[3];
                shield.iGuardModVal = statPoints[4];
                /*
                shield.iGuardModVal = 300;
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
            autoAbilities.DeleteDB3(@"\db\resident\bt_auto_ability.db3");
        }
    }
}
