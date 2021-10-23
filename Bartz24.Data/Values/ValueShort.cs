using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    public static class ValueShort
    {
        public static short ReadShort(this byte[] data, int index)
        {
            byte[] temp = data.SubArray(index, 2);
            return BitConverter.ToInt16(DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp, 0);
        }

        public static void SetShort(this byte[] data, int index, short value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            data.SetSubArray(index, DataExtensions.Mode == ByteMode.BigEndian ? temp.ReverseArray() : temp);
        }
    }
}
