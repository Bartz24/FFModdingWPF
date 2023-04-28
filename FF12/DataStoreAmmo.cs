using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreAmmo : DataStoreEquip
{
    public byte AttackPower
    {
        get => Data.ReadByte(0x1A);
        set => Data.SetByte(0x1A, value);
    }
    public byte Evade
    {
        get => Data.ReadByte(0x1D);
        set => Data.SetByte(0x1D, value);
    }
    public byte StatusChance
    {
        get => Data.ReadByte(0x1F);
        set => Data.SetByte(0x1F, value);
    }
    public Element Elements
    {
        get => (Element)Data.ReadByte(0x1E);
        set => Data.SetByte(0x1E, (byte)value);
    }
    public Status StatusEffects
    {
        get => (Status)Data.ReadUInt(0x20);
        set => Data.SetUInt(0x20, (uint)value);
    }
}
