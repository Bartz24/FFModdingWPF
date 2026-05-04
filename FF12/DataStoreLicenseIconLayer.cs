using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreLicenseIconLayer : DataStore
{
    public byte TextureGroup
    {
        get => Data.ReadByte(0x00);
        set => Data.SetByte(0x00, value);
    }
    public byte TextureSection
    {
        get => Data.ReadByte(0x01);
        set => Data.SetByte(0x01, value);
    }
    public ushort Animation
    {
        get => Data.ReadUShort(0x02);
        set => Data.SetUShort(0x02, value);
    }
    public byte ClutLink
    {
        get => Data.ReadByte(0x04);
        set => Data.SetByte(0x04, value);
    }
    public byte TextureLink
    {
        get => Data.ReadByte(0x05);
        set => Data.SetByte(0x05, value);
    }
    public sbyte XPos
    {
        get => Data.ReadSByte(0x06);
        set => Data.SetSByte(0x06, value);
    }
    public sbyte YPos
    {
        get => Data.ReadSByte(0x07);
        set => Data.SetSByte(0x07, value);
    }
    public override int GetDefaultLength()
    {
        return 0x08;
    }
}
