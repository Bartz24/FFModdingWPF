using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;
using static FF13_2Rando.CrystariumRando;

namespace FF13_2Rando
{
    public class EquipRando : Randomizer
    {
        public DataStoreDB3<DataStoreItemWeapon> itemWeapons = new DataStoreDB3<DataStoreItemWeapon>();
        public DataStoreDB3<DataStoreItem> items = new DataStoreDB3<DataStoreItem>();

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
            itemWeapons.LoadDB3("13-2", @"\db\resident\item_weapon.wdb");
            FileHelpers.CopyFile(SetupData.OutputFolder + @"\db\resident\item_weapon.wdb", SetupData.OutputFolder + @"\db\resident\item_weapon.wdb.orig");
            items.LoadDB3("13-2", @"\db\resident\item.wdb");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (FF13_2Flags.Stats.EquipStats.FlagEnabled)
            {
                FF13_2Flags.Stats.EquipStats.SetRand();
                RandomizeStats();
                RandomNum.ClearRand();
            }

            if (FF13_2Flags.Stats.EquipPassives.FlagEnabled)
            {
                FF13_2Flags.Stats.EquipPassives.SetRand();
                RandomizePassives();
                RandomNum.ClearRand();
            }

            if (FF13_2Flags.Stats.EquipWeights.FlagEnabled)
            {
                FF13_2Flags.Stats.EquipWeights.SetRand();
                RandomizeWeights();
                RandomNum.ClearRand();
            }
        }


        private void RandomizeStats()
        {
            foreach (DataStoreItemWeapon weapon in itemWeapons.Values.Where(w => w.name.Contains("wea")))
            {
                StatPoints statPoints;
                    Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(1, 300),
                        new Tuple<int, int>(1, 300)
                    };
                    float[] weights = new float[] { 1, 1 };
                    int[] zeros = new int[] { 0, 0 };
                    int[] negs = new int[] { 0, 0 };
                    statPoints = new StatPoints(bounds, weights, zeros, negs);
                statPoints.Randomize(new int[] { weapon.i16AttackModVal, weapon.i16MagicModVal });

                weapon.i16AttackModVal = statPoints[0];
                weapon.i16MagicModVal = statPoints[1];

#if DEBUG
                /*weapon.i16AttackModVal = 10000;
                weapon.i16MagicModVal = 10000;
                weapon.i16HpModVal = 30000;
                weapon.i16AtbSpeedModVal = 100;*/
#endif

            }
        }

        private void RandomizePassives()
        {
            CrystariumRando crystariumRando = Randomizers.Get<CrystariumRando>("Crystarium");
            List<AbilityData> filteredAbilities = crystariumRando.abilityData.Values.Where(a => a.Role == "" && !a.Traits.Contains("Mon")).ToList();
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility_string != ""))
            {
                IList<AbilityData> list = filteredAbilities.Where(a => (!a.Traits.Contains("Noel") || equip.name.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.name.Contains("ser"))).ToList().Shuffle();
                equip.sAbility_string = list.First().ID;
            }
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility2_string != ""))
            {
                IList<AbilityData> list = filteredAbilities.Where(a => a.ID != equip.sAbility_string && (!a.Traits.Contains("Noel") || equip.name.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.name.Contains("ser"))).ToList().Shuffle();
                equip.sAbility2_string = list.First().ID;
            }
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility3_string != ""))
            {
                IList<AbilityData> list = filteredAbilities.Where(a => a.ID != equip.sAbility_string && a.ID != equip.sAbility2_string && (!a.Traits.Contains("Noel") || equip.name.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.name.Contains("ser"))).ToList().Shuffle();
                equip.sAbility3_string = list.First().ID;
            }
        }

        private void RandomizeWeights()
        {
            foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(e => e.u7Cost > 0))
            {
                int range = FF13_2Flags.Stats.WeightRange.Value;
                equip.u7Cost = RandomNum.RandInt(Math.Max(1, equip.u7Cost - range), Math.Min(100, equip.u7Cost + range));
            }
        }

        public override void Save()
        {
            items.SaveDB3(@"\db\resident\item.wdb");
            itemWeapons.SaveDB3(@"\db\resident\item_weapon.wdb");

            TempSaveFix();
        }
        private void TempSaveFix()
        {
            byte[] origData = File.ReadAllBytes(SetupData.OutputFolder + @"\db\resident\item_weapon.wdb.orig");
            byte[] data = File.ReadAllBytes(SetupData.OutputFolder + @"\db\resident\item_weapon.wdb");

            if (data.Length < origData.Length)
            {

                int startQstData = (int)data.ReadUInt(0xE0);
                List<DataStoreItemWeapon> values = itemWeapons.Values.ToList();
                for (int i = 0; i < values.Count; i++)
                {
                    byte[] fix2 = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    fix2.SetByte(3, (byte)values[i].u14DisasRate3);
                    data = data.SubArray(0, startQstData + 72 * i + 64).Concat(fix2).Concat(data.SubArray(startQstData + 72 * i + 68, data.Length - (startQstData + 72 * i + 68)));
                    data.SetUInt(startQstData + 72 * i, (uint)values[i].sWeaponCharaSpecId_pointer);

                    data.SetUInt(0xE0 + 32 * i, (uint)(startQstData + 72 * i));
                }

                File.WriteAllBytes(SetupData.OutputFolder + @"\db\resident\item_weapon.wdb", data);
            }
            File.Delete(SetupData.OutputFolder + @"\db\resident\item_weapon.wdb.orig");
        }

        private string GetItemName(string itemID)
        {
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            string name = textRando.mainSysUS[items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
                name = name.Substring(0, name.IndexOf("{End}"));

            return name;
        }
    }
}
