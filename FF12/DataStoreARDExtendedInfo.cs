using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreARDExtendedInfo : DataStore
{
    public uint Model
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }
    public override int GetDefaultLength()
    {
        return 0x54;
    }
}
