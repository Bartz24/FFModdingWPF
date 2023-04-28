using System;

namespace Bartz24.Data;

public static class ValueUInt
{
    public static uint ReadUInt(this byte[] data, int index)
    {
        byte[] temp = data.SubArray(index, 4);
        return BitConverter.ToUInt32(DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp, 0);
    }

    public static void SetUInt(this byte[] data, int index, uint value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        data.SetSubArray(index, DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp);
    }
}
