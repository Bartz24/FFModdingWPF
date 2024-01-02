using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF12Rando;

public class TreasureLocation : FF12ItemLocation, DataStoreItemProvider<DataStoreTreasure>
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

    public TreasureLocation(SeedGenerator generator, string[] row, int index) : base(generator, row)
    {
        Name = row[0] + " - " + row[1] + " Treasure";
        ID = row[2] + ":" + index;
        Index = index;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return base.AreItemReqsMet(items) && Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreTreasure t = GetItemData(false);
        if (newItem == null)
        {
            t.GilCommon = 0;
            t.GilRare = 0;
            t.GilChance = 0;
            t.CommonItem1ID = 0xFFFF;
            t.CommonItem2ID = 0xFFFF;
            t.RareItem1ID = 0xFFFF;
            t.RareItem2ID = 0xFFFF;
        }
        else if (newItem == "Gil")
        {
            t.GilChance = 100;
            t.GilCommon = (ushort)Math.Min(newCount, 65535);
            t.GilRare = (ushort)Math.Min(newCount * 2, 65535);
        }
        else
        {
            string upgradeItem = newItem;
            EquipRando equipRando = Generator.Get<EquipRando>();
            if (equipRando.itemData.ContainsKey(newItem) && !string.IsNullOrEmpty(equipRando.itemData[newItem].Upgrade))
            {
                upgradeItem = equipRando.itemData[newItem].Upgrade;
            }

            ushort id = Convert.ToUInt16(newItem, 16);
            ushort upgradeID = Convert.ToUInt16(upgradeItem, 16);
            t.GilChance = 0;
            t.CommonItem1ID = id;
            t.CommonItem2ID = id;
            t.RareItem1ID = upgradeID;
            t.RareItem2ID = upgradeID;
        }
    }

    public override (string Item, int Amount)? GetItem(bool orig)
    {
        DataStoreTreasure t = GetItemData(orig);
        return t.GilChance == 100 ? ((string, int)?)("Gil", t.GilCommon) : (t.CommonItem1ID.ToString("X4"), 1);
    }

    public DataStoreTreasure GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.ebpAreasOrig[MapID].TreasureList[Index] : treasureRando.ebpAreas[MapID].TreasureList[Index];
    }

    public override bool CanReplace(ItemLocation location)
    {
        if (GetItemData(true).GilChance == 100)
        {
            // Gil can go in other rewards of index 0 or treasures
            return location is RewardLocation r && r.Index == 0 || location is TreasureLocation;
        }
        else
        {
            // Items can not go into rewards of index 0
            return location is not RewardLocation r || r.Index != 0;
        }
    }
}
