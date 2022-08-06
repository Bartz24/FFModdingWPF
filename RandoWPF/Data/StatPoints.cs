using System;
using System.Linq;

namespace Bartz24.RandoWPF
{
    public class StatPoints
    {
        private StatValues Values { get; set; }
        private Tuple<int, int>[] Bounds { get; set; }
        private float[] Weights { get; set; }
        private int[] ZeroChances { get; set; }
        private int[] NegChances { get; set; }
        private float Rate { get; set; }
        public StatPoints(Tuple<int, int>[] bounds, float[] weights, int[] zeroChances, int[] negChances, float rate = 0.2f)
        {
            Values = new StatValues(bounds.Length);
            Bounds = bounds;
            Weights = weights;
            ZeroChances = zeroChances;
            NegChances = negChances;
            Rate = rate;
        }

        public void Randomize(int[] actual)
        {
            Tuple<int, int>[] modBounds = null;
            int total = -1;
            do
            {
                do
                {
                    do
                    {
                        modBounds = Enumerable.Range(0, Bounds.Length).Select(i =>
                        {
                            if (RandomNum.RandInt(0, 99) < ZeroChances[i])
                                return new Tuple<int, int>(0, 0);
                            else if (RandomNum.RandInt(0, 99) >= NegChances[i])
                                return new Tuple<int, int>(0, (int)(Bounds[i].Item2 * Weights[i]));
                            else
                                return new Tuple<int, int>((int)(Bounds[i].Item1 * Weights[i]), (int)(Bounds[i].Item2 * Weights[i]));
                        }).ToArray();
                    } while (modBounds.Where(b => b.Item2 == 0).Count() == Bounds.Count());

                    total = Enumerable.Range(0, Bounds.Length).Select(i =>
                    {
                        int val = (int)((actual[i] - Bounds[i].Item1) * Weights[i]);
                        if (modBounds[i].Item1 == 0)
                            val += (int)(Bounds[i].Item1 * Weights[i]);
                        return val;
                    }).Sum();
                } while (StatValues.GetBoundsSum(modBounds) < total);

                Values.Randomize(modBounds, total, Rate);

                Enumerable.Range(0, Bounds.Length).ToList().ForEach(i =>
                {
                    Values[i] = (int)(Values[i] / Weights[i]);
                });
                Enumerable.Range(0, Bounds.Length).Where(i => Weights[i] <= 1 && modBounds[i].Item1 != modBounds[i].Item2).ToList().ForEach(i =>
                {
                    Values[i] += RandomNum.RandInt((int)(-1 / Weights[i]), (int)(1 / Weights[i]));
                });
                Enumerable.Range(0, Bounds.Length).ToList().ForEach(i =>
                {
                    Values[i] = Math.Max(Math.Min(Values[i], Bounds[i].Item2), Bounds[i].Item1);
                });
            } while (Values.Vals.Max() <= 0 && total > 0);
        }
        public int this[int i]
        {
            get
            {
                return Values[i];
            }
            set
            {
                Values[i] = value;
            }
        }
    }
}
