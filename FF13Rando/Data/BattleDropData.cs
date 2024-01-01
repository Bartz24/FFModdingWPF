using Bartz24.FF13;
using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13Rando;

public class BattleDropData : FF13ItemLocation, DataStoreItemProvider<DataStoreBtScene>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(1)]
    public override string Name { get; set; }
    public override string LocationImagePath { get; set; }
    [RowIndex(3)]
    public override ItemReq Requirements { get; set; }
    [RowIndex(4)]
    public override List<string> Traits { get; set; }
    [RowIndex(2)]
    public override List<string> Areas { get; set; }
    public override List<string> Characters { get; set; }
    [RowIndex(5)]
    public override int BaseDifficulty { get; set; }

    public BattleDropData(SeedGenerator generator, string[] row) : base(generator, row)
    {
        Characters = FF13RandoHelpers.ParseReqCharas(row[6]);
    }

    public override bool IsValid(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreBtScene s = GetItemData(false);
        s.sDrop100Id_string = newItem;
        s.u8NumDrop100 = (byte)newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreBtScene s = GetItemData(orig);
        return (s.sDrop100Id_string, s.u8NumDrop100);
    }

    public DataStoreBtScene GetItemData(bool orig)
    {
        BattleRando battleRando = Generator.Get<BattleRando>();
        return orig ? battleRando.btsceneOrig[ID] : battleRando.btscene[ID];
    }
}
