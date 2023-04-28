using System;
using System.Linq;

namespace Bartz24.RandoWPF;

public class StatValues
{
    public int[] Vals { get; set; }

    public StatValues(int count)
    {
        Vals = new int[count];
    }

    public void Randomize(int variance, float rate = 0.2f)
    {
        Randomize(GetVarianceBounds(variance), variance * Vals.Length, rate);
    }

    public void Randomize((int, int)[] bounds, long amount, float rate = 0.2f)
    {
        int randTotal = (int)Math.Min(Math.Min(amount, GetBoundsSum(bounds)), int.MaxValue);
        while (Vals.Sum() < randTotal)
        {
            int select = SelectNext();
            int val = (int)Math.Max((randTotal - Vals.Sum()) * BoundMult(bounds, select) * rate, 1);
            Vals[select] += Math.Min(bounds[select].Item2 - bounds[select].Item1 - Vals[select], val);
        }

        for (int i = 0; i < Vals.Length; i++)
        {
            Vals[i] += bounds[i].Item1;
        }
    }

    public static long GetBoundsSum((int, int)[] bounds)
    {
        return bounds.Select(t => t.Item2 - (long)t.Item1).Sum();
    }

    protected virtual int SelectNext()
    {
        return RandomNum.RandInt(0, Vals.Length - 1);
    }

    private float BoundMult((int, int)[] bounds, int select)
    {
        return (bounds[select].Item2 - bounds[select].Item1) / bounds.Select(t => (float)(t.Item2 - t.Item1)).Sum();
    }

    public int this[int i]
    {
        get => Vals[i];
        set => Vals[i] = value;
    }

    public (int, int)[] GetVarianceBounds(int variance)
    {
        return Enumerable.Range(0, Vals.Length).Select(i => (100 - variance, int.MaxValue)).ToArray();
    }
    public long GetTotalPoints((int, int)[] bounds)
    {
        return bounds.Select(b => ((b.Item1 + b.Item2) / 2L) - b.Item1).Sum();
    }
}
