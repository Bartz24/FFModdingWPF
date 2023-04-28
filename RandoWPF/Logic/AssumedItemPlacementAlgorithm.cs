using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class AssumedItemPlacementAlgorithm<T> : ItemPlacementAlgorithm<T> where T : ItemLocation
{
    public AssumedItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
    {
    }

    protected override bool TryImportantPlacement(int attempt, List<string> locations, List<string> important, List<string> accessibleAreas)
    {
        List<string> remaining = important.Where(t => !Placement.ContainsValue(t)).Shuffle();
        Dictionary<string, int> items = Logic.GetItemsAvailable(remaining.ToDictionary(l => l, l => l));

        List<string> remainingLogic = remaining.Where(t => Logic.RequiresDepthLogic(t)).Shuffle();

        remainingLogic = PrioritizeLockedItems(locations, remainingLogic, important);
        foreach (string rep in remainingLogic)
        {
            Iterations++;
            UpdateProgress(attempt, Placement.Count, important.Count);
            (string, int)? nextItem = Logic.GetLocationItem(rep);
            RemoveItems(locations, items, nextItem, rep);
            if (nextItem == null)
            {
                if (ItemLocations[rep].Traits.Contains("Fake"))
                {
                    Placement.Add(rep, rep);
                    if (Placement.Count == important.Count)
                    {
                        return true;
                    }
                }

                continue;
            }

            List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());

            List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Shuffle();
            int count = possible.Count;
            if (possible.Count > 0)
            {
                (string, int) nextPlacement = Logic.SelectNext(items, possible, rep);
                string next = nextPlacement.Item1;
                int depth = nextPlacement.Item2;
                string hint = null;
                if (Logic.IsHintable(rep))
                {
                    hint = Logic.AddHint(items, next, rep, depth);
                }

                Placement.Add(next, rep);
                Depths.Add(next, depth);
                if (Placement.Count == important.Count)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        List<string> remainingOther = remaining.Where(t => !Logic.RequiresDepthLogic(t)).Shuffle();
        foreach (string rep in remainingOther)
        {
            Iterations++;
            UpdateProgress(attempt, Placement.Count, important.Count);
            (string, int)? nextItem = Logic.GetLocationItem(rep);
            RemoveItems(locations, items, nextItem, rep);
            if (nextItem == null)
            {
                if (ItemLocations[rep].Traits.Contains("Fake"))
                {
                    Placement.Add(rep, rep);
                    if (Placement.Count == important.Count)
                    {
                        return true;
                    }
                }

                continue;
            }

            List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());

            List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsAllowed(t, rep)).ToList();
            if (possible.Count > 0)
            {
                string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                string hint = null;
                if (Logic.IsHintable(rep))
                {
                    hint = Logic.AddHint(items, next, rep, 0);
                }

                Placement.Add(next, rep);
                if (Placement.Count == important.Count)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public virtual void RemoveItems(List<string> locations, Dictionary<string, int> items, (string, int)? nextItem, string rep)
    {
        if (nextItem != null)
        {
            items[nextItem.Value.Item1] -= nextItem.Value.Item2;
        }
    }

    protected override void UpdateProgress(int i, int items, int maxItems)
    {
        SetProgressFunc($"Item Placement Attempt {i + 1}" + (maxFailCount == -1 ? "" : $" of {maxFailCount}") + $" ({items} out of {maxItems} items placed)", items, maxItems);
    }
}
