using Bartz24.LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;
public class EPReqComponent : ItemLocationReqComponent
{
    private SeedGenerator generator;

    public EPReqComponent(SeedGenerator generator) : base()
    {
        this.generator = generator;
    }

    public override bool AreItemReqsMet(ProgressionState state)
    {
        return HasEP(state);
    }

    public bool HasEP(ProgressionState state)
    {
        QuestRando questRando = generator.Get<QuestRando>();
        TreasureRando treasureRando = generator.Get<TreasureRando>();

        foreach (DataStoreRQuest quest in questRando.questRewards.Values.Where(q => q.iMaxGp > 0))
        {
            if (quest.record == "qst_027" && treasureRando.ItemLocations["tre_qst_027"].AreItemReqsMet(state)) // Peace and Quiet, Kupo
            {
                return true;
            }

            if (quest.record == "qst_028" && treasureRando.ItemLocations["tre_qst_028"].AreItemReqsMet(state)) // Saving an Angel
            {
                return true;
            }

            if (quest.record == "qst_046" && treasureRando.ItemLocations["tre_qst_046"].AreItemReqsMet(state)) // Adonis's Audition
            {
                return true;
            }

            if (quest.record == "qst_062" && treasureRando.ItemLocations["tre_qst_062"].AreItemReqsMet(state)) // Fighting Actress
            {
                return true;
            }

            if (quest.record == "qst_9000" && treasureRando.hintData["fl_mnlx_005e"].Requirements.IsValid(state)) // 1-5
            {
                return true;
            }

            if (quest.record == "qst_9010" && treasureRando.hintData["fl_mnyu_004e"].Requirements.IsValid(state)) // 2-3
            {
                return true;
            }

            if (quest.record == "qst_9020" && treasureRando.hintData["fl_mndd_005e"].Requirements.IsValid(state)) // 4-5
            {
                return true;
            }

            if (quest.record == "qst_9030" && treasureRando.hintData["fl_mnwl_003e"].Requirements.IsValid(state)) // 3-3
            {
                return true;
            }

            if (quest.record == "qst_9040" && treasureRando.ItemLocations["tre_qst_027_2"].Requirements.IsValid(state)) // Ereshkigal
            {
                return true;
            }

            if (quest.record == "qst_9050" && treasureRando.hintData["fl_mnsz_001e"].Requirements.IsValid(state))
            {
                return true;
            }
        }

        return false;
    }
}
