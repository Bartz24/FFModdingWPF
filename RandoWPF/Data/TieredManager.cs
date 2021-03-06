using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public class TieredManager<T>
    {
        public List<Tiered<T>> list = new List<Tiered<T>>();

        public List<Tiered<T>> GetTiered(int rank, int count)
        {
            return list.Where(t => rank >= t.LowBound && rank <= t.GetHighBound(count)).ToList();
        }

        public Tuple<T,int> Get(int rank, int maxCount, Func<Tiered<T>,long> weightFunc = null, bool anyRandom = false)
        {
            if (weightFunc == null)
                weightFunc = t => t.Weight;
            List<Tiered<T>> items = GetTiered(rank, maxCount);
            Tiered<T> tiered = RandomNum.SelectRandomWeighted(items, weightFunc);
            return Get(rank,maxCount, tiered, null, anyRandom);
        }

        public Tuple<T, int> Get(int rank, int maxCount, Tiered<T> tiered, Func<T, bool> meetsReq = null, bool anyRandom = false)
        {
            List<Tuple<T, int>> possible = tiered == null ? new List<Tuple<T, int>>() : tiered.Get(rank, maxCount, meetsReq, anyRandom);
            if (possible.Count == 0)
                return new Tuple<T, int>(default(T), 0);
            return possible[RandomNum.RandInt(0, possible.Count - 1)];
        }

        public int GetRank(T obj, int count=1)
        {
            int avg = 0, avgCount = 0;
            foreach(Tiered<T> tiered in list)
            {
                int rank = tiered.GetRank(obj, count);
                if (rank != -1)
                {
                    if (count > 99)
                        Console.WriteLine(tiered.GetRank(obj, count));
                    avg += rank;
                    avgCount++;
                }
                    
            }
            return avgCount == 0 ? -1 : (int)Math.Round((float)avg / avgCount);
        }

        public int GetLowBound()
        {
            int lowest = Int32.MaxValue;
            list.ForEach(t => {
                if (t.LowBound < lowest)
                    lowest = t.LowBound;
            });
            return lowest;
        }

        public int GetHighBound()
        {
            int highest = Int32.MinValue;
            list.ForEach(t => {
                if (t.HighBound > highest)
                    highest = t.HighBound;
            });
            return highest;
        }
    }
}
