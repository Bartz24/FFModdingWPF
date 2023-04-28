using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreItemEntry : DataStore
{
    public ushort Item
    {
        get => Data.ReadUShort(0x0);
        set => Data.SetUShort(0x0, value);
    }
    public override int GetDefaultLength()
    {
        return 0x2;
    }
}
