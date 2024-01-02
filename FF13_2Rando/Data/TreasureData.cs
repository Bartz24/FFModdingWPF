using Bartz24.FF13_2;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF13_2Rando;

public class TreasureData : FF13_2ItemLocation, DataStoreItemProvider<DataStoreRTreasurebox>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(1)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(3)]
    public override int MogLevel { get; set; }
    [RowIndex(5)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(6)]
    public override List<string> Traits { get; set; }
    [RowIndex(2)]
    public override List<string> Areas { get; set; }
    [RowIndex(4)]
    public override List<string> RequiredAreas { get; set; }

    public override int BaseDifficulty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public TreasureData(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreRTreasurebox t = GetItemData(false);
        t.s11ItemResourceId_string = newItem;
        t.iItemCount = newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreRTreasurebox t = GetItemData(orig);

        int count = t.iItemCount;
        if (Traits.Contains("Event") && t.s11ItemResourceId_string.StartsWith("frg"))
        {
            count = 1;
        }

        return (t.s11ItemResourceId_string, count);
    }

    public DataStoreRTreasurebox GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.treasuresOrig[ID] : treasureRando.treasures[ID];
    }
}
