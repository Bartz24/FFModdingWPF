using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class TieredManager<T>
{
    public List<Tiered<T>> list = new();

    public List<Tiered<T>> GetTiered(int rank, int count)
    {
        return list.Where(t => rank >= t.LowBound && rank <= t.GetHighBound(count)).ToList();
    }

    public (T, int) Get(int rank, int maxCount, Func<Tiered<T>, long> weightFunc = null, bool anyRandom = false)
    {
        weightFunc ??= t => t.Weight;
        List<Tiered<T>> items = GetTiered(rank, maxCount);
        Tiered<T> tiered = RandomNum.SelectRandomWeighted(items, weightFunc);
        return Get(rank, maxCount, tiered, null, anyRandom);
    }

    public (T, int) Get(int rank, int maxCount, Tiered<T> tiered, Func<T, bool> meetsReq = null, bool anyRandom = false)
    {
        List<(T, int)> possible = tiered == null ? new List<(T, int)>() : tiered.Get(rank, maxCount, meetsReq, anyRandom);
        return possible.Count == 0 ? (default(T), 0) : possible[RandomNum.RandInt(0, possible.Count - 1)];
    }

    public int GetRank(T obj, int count = 1)
    {
        int avg = 0, avgCount = 0;
        foreach (Tiered<T> tiered in list)
        {
            int rank = tiered.GetRank(obj, count);
            if (rank != -1)
            {
                if (count > 99)
                {
                    Console.WriteLine(tiered.GetRank(obj, count));
                }

                avg += rank;
                avgCount++;
            }
        }

        return avgCount == 0 ? -1 : (int)Math.Round((float)avg / avgCount);
    }

    public int GetLowBound()
    {
        int lowest = int.MaxValue;
        list.ForEach(t =>
        {
            if (t.LowBound < lowest)
            {
                lowest = t.LowBound;
            }
        });
        return lowest;
    }

    public int GetHighBound()
    {
        int highest = int.MinValue;
        list.ForEach(t =>
        {
            if (t.HighBound > highest)
            {
                highest = t.HighBound;
            }
        });
        return highest;
    }
}
