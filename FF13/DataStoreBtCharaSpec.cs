using Bartz24.Data;

namespace Bartz24.FF13;

public class DataStoreBtCharaSpec : DataStoreWDBEntry
{
    public uint sCharaSpec_pointer
    {
        get => Data.ReadUInt(0x0);
        set => Data.SetUInt(0x0, value);
    }
    public string sCharaSpec_string { get; set; }
    public uint sDropItem0_pointer
    {
        get => Data.ReadUInt(0xF0);
        set => Data.SetUInt(0xF0, value);
    }
    public string sDropItem0_string { get; set; }
    public uint sDropItem1_pointer
    {
        get => Data.ReadUInt(0xF4);
        set => Data.SetUInt(0xF4, value);
    }
    public string sDropItem1_string { get; set; }

    public byte u8NumDrop
    {
        get => Data.ReadByte(0x14C);
        set => Data.SetByte(0x14C, value);
    }
    public ushort u16DropChance0
    {
        get => Data.ReadUShort(0x164);
        set => Data.SetUShort(0x164, value);
    }
    public ushort u16DropChance1
    {
        get => Data.ReadUShort(0x160);
        set => Data.SetUShort(0x160, value);
    }

    public override int GetDefaultLength()
    {
        return 0x168;
    }
}
