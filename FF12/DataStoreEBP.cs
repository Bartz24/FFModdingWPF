using Bartz24.Data;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF12
{
    public class DataStoreEBP : DataStore
    {
        private int treasureOffset, treasureCount;
        protected byte[] header, footer;
        public DataStoreList<DataStoreTreasure> TreasureList { get; set; }

        public List<int> trapNonRespawnable = new List<int>();

        public DataStoreEBP(int treasureOffset, int treasureCount)
        {
            this.treasureOffset = treasureOffset;
            this.treasureCount = treasureCount;
        }

        public override void LoadData(byte[] data, int offset = 0)
        {
            int section0 = (int)data.ReadUInt(0x10);
            int dynamicOffset = (int)data.ReadUInt(section0 + 0x40);
            int treasureDataOffset = section0 + dynamicOffset + treasureOffset;
            int trapOffsetPointer = (int)data.ReadUInt(section0 + 0x6C);
            int trapOffset = (int)data.ReadUInt(section0 + trapOffsetPointer + 4) & 0x00FFFFFF;
            int mspOffset = (int)data.ReadUInt(section0 + 0x40) + trapOffset + section0;
            int trapCount = (int)data.ReadUInt(mspOffset);
            header = data.SubArray(0, treasureDataOffset);

            int size = 0x18;
            TreasureList = new DataStoreList<DataStoreTreasure>();
            TreasureList.LoadData(data.SubArray(treasureDataOffset, size * treasureCount));

            for (int i = 0; i < trapCount; i++)
            {
                int trapBase = (int)data.ReadUInt(mspOffset + i * 4 + 4);
                trapNonRespawnable.Add(data.ReadByte(mspOffset + trapBase + 4));
            }

            footer = data.SubArray(treasureDataOffset + size * treasureCount, data.Length - (treasureDataOffset + size * treasureCount));
        }

        public override byte[] Data
        {
            get
            {
                return header.Concat(TreasureList.Data).Concat(footer);
            }
        }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}
