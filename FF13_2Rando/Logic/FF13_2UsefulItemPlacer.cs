using Bartz24.RandoWPF;

namespace FF13_2Rando.Logic;

public class FF13_2UsefulItemPlacer : UsefulItemPlacer<FF13_2ItemLocation>
{
    public FF13_2UsefulItemPlacer(SeedGenerator generator, bool logWarnings) : base(generator, logWarnings)
    {
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig)
    {
        return orig;
    }
}

