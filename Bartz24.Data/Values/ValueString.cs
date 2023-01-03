using System;
using System.Text;

namespace Bartz24.Data
{
    public static class ValueString
    {
        public static string ReadString(this byte[] data, int index)
        {
            int size = data.FindIndexNextByte(index, 0) - index;
            byte[] strData = data.SubArray(index, size + 1);
            if (strData.Length == 1 && strData[0] == 0)
                return "";
            else
                return Encoding.UTF8.GetString(strData.SubArray(0, strData.Length - 1));
        }

        public static void SetString(this byte[] data, int index, string value, int length = - 1)
        {
            if (length == -1)
                length = value.Length + 1;
            byte[] strData = Encoding.UTF8.GetBytes(value);
            for(int i = 0; i < length - value.Length; i++)
            {
                strData = strData.Concat(new byte[] { 0 });
            }
            data.SetSubArray(index, strData);
        }
    }
}
