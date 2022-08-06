using Bartz24.Data;
using System.Linq;

namespace Bartz24.FF12
{
    public class DataStoreBPEquip : DataStore
    {
        protected byte[] header;
        public DataStoreList<DataStoreEquip> EquipDataList { get; set; }
        public DataStoreList<DataStoreAttribute> AttributeDataList { get; set; }

        public override void LoadData(byte[] data, int offset = 0)
        {
            {
                int start = data.ReadUShort(0xC);
                header = data.SubArray(0, start);
                int count = data.ReadUShort(0x4);
                int size = data.ReadUShort(0x8);

                EquipDataList = new DataStoreList<DataStoreEquip>();
                EquipDataList.LoadData(data.SubArray(start, size * count));

                for (int i = 0; i < EquipDataList.Count; i++)
                {
                    if (EquipDataList[i].Type == EquipType.Armor || EquipDataList[i].Type == EquipType.Accessory)
                    {
                        DataStoreArmor d = new DataStoreArmor();
                        d.LoadData(EquipDataList[i].Data);
                        EquipDataList[i] = d;
                    }
                    else if (EquipDataList[i].Category == EquipCategory.Shield)
                    {
                        DataStoreShield d = new DataStoreShield();
                        d.LoadData(EquipDataList[i].Data);
                        EquipDataList[i] = d;
                    }
                    else if (EquipDataList[i].Category == EquipCategory.Arrow || EquipDataList[i].Category == EquipCategory.Shot || EquipDataList[i].Category == EquipCategory.Bolt || EquipDataList[i].Category == EquipCategory.Bomb)
                    {
                        DataStoreShield d = new DataStoreShield();
                        d.LoadData(EquipDataList[i].Data);
                        EquipDataList[i] = d;
                    }
                    else
                    {
                        DataStoreWeapon d = new DataStoreWeapon();
                        d.LoadData(EquipDataList[i].Data);
                        EquipDataList[i] = d;
                    }
                }
            }
            {
                int start = data.ReadUShort(0x18);
                int size = 24;
                int count = (data.Length - start) / size;

                AttributeDataList = new DataStoreList<DataStoreAttribute>();
                AttributeDataList.LoadData(data.SubArray(start, size * count));
            }
        }

        public override byte[] Data
        {
            get
            {
                return header.Concat(EquipDataList.Data).Concat(AttributeDataList.Data);
            }
        }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}
