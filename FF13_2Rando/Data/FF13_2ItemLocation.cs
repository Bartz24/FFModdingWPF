using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando
{
    public abstract class FF13_2ItemLocation : ItemLocation
    {
        public abstract List<string> RequiredAreas { get; }
        public abstract int MogLevel { get; }
    }
}
