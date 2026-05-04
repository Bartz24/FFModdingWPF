using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreArmor : DataStoreEquip
{
    public enum ArmorType
    {
        LightArmor = 0,
        MysticArmor = 1,
        HeavyArmor = 2,
        Unknown = 3
    }
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

    public ArmorType ArmorCategory
    {
        get =>
            Icon is 19 or 22 ? ArmorType.LightArmor :
            Icon is 20 or 23 ? ArmorType.MysticArmor :
            Icon is 21 or 24 ? ArmorType.HeavyArmor :
            ArmorType.Unknown;
    }
}
