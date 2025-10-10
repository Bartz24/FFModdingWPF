using Bartz24.LR;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;

public class TreasureLocation : ItemLocation, IDataStoreItemProvider<DataStoreRTreasurebox>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(2)]
    public override string Name { get; set; }
    [RowIndex(6)]
    public override string LocationImagePath { get; set; }
    [RowIndex(5)]
    public override int BaseDifficulty { get; set; }
    [RowIndex(4)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(3)]
    public override List<string> Traits { get; set; }
    [RowIndex(1)]
    public override List<string> Areas { get; set; }

    private readonly TreasureRando rando;

    public TreasureLocation(SeedGenerator generator, string[] row, TreasureRando treasureRando) : base(generator, row)
    {
        rando = treasureRando;
    }

    public override List<ItemLocationReqComponent> GetComponents()
    {
        var list = base.GetComponents();
        if (Traits.Contains("EP"))
        {
            list.Add(new EPReqComponent(Generator));
        }
        return list;
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreRTreasurebox t = GetItemData(false);
        t.s11ItemResourceId = newItem;
        t.iItemCount = newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreRTreasurebox t = GetItemData(orig);
        return (t.s11ItemResourceId, t.iItemCount);
    }

    public DataStoreRTreasurebox GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.treasuresOrig[ID] : treasureRando.treasures[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        if (GetItemData(true).s11ItemResourceId == "")
        {
            // Gil can go in other treasures except for battle or trade rewards
            return location is TreasureLocation && !location.Traits.Contains("Battle") && !location.Traits.Contains("Trade");
        }
        else if (GetItemData(true).s11ItemResourceId.StartsWith("it"))
        {
            // Consumables cannot go in quest or battle rewards
            if (location.Traits.Contains("Quest") || location.Traits.Contains("Battle"))
            {
                return false;
            }
        }
        else if(GetItemData(true).s11ItemResourceId.StartsWith("ti") || GetItemData(true).s11ItemResourceId == "at900_00")
        {
            // EP abilities cannot go in CoP, quest, battle, or trade rewards
            if (location.Traits.Contains("CoP") || location.Traits.Contains("Quest") || location.Traits.Contains("Battle") || location.Traits.Contains("Trade"))
            {
                return false;
            }
        }

        if (GetItemData(true).iItemCount > 1)
        {
            // Multiples cannot go into trade rewards
            if (location.Traits.Contains("Trade"))
            {
                return false;
            }
        }

        return true;
    }
}
