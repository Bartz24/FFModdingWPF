using System;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

public class StatRandomizer<T> where T : Enum
{
    private Dictionary<T, StatDef<T>> Stats { get; set; } = new Dictionary<T, StatDef<T>>();

    public StatDef<T> this[T stat]
    {
        get => Stats[stat];
        set => Stats[stat] = value;
    }

    public StatRandomizer()
    {

    }

    public void Randomize()
    {
        foreach (StatDef<T> stat in Stats.Values)
        {
            stat.RandomizeFunc();
        }
    }
}
