using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13_2Rando;

public class FF13_2AssumedItemPlacementLogic : FF13_2ItemPlacementLogic
{
    private readonly List<string> WildGatesOpenOrder = new();
    private readonly List<string> AreaStart = new();
    private readonly List<string> AreaRemoveOrder = new();
    public new List<string> AreaUnlockOrder
    {
        get
        {
            List<string> list = new(AreaRemoveOrder);
            List<string> starting = AreaStart.Where(a => !list.Contains(a)).ToList();
            starting = starting.OrderByDescending(a =>
            {
                return cruxRando.GetIDsForOpening(a, false).Select(g => cruxRando.gateData[g]).Where(g => g.Location == "start").Count() > 0 ? 0 : 1;
            }).ToList();
            list.AddRange(starting);
            list.Reverse();
            return list;
        }
    }
    public FF13_2AssumedItemPlacementLogic(ItemPlacementAlgorithm<FF13_2ItemLocation> algorithm, SeedGenerator randomizers) : base(algorithm, randomizers)
    {
    }

    public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
    {
        List<string> list = new()
        {
            "start"
        };
        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
        {
            list.Add("h_hm_AD0003");
        }

        if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
        {
            list.Add("h_bj_AD0005");
        }

        int oldCount = -1;
        list.AddRange(soFar);
        list = list.Distinct().ToList();

        while (list.Count != oldCount)
        {
            oldCount = list.Count;
            int wildsNeeded = cruxRando.GetWildsNeeded(list);
            int gravitonsHeld = items.Keys.Where(i => i.StartsWith("frg_cmn_gvtn")).Select(i => items[i]).Sum();

            GateData g = cruxRando.gateData.Values.Where(g =>
            {
                if (!list.Contains(g.Location))
                {
                    return false;
                }

                string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                if (!cruxRando.placement.ContainsKey(nextLocation))
                {
                    return false;
                }

                if (g.Traits.Contains("Wild"))
                {
                    if (WildGatesOpenOrder.Contains(g.ID))
                    {
                        wildsNeeded = Math.Max(WildGatesOpenOrder.IndexOf(g.ID) + 1, wildsNeeded);
                    }

                    if ((items.ContainsKey("opt_silver") ? items["opt_silver"] : 0) < wildsNeeded)
                    {
                        return false;
                    }
                }

                return (!g.Traits.Contains("Graviton") || gravitonsHeld >= 5)
&& g.ItemRequirements.IsValid(items) && list.Intersect(g.Requirements).Count() == g.Requirements.Count;
            }).Where(g =>
            {
                string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                return !list.Contains(cruxRando.placement[nextLocation]);
            }).Shuffle().FirstOrDefault();

            if (g != null)
            {
                string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                if (!list.Contains(cruxRando.placement[nextLocation]))
                {
                    list.Add(cruxRando.placement[nextLocation]);
                    if (g.Traits.Contains("Wild") && !WildGatesOpenOrder.Contains(g.ID))
                    {
                        WildGatesOpenOrder.Add(g.ID);
                    }
                }
            }

            // Unlock Void after Ch 2
            if (list.Contains("h_gh_AD0010") && list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && !list.Contains("h_sp_NA0001"))
            {
                list.Add("h_sp_NA0001");
            }

            int gravitons = items.Keys.Where(i => i.StartsWith("frg_cmn_gvtn")).Select(i => items[i]).Sum();
            // Unlock Dying World/Bodhum 700 after Academia 4XX and Graviton
            if (list.Contains("h_aa_AD0400") && gravitons >= 5 && !list.Contains("h_dd_AD0700") && !list.Contains("h_hm_AD0700") && !list.Contains("h_zz_NA0950"))
            {
                list.Add("h_dd_AD0700");
                list.Add("h_hm_AD0700");
                list.Add("h_zz_NA0950");
            }

            // Unlock Serendipity after Yaschas 1X and Sunleth 300
            if (list.Contains("h_sn_AD0300") && list.Contains("h_gd_NA0000") && list.Contains("h_gh_AD0010") && list.Contains("h_cl_NA0000") && !list.Contains("h_cs_NA0000"))
            {
                list.Add("h_cs_NA0000");
            }

            int prevMaxDepth = AreaDepths.Count == 0 ? 0 : AreaDepths.Values.Max();

            list.Where(l => !AreaDepths.ContainsKey(l)).ForEach(l => AreaDepths.Add(l, prevMaxDepth + 1));
        }

        if (AreaStart.Count == 0)
        {
            AreaStart.AddRange(list);
        }
        else if (AreaStart.Where(a => !list.Contains(a) && !AreaRemoveOrder.Contains(a)).Count() > 0)
        {
            AreaRemoveOrder.AddRange(AreaStart.Where(a => !list.Contains(a) && !AreaRemoveOrder.Contains(a)));
        }

        return list;
    }
}
