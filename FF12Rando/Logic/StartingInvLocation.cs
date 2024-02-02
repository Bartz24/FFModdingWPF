using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;

namespace FF12Rando;

public class StartingInvLocation : FF12ItemLocation, IDataStoreItemProvider<DataStorePartyMember>
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

    public StartingInvLocation(SeedGenerator generator, string[] row, int index) : base(generator, row)
    {
        ID = row[2] + "::" + index;
        Index = index;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return base.AreItemReqsMet(items) && Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStorePartyMember c = GetItemData(false);
        List<ushort> itemIDs = c.ItemIDs;
        List<byte> itemAmounts = c.ItemAmounts;

        if (newItem == null)
        {
            itemIDs[Index] = 0xFFFF;
            itemAmounts[Index] = 0;
        }
        else
        {
            ushort id = Convert.ToUInt16(newItem, 16);
            itemIDs[Index] = id;
            itemAmounts[Index] = (byte)newCount;
        }

        c.ItemIDs = itemIDs;
        c.ItemAmounts = itemAmounts;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStorePartyMember c = GetItemData(orig);
        if (c.ItemIDs[Index] == 0xFFFF)
        {
            return null;
        }
        else
        {
            return (c.ItemIDs[Index].ToString("X4"), c.ItemAmounts[Index]);
        }
    }

    public DataStorePartyMember GetItemData(bool orig)
    {
        PartyRando partyRando = Generator.Get<PartyRando>();
        return orig ? partyRando.partyOrig[IntID] : partyRando.party[IntID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        // Cannot be placed in rewards index 0
        return location is not RewardLocation reward || reward.Index != 0;
    }
}
