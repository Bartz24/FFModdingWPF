using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    public static class ValueByte
    {
        public static byte ReadByte(this byte[] data, int index)
        {
            return data[index];
        }

        public static void SetByte(this byte[] data, int index, byte value)
        {
            data.SetSubArray(index,new byte[] { value });
        }
    }
}
