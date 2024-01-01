using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public abstract class FF13_2ItemLocation : ItemLocation
{
    public FF13_2ItemLocation(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public abstract List<string> RequiredAreas { get; set; }
    public abstract int MogLevel { get; set; }
}
