using System;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

public class RandomizerManager : List<Randomizer>
{
    public Action<string, int, int> SetUIProgress { get; set; }
    public T Get<T>() where T : Randomizer
    {
        foreach (Randomizer randomizer in this)
        {
            if (randomizer.GetType() == typeof(T))
            {
                return (T)randomizer;
            }
        }

        return null;
    }
}
