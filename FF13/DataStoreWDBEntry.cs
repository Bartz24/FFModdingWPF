using Bartz24.Data;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Bartz24.FF13
{
    public class DataStoreWDBEntry : DataStore
    {
        public string ID { get; set; }

        public override int GetDefaultLength()
        {
            return -1;
        }
    }
}
