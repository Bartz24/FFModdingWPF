using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;
public class FF12FakeLocation : FakeLocation
{
    public FF12FakeLocation(SeedGenerator generator, string[] row, string fakeItem) : base(generator, row, fakeItem)
    {
    }

    public override (string Item, int Amount)? GetItem(bool orig)
    {
        var item = base.GetItem(orig);
        if (item == null)
        {
            return null;
        }

        return item;
    }

    public override List<ItemLocationReqComponent> GetComponents()
    {
        var list = base.GetComponents();
        list.Add(new CharReqComponent(BaseDifficulty));
        return list;
    }
}
