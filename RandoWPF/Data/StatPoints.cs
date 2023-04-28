using System;
using System.Linq;

namespace Bartz24.RandoWPF;

public class StatPoints
{
    private StatValuesChanceSelect Values { get; set; }
    private (int, int)[] Bounds { get; set; }
    private float[] Weights { get; set; }
    private int[] ZeroChances { get; set; }
    private int[] NegChances { get; set; }
    private float Rate { get; set; }
    public StatPoints((int, int)[] bounds, float[] weights, int[] chances, int[] zeroChances, int[] negChances, float rate = 0.2f)
    {
        Values = new StatValuesChanceSelect(chances);
        Bounds = bounds;
        Weights = weights;
        ZeroChances = zeroChances;
        NegChances = negChances;
        Rate = rate;
    }

    public void Randomize(int[] actual)
    {
        (int, int)[] modBounds = null;
        int total = -1;
        do
        {
            int attempts = 0;
            do
            {
                do
                {
                    modBounds = Enumerable.Range(0, Bounds.Length).Select(i =>
                    {
                        return RandomNum.RandInt(0, 99) < ZeroChances[i] * (100 - attempts) / 100
                            ? (0, 0)
                            : RandomNum.RandInt(0, 99) >= NegChances[i]
                            ? (0, (int)(Bounds[i].Item2 * Weights[i]))
                            : ((int)(Bounds[i].Item1 * Weights[i]), (int)(Bounds[i].Item2 * Weights[i]));
                    }).ToArray();
                    attempts++;
                } while (modBounds.Where(b => b.Item2 == 0).Count() == Bounds.Count());

                total = Enumerable.Range(0, Bounds.Length).Select(i =>
                {
                    int val = (int)((actual[i] - Bounds[i].Item1) * Weights[i]);
                    if (modBounds[i].Item1 == 0)
                    {
                        val += (int)(Bounds[i].Item1 * Weights[i]);
                    }

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
        get => Values[i];
        set => Values[i] = value;
    }
}
