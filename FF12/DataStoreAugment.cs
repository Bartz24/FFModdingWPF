using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreAugment : DataStore
{
    public string ID { get => IntID.ToString("X4"); }
    public int IntID { get; set; }
    public ushort EquipDescAddress
    {
        get => Data.ReadUShort(0x00);
        set => Data.SetUShort(0x00, value);
    }
    public ushort LicenseDescAddress
    {
        get => Data.ReadUShort(0x02);
        set => Data.SetUShort(0x02, value);
    }
    public ushort Value
    {
        get => Data.ReadUShort(0x04);
        set => Data.SetUShort(0x04, value);
    }
    public override int GetDefaultLength()
    {
        return 0x08;
    }
}
