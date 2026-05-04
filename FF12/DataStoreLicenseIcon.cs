using Bartz24.Data;
using System.Linq;

namespace Bartz24.FF12;

public class DataStoreLicenseIcon : DataStore
{
    public enum Type
    {
        Available = 0,
        Obtained = 1
    }
    public DataStoreList<DataStoreLicenseIconLayer> this[Type type] => type == Type.Available ? AvailableLayers : ObtainedLayers;
    public DataStoreList<DataStoreLicenseIconLayer> AvailableLayers { get; set; }
    public DataStoreList<DataStoreLicenseIconLayer> ObtainedLayers { get; set; }

    public override void LoadData(byte[] data, int offset = 0)
    {
        AvailableLayers = new();
        AvailableLayers.LoadData(data.SubArray(offset, 0x20));

        ObtainedLayers = new();
        ObtainedLayers.LoadData(data.SubArray(offset + 0x20, 0x20));
    }

    public override byte[] Data => AvailableLayers.Data.Concat(ObtainedLayers.Data).ToArray();
    public override int GetDefaultLength()
    {
        return 0x40;
    }
}
