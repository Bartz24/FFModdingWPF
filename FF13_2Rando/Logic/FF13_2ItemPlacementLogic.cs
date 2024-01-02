using Bartz24.Data;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace FF13_2Rando;

public class FF13_2ItemPlacementLogic : ItemPlacementLogic<FF13_2ItemLocation>
{
    protected TreasureRando treasureRando;
    protected HistoriaCruxRando cruxRando;

    protected Dictionary<string, int> AreaDepths = new();
    public List<string> AreaUnlockOrder { get; } = new List<string>();

    public FF13_2ItemPlacementLogic(ItemPlacementAlgorithm<FF13_2ItemLocation> algorithm, SeedGenerator randomizers) : base(algorithm)
    {
        treasureRando = randomizers.Get<TreasureRando>();
        cruxRando = randomizers.Get<HistoriaCruxRando>();
    }

    public override string AddHint(string location, string replacement, int itemDepth)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]--);
        return null;
    }

    public override int GetNextDepth(Dictionary<string, int> items, string location)
    {
        return ItemLocations[location].Areas.Select(l => AreaDepths.ContainsKey(l) ? AreaDepths[l] : 0).Max();
    }

    public override bool IsHintable(string location)
    {
        if (!FF13_2Flags.Items.KeyWild.Enabled && IsWildArtefactKeyItem(location))
        {
            return false;
        }

        return (FF13_2Flags.Items.KeyGraviton.Enabled || !IsGravitonKeyItem(location))
&& (FF13_2Flags.Items.KeySide.Enabled || !IsSideKeyItem(location))
&& (FF13_2Flags.Items.KeyGateSeal.Enabled || !IsGateSealKeyItem(location)) && IsImportantKeyItem(location);
    }

    public override bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable)
    {
        return ItemLocations[location].AreItemReqsMet(items) &&
            ItemLocations[location].Areas.Intersect(areasAvailable).Count() > 0 &&
            ItemLocations[location].RequiredAreas.Intersect(areasAvailable).Count() == ItemLocations[location].RequiredAreas.Count &&
            IsAllowed(location, replacement) &&
            cruxRando.GetMogLevel(areasAvailable) >= ItemLocations[location].MogLevel;
    }

    public override void RemoveHint(string hint, string location)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]++);
    }

    public override bool RequiresDepthLogic(string location)
    {
        return IsImportantKeyItem(location);
    }

    public override bool RequiresLogic(string location)
    {
        return IsImportantKeyItem(location) || ItemLocations[location].GetItem(true).Value.Item1.StartsWith("frg");
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

                return (!g.Traits.Contains("Wild") || (items.ContainsKey("opt_silver") ? items["opt_silver"] : 0) >= wildsNeeded)
&& (!g.Traits.Contains("Graviton") || gravitonsHeld >= 5)
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

        AreaUnlockOrder.AddRange(list.Where(a => !AreaUnlockOrder.Contains(a)));
        return list;
    }

    protected override bool IsAllowedReplacement(string old, string rep)
    {
        if (ItemLocations[old].Traits.Contains("Fixed") || ItemLocations[rep].Traits.Contains("Fixed"))
        {
            return old == rep;
        }

        if (!FF13_2Flags.Items.KeyWild.Enabled && (IsWildArtefactKeyItem(rep) || IsWildArtefactKeyItem(old)))
        {
            return old == rep;
        }

        if (!FF13_2Flags.Items.KeyGraviton.Enabled && (IsGravitonKeyItem(rep) || IsGravitonKeyItem(old)))
        {
            return old == rep;
        }

        if (!FF13_2Flags.Items.KeySide.Enabled && (IsSideKeyItem(rep) || IsSideKeyItem(old)))
        {
            return old == rep;
        }

        if (!FF13_2Flags.Items.KeyGateSeal.Enabled && (IsGateSealKeyItem(rep) || IsGateSealKeyItem(old)))
        {
            return old == rep;
        }

        List<string> specialTraits = new();
        if (ItemLocations[old].Traits.Contains("Brain"))
        {
            specialTraits.Add("Brain");
            if (IsImportantKeyItem(rep) && !IsImportantKeyItem(old) && !FF13_2Flags.Items.KeyPlaceBrainBlast.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old] is SearchItemData)
        {
            if (IsImportantKeyItem(rep) && ItemLocations[old].GetItem(true).Value.Item1.StartsWith("mcr") && !FF13_2Flags.Items.KeyPlaceThrowCryst.Enabled)
            {
                return false;
            }

            if (IsImportantKeyItem(rep) && !ItemLocations[old].GetItem(true).Value.Item1.StartsWith("mcr") && !FF13_2Flags.Items.KeyPlaceThrowJunk.Enabled)
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1.StartsWith("frg"))
            {
                return false;
            }
        }
        else
        {
            if (IsImportantKeyItem(rep) && !IsImportantKeyItem(old) && !FF13_2Flags.Items.KeyPlaceTreasure.Enabled && specialTraits.Count == 0)
            {
                return false;
            }

            if (!IsImportantKeyItem(rep) && IsImportantKeyItem(old) && !FF13_2Flags.Items.KeyPlaceTreasure.Enabled && specialTraits.Count == 0)
            {
                return false;
            }
        }

        return true;
    }
    private bool IsImportantKeyItem(string location)
    {
        return IsWildArtefactKeyItem(location) || IsGravitonKeyItem(location) || IsSideKeyItem(location) || IsGateSealKeyItem(location);
    }

    private bool IsWildArtefactKeyItem(string location)
    {
        return ItemLocations[location].Traits.Contains("Wild");
    }

    private bool IsGravitonKeyItem(string location)
    {
        return ItemLocations[location].Traits.Contains("Graviton");
    }

    private bool IsSideKeyItem(string location)
    {
        return ItemLocations[location].Traits.Contains("SideKey");
    }

    private bool IsGateSealKeyItem(string location)
    {
        return ItemLocations[location].Traits.Contains("GateSeal");
    }

    public override int GetPlacementDifficultyIndex()
    {
        return FF13_2Flags.Items.KeyDepth.SelectedIndex;
    }

    public override void Clear()
    {

    }
}
