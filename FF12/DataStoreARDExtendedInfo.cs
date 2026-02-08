using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreARDExtendedInfo : DataStore
{
    public uint Model
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }
    public byte Genus
    {
        get => Data.ReadByte(0x5);
        set => Data.SetByte(0x5, value);
    }

    public bool HasFlyingInfo
    {
        get => Data.ReadBinary(0x12, 7);
        set => Data.SetBinary(0x12, 7, value);
    }

    public bool IsFlying
    {
        get => Data.ReadBinary(0x18, 4);
        set => Data.SetBinary(0x18, 4, value);
    }

    public DataStoreARD ParentARD { get; set; }

    public override int GetDefaultLength()
    {
        return 0x54;
    }
}
