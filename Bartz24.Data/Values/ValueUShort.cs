using System;

namespace Bartz24.Data;

public static class ValueUShort
{
    public static ushort ReadUShort(this byte[] data, int index)
    {
        byte[] temp = data.SubArray(index, 2);
        return BitConverter.ToUInt16(DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp, 0);
    }

    public static void SetUShort(this byte[] data, int index, ushort value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        data.SetSubArray(index, DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp);
    }
}
