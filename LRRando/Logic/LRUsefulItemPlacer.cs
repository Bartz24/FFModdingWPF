using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRUsefulItemPlacer : UsefulItemPlacer<ItemLocation>
{
    public LRUsefulItemPlacer(SeedGenerator generator, bool logWarnings) : base(generator, logWarnings)
    {
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig)
    {
        return orig;
    }
}
