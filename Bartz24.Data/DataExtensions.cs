using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    public enum ByteMode
    {
        BigEndian,
        LittleEndian
    }
    public static class DataExtensions
    {
        public static ByteMode Mode = ByteMode.BigEndian;
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void SetSubArray<T>(this T[] data, int index, T[] subArray)
        {
            for (int i = 0; i < subArray.Length; i++)
                data[index + i] = subArray[i];
        }

        public static T[] ReverseArray<T>(this T[] data)
        {
            T[] newArray = new T[data.Length];
            Array.Copy(data, newArray, data.Length);
            Array.Reverse(newArray);
            return newArray;
        }

        public static T[] Concat<T>(this T[] data, T[] data2)
        {
            T[] newArray = new T[data.Length + data2.Length];
            Array.Copy(data, newArray, data.Length);
            Array.Copy(data2, 0, newArray, data.Length, data2.Length);
            return newArray;
        }

        public static List<int> IndexesOf<T>(this T[] data, T[] data2)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < data.Length - data2.Length; i++)
            {
                if (data.SubArray(i, data2.Length).SequenceEqual(data2))
                {
                    list.Add(i);
                }
            }
            return list;
        }

        public static int FindIndexNextByte(this byte[] data, int offset, byte value)
        {
            while (data[offset] != value && offset < data.Length - 1)
            {
                offset++;
            }
            return offset;
        }

        public static T[] Replace<T>(this T[] data, T[] data2, T[] newData)
        {
            IndexesOf(data, data2).ForEach(i => data.SetSubArray(i, newData));
            return data;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static void Swap<T>(this List<T> list, int i1, int i2)
        {
            T temp = list[i1];
            list.Insert(i1, list[i2]);
            list.RemoveAt(i1 + 1);
            list.Insert(i2, temp);
            list.RemoveAt(i2 + 1);
        }

        public static void Swap<K, T>(this Dictionary<K, T> dict, K k1, K k2)
        {
            T temp = dict[k1];
            dict[k1] = dict[k2];
            dict[k2] = temp;
        }

        public static List<T> Replace<T>(this List<T> list, T origValue, T newValue)
        {
            int index = list.IndexOf(origValue);
            if (index > -1)
            {
                list[index] = newValue;
            }
            return list;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            enumerable.ToList().ForEach(action);
        }
        public static E GetEnumValue<E>(string name)
        {
            return ((E[])Enum.GetValues(typeof(E)))[Enum.GetNames(typeof(E)).ToList().IndexOf(name)];
        }

        public static string SeparateWords(this object str)
        {
            return Regex.Replace(str.ToString(), "([a-z])([A-Z])", "$1 $2");
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

        public static void SetPropValue(this Object obj, String name, Object value)
        {
            PropertyInfo info = null;
            string[] parts = name.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (obj == null) { return; }

                Type type = obj.GetType();
                info = type.GetProperty(part);
                if (info == null) { return; }

                if (i == parts.Length - 1)
                {
                    info.SetValue(obj, value);
                    return;
                }
                obj = info.GetValue(obj, null);
            }
        }

        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }
        public static int FindClosingBracketIndex(this string text, char openedBracket = '{', char closedBracket = '}')
        {
            int index = text.IndexOf(openedBracket);
            int bracketCount = 1;
            var textArray = text.ToCharArray();

            for (int i = index + 1; i < textArray.Length; i++)
            {
                if (textArray[i] == openedBracket)
                {
                    bracketCount++;
                }
                else if (textArray[i] == closedBracket)
                {
                    bracketCount--;
                }

                if (bracketCount == 0)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        public static int RoundToSignificantDigits(this int i, int digits)
        {
            if (i == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(i))) + 1);
            return (int)(scale * Math.Round(i / scale, digits));
        }
    }
}
