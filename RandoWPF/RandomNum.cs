using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bartz24.RandoWPF;

public class RandomNum
{
    private static Random rand = null;

    public static void SetRand(Random random)
    {
        if (rand != null)
        {
            throw new NullReferenceException("Random has not been cleared yet!");
        }

        rand = random;
    }

    public static void ClearRand()
    {
        rand = null;
    }

    /// <summary>
    /// Gets a random number from (low, high)
    /// </summary>
    /// <param name="low"></param>
    /// <param name="high"></param>
    /// <returns></returns>
    public static int RandInt(int low, int high)
    {
        CheckRand();
        return rand.Next(low, high + 1);
    }

    /// <summary>
    /// Gets a random number from (low, high)
    /// </summary>
    /// <param name="low"></param>
    /// <param name="high"></param>
    /// <returns></returns>
    public static long RandLong(long low, long high)
    {
        CheckRand();
        return rand.NextInt64(low, high + 1);
    }

    private static void CheckRand()
    {
        if (rand == null)
        {
            throw new NullReferenceException("Random has not been set!");
        }
    }

    public static int RandSeed()
    {
        SetRand(new Random());
        int val = RandInt((int)1e8, (int)1e9 - 1);
        ClearRand();
        return val;
    }

    public static int RandIntNorm(double center, double std, int low, int high)
    {
        CheckRand();
        int randNormal = int.MinValue;
        while (randNormal < low || randNormal > high)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            randNormal = (int)Math.Round(center + (std * randStdNormal));
        }

        return randNormal;
    }

    public static T SelectRandom<T>(List<T> list)
    {
        return list[RandomNum.RandInt(0, list.Count - 1)];
    }

    public static T SelectRandom<T>(IEnumerable<T> list)
    {
        return list.ElementAt(RandomNum.RandInt(0, list.Count() - 1));
    }

    public static T SelectRandomWeighted<T>(List<T> list, Func<T, long> weightFunc)
    {
        CheckRand();
        long totalWeight = 0;
        T selected = default;
        foreach (var item in list)
        {
            long weight = weightFunc(item);
            if (weight < 0)
            {
                throw new Exception("Weight function cannot be negative");
            }

            if (weight == 0)
            {
                continue;
            }

            totalWeight += weight;
            if (RandLong(0, totalWeight - 1) < weight)
            {
                selected = item;
            }
        }

        if (totalWeight == 0)
        {
            throw new Exception("Total weight cannot be 0");
        }

        return selected;
    }

    /// <summary>
    /// Shuffles based on a function where an object has a higher chance of replacing each index or object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="weightFunc">indexFrom, objFrom, indexTo, objTo; return long</param>
    /// <returns></returns>
    public static List<T> ShuffleWeightedOrder<T>(List<T> list, Func<int, T, int, T, long> weightFunc)
    {
        CheckRand();
        List<T> newList = new(list);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = SelectRandomWeighted(Enumerable.Range(0, list.Count).ToList(), i => weightFunc(n, newList[n], i, newList[i]));
            (newList[n], newList[k]) = (newList[k], newList[n]);
        }

        return newList;
    }
    public static List<T> ShuffleLocalized<T>(List<T> list, int distance)
    {
        return ShuffleWeightedOrder(list, (from, _, to, _) =>
        {
            return to < from - distance || to > from + distance ? 0 : 1;
        });
    }

    public static int GetIntSeed(string seed)
    {
        try
        {
            return int.Parse(seed.Trim());
        }
        catch (Exception)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(seed.Trim());
            return (bytes.Sum(b => (int)Math.Pow(b + bytes[b % bytes.Length], 2.4)) * bytes.Length) - bytes.Length;
        }
    }

    public static string GetHash(int length, int numBase = 10)
    {
        if (numBase is > 10 or < 1)
        {
            throw new Exception("Base not supported: " + numBase);
        }

        int sum = 0;
        foreach (Flag flag in RandoFlags.FlagsList)
        {
            if (!flag.Aesthetic)
            {
                flag.SetRand();
                sum = (sum + RandomNum.RandInt(0, 1000000)) % 10000000;
                ClearRand();
            }
        }

        Random random = new(sum);
        string s = "";
        for (int i = 0; i < length; i++)
        {
            s += random.Next(0, numBase - 1).ToString();
        }

        return s;
    }
}
