using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // If the item is a character, map to the correct one
        PartyRando partyRando = Generator.Get<PartyRando>();
        if (partyRando.CharacterMapping.Contains(item.Value.Item))
        {
            int index = partyRando.CharacterMapping.ToList().IndexOf(item.Value.Item);
            return (partyRando.CharacterMapping[partyRando.Characters[index]], item.Value.Amount);
        }

        return item;
    }
}
