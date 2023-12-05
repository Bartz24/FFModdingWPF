using Bartz24.Data;

namespace Bartz24.FF12;

public class DataStoreBPShop : DataStoreBPSection<DataStoreShop>
{

    public override void LoadData(byte[] data, int offset = 0)
    {
        data.ReadUShort(0xC);
        header = data.SubArray(0, data.Length); // Treat as all data.
        data.ReadUShort(0x4);
        data.ReadUShort(0x8);

        DataList = new DataStoreList<DataStoreShop>();
        while (offset < data.Length)
        {
            if (data.ReadByte(offset) == 0x64 && data.ReadByte(offset + 1) == 0x02 && data.ReadByte(offset + 2) == 0x00 && data.ReadByte(offset + 3) == 0x00)
            {
                DataStoreShop shop = new();
                shop.LoadData(data, offset);
                shop.ID = DataList.Count;
                DataList.Add(shop, DataList.Count);
            }

            offset += 4;
        }
    }

    public override byte[] Data
    {
        get
        {
            byte[] outData = header.SubArray(0, header.Length);
            DataList.ForEach(s => outData.SetSubArray(s.Offset, s.Data));
            return outData;
        }
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
