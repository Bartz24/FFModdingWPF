using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreShop : DataStore
    {
        protected byte[] header;
        public int Offset { get; set; }
        public ushort Count { get; set; }
        public DataStoreList<DataStoreItemEntry> ItemsList { get; set; }

        public override void LoadData(byte[] data, int offset = 0)
        {
            Offset = offset;
            header = data.SubArray(offset, 0xC);
            Count = data.ReadUShort(offset + 0x6);


            int size = 0x2;
            ItemsList = new DataStoreList<DataStoreItemEntry>();
            ItemsList.LoadData(data.SubArray(offset + 0xC, size * Count));
        }

        public override byte[] Data
        {
            get
            {
                return header.Concat(ItemsList.Data);
            }
        }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}
