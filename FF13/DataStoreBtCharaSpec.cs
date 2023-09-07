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


    public uint u24MaxHp
    {
        get { return Data.ReadUInt(0x10); }
        set { Data.SetUInt(0x10, value); }
    }

    public uint u12MaxBp
    {
        get { return Data.ReadUInt(0x24); }
        set { Data.SetUInt(0x24, value); }
    }

    public uint u24AbilityPoint
    {
        get { return Data.ReadUInt(0xEC); }
        set { Data.SetUInt(0xEC, value); }
    }
    public byte Level
    {
        get { return Data.ReadByte(0xFE); }
        set { Data.SetByte(0xFE, value); }
    }

    public ushort u16StatusMgk
    {
        get { return Data.ReadUShort(0x10C); }
        set { Data.SetUShort(0x10C, value); }
    }

    public ushort u16StatusStr
    {
        get { return Data.ReadUShort(0x10E); }
        set { Data.SetUShort(0x10E, value); }
    }

    public ushort u12BrChainBonus
    {
        get { return Data.ReadUShort(0x126); }
        set { Data.SetUShort(0x126, value); }
    }
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
