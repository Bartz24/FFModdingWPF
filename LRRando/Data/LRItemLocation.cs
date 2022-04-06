using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public abstract class LRItemLocation : ItemLocation
    {
        public abstract int Difficulty { get; }
    }
}
