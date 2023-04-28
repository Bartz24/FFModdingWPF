using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreArmor : DataStoreEquip
{
    public byte Defense
    {
        get => Data.ReadByte(0x18);
        set => Data.SetByte(0x18, value);
    }
    public byte MagickResist
    {
        get => Data.ReadByte(0x19);
        set => Data.SetByte(0x19, value);
    }
    public byte AugmentOffset
    {
        get => Data.ReadByte(0x1A);
        set => Data.SetByte(0x1A, value);
    }
}
