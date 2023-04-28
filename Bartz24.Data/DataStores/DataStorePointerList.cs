using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.Data;

public class DataStorePointerList<T> : DataStoreList<T> where T : DataStore, new()
{
    public readonly T NULL;
    protected Dictionary<int, int> pointerToIndexDictionary = new();

    public DataStorePointerList(T nullVal)
    {
        NULL = nullVal;
    }
    public override void LoadData(byte[] data, int offset = 0)
    {
        list.Clear();
        while (offset < data.Length)
        {
            T val = new();
            val.LoadData(data, offset);
            Add(val, offset);
            offset += val.Length;
        }
    }

    public Dictionary<int, int> GetNewPointers()
    {
        Dictionary<int, int> newDict = new();
        int offset = 0;
        pointerToIndexDictionary.Keys.ForEach(key =>
        {
            newDict.Add(key, offset);
            offset += this[key].Length;
        });
        return newDict;
    }

    public void UpdatePointers()
    {
        Dictionary<int, int> newPointers = GetNewPointers();
        Dictionary<int, int> newDict = new();
        pointerToIndexDictionary.Keys.ForEach(key =>
        {
            newDict.Add(newPointers[key], pointerToIndexDictionary[key]);
        });
        pointerToIndexDictionary = newDict;
    }

    public new T this[int i]
    {
        get => !pointerToIndexDictionary.ContainsKey(i) ? NULL : list[pointerToIndexDictionary[i]];
        set
        {
            list[pointerToIndexDictionary[i]] = value;
            UpdatePointers();
        }
    }

    public override void Clear()
    {
        base.Clear();
        pointerToIndexDictionary.Clear();
    }

    public override void Add(T obj, int i)
    {
        pointerToIndexDictionary.Add(i, list.Count);
        base.Add(obj, list.Count);
    }

    public override int IndexOf(T obj)
    {
        IEnumerable<int> values = pointerToIndexDictionary.Keys.Where(key => this[key].Equals(obj));
        return Contains(obj) && values.Count() == 0 ? throw new Exception("Invalid check!") : values.Count() > 0 ? values.First() : -1;
    }
}
