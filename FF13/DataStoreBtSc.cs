using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13
{
    public class DataStoreBtSc : DataStoreWDBEntry
    {
        public uint sEntryBtChSpec_pointer
        {
            get { return Data.ReadUInt(0x0); }
            set { Data.SetUInt(0x0, value); }
        }
        public string sEntryBtChSpec_string { get; set; }

        public override int GetDefaultLength()
        {
            return 0x54;
        }
    }
}
