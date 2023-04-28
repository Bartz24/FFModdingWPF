using Bartz24.Data;
using System.Collections.Generic;

namespace Bartz24.FF12;

public class DataStoreARD : DataStore
{
    protected byte[] allData;
    public DataStoreList<DataStoreARDExtendedInfo> ExtendedInfo { get; set; }
    public DataStoreList<DataStoreARDBasicInfo> BasicInfo { get; set; }
    public DataStoreList<DataStoreARDStats> DefaultStats { get; set; }
    public DataStoreList<DataStoreARDStats> LevelStats { get; set; }

    public List<int> trapNonRespawnable = new();

    public override void LoadData(byte[] data, int offset = 0)
    {
        allData = data.SubArray(0, data.Length);

        int address = (int)data.ReadUInt(0x10);
        int count = (int)data.ReadUInt(address + 4);
        int size = 0x54;
        ExtendedInfo = new DataStoreList<DataStoreARDExtendedInfo>();
        ExtendedInfo.LoadData(data.SubArray(address + 0x20, size * count));

        address = (int)data.ReadUInt(0x18);
        count = (int)data.ReadUInt(address + 4);
        size = 0x58;
        BasicInfo = new DataStoreList<DataStoreARDBasicInfo>();
        BasicInfo.LoadData(data.SubArray(address + 0x20, size * count));

        address = (int)data.ReadUInt(0x24);
        count = (int)data.ReadUInt(address + 4);
        size = 0x38;
        DefaultStats = new DataStoreList<DataStoreARDStats>();
        DefaultStats.LoadData(data.SubArray(address + 0x20, size * count));

        address = (int)data.ReadUInt(0x28);
        count = (int)data.ReadUInt(address + 4);
        size = 0x38;
        LevelStats = new DataStoreList<DataStoreARDStats>();
        LevelStats.LoadData(data.SubArray(address + 0x20, size * count));
    }

    public override byte[] Data
    {
        get
        {
            byte[] outData = allData.SubArray(0, allData.Length);

            int address = (int)outData.ReadUInt(0x10);
            outData.SetSubArray(address + 0x20, ExtendedInfo.Data);
            address = (int)outData.ReadUInt(0x18);
            outData.SetSubArray(address + 0x20, BasicInfo.Data);
            address = (int)outData.ReadUInt(0x24);
            outData.SetSubArray(address + 0x20, DefaultStats.Data);
            address = (int)outData.ReadUInt(0x28);
            outData.SetSubArray(address + 0x20, LevelStats.Data);

            return outData;
        }
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
