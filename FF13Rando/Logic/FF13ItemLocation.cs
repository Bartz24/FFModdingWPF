using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando
{
    public abstract class FF13ItemLocation : ItemLocation
    {
        public abstract List<string> Characters { get; }
    }
}
