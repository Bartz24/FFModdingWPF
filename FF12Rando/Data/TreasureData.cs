using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF12Rando;

public class TreasureData : ItemLocation, DataStoreItemProvider<DataStoreTreasure>
{
    public override string ID { get; set; }
    public int Index { get; set; }
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(1)]
    public string Subarea { get; set; }
    [RowIndex(2)]
    public string MapID { get; set; }
    [RowIndex(5)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(7)]
    public override List<string> Traits { get; set; }
    [RowIndex(0)]
    public override List<string> Areas { get; set; }
    [RowIndex(6)]
    public override int BaseDifficulty { get; set; }

    public TreasureData(SeedGenerator generator, string[] row, int index) : base(generator, row)
    {
        Name = row[0] + " - " + row[1] + " Treasure";
        ID = row[2] + ":" + index;
        Index = index;
    }

    public override bool IsValid(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreTreasure t = GetItemData(false);
        if (newItem == "Gil")
        {
            t.GilChance = 100;
            t.GilCommon = (ushort)Math.Min(newCount, 65535);
            t.GilRare = (ushort)Math.Min(newCount * 2, 65535);
        }
        else
        {
            ushort id = Convert.ToUInt16(newItem, 16);
            t.GilChance = 0;
            t.CommonItem1ID = id;
            t.CommonItem2ID = id;
            t.RareItem1ID = id;
            t.RareItem2ID = id;
        }
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreTreasure t = GetItemData(orig);
        return t.GilChance == 100 ? ((string, int)?)("Gil", t.GilCommon) : (t.CommonItem1ID.ToString("X4"), 1);
    }

    public DataStoreTreasure GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.ebpAreasOrig[MapID].TreasureList[Index] : treasureRando.ebpAreas[MapID].TreasureList[Index];
    }
}
