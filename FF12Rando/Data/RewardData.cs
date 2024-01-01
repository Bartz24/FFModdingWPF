using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF12Rando;

public class RewardData : ItemLocation, DataStoreItemProvider<DataStoreReward>
{
    public override string ID { get; set; }
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
    [RowIndex(6)]
    public List<string> FakeItems { get; set; }

    public bool IsFakeOnly { get; set; }

    public RewardData Parent { get; set; }

    public RewardData(SeedGenerator generator, string[] row, int index, int fakeID, bool isAttachedFake = false) : base(generator, row)
    {
        if (isAttachedFake && !Traits.Contains("Fake"))
        {
            Traits.Add("Fake");
        }
        else if (!isAttachedFake && Traits.Contains("Fake"))
        {
            // Fake only rewards are explicitly defined as Fake, attached fakes are added automatically
            IsFakeOnly = true;
        }

        IntID = !Traits.Contains("Fake") ? Convert.ToInt32(row[2], 16) : fakeID;
        ID = (isAttachedFake ? "_" : "") + row[2] + ":" + index;
        Index = index;
        Parent = this;

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

    public override bool IsValid(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        if (Traits.Contains("Fake"))
        {
            return;
        }

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
        if (Traits.Contains("Fake"))
        {
            return null;
        }

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
}
