using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public abstract class FF13ItemLocation : ItemLocation
    {
        public abstract List<string> Characters { get; }
    }
}
