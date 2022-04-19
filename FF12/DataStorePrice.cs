using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStorePrice : DataStore
    {
        public uint Price
        {
            get { return Data.ReadUInt(0x0); }
            set { Data.SetUInt(0x0, value); }
        }
        public override int GetDefaultLength()
        {
            return 0x4;
        }
    }
}
