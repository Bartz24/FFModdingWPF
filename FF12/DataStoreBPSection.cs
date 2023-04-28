using Bartz24.Data;
using System.Linq;

namespace Bartz24.FF12;

public class DataStoreBPSection<T> : DataStore where T : DataStore, new()
{
    protected byte[] header;
    public T this[int index] => DataList[index];
    public DataStoreList<T> DataList { get; set; }

    public override void LoadData(byte[] data, int offset = 0)
    {
        int start = data.ReadUShort(0xC);
        header = data.SubArray(0, start);
        int count = data.ReadUShort(0x4);
        int size = data.ReadUShort(0x8);

        DataList = new DataStoreList<T>();
        DataList.LoadData(data.SubArray(start, size * count));
    }

    public override byte[] Data => header.Concat(DataList.Data);

    public override int GetDefaultLength()
    {
        return -1;
    }
}
