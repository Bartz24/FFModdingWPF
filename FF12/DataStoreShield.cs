using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreShield : DataStoreEquip
    {
        public byte Evade
        {
            get { return Data.ReadByte(0x18); }
            set { Data.SetByte(0x18, value); }
        }
        public byte MagickEvade
        {
            get { return Data.ReadByte(0x19); }
            set { Data.SetByte(0x19, value); }
        }
    }
}
