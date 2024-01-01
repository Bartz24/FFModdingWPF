using Bartz24.Data;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Generic;
using System.Linq;
using static LRRando.TreasureRando;

namespace LRRando;

public class LRItemPlacementLogic : ItemPlacementLogic<ItemLocation>
{
    private readonly TreasureRando treasureRando;

    public LRItemPlacementLogic(ItemPlacementAlgorithm<ItemLocation> algorithm, SeedGenerator randomizers) : base(algorithm)
    {
        treasureRando = randomizers.Get<TreasureRando>();
    }

    public override string AddHint(string location, string replacement, int itemDepth)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]--);

        if (IsHintable(replacement))
        {
            List<HintData> possible = treasureRando.hintData.Values.Where(h =>
            {
                return h.Requirements.IsValid(GetItemsAvailable()) && !h.Requirements.GetPossibleRequirements().Contains(replacement);
            }).Shuffle().OrderByDescending(h =>
            {
                if (LRFlags.Other.HintsDepth.Enabled)
                {
                    int hintDepth = GetReqsMaxDepth(h.Requirements);
                    if (hintDepth > itemDepth)
                    {
                        return false;
                    }
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
            List<string> reqChecks = Algorithm.Placement.Keys.Where(t => ItemLocations[Algorithm.Placement[t]].GetItem(true).Value.Item1 == item && !ItemLocations[t].Requirements.GetPossibleRequirements().Contains(item)).ToList();
            return reqChecks.Select(t => Algorithm.Depths[t] + GetReqsMaxDepth(ItemLocations[t].Requirements)).DefaultIfEmpty(0).Max();
        }).DefaultIfEmpty(0).Max();
    }

    public override int GetNextDepth(Dictionary<string, int> items, string location)
    {
        int minItems = 8;
        int keyItemsFound = Algorithm.Placement.Where(p => treasureRando.IsImportantKeyItem(p.Value)).Count();
        // Early day/easier checks have higher "depths" as minItems is low to start chains
        float diffModifier = Math.Min(minItems, keyItemsFound) / (float)minItems;
        int maxDifficulty = ItemLocations.Values.Select(t => t.BaseDifficulty).Max();
        int val = (int)((diffModifier * ItemLocations[location].BaseDifficulty) + ((1 - diffModifier) * (maxDifficulty - ItemLocations[location].BaseDifficulty)));

        if (ItemLocations[location].Traits.Contains("CoP"))
        {
            val = (int)Math.Round(Math.Pow(val, 0.75f)) / 2;
        }

        if (ItemLocations[location].Areas[0] == "Ultimate Lair")
        {
            val = (int)Math.Round(Math.Pow(val, 0.75f));
        }

        return RandomNum.RandInt(Math.Max(0, val - 2), val + 2);
    }

    public override bool IsHintable(string location)
    {
        if (LRFlags.Items.KeyItems.SelectedKeys.Contains("key_d_key") && LRFlags.Other.HintsPilgrim.FlagEnabled && treasureRando.IsPilgrimKeyItem(location))
        {
            return true;
        }

        if (LRFlags.Items.KeyItems.SelectedKeys.Contains(ItemLocations[location].GetItem(true).Value.Item1) && treasureRando.IsImportantKeyItem(location))
        {
            return true;
        }

        if (treasureRando.IsEPAbility(location))
        {
            if (!LRFlags.StatsAbilities.EPAbilities.FlagEnabled || !LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(ItemLocations[location].GetItem(true).Value.Item1))
            {
                return false;
            }

            if (LRFlags.Other.HintsEP.FlagEnabled)
            {
                return true;
            }
        }

        return false;
    }

    public override bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable)
    {
        return ItemLocations[location].IsValid(items) &&
            ItemLocations[location].Areas.Intersect(areasAvailable).Count() > 0 &&
            IsAllowed(location, replacement);
    }

    public override void RemoveHint(string hint, string location)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]++);
        treasureRando.hintsMain[hint].Remove(location);
    }

    public override bool RequiresDepthLogic(string location)
    {
        return treasureRando.IsImportantKeyItem(location);
    }

    public override bool RequiresLogic(string location)
    {
        if (ItemLocations[location].Traits.Contains("Same"))
        {
            return true;
        }

        if (treasureRando.IsImportantKeyItem(location))
        {
            return true;
        }

        if (ItemLocations[location].GetItem(true).Value.Item1.StartsWith("libra"))
        {
            return true;
        }

        return treasureRando.IsEPAbility(location)
|| ItemLocations[location].GetItem(true).Value.Item1.StartsWith("it")
|| ItemLocations[location].GetItem(true).Value.Item1 == "" || ItemLocations[location].GetItem(true).Value.Item2 > 1;
    }

    public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
    {
        return ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList();
    }

    protected override bool IsAllowedReplacement(string old, string rep)
    {
        foreach (string item in LRFlags.Items.KeyItems.DictValues.Keys)
        {
            if (!LRFlags.Items.KeyItems.SelectedKeys.Contains(item) && (ItemLocations[rep].GetItem(true).Value.Item1 == item || ItemLocations[old].GetItem(true).Value.Item1 == item))
            {
                return old == rep;
            }
        }

        foreach (string abi in LRFlags.StatsAbilities.EPAbilitiesPool.DictValues.Keys)
        {
            if ((!LRFlags.StatsAbilities.EPAbilities.FlagEnabled || !LRFlags.StatsAbilities.EPAbilitiesPool.SelectedKeys.Contains(abi)) && (ItemLocations[rep].GetItem(true).Value.Item1 == abi || ItemLocations[old].GetItem(true).Value.Item1 == abi))
            {
                return old == rep;
            }
        }

        if (ItemLocations[rep].Traits.Contains("Same") || ItemLocations[old].Traits.Contains("Same"))
        {
            return old == rep;
        }

        if (ItemLocations[old].Traits.Contains("Missable"))
        {
            if (treasureRando.IsImportantKeyItem(rep))
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1.StartsWith("libra"))
            {
                return false;
            }

            if (!LRFlags.Items.EPMissable.Enabled && treasureRando.IsEPAbility(rep))
            {
                return false;
            }
        }

        List<string> specialTraits = new();
        if (ItemLocations[old].Traits.Contains("CoP"))
        {
            specialTraits.Add("CoP");
            if (treasureRando.IsEPAbility(rep))
            {
                return false;
            }

            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !LRFlags.Items.KeyPlaceCoP.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Grindy"))
        {
            specialTraits.Add("Grindy");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !LRFlags.Items.KeyPlaceGrindy.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Superboss"))
        {
            specialTraits.Add("Superboss");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !LRFlags.Items.KeyPlaceSuperboss.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Quest"))
        {
            specialTraits.Add("Quest");
            if (treasureRando.IsEPAbility(rep))
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1.StartsWith("it"))
            {
                return false;
            }

            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !LRFlags.Items.KeyPlaceQuest.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Battle"))
        {
            if (treasureRando.IsEPAbility(rep))
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1.StartsWith("it"))
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item2 > 1)
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1 == "")
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Trade"))
        {
            if (treasureRando.IsEPAbility(rep))
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item2 > 1)
            {
                return false;
            }

            if (ItemLocations[rep].GetItem(true).Value.Item1 == "")
            {
                return false;
            }
        }

        return (!treasureRando.IsImportantKeyItem(rep) || treasureRando.IsPilgrimKeyItem(rep) || (treasureRando.IsImportantKeyItem(old) && !treasureRando.IsPilgrimKeyItem(old)) || LRFlags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0)
&& ((treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsPilgrimKeyItem(rep)) || !treasureRando.IsImportantKeyItem(old) || treasureRando.IsPilgrimKeyItem(old) || LRFlags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0);
    }

    public override int GetPlacementDifficultyIndex()
    {
        return LRFlags.Items.KeyDepth.SelectedIndex;
    }

    public override void Clear()
    {
        treasureRando.hintsMain.Values.ForEach(l => l.Clear());
    }
}
