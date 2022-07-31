using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13
{
    public class DataStoreBtScene : DataStoreWDBEntry
    {
        public uint sDrop100Id_pointer
        {
            get { return Data.ReadUInt(0x34); }
            set { Data.SetUInt(0x34, value); }
        }
        public string sDrop100Id_string { get; set; }
        public byte u8NumDrop100
        {
            get { return Data.ReadByte(0x50); }
            set { Data.SetByte(0x50, value); }
        }

        public override int GetDefaultLength()
        {
            return 0x60;
        }
    }
}
