﻿using System;

namespace Bartz24.Data;

public abstract class DataStore
{
    public virtual byte[] Data { get; set; }

    public virtual void LoadData(byte[] data, int offset = 0)
    {
        if (GetDefaultLength() > 0)
        {
            Data = data.SubArray(offset, GetDefaultLength());
        }
    }

    public virtual int Length => Data.Length;

    public abstract int GetDefaultLength();

    public virtual void UpdateStringPointers(DataStorePointerList<DataStoreString> list) { }
    public void UpdateStringPointer(DataStorePointerList<DataStoreString> list, string actualString, uint pointer, Action<string> setString, Action<uint> setPointer)
    {
        UpdateStringPointer(list, actualString, pointer, setString, setPointer, v => { });
    }
    public void UpdateStringPointer(DataStorePointerList<DataStoreString> list, string actualString, uint pointer, Action<string> setString, Action<uint> setStartingPointer, Action<uint> setEndingPointer)
    {
        if (actualString != null)
        {
            DataStoreString value = new() { Value = actualString };
            if (!list.Contains(value))
            {
                list.Add(value, list.Length);
            }

            pointer = (uint)list.IndexOf(value);
            setStartingPointer.Invoke(pointer);
            setEndingPointer.Invoke(pointer + (uint)value.Value.Length);
        }

        setString.Invoke(list[(int)pointer].Value);
    }
}
