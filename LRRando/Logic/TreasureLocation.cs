using Bartz24.LR;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace LRRando.Logic;

public class TreasureLocation : ItemLocation, IDataStoreItemProvider<DataStoreRTreasurebox>
{
    [RowIndex(0)]
    public override string ID { get; set; }
    [RowIndex(2)]
    public override string Name { get; set; }
    [RowIndex(6)]
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

    public TreasureLocation(SeedGenerator generator, string[] row, TreasureRando treasureRando) : base(generator, row)
    {
        rando = treasureRando;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return (!Traits.Contains("EP") || HasEP(items)) && Requirements.IsValid(items);
    }

    public bool HasEP(Dictionary<string, int> items)
    {
        QuestRando questRando = rando.Generator.Get<QuestRando>();

        foreach (DataStoreRQuest quest in questRando.questRewards.Values.Where(q => q.iMaxGp > 0))
        {
            if (quest.name == "qst_027" && rando.ItemLocations["tre_qst_027"].AreItemReqsMet(items)) // Peace and Quiet, Kupo
            {
                return true;
            }

            if (quest.name == "qst_028" && rando.ItemLocations["tre_qst_028"].AreItemReqsMet(items)) // Saving an Angel
            {
                return true;
            }

            if (quest.name == "qst_046" && rando.ItemLocations["tre_qst_046"].AreItemReqsMet(items)) // Adonis's Audition
            {
                return true;
            }

            if (quest.name == "qst_062" && rando.ItemLocations["tre_qst_062"].AreItemReqsMet(items)) // Fighting Actress
            {
                return true;
            }

            if (quest.name == "qst_9000" && rando.hintData["fl_mnlx_005e"].Requirements.IsValid(items)) // 1-5
            {
                return true;
            }

            if (quest.name == "qst_9010" && rando.hintData["fl_mnyu_004e"].Requirements.IsValid(items)) // 2-3
            {
                return true;
            }

            if (quest.name == "qst_9020" && rando.hintData["fl_mndd_005e"].Requirements.IsValid(items)) // 4-5
            {
                return true;
            }

            if (quest.name == "qst_9030" && rando.hintData["fl_mnwl_003e"].Requirements.IsValid(items)) // 3-3
            {
                return true;
            }

            if (quest.name == "qst_9040" && rando.ItemLocations["tre_qst_027_2"].Requirements.IsValid(items)) // Ereshkigal
            {
                return true;
            }

            if (quest.name == "qst_9050" && rando.hintData["fl_mnsz_001e"].Requirements.IsValid(items))
            {
                return true;
            }
        }

        return false;
    }

    public override void SetItem(string newItem, int newCount)
    {
        LogSetItem(newItem, newCount);
        DataStoreRTreasurebox t = GetItemData(false);
        t.s11ItemResourceId_string = newItem;
        t.iItemCount = newCount;
    }

    public override (string, int)? GetItem(bool orig)
    {
        DataStoreRTreasurebox t = GetItemData(orig);
        return (t.s11ItemResourceId_string, t.iItemCount);
    }

    public DataStoreRTreasurebox GetItemData(bool orig)
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        return orig ? treasureRando.treasuresOrig[ID] : treasureRando.treasures[ID];
    }

    public override bool CanReplace(ItemLocation location)
    {
        return false;
    }
}
