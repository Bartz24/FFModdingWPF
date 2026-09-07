using Bartz24.Data;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF12;

public class DataStoreBPShop : DataStoreBPSection<DataStoreShop>
{
    public const uint RootTag = 0x64;
    public const uint ShopTag = 0x164;
    public const uint ItemListTag = 0x264;

    public const int NodeHeaderSize = 0xC;
    public const int PointerEntrySize = 0x8;
    public const int ItemEntrySize = 0x2;

    private const int NodeAlignment = 4;
    private const int FileAlignment = 8;

    // Root entry ids and the shop keys are kept exactly as read; only offsets are recalculated.
    private uint[] rootIDs;
    private List<uint[]> shopKeys;

    // Which item lists belong to which shop, as indices into DataList.
    private List<int[]> shopLists;

    // Where each node started in the file it was read from. Nodes are written back in this order,
    // which is by offset and does not follow the root table.
    private uint[] shopNodeOffsets;
    private uint[] listNodeOffsets;

    public override void LoadData(byte[] data, int offset = 0)
    {
        header = data.SubArray(0, NodeHeaderSize);

        int rootCount = data.ReadUShort(0x6);
        int rootStart = (int)data.ReadUInt(0x8);

        rootIDs = new uint[rootCount];
        shopKeys = new List<uint[]>();
        shopLists = new List<int[]>();
        shopNodeOffsets = new uint[rootCount];
        List<uint> listOffsets = new();
        DataList = new DataStoreList<DataStoreShop>();

        for (int i = 0; i < rootCount; i++)
        {
            int entry = rootStart + (i * PointerEntrySize);
            rootIDs[i] = data.ReadUInt(entry);

            int shopNode = (int)data.ReadUInt(entry + 0x4);
            shopNodeOffsets[i] = (uint)shopNode;
            int listCount = data.ReadUShort(shopNode + 0x6);
            int listStart = (int)data.ReadUInt(shopNode + 0x8);

            uint[] keys = new uint[listCount];
            int[] lists = new int[listCount];
            for (int j = 0; j < listCount; j++)
            {
                int listEntry = listStart + (j * PointerEntrySize);
                keys[j] = data.ReadUInt(listEntry);

                DataStoreShop shop = new();
                int listNode = (int)data.ReadUInt(listEntry + 0x4);
                shop.LoadData(data, listNode);
                shop.ID = DataList.Count;
                listOffsets.Add((uint)listNode);
                lists[j] = DataList.Count;
                DataList.Add(shop, DataList.Count);
            }

            shopKeys.Add(keys);
            shopLists.Add(lists);
        }

        listNodeOffsets = listOffsets.ToArray();
    }

    public override byte[] Data
    {
        get
        {
            int rootStart = (int)header.ReadUInt(0x8);
            int pos = rootStart + (rootIDs.Length * PointerEntrySize);

            // Lay the nodes out first so every pointer is known before anything is written. Order is
            // the one they were read in, so a file that has not been resized comes back unchanged.
            List<(uint Original, int Shop, int List)> layout = new();
            for (int i = 0; i < rootIDs.Length; i++)
            {
                layout.Add((shopNodeOffsets[i], i, -1));
                layout.AddRange(shopLists[i].Select(index => (listNodeOffsets[index], i, index)));
            }

            layout.Sort((a, b) => a.Original.CompareTo(b.Original));

            uint[] shopOffsets = new uint[rootIDs.Length];
            foreach ((uint _, int shop, int list) in layout)
            {
                pos = Align(pos, NodeAlignment);
                if (list < 0)
                {
                    shopOffsets[shop] = (uint)pos;
                    pos += NodeHeaderSize + (shopKeys[shop].Length * PointerEntrySize);
                }
                else
                {
                    DataList[list].Offset = (uint)pos;
                    pos += NodeHeaderSize + (DataList[list].Count * ItemEntrySize);
                }
            }

            byte[] outData = new byte[Align(pos, FileAlignment)];
            outData.SetSubArray(0, NodeHeader(RootTag, PointerEntrySize, (ushort)rootIDs.Length, (uint)rootStart));

            for (int i = 0; i < rootIDs.Length; i++)
            {
                int entry = rootStart + (i * PointerEntrySize);
                outData.SetUInt(entry, rootIDs[i]);
                outData.SetUInt(entry + 0x4, shopOffsets[i]);

                int listStart = (int)shopOffsets[i] + NodeHeaderSize;
                outData.SetSubArray((int)shopOffsets[i], NodeHeader(ShopTag, PointerEntrySize, (ushort)shopKeys[i].Length, (uint)listStart));

                for (int j = 0; j < shopKeys[i].Length; j++)
                {
                    DataStoreShop shop = DataList[shopLists[i][j]];
                    int listEntry = listStart + (j * PointerEntrySize);
                    outData.SetUInt(listEntry, shopKeys[i][j]);
                    outData.SetUInt(listEntry + 0x4, shop.Offset);
                    outData.SetSubArray((int)shop.Offset, shop.Data);
                }
            }

            return outData;
        }
    }

    public static byte[] NodeHeader(uint tag, ushort elementSize, ushort count, uint dataOffset)
    {
        byte[] node = new byte[NodeHeaderSize];
        node.SetUInt(0x0, tag);
        node.SetUShort(0x4, elementSize);
        node.SetUShort(0x6, count);
        node.SetUInt(0x8, dataOffset);
        return node;
    }

    private static int Align(int value, int alignment)
    {
        return (value + alignment - 1) / alignment * alignment;
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
