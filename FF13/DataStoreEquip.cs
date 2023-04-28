using Bartz24.Data;

namespace Bartz24.FF13;

public class DataStoreEquip : DataStoreWDBEntry
{
    public string sPassive_string { get; set; }

    public uint sPassive_pointer
    {
        get => Data.ReadUInt(0x8);
        set => Data.SetUInt(0x8, value);
    }
    public string sPassiveDisplayName_string { get; set; }

    public uint sPassiveDisplayName_pointer
    {
        get => Data.ReadUInt(0xC);
        set => Data.SetUInt(0xC, value);
    }
    public string sUpgradeInto_string { get; set; }

    public uint sUpgradeInto_pointer
    {
        get => Data.ReadUInt(0x14);
        set => Data.SetUInt(0x14, value);
    }
    public string sHelpDisplay_string { get; set; }

    public uint sHelpDisplay_pointer
    {
        get => Data.ReadUInt(0x18);
        set => Data.SetUInt(0x18, value);
    }

    public uint u16BuyPriceIncrease
    {
        get => Data.ReadUInt(0x1C);
        set => Data.SetUInt(0x1C, value);
    }

    public uint u16SellPriceIncrease
    {
        get => Data.ReadUInt(0x20);
        set => Data.SetUInt(0x20, value);
    }

    public byte u1MaxLevel
    {
        get => (byte)((((Data.ReadByte(0x38) % 0x10) - 2) * 64) + (Data.ReadByte(0x39) / 4));
        set
        {
            Data.SetByte(0x38, (byte)((Data.ReadByte(0x38) / 0x10 * 0x10) + (value / 64) + 2));
            Data.SetByte(0x39, (byte)(value % 64 * 4));
        }
    }
    public byte u1StatType1
    {
        get => Data.ReadByte(0x41, 4, 4);
        set => Data.SetByte(0x41, value, 4, 4);
    }
    public ushort u8StatType2
    {
        get => Data.ReadUShort(0x42);
        set => Data.SetUShort(0x42, value);
    }
    public ushort u8StatIncrease
    {
        get => Data.ReadUShort(0x44);
        set => Data.SetUShort(0x44, value);
    }
    public short i8StatInitial
    {
        get => Data.ReadShort(0x46);
        set => Data.SetShort(0x46, value);
    }
    public ushort u8StrengthIncrease
    {
        get => Data.ReadUShort(0x48);
        set => Data.SetUShort(0x48, value);
    }
    public short i8StrengthInitial
    {
        get => Data.ReadShort(0x4A);
        set => Data.SetShort(0x4A, value);
    }
    public ushort u8MagicIncrease
    {
        get => Data.ReadUShort(0x4C);
        set => Data.SetUShort(0x4C, value);
    }
    public short i8MagicInitial
    {
        get => Data.ReadShort(0x4E);
        set => Data.SetShort(0x4E, value);
    }

    public override int GetDefaultLength()
    {
        return 0x68;
    }
}
