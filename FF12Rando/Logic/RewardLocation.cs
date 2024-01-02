using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF12Rando;

public class RewardLocation : FF12ItemLocation, DataStoreItemProvider<DataStoreReward>
{
    public override string ID { get; set; }
    [RowIndex(2), FieldTypeOverride(FieldType.HexInt)]
    public int IntID { get; set; }
    public int Index { get; set; }
    [RowIndex(1)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(3)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(5)]
    public override List<string> Traits { get; set; }
    [RowIndex(0)]
    public override List<string> Areas { get; set; }
    [RowIndex(4)]
    public override int BaseDifficulty { get; set; }

    public RewardLocation(SeedGenerator generator, string[] row, int index) : base(generator, row)
    {
        ID = row[2] + ":" + index;
        Index = index;

        if (Traits.Contains("WritTomaj"))
        {
            if (Index != 1)
            {
                Traits.Remove("WritTomaj");
            }
            else
            {
                Traits.Remove("MainKey");
            }
        }

        if (Traits.Contains("WritCid2") && Index != 1)
        {
            Traits.Remove("WritCid2");
        }
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return base.AreItemReqsMet(items) && Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreReward r = GetItemData(false);
        if (Index == 0)
        {
            if (newItem == null)
            {
                r.Gil = 0;
            }
            else
            {
                r.Gil = (uint)newCount;
            }
        }
        else
        {
            if (newItem == null || newItem == "FFFF")
            {
                if (Index == 1)
                {
                    r.Item1ID = 0xFFFF;
                    r.Item1Amount = 255;
                }
                else if (Index == 2)
                {
                    r.Item2ID = 0xFFFF;
                    r.Item2Amount = 255;
                }
            }
            else
            {
                ushort id = Convert.ToUInt16(newItem, 16);
                if (Index == 1)
                {
                    r.Item1ID = id;
                    r.Item1Amount = (ushort)newCount;
                }
                else if (Index == 2)
                {
                    r.Item2ID = id;
                    r.Item2Amount = (ushort)newCount;
                }
            }
        }
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreReward r = GetItemData(orig);
        return Index == 0
            ? r.Gil == 0 ? null : ("Gil", (int)r.Gil)
            : Index == 1
                ? r.Item1ID == 0xFFFF ? null : (r.Item1ID.ToString("X4"), r.Item1Amount)
                : r.Item2ID == 0xFFFF ? null : (r.Item2ID.ToString("X4"), r.Item2Amount);
    }

    public DataStoreReward GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.rewardsOrig[IntID - 0x9000] : treasureRando.rewards[IntID - 0x9000];
    }

    public override bool CanReplace(ItemLocation location)
    {
        if (Index == 0)
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
