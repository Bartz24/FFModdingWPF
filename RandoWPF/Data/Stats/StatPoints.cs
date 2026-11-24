using System;
using System.Linq;

namespace Bartz24.RandoWPF;

public class StatPoints
{
    private const int MaxRetries = 100;

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
        // An item with no positive stat at all has nothing to hand out: keep it as it is rather
        // than invent stats for it (or produce an all-zero item with a -1 from the jitter).
        if (actual.All(v => v <= 0))
        {
            KeepVanilla(actual);
            return;
        }

        // Every other item is randomized. When its weighted stats net out at zero or less (a
        // penalty such as LR's ATB -15 outweighing the rest), its points come from its positive
        // stats alone, so it ends up about as strong as its good side. Stats that always roll a
        // negative range (FF12's charge time) are part of every roll and stay in.
        int[] budget = GetBasePoints(actual) > 0
            ? actual
            : Enumerable.Range(0, Bounds.Length).Select(i => AlwaysRollsNegative(i) ? actual[i] : Math.Max(actual[i], 0)).ToArray();

        (int, int)[] modBounds = null;
        int total = -1;
        int retries = 0;
        do
        {
            if (retries++ >= MaxRetries)
            {
                KeepVanilla(actual);
                return;
            }

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
                    int val = (int)((budget[i] - Bounds[i].Item1) * Weights[i]);
                    if (modBounds[i].Item1 == 0)
                    {
                        val += (int)(Bounds[i].Item1 * Weights[i]);
                    }

                    return val;
                }).Sum();
            } while (StatValues.GetBoundsSum(modBounds) < total && total > 0);

            // The item has a positive stat, so it always gets at least a point, even when its
            // stats are too small to be worth one at their weights (FF12's Str 1 accessories).
            total = Math.Max(total, 1);

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
                // Clamp to the range this stat rolled: a stat that did not roll negative
                // must not be pushed below zero by the jitter.
                int lower = modBounds[i].Item1 < 0
                    ? Bounds[i].Item1
                    : Math.Min(Math.Max(Bounds[i].Item1, 0), Bounds[i].Item2);
                Values[i] = Math.Max(Math.Min(Values[i], Bounds[i].Item2), lower);
            });
        } while (Values.Vals.Max() <= 0);
    }

    /// <summary>The lowest roll total: only stats that always roll a negative range get their offset.</summary>
    private int GetBasePoints(int[] actual)
    {
        return Enumerable.Range(0, Bounds.Length).Select(i =>
        {
            int val = (int)((actual[i] - Bounds[i].Item1) * Weights[i]);
            if (!AlwaysRollsNegative(i))
            {
                val += (int)(Bounds[i].Item1 * Weights[i]);
            }

            return val;
        }).Sum();
    }

    private bool AlwaysRollsNegative(int i)
    {
        return ZeroChances[i] <= 0 && NegChances[i] >= 100 && (int)(Bounds[i].Item1 * Weights[i]) != 0;
    }

    private void KeepVanilla(int[] actual)
    {
        Enumerable.Range(0, Bounds.Length).ToList().ForEach(i =>
        {
            Values[i] = Math.Max(Math.Min(actual[i], Bounds[i].Item2), Bounds[i].Item1);
        });
    }

    public int this[int i]
    {
        get => Values[i];
        set => Values[i] = value;
    }
}
