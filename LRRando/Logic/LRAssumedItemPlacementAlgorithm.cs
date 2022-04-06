using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRRando.TreasureRando;

namespace LRRando
{
    public class LRAssumedItemPlacementAlgorithm : AssumedItemPlacementAlgorithm<LRItemLocation>
    {
        TreasureRando treasureRando;

        Dictionary<string, int> AreaDepths = new Dictionary<string, int>();

        public LRAssumedItemPlacementAlgorithm(Dictionary<string, LRItemLocation> itemLocations, List<string> hintsByLocations, RandomizerManager randomizers) : base(itemLocations, hintsByLocations)
        {
            treasureRando = randomizers.Get<TreasureRando>("Treasures");
        }

        public override string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth)
        {
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]--);

            if (IsHintable(replacement))
            {
                List<HintData> possible = treasureRando.hintData.Values.Where(h =>
                {
                    if (!h.Requirements.IsValid(GetItemsAvailable()))
                        return false;
                    if (h.Requirements.GetPossibleRequirements().Contains(replacement))
                        return false;
                    return true;
                }).ToList().Shuffle().OrderByDescending(h =>
                {
                    if (LRFlags.Other.HintsDepth.Enabled)
                    {
                        int hintDepth = GetReqsMaxDepth(h.Requirements);
                        if (hintDepth > itemDepth)
                            return false;
                    }
                    return true;
                }).ThenBy(h => treasureRando.hintsMain[h.ID].Count).ToList();

                string next = possible.First().ID;
                treasureRando.hintsMain[next].Add(location);
                return next;
            }
            return null;
        }
        private int GetReqsMaxDepth(ItemReq req)
        {
            return req.GetPossibleRequirements().Select(item =>
            {
                return Placement.Keys.Where(t => GetLocationItem(Placement[t]).Item1 == item).Select(t => Depths[t]).DefaultIfEmpty(0).Max();
            }).DefaultIfEmpty(0).Max();
        }

        public override int GetNextDepth(Dictionary<string, int> items, string location)
        {
            int minItems = 8;
            int keyItemsFound = Placement.Where(p => IsKeyItem(p.Value)).Count();
            // Early day/easier checks have higher "depths" as minItems is low to start chains
            float diffModifier = Math.Min(minItems, keyItemsFound) / (float)minItems;
            int maxDifficulty = ItemLocations.Values.Select(t => t.Difficulty).Max();
            int val = (int)(diffModifier * ItemLocations[location].Difficulty + (1 - diffModifier) * (maxDifficulty - ItemLocations[location].Difficulty));

            if (ItemLocations[location].Traits.Contains("CoP"))
                val = (int)Math.Round(Math.Pow(val, 0.75f)) / 2;
            return RandomNum.RandInt(Math.Max(0, val - 2), val + 2);
        }

        public override bool IsHintable(string location)
        {
            if (IsKeyItem(location) && !IsPilgrimKeyItem(location))
                return true;
            if (IsPilgrimKeyItem(location) && LRFlags.Other.HintsPilgrim.FlagEnabled)
                return true;
            if (LRFlags.Other.HintsEP.FlagEnabled && (GetLocationItem(location).Item1.StartsWith("ti") || GetLocationItem(location).Item1 == "at900_00"))
                return true;
            return false;
        }

        public override bool IsValid(string location, string replacement, string area, Dictionary<string, int> items, List<string> areasAvailable)
        {
            return ItemLocations[location].IsValid(items) &&
                (area == null || ItemLocations[location].Areas.Contains(area)) &&
                IsAllowed(location, replacement);
        }

        public override void RemoveHint(string hint, string location)
        {
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]++);
        }

        public override bool RequiresDepthLogic(string location)
        {
            return IsKeyItem(location) || IsEPAbility(location);
        }

        public override bool RequiresLogic(string location)
        {
            if (ItemLocations[location].Traits.Contains("Same"))
                return true;
            if (IsKeyItem(location))
                return true;
            if (GetLocationItem(location).Item1.StartsWith("libra"))
                return true;
            if (IsEPAbility(location))
                return true;
            if (GetLocationItem(location).Item1.StartsWith("it"))
                return true;
            if (GetLocationItem(location).Item1 == "")
                return true;
            if (GetLocationItem(location).Item2 > 1)
                return true;
            return false;
        }

        public override Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    return t.GetData(orig ? treasureRando.treasuresOrig[key] : treasureRando.treasures[key]);
                case TreasureRando.BattleDropData t:
                    BattleRando battleRando = treasureRando.Randomizers.Get<BattleRando>("Battles");
                    return t.GetData(orig ? battleRando.btScenesOrig[key] : battleRando.btScenes[key]);
                default:
                    return base.GetLocationItem(key, orig);
            }
        }

        public override void SetLocationItem(string key, string item, int count)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    t.SetData(treasureRando.treasures[key], item, count);
                    break;
                case TreasureRando.BattleDropData t:
                    BattleRando battleRando = treasureRando.Randomizers.Get<BattleRando>("Battles");
                    t.SetData(battleRando.btScenes[key], item, count);
                    break;
                default:
                    base.SetLocationItem(key, item, count);
                    break;
            }
        }

        public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            return ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList();
        }

        private bool IsAllowed(string old, string rep)
        {
            if (!LRFlags.Items.Pilgrims.Enabled && (IsPilgrimKeyItem(rep) || IsPilgrimKeyItem(old)))
                return old == rep;
            if (!LRFlags.Items.KeyMain.Enabled && (IsMainKeyItem(rep) || IsMainKeyItem(old)))
                return old == rep;
            if (!LRFlags.Items.KeySide.Enabled && (IsSideKeyItem(rep) || IsSideKeyItem(old)))
                return old == rep;
            if (!LRFlags.Items.KeyCoP.Enabled && (IsCoPKeyItem(rep) || IsCoPKeyItem(old)))
                return old == rep;
            if (!LRFlags.Items.EPLearns.Enabled && (IsEPAbility(rep) || IsEPAbility(old)))
                return old == rep;
            if (!LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled && (GetLocationItem(rep).Item1 == "ti830_00" || GetLocationItem(old).Item1 == "ti830_00"))
                return old == rep;
            if (!LRFlags.StatsAbilities.EPAbilitiesChrono.Enabled && (GetLocationItem(rep).Item1 == "ti840_00" || GetLocationItem(old).Item1 == "ti840_00"))
                return old == rep;
            if (!LRFlags.StatsAbilities.EPAbilitiesTp.Enabled && (GetLocationItem(rep).Item1 == "ti810_00" || GetLocationItem(old).Item1 == "ti810_00"))
                return old == rep;

            if (ItemLocations[rep].Traits.Contains("Same") || ItemLocations[old].Traits.Contains("Same"))
                return old == rep;

            if (ItemLocations[old].Traits.Contains("Missable"))
            {
                if (IsKeyItem(rep))
                    return false;
                if (GetLocationItem(rep).Item1.StartsWith("libra"))
                    return false;
                if (!LRFlags.Items.EPMissable.Enabled && IsEPAbility(rep))
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("CoP"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (IsKeyItem(rep) && !IsKeyItem(old) && !LRFlags.Items.KeyPlaceCoP.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Grindy"))
            {
                if (IsKeyItem(rep) && !IsKeyItem(old) && !LRFlags.Items.KeyPlaceGrindy.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Quest"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (GetLocationItem(rep).Item1.StartsWith("it"))
                    return false;
                if (IsKeyItem(rep) && !IsKeyItem(old) && !LRFlags.Items.KeyPlaceQuest.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Battle"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (GetLocationItem(rep).Item1.StartsWith("it"))
                    return false;
                if (GetLocationItem(rep).Item2 > 1)
                    return false;
                if (GetLocationItem(rep).Item1 == "")
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Trade"))
            {
                if (IsEPAbility(rep))
                    return false;
                if (GetLocationItem(rep).Item2 > 1)
                    return false;
                if (GetLocationItem(rep).Item1 == "")
                    return false;
            }

            if (IsKeyItem(rep) && !IsPilgrimKeyItem(rep) && (!IsKeyItem(old) || IsPilgrimKeyItem(old)) && !LRFlags.Items.KeyPlaceTreasure.Enabled)
                return false;
            if ((!IsKeyItem(rep) || IsPilgrimKeyItem(rep)) && IsKeyItem(old) && !IsPilgrimKeyItem(old) && !LRFlags.Items.KeyPlaceTreasure.Enabled)
                return false;
            return true;
        }

        public bool IsEPAbility(string t, bool orig = true)
        {
            return GetLocationItem(t, orig).Item1.StartsWith("ti") || GetLocationItem(t, orig).Item1 == "at900_00";
        }

        public bool IsKeyItem(string t, bool orig = true)
        {
            return GetLocationItem(t, orig).Item1.StartsWith("key") || GetLocationItem(t, orig).Item1 == "cos_fa00";
        }

        public bool IsMainKeyItem(string t)
        {
            return ItemLocations[t].Traits.Contains("MainKey");
        }

        public bool IsSideKeyItem(string t)
        {
            return ItemLocations[t].Traits.Contains("SideKey");
        }

        public bool IsCoPKeyItem(string t)
        {
            return ItemLocations[t].Traits.Contains("CoPKey");
        }

        public bool IsPilgrimKeyItem(string t)
        {
            return ItemLocations[t].Traits.Contains("Pilgrim");
        }


        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (LRFlags.Items.KeyDepth.SelectedValue == LRFlags.Items.KeyDepth.Values[LRFlags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                int index = LRFlags.Items.KeyDepth.Values.IndexOf(LRFlags.Items.KeyDepth.SelectedValue);
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

        
    }
}
