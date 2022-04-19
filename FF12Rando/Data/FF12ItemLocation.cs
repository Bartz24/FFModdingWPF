using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando
{
    public abstract class FF12ItemLocation : ItemLocation
    {
        public abstract int Difficulty { get; }
    }
}
