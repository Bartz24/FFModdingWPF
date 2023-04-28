using Bartz24.Data;

namespace Bartz24.FF13;

public class DataStoreBtConstant : DataStoreWDBEntry
{
    public uint u16UintValue
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }

    public override int GetDefaultLength()
    {
        return 0xC;
    }
}
