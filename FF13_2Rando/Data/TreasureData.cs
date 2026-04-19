using Bartz24.FF13_2;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF13_2Rando;

public class TreasureData : FF13_2ItemLocation, IDataStoreItemProvider<DataStoreRTreasurebox>
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

    // TODO: proper impl
    public override int BaseDifficulty { get => 1; set => throw new NotImplementedException(); }

    public TreasureData(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreRTreasurebox t = GetItemData(false);
        t.s11ItemResourceId = newItem;

        if (newItem.StartsWith("frg"))
        {
            if (Traits.Contains("Event"))
            {
                newCount = 1;
            }
            else if (Traits.Contains("ScrEvent"))
            {
                newCount = 0;
            }
        }
        t.iItemCount = newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreRTreasurebox t = GetItemData(orig);

        int count = t.iItemCount;
        if (t.s11ItemResourceId.StartsWith("frg"))
        {
            count = 1;
        }
        return (t.s11ItemResourceId, count);
    }

    public DataStoreRTreasurebox GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.treasuresOrig[ID] : treasureRando.treasures[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        if (location.Traits.Contains("Brain"))
        {
            // TODO
            return false;
        }

        if(!FF13_2Flags.Items.KeyPlaceTreasure.Enabled && 
            (
                location.Traits.Contains("Wild") || location.Traits.Contains("Graviton")||
                location.Traits.Contains("SideKey")|| location.Traits.Contains("GateSeal") ||
                location.Traits.Contains("Fragment") || location.Traits.Contains("Artefact")
            )
            )
        {
            return false;
        }

        if(!FF13_2Flags.Items.KeyPlaceParadox.Enabled && location.Traits.Contains("Paradox"))
        {
            return false;
        }

        return true;
    }

    public override string GetRequirementString()
    {
        return "Location: "+string.Join(",", Areas) +" - Extra Areas: " + string.Join(",", RequiredAreas) + " - Mog level: " + MogLevel + " - " + base.GetRequirementString();
    }
}
