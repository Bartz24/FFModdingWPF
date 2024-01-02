using Bartz24.FF13_2;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF13_2Rando;

public class SearchItemData : FF13_2ItemLocation, DataStoreItemProvider<DataStoreSearchItem>
{
    public override string ID { get; set; }
    [RowIndex(1)]
    public int Index { get; set; }
    [RowIndex(2)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    public override int MogLevel { get; set; }
    [RowIndex(5)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(6)]
    public override List<string> Traits { get; set; }
    [RowIndex(3)]
    public override List<string> Areas { get; set; }
    [RowIndex(4)]
    public override List<string> RequiredAreas { get; set; }

    public override int BaseDifficulty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public SearchItemData(SeedGenerator generator, string[] row) : base(generator, row)
    {
        ID = row[0] + ":" + row[1];
        MogLevel = 2;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        if (Traits.Contains("Event") && newItem.StartsWith("frg"))
        {
            newCount = 0;
        }

        LogSetItem(newItem, newCount);
        DataStoreSearchItem s = GetItemData(false);
        s.SetItem(Index, newItem);
        s.SetCount(Index, newCount);
        s.SetMax(Index, 1);
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreSearchItem s = GetItemData(orig);
        int count = s.GetCount(Index);
        int max = s.GetMax(Index);
        return (s.GetItem(Index), max == 0 ? count : count * max);
    }

    public DataStoreSearchItem GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        string searchID = ID.Substring(0, ID.IndexOf(":"));
        return orig ? treasureRando.searchOrig[searchID] : treasureRando.search[searchID];
    }
}