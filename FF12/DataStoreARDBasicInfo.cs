using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreARDBasicInfo : DataStore
{
    public byte ExtendedInfoIndex
    {
        get => Data.ReadByte(0x0);
        set => Data.SetByte(0x0, value);
    }
    public ushort NameID
    {
        get => Data.ReadUShort(0x08);
        set => Data.SetUShort(0x08, value);
    }
    public byte DefaultStatsIndex
    {
        get => Data.ReadByte(0x22);
        set => Data.SetByte(0x22, value);
    }
    public byte LevelStatsIndex
    {
        get => Data.ReadByte(0x24);
        set => Data.SetByte(0x24, value);
    }
    public ushort SizeX
    {
        get => Data.ReadUShort(0x0A);
        set => Data.SetUShort(0x0A, value);
    }
    public ushort SizeY
    {
        get => Data.ReadUShort(0x0C);
        set => Data.SetUShort(0x0C, value);
    }
    public ushort SizeZ
    {
        get => Data.ReadUShort(0x0E);
        set => Data.SetUShort(0x0E, value);
    }

    public override int GetDefaultLength()
    {
        return 0x58;
    }
}
