using Bartz24.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using System.Collections.Generic;

namespace Bartz24.FF13_2_LR
{
    public class DataStoreDB3SubEntry : DataStoreDB3Entry
    {
        public string name { get; set; }

        public virtual Dictionary<string, int> GetStringArrayMapping() { return new Dictionary<string, int>(); }
    }
}
