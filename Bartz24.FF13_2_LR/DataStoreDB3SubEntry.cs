using System.Collections.Generic;

namespace Bartz24.FF13_2_LR;

public class DataStoreDB3SubEntry : DataStoreDB3Entry
{
    public string name { get; set; }

    public virtual Dictionary<string, int> GetStringArrayMapping() { return new Dictionary<string, int>(); }
}
