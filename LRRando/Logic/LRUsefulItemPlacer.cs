using Bartz24.RandoWPF;

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
