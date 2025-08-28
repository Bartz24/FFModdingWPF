using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class FakeLocation : ItemLocation
{
    [RowIndex(0)]
    public override List<string> Areas { get; set; }
    [RowIndex(1)]
    public override string Name { get; set; }
    [RowIndex(2)]
    public override string ID { get; set; }
    [RowIndex(3)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(4)]
    public override List<string> Traits { get; set; }
    [RowIndex(5)]
    public override int BaseDifficulty { get; set; }
    public string FakeItem { get; set; }
    public int Amount { get; set; }
    public override string LocationImagePath { get; set; }

    public FakeLocation(SeedGenerator generator, string[] row, string fakeItem, int amount = 1) : base(generator, row)
    {
        if (!Traits.Contains("Fake"))
        {
            Traits.Add("Fake");
        }

        FakeItem = fakeItem;
        Amount = amount;
    }

    public override (string Item, int Amount)? GetItem(bool orig)
    {
        return (FakeItem, Amount);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        // Do nothing
    }

    public override bool CanReplace(ItemLocation location)
    {
        // Only if the same as this
        return this == location;
    }
}
