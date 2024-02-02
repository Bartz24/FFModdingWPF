using Bartz24.FF13;
using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando;

public class TreasureData : FF13ItemLocation, IDataStoreItemProvider<DataStoreTreasurebox>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(1)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(3)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(4)]
    public override List<string> Traits { get; set; }
    [RowIndex(2)]
    public override List<string> Areas { get; set; }
    public override List<string> Characters { get; set; }
    [RowIndex(5)]
    public override int BaseDifficulty { get; set; }

    public TreasureData(SeedGenerator generator, string[] row) : base(generator, row)
    {
        Characters = FF13RandoHelpers.ParseReqCharas(row[6]);
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreTreasurebox t = GetItemData(false);
        t.sItemResourceId_string = newItem;
        t.iItemCount = (uint)newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreTreasurebox t = GetItemData(orig);
        return (t.sItemResourceId_string, (int)t.iItemCount);
    }

    public DataStoreTreasurebox GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.treasuresOrig[ID] : treasureRando.treasures[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        throw new System.NotImplementedException();
    }
}
