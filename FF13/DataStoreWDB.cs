using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bartz24.FF13;

public class DataStoreWDB<T> : DataStoreWDBEntry where T : DataStoreWDBEntry, new()
{
    private byte[] Header = new byte[0x10];
    private byte[] StrTypeList = null;
    private byte[] TypeList = null;
    private byte[] Version = null;
    private readonly Dictionary<string, T> Entries = new();
    private DataStoreStringPointerList StringList;
    private Dictionary<string, string> StringPointerMapping;
    private Dictionary<string, string> StringPointerEndingMapping;

    public T this[string id] => Entries[id];

    public List<string> Keys => Entries.Keys.ToList();
    public List<T> Values => Entries.Values.ToList();

    public List<KeyValuePair<string, T>> Entries => Data.Keys.Select(k => KeyValuePair.Create(k, Data[k])).ToList();

    public void Add(string id, T data)
    {
        Entries.Add(id, data);
    }
    public void Add(T data)
    {
        Entries.Add(data.ID, data);
    }
    public T Copy(string original, string newName)
    {
        T newData = new();
        newData.LoadData(new byte[Entries[original].Data.Length]);
        Entries[original].CopyPropertiesTo(newData);
        newData.LoadData(Entries[original].Data.ToArray());
        newData.ID = newName;
        Add(newData);

        return newData;
    }

    public void Swap(string name1, string name2)
    {
        Entries.Swap(name1, name2);
        Entries[name1].ID = name2;
        Entries[name2].ID = name1;
    }

    public void Rename(string name, string newName)
    {
        Copy(name, newName);
        Entries.Remove(name);
    }

    public void Load(string path)
    {
        LoadData(File.ReadAllBytes(path));
    }

    public override void LoadData(byte[] data, int wdbOffset = 0)
    {
        FindStringMappings();

        byte[] wdbData = data.SubArray(wdbOffset, data.Length - wdbOffset);
        Header = wdbData.SubArray(0, 0x10);

        uint count = Header.ReadUInt(0x04);

        for (int i = 0; i < count; i++)
        {
            string id = wdbData.ReadString(0x10 + (i * 0x20));
            uint offset = wdbData.ReadUInt(0x20 + (i * 0x20));
            uint size = wdbData.ReadUInt(0x24 + (i * 0x20));

            if (id == "!!string")
            {
                StringList = new DataStoreStringPointerList(new DataStoreString() { Value = "" });
                StringList.LoadData(wdbData.SubArray((int)offset, (int)size));
            }
            else if (id == "!!strtypelist")
            {
                StrTypeList = wdbData.SubArray((int)offset, (int)size);
            }
            else if (id == "!!typelist")
            {
                TypeList = wdbData.SubArray((int)offset, (int)size);
            }
            else if (id == "!!version")
            {
                Version = wdbData.SubArray((int)offset, (int)size);
            }
            else
            {
                T newEntry = new()
                {
                    ID = id
                };
                newEntry.LoadData(wdbData.SubArray((int)offset, (int)size));
                Add(newEntry);
            }
        }

        UpdateStringValues();
    }

    private void FindStringMappings()
    {
        StringPointerMapping = new Dictionary<string, string>();
        StringPointerEndingMapping = new Dictionary<string, string>();
        foreach (PropertyInfo p in typeof(T).GetProperties())
        {
            if (p.Name.EndsWith("_pointer"))
            {
                string prefix = p.Name.Substring(0, p.Name.LastIndexOf("_pointer"));
                StringPointerMapping.Add(prefix + "_pointer", prefix + "_string");
            }

            if (p.Name.EndsWith("_pointer_end"))
            {
                string prefix = p.Name.Substring(0, p.Name.LastIndexOf("_pointer_end"));
                StringPointerEndingMapping.Add(prefix + "_pointer", prefix + "_pointer_end");
            }
        }
    }

    public void Save(string path)
    {
        File.WriteAllBytes(path, Data);
    }
    
    public override byte[] Data
    {
        get
        {
            UpdateStringPointers();

            byte[] newData = new byte[0];

            newData = newData.Concat(Header).ToArray();
            newData.SetUInt(0x04, (uint)(Entries.Count + 4));

            uint dataOffset = 0x10 + (uint)((Entries.Count + 4) * 0x20);

            newData = newData.Concat(CreateDataEntry("!!string", dataOffset, (uint)StringList.Data.Length));
            dataOffset += (uint)StringList.Data.Length;
            newData = newData.Concat(CreateDataEntry("!!strtypelist", dataOffset, (uint)StrTypeList.Length));
            dataOffset += (uint)StrTypeList.Length;
            newData = newData.Concat(CreateDataEntry("!!typelist", dataOffset, (uint)TypeList.Length));
            dataOffset += (uint)TypeList.Length;
            newData = newData.Concat(CreateDataEntry("!!version", dataOffset, (uint)Version.Length));
            dataOffset += (uint)Version.Length;

            List<string> sorted = GetSortOrder();
            for (int i = 0; i < Entries.Count; i++)
            {
                string id = sorted[i];
                newData = newData.Concat(CreateDataEntry(id, dataOffset, (uint)Entries[id].Data.Length));
                dataOffset += (uint)Entries[id].Data.Length;
            }

            newData = newData.Concat(StringList.Data);
            newData = newData.Concat(StrTypeList);
            newData = newData.Concat(TypeList);
            newData = newData.Concat(Version);

            for (int i = 0; i < Entries.Count; i++)
            {
                string id = sorted[i];
                newData = newData.Concat(Entries[id].Data);
            }

            return newData;
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

    private void UpdateStringPointers()
    {
        Values.ForEach(entry =>
        {
            StringPointerMapping.ForEach(p =>
            {
                string value = entry.GetPropValue<string>(p.Value);
                DataStoreString s = new() { Value = value };
                if (value != "" && !StringList.Contains(s))
                {
                    StringList.Add(s, StringList.Length);
                }
            });
        });

        StringList.UpdatePointers();

        Values.ForEach(entry =>
        {
            StringPointerMapping.ForEach(p =>
            {
                string value = entry.GetPropValue<string>(p.Value);
                uint pointer = entry.GetPropValue<uint>(p.Key);
                if (value != "")
                {
                    DataStoreString match = StringList.ToList().FirstOrDefault(s => s.Value == value);
                    entry.SetPropValue(p.Key, (uint)StringList.IndexOf(match));
                }
                else
                {
                    DataStoreString match = StringList.ToList().Skip(1).FirstOrDefault();
                    entry.SetPropValue(p.Key, (uint)StringList.IndexOf(match) - 1);
                }
            });
            StringPointerEndingMapping.ForEach(p =>
            {
                string value = entry.GetPropValue<string>(StringPointerMapping[p.Key]);
                uint pointer = entry.GetPropValue<uint>(p.Key);
                entry.SetPropValue(p.Value, (uint)(pointer + value.Length));
            });
        });
    }

    private void UpdateStringValues()
    {
        Values.ForEach(entry =>
        {
            StringPointerMapping.ForEach(p =>
            {
                string value = entry.GetPropValue<string>(p.Value);
                uint pointer = entry.GetPropValue<uint>(p.Key);
                entry.SetPropValue(p.Value, StringList[(int)pointer].Value);
            });
        });
    }

    private List<string> GetSortOrder()
    {
        return Entries.Keys.OrderBy(s => s, StringComparer.Ordinal).ToList();
    }
}
