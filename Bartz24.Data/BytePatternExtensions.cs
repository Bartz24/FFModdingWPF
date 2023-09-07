using System;
using System.Collections.Generic;

namespace Bartz24.Data;

public static class BytePatternExtensions
{
    private const byte WildcardByte = 0x3F;

    public static List<int> FindPattern(this byte[] byteArray, string pattern)
    {
        List<int> matches = new();
        byte[] patternBytes = ParsePattern(pattern);
        int[] prefixArray = ComputePrefixArray(patternBytes);

        int i = 0; // Index in the byteArray
        int j = 0; // Index in the patternBytes

        while (i < byteArray.Length)
        {
            if (byteArray[i] == patternBytes[j] || patternBytes[j] == WildcardByte)
            {
                i++;
                j++;
            }

            if (j == patternBytes.Length)
            {
                matches.Add(i - j);
                j = prefixArray[j - 1];
            }
            else if (i < byteArray.Length && byteArray[i] != patternBytes[j] && patternBytes[j] != WildcardByte)
            {
                if (j != 0)
                {
                    j = prefixArray[j - 1];
                }
                else
                {
                    i++;
                }
            }
        }

        return matches;
    }

    private static byte[] ParsePattern(string pattern)
    {
        string[] parts = pattern.Split(' ');
        List<byte> bytes = new();

        foreach (string part in parts)
        {
            if (part == "??")
            {
                bytes.Add(WildcardByte);
            }
            else
            {
                bytes.Add(Convert.ToByte(part, 16));
            }
        }

        return bytes.ToArray();
    }

    private static int[] ComputePrefixArray(byte[] patternBytes)
    {
        int[] prefixArray = new int[patternBytes.Length];
        int length = 0;
        int i = 1;

        while (i < patternBytes.Length)
        {
            if (patternBytes[i] == patternBytes[length] || patternBytes[length] == WildcardByte)
            {
                length++;
                prefixArray[i] = length;
                i++;
            }
            else
            {
                if (length != 0)
                {
                    length = prefixArray[length - 1];
                }
                else
                {
                    prefixArray[i] = 0;
                    i++;
                }
            }
        }

        return prefixArray;
    }
}