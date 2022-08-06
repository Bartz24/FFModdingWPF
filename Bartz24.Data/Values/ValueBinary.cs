using System.Collections;

namespace Bartz24.Data
{
    public static class ValueBinary
    {
        public static bool ReadBinary(this byte[] data, int index, int binaryIndex)
        {
            return new BitArray(new byte[] { data.ReadByte(index) })[binaryIndex];
        }

        public static void SetBinary(this byte[] data, int index, int binaryIndex, bool value)
        {
            BitArray array = new BitArray(new byte[] { data.ReadByte(index) });
            array[binaryIndex] = value;
            byte[] bytes = new byte[1];
            array.CopyTo(bytes, 0);
            data.SetSubArray(index, bytes);
        }
    }
}
