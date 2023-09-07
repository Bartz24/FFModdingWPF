using Bartz24.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bartz24.FF13;

public class DataStoreLYB : DataStore
{
    private byte[] baseData;
    public Dictionary<int, string> EnemyCharasets { get; set; } = new();
    public override int GetDefaultLength()
    {
        return -1;
    }

    public override byte[] Data
    {
        get
        {
            byte[] data = baseData;
            foreach (KeyValuePair<int, string> pair in EnemyCharasets)
            {
                data.SetString(pair.Key, pair.Value, 16);
            }

            return data;
        }
        set
        {
            baseData = value;

            foreach (int i in Data.FindPattern("00 00 00 01 6D"))
            {
                EnemyCharasets.Add(i + 4, baseData.ReadString(i + 4));
            }
        }
    }

    public override void LoadData(byte[] data, int offset = 0)
    {
        Data = data.SubArray(offset, data.Length - offset);
    }
}
