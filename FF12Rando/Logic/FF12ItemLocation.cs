using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;
public abstract class FF12ItemLocation : ItemLocation
{
    protected FF12ItemLocation(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public override List<ItemLocationReqComponent> GetComponents()
    {
        var list = base.GetComponents();
        list.Add(new CharReqComponent(BaseDifficulty));
        return list;
    }
}
