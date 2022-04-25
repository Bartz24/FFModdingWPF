using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando
{
    public class FF13_2AssumedItemPlacementAlgorithm : AssumedItemPlacementAlgorithm<FF13_2ItemLocation>
    {
        TreasureRando treasureRando;
        HistoriaCruxRando cruxRando;

        Dictionary<string, int> AreaDepths = new Dictionary<string, int>();

        List<string> WildGatesOpenOrder = new List<string>();

        public FF13_2AssumedItemPlacementAlgorithm(Dictionary<string, FF13_2ItemLocation> itemLocations, List<string> hintsByLocations, RandomizerManager randomizers, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
        {
            treasureRando = randomizers.Get<TreasureRando>("Treasures");
            cruxRando = randomizers.Get<HistoriaCruxRando>("Historia Crux");
        }

        public override string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth)
        {
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]--);
            return null;
        }

        public override int GetNextDepth(Dictionary<string, int> items, string location)
        {
            return ItemLocations[location].Areas.Select(l => AreaDepths.ContainsKey(l) ? AreaDepths[l] : 0).Max();
        }

        public override bool IsHintable(string location)
        {
            if (!FF13_2Flags.Items.KeyWild.Enabled && IsWildArtefactKeyItem(location))
                return false;
            if (!FF13_2Flags.Items.KeyGraviton.Enabled && IsGravitonKeyItem(location))
                return false;
            if (!FF13_2Flags.Items.KeySide.Enabled && IsSideKeyItem(location))
                return false;
            if (!FF13_2Flags.Items.KeyGateSeal.Enabled && IsGateSealKeyItem(location))
                return false;
            if (IsImportantKeyItem(location))
                return true;
            return false;
        }

        public override bool IsValid(string location, string replacement, string area, Dictionary<string, int> items, List<string> areasAvailable)
        {
            return ItemLocations[location].IsValid(items) &&
                (area == null || ItemLocations[location].Areas.Contains(area)) &&
                ItemLocations[location].RequiredAreas.Intersect(areasAvailable).Count() == ItemLocations[location].RequiredAreas.Count &&
                IsAllowed(location, replacement) &&
                cruxRando.GetMogLevel(areasAvailable) >= ItemLocations[location].MogLevel;
        }

        public override void RemoveHint(string hint, string location)
        {
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]++);
        }

        public override bool RequiresDepthLogic(string location)
        {
            return IsImportantKeyItem(location);
        }

        public override bool RequiresLogic(string location)
        {
            if (IsImportantKeyItem(location))
                return true;
            if (GetLocationItem(location).Item1.StartsWith("frg"))
                return true;
            return false;
        }

        public override Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    Tuple<string, int> tuple = t.GetData(orig ? treasureRando.treasuresOrig[key] : treasureRando.treasures[key]);
                    if (ItemLocations[key].Traits.Contains("Event") && tuple.Item1.StartsWith("frg"))
                        return new Tuple<string, int>(tuple.Item1, 1);

                    return tuple;
                case TreasureRando.SearchItemData s:
                    string id = key.Substring(0, key.IndexOf(":"));
                    return s.GetData(orig ? treasureRando.searchOrig[id] : treasureRando.search[id]);
                default:
                    return base.GetLocationItem(key, orig);
            }
        }

        public override void SetLocationItem(string key, string item, int count)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    if (ItemLocations[key].Traits.Contains("Event") && item.StartsWith("frg"))
                        count = 0;
                    t.SetData(treasureRando.treasures[key], item, count);
                    break;
                case TreasureRando.SearchItemData s:
                    string id = key.Substring(0, key.IndexOf(":"));
                    s.SetData(treasureRando.search[id], item, count);
                    break;
                default:
                    base.SetLocationItem(key, item, count);
                    break;
            }
        }

        public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            List<string> list = new List<string>();
            list.Add("start");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 0)
                list.Add("h_hm_AD0003");
            if (FF13_2Flags.Other.ForcedStart.Values.IndexOf(FF13_2Flags.Other.ForcedStart.SelectedValue) > 1)
                list.Add("h_bj_AD0005");
            int oldCount = -1;
            list.AddRange(soFar);
            list = list.Distinct().ToList();

            while (list.Count != oldCount)
            {
                oldCount = list.Count;
                int wildsNeeded = cruxRando.GetWildsNeeded(list);
                int gravitonsHeld = items.Keys.Where(i => i.StartsWith("frg_cmn_gvtn")).Select(i => items[i]).Sum();

                HistoriaCruxRando.GateData g = cruxRando.gateData.Values.Where(g =>
                {
                    if (!list.Contains(g.Location))
                        return false;
                    string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                    if (!cruxRando.placement.ContainsKey(nextLocation))
                        return false;
                    if (g.Traits.Contains("Wild"))
                    {
                        if (WildGatesOpenOrder.Contains(g.ID))
                            wildsNeeded = Math.Max(WildGatesOpenOrder.IndexOf(g.ID) + 1, wildsNeeded);
                        if ((items.ContainsKey("opt_silver") ? items["opt_silver"] : 0) < wildsNeeded)
                            return false;
                    }
                    if (g.Traits.Contains("Graviton") && gravitonsHeld < 5)
                        return false;
                    if (!g.ItemRequirements.IsValid(items))
                        return false;
                    if (list.Intersect(g.Requirements).Count() != g.Requirements.Count)
                        return false;
                    return true;
                }).Where(g =>
                {
                    string nextLocation = cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Substring(0, cruxRando.gateTableOrig[g.ID].sOpenHistoria1_string.Length - 2);
                    return !list.Contains(cruxRando.placement[nextLocation]);
                }).ToList().Shuffle().FirstOrDefault();

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
                    list.Add("h_sp_NA0001");

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
                    list.Add("h_cs_NA0000");

                int prevMaxDepth = AreaDepths.Count == 0 ? 0 : AreaDepths.Values.Max();

                list.Where(l => !AreaDepths.ContainsKey(l)).ForEach(l => AreaDepths.Add(l, prevMaxDepth + 1));
            }

            return list;
        }

        public override bool IsAllowed(string old, string rep, bool orig = true)
        {
            if (!FF13_2Flags.Items.KeyWild.Enabled && (IsWildArtefactKeyItem(rep) || IsWildArtefactKeyItem(old)))
                return old == rep;
            if (!FF13_2Flags.Items.KeyGraviton.Enabled && (IsGravitonKeyItem(rep) || IsGravitonKeyItem(old)))
                return old == rep;
            if (!FF13_2Flags.Items.KeySide.Enabled && (IsSideKeyItem(rep) || IsSideKeyItem(old)))
                return old == rep;
            if (!FF13_2Flags.Items.KeyGateSeal.Enabled && (IsGateSealKeyItem(rep) || IsGateSealKeyItem(old)))
                return old == rep;
            if (ItemLocations[old].Traits.Contains("Brain"))
            {
                if (IsImportantKeyItem(rep) && !IsImportantKeyItem(old) && !FF13_2Flags.Items.KeyPlaceBrainBlast.Enabled)
                    return false;
            }
            else
            {
                if (IsImportantKeyItem(rep) && !IsImportantKeyItem(old) && !FF13_2Flags.Items.KeyPlaceTreasure.Enabled)
                    return false;
            }
            if (ItemLocations[old] is TreasureRando.SearchItemData)
            {
                if (IsImportantKeyItem(rep) && GetLocationItem(old).Item1.StartsWith("mcr") && !FF13_2Flags.Items.KeyPlaceThrowCryst.Enabled)
                    return false;
                if (IsImportantKeyItem(rep) && !GetLocationItem(old).Item1.StartsWith("mcr") && !FF13_2Flags.Items.KeyPlaceThrowJunk.Enabled)
                    return false;
                if (GetLocationItem(rep).Item1.StartsWith("frg"))
                    return false;
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


        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (FF13_2Flags.Items.KeyDepth.SelectedValue == FF13_2Flags.Items.KeyDepth.Values[FF13_2Flags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                int index = FF13_2Flags.Items.KeyDepth.Values.IndexOf(FF13_2Flags.Items.KeyDepth.SelectedValue);
                float expBase = 1;
                if (index == 0)
                    expBase = 1;
                if (index == 1)
                    expBase = 1.1f;
                if (index == 2)
                    expBase = 1.2f;
                if (index == 3)
                    expBase = 1.5f;
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)Math.Pow(expBase, possDepths[s]));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }

        public override void Clear()
        {

        }
    }
}
