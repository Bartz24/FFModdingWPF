using Bartz24.LR;
using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace LRRando.Logic;

public class BattleDropLocation : ItemLocation, IDataStoreItemProvider<DataStoreBtScene>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(2)]
    public override string Name { get; set; }
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

    public BattleDropLocation(SeedGenerator generator, string[] row, TreasureRando treasureRando) : base(generator, row)
    {
        Traits.Add("Battle");

        rando = treasureRando;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreBtScene b = GetItemData(false);
        b.sDropItem0_string = newItem;
        b.u16DropProb0 = 10000;
        b.u8NumDrop0 = newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreBtScene b = GetItemData(orig);
        return (b.sDropItem0_string, b.u8NumDrop0);
    }

    public DataStoreBtScene GetItemData(bool orig)
    {
        BattleRando battleRando = Generator.Get<BattleRando>();
        return orig ? battleRando.btScenesOrig[ID] : battleRando.btScenes[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        return false;
    }
}
