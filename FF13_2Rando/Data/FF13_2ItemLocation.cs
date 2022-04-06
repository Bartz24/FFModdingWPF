using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando
{
    public abstract class FF13_2ItemLocation : ItemLocation
    {
        public abstract List<string> RequiredAreas { get; }
        public abstract int MogLevel { get; }
    }
}
