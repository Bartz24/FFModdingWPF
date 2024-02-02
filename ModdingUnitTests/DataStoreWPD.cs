using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bartz24.FF13;

public class DataStoreWPD : DataStore
{
    private byte[] Header = new byte[0x10];
    private readonly List<string> Entries = new();

    public List<string> Keys => Entries.ToList();

    public void Load(string path)
    {
        LoadData(File.ReadAllBytes(path));
    }

    public override void LoadData(byte[] data, int wdbOffset = 0)
    {
        byte[] wdbData = data.SubArray(wdbOffset, data.Length - wdbOffset);
        Header = wdbData.SubArray(0, 0x10);

        uint count = Header.ReadUInt(0x04);

        for (int i = 0; i < count; i++)
        {
            string id = wdbData.ReadString(0x10 + (i * 0x20));
            uint offset = wdbData.ReadUInt(0x20 + (i * 0x20));
            uint size = wdbData.ReadUInt(0x24 + (i * 0x20));

            Entries.Add(id);
        }
    }
    
    public override byte[] Data
    {
        get
        {
            return Header;
        }
    }

    private byte[] CreateDataEntry(string id, uint offset, uint size)
    {
        byte[] entry = new byte[0x20];
        entry.SetString(0, id, 16);
        entry.SetUInt(0x10, offset);
        entry.SetUInt(0x14, size);

        return entry;
    }

    public override int GetDefaultLength()
    {
        return -1;
    }
}
