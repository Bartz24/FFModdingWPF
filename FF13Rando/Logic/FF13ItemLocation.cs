using Bartz24.Data;
using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando;

public abstract class FF13ItemLocation : ItemLocation
{
    public FF13ItemLocation(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public abstract List<string> Characters { get; set; }
}
