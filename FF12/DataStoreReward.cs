using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreReward : DataStore
{
    public string ID { get => IntID.ToString("X4"); }
    public int IntID { get; set; }

    public uint Gil
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }
    public ushort Item1ID
    {
        get => Data.ReadUShort(0x4);
        set => Data.SetUShort(0x4, value);
    }
    public ushort Item1Amount
    {
        get => Data.ReadUShort(0x6);
        set => Data.SetUShort(0x6, value);
    }
    public ushort Item2ID
    {
        get => Data.ReadUShort(0x8);
        set => Data.SetUShort(0x8, value);
    }
    public ushort Item2Amount
    {
        get => Data.ReadUShort(0xA);
        set => Data.SetUShort(0xA, value);
    }
    public override int GetDefaultLength()
    {
        return 0xC;
    }
}
