using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13
{
    public class DataStoreBtCharaSpec : DataStoreWDBEntry
    {
        public uint sDropItem0_pointer
        {
            get { return Data.ReadUInt(0xF0); }
            set { Data.SetUInt(0xF0, value); }
        }
        public string sDropItem0_string { get; set; }
        public uint sDropItem1_pointer
        {
            get { return Data.ReadUInt(0xF4); }
            set { Data.SetUInt(0xF4, value); }
        }
        public string sDropItem1_string { get; set; }

        public override int GetDefaultLength()
        {
            return 0x168;
        }
    }
}
