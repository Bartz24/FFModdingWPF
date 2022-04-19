using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreItemEntry : DataStore
    {
        public ushort Item
        {
            get { return Data.ReadUShort(0x0); }
            set { Data.SetUShort(0x0,value); }
        }
        public override int GetDefaultLength()
        {
            return 0x2;
        }
    }
}
