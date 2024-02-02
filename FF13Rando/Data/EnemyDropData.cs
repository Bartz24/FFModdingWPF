using Bartz24.FF13;
using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando;

public class EnemyDropData : FF13ItemLocation, IDataStoreItemProvider<DataStoreBtCharaSpec>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(1)]
    public int Index { get; set; }
    [RowIndex(2)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(4)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(5)]
    public override List<string> Traits { get; set; }
    [RowIndex(3)]
    public override List<string> Areas { get; set; }
    public override List<string> Characters { get; set; }
    [RowIndex(6)]
    public List<string> LinkedIDs { get; set; }
    [RowIndex(7)]
    public override int BaseDifficulty { get; set; }

    public EnemyDropData(SeedGenerator generator, string[] row) : base(generator, row)
    {
        Characters = FF13RandoHelpers.ParseReqCharas(row[8]);
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreBtCharaSpec s = GetItemData(false);
        if (Index == 0)
        {
            s.sDropItem0_string = newItem;
        }
        else
        {
            s.sDropItem1_string = newItem;
        }

        if (s.u8NumDrop > 0)
        {
            s.u8NumDrop = (byte)newCount;
        }

        // Set linked items
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        LinkedIDs.ForEach(other =>
        {
            DataStoreBtCharaSpec otherEnemy = enemyRando.btCharaSpec[other];
            otherEnemy.sDropItem0_string = s.sDropItem0_string;
            otherEnemy.sDropItem1_string = s.sDropItem1_string;
            otherEnemy.u8NumDrop = s.u8NumDrop;
        });
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreBtCharaSpec s = GetItemData(orig);
        return (Index == 0 ? s.sDropItem0_string : s.sDropItem1_string, s.u8NumDrop);
    }

    public DataStoreBtCharaSpec GetItemData(bool orig)
    {
        EnemyRando enemyRando = Generator.Get<EnemyRando>();
        return orig ? enemyRando.btCharaSpecOrig[ID] : enemyRando.btCharaSpec[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        throw new System.NotImplementedException();
    }
}
