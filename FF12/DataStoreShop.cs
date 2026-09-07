using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF12;

// One shop's item list. See DataStoreBPShop for the node layout.
public class DataStoreShop : DataStore
{
    public int ID { get; set; }
    public ushort Count => (ushort)ItemsList.Count;
    public DataStoreList<DataStoreItemEntry> ItemsList { get; set; }

    // Where this node goes in the rebuilt file. Set by DataStoreBPShop before reading Data.
    public uint Offset { get; set; }

    public override void LoadData(byte[] data, int offset = 0)
    {
        int count = data.ReadUShort(offset + 0x6);
        ItemsList = new DataStoreList<DataStoreItemEntry>();
        ItemsList.LoadData(data.SubArray((int)data.ReadUInt(offset + 0x8), DataStoreBPShop.ItemEntrySize * count));
    }

    public override byte[] Data => DataStoreBPShop.NodeHeader(DataStoreBPShop.ItemListTag, DataStoreBPShop.ItemEntrySize, Count, Offset + DataStoreBPShop.NodeHeaderSize).Concat(ItemsList.Data);

    public List<string> GetItems()
    {
        return ItemsList.Select(i => i.Item.ToString("X4")).Where(i => i != "FFFF").ToList();
    }

    public void SetItems(List<string> items)
    {
        // The list is rebuilt outright, so the shop can grow or shrink freely.
        ItemsList = new DataStoreList<DataStoreItemEntry>();
        for (int i = 0; i < items.Count; i++)
        {
            DataStoreItemEntry entry = new() { Data = new byte[DataStoreBPShop.ItemEntrySize] };
            entry.Item = Convert.ToUInt16(items[i], 16);
            ItemsList.Add(entry, i);
        }
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
