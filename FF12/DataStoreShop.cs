using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF12;

public class DataStoreShop : DataStore
{
    protected byte[] header;
    public int Offset { get; set; }
    public ushort Count { get; set; }
    public DataStoreList<DataStoreItemEntry> ItemsList { get; set; }

    public override void LoadData(byte[] data, int offset = 0)
    {
        Offset = offset;
        header = data.SubArray(offset, 0xC);
        Count = data.ReadUShort(offset + 0x6);

        int size = 0x2;
        ItemsList = new DataStoreList<DataStoreItemEntry>();
        ItemsList.LoadData(data.SubArray(offset + 0xC, size * Count));
    }

    public override byte[] Data => header.Concat(ItemsList.Data);

    public List<string> GetItems()
    {
        return ItemsList.Select(i => i.Item.ToString("X4")).Where(i => i != "FFFF").ToList();
    }

    public void SetItems(List<string> items)
    {
        for (int i = 0; i < Count; i++)
        {
            if (i >= items.Count)
            {
                ItemsList[i].Item = 0xFFFF;
            }
            else
            {
                ushort id = Convert.ToUInt16(items[i], 16);
                ItemsList[i].Item = id;
            }
        }
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
