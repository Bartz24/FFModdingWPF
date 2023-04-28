using System.Linq;

namespace Bartz24.FF13_2_LR;

public class DataStoreDB3String : DataStoreDB3SubEntry
{
    public int pointer { get; set; }

    public int GetStringLength()
    {
        return name.Select(c => c >= 0x200 ? 3 : 1).Sum();
    }
}
