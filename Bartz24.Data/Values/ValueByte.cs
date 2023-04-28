using System;

namespace Bartz24.Data;

public static class ValueByte
{
    public static byte ReadByte(this byte[] data, int byteIndex)
    {
        return data[byteIndex];
    }
    public static byte ReadByte(this byte[] data, int byteIndex, int bitIndex, int bitLength = 8)
    {
        if (bitIndex == 0 && bitLength == 8)
        {
            return data.ReadByte(byteIndex);
        }
        else if (bitIndex + bitLength <= 8)
        {
            byte byteOut = data[byteIndex];
            byteOut <<= bitIndex;
            byteOut >>= 8 - bitLength;
            return byteOut;
        }
        else
        {
            byte byte1 = data[byteIndex];
            byte1 <<= bitIndex;
            byte byte2 = data[byteIndex + 1];
            byte2 >>= 8 - bitIndex;
            byte byteOut = (byte)((byte1 + byte2) & 0xFF);
            return byteOut;
        }
    }

    public static void SetByte(this byte[] data, int byteIndex, byte value)
    {
        data.SetSubArray(byteIndex, new byte[] { value });
    }

    public static void SetByte(this byte[] data, int byteIndex, byte value, int bitIndex, int bitLength = 8)
    {
        if (bitIndex == 0 && bitLength == 8)
        {
            data.SetByte(byteIndex, value);
        }
        else if (bitIndex + bitLength <= 8)
        {
            byte newData = (byte)(value << (8 - bitLength));
            newData >>= bitIndex;
            byte left = (byte)(data[byteIndex] & (0xFF << (8 - bitIndex)));
            byte right = (byte)(data[byteIndex] & (0xFF << (8 - bitIndex + bitLength)));
            data.SetByte(byteIndex, (byte)(left + newData + right));
        }
        else
        {
            byte newDataLeft = (byte)(value >> bitIndex);
            byte newDataRight = (byte)(value << (8 - bitIndex));
            byte left = (byte)(data[byteIndex] & (0xFF << (8 - bitIndex)));
            byte right = (byte)(data[byteIndex + 1] & (0xFF << (8 - (bitIndex + bitLength - 8))));
            data.SetSubArray(byteIndex, new byte[] { (byte)(left + newDataLeft), (byte)(right + newDataRight) });
        }
    }
}
