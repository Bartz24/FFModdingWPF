using System;
using System.Linq;
using System.Numerics;

namespace Bartz24.RandoWPF;

public class StatDef<T> where T : Enum
{
    public T Type { get; init; }

    public int MinValue { get; init; }
    public int MaxValue { get; init; }

    private double _multiplier = 1.0;
    public double Multiplier
    {
        get => _multiplier;
        set => _multiplier = Math.Clamp(value, MinMultiplier, MaxMultiplier);
    }
    public double MaxMultiplier { get; init; } = 100000;
    public double MinMultiplier { get; init; } = 0;

    /// <summary>
    /// /// input2: float to determine how much to randomize. normally 1
    /// output: new mult value
    /// </summary>
    public Action<double> RandomizeFunc { get; set; }

    /// <summary>
    /// Higher = randomized earlier
    /// </summary>
    public int RandomizeOrderPriority { get; init; } = 0;
    public bool RandomizeDirectly { get; init; } = true;

    public int ApplyMult(int original)
    {
        return (int)Math.Clamp(Math.Round(original * Multiplier), MinValue, MaxValue);
    }

    /// <summary>
    /// Less extreme version which applies sqrts to reduce effect of multiple applications
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public int ApplyMultControlled(int original)
    {
        return (int)Math.Round(original * (Multiplier > 1 ? Math.Sqrt(Math.Sqrt(Multiplier)) : Math.Sqrt(Multiplier)));
    }
}
