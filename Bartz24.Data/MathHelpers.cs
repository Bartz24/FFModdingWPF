using System;
using System.Linq;

namespace Bartz24.Data;

public class MathHelpers
{
    // Sequence must only contain once of each value from [0, n)
    public static long EncodeNaturalSequence(long[] seq, int pow)
    {
        int n = seq.Length;
        return Enumerable.Range(0, n).Sum(i => seq[i] * (long)Math.Pow(pow, i));
    }

    public static long[] DecodeNaturalSequence(long val, int n, int pow)
    {
        long[] seq = new long[n];
        for (int i = 0; i < n; i++)
        {
            long mod = val % (long)Math.Pow(pow, i + 1);
            seq[i] = mod / (long)Math.Pow(pow, i);
            val -= mod;
        }

        return seq;
    }

    public static int[] SplitBetween(int total, int count)
    {
        int[] array = Enumerable.Range(0, count).Select(_ => total / count).ToArray();
        int left = total - array.Sum();
        for (int i = 0; i < left; i++)
        {
            array[i]++;
        }

        return array;
    }
    public static int RoundToInterval(int i, int interval)
    {
        return interval == 0
            ? throw new ArgumentException("The specified interval cannot be 0.", nameof(interval))
            : ((int)Math.Round(i / (double)interval)) * interval;
    }
}
