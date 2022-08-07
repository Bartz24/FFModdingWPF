using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bartz24.RandoWPF
{
    public class RandomNum
    {
        private static Random rand = null;

        public static void SetRand(Random random)
        {
            if (rand != null)
                throw new NullReferenceException("Random has not been cleared yet!");
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
            return (long)Math.Round(rand.NextDouble() * (high - low)) + low;
        }

        private static void CheckRand()
        {
            if (rand == null)
                throw new NullReferenceException("Random has not been set!");
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
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = center + std * randStdNormal;
            return Math.Min(high, Math.Max(low, (int)Math.Round(randNormal)));
        }

        public static T SelectRandom<T>(List<T> list)
        {
            return list[RandomNum.RandInt(0, list.Count - 1)];
        }

        public static T SelectRandomWeighted<T>(List<T> list, Func<T, long> weightFunc)
        {
            CheckRand();
            List<T> weightedList = list.Where(t => weightFunc.Invoke(t) > 0).ToList();
            if (weightedList.Count == 0)
                return default(T);
            long totalWeight = weightedList.Sum(t => weightFunc.Invoke(t));
            if (totalWeight == 0)
                throw new Exception("Total weight cannot be 0");
            int i = 0;
            long index = RandLong(0, totalWeight - 1);
            while (index >= weightFunc.Invoke(weightedList[i]))
            {
                index -= weightFunc.Invoke(weightedList[i]);
                i++;
            }
            return weightedList[i];
        }

        public static int GetIntSeed(string seed)
        {
            try
            {
                return Int32.Parse(seed.Trim());
            }
            catch (Exception)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(seed.Trim());
                return (int)bytes.Sum(b => (int)Math.Pow(b + bytes[b % bytes.Length], 2.4)) * (int)bytes.Length - (int)bytes.Length;
            }
        }

        public static string GetHash(int length, int numBase = 10)
        {
            if (numBase > 10 || numBase < 1)
                throw new Exception("Base not supported: " + numBase);
            int sum = 0;
            foreach (Flag flag in Flags.FlagsList)
            {
                if (!flag.Aesthetic)
                {
                    flag.SetRand();
                    sum = (sum + RandomNum.RandInt(0, 1000000)) % 10000000;
                    ClearRand();
                }
            }
            Random random = new Random(sum);
            string s = "";
            for (int i = 0; i < length; i++)
            {
                s += random.Next(0, numBase - 1).ToString();
            }
            return s;
        }
    }
}
