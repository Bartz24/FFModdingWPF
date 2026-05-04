using Bartz24.RandoWPF;
using System;
using System.Linq;

namespace FF12Rando;
public class APPartyRando : PartyRando
{
    public APPartyRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    protected override bool IsPartyRandomized()
    {
        return !RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().CharacterOrder.SequenceEqual(new int[] { 0, 1, 2, 3, 4, 5 });
    }

    protected override int[] GetShuffledPartyOrder()
    {
        return RandoFlags.GetArchipelagoData<FF12ArchipelagoData>().CharacterOrder.ToArray();
    }
}
