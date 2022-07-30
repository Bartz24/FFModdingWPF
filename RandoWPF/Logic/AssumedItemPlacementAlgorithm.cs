using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public abstract class AssumedItemPlacementAlgorithm<T> : ItemPlacementAlgorithm<T> where T : ItemLocation
    {
        public AssumedItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
        {
        }

        protected override bool TryImportantPlacement(int attempt, List<string> locations, List<string> important, List<string> accessibleAreas)
        {
            List<string> remaining = important.Where(t => !Placement.ContainsValue(t)).ToList().Shuffle().ToList();
            Dictionary<string, int> items = GetItemsAvailable(remaining.ToDictionary(l => l, l => l));

            List<string> remainingLogic = remaining.Where(t => RequiresDepthLogic(t)).ToList().Shuffle().ToList();

            remainingLogic = PrioritizeLockedItems(locations, remainingLogic, important);
            foreach (string rep in remainingLogic)
            {
                UpdateProgress(attempt, Placement.Count, important.Count);
                Tuple<string, int> nextItem = GetLocationItem(rep);
                RemoveItems(locations, items, nextItem, rep);
                if (nextItem == null)
                {
                    if (ItemLocations[rep].Traits.Contains("Fake"))
                    {
                        Placement.Add(rep, rep);
                        if (Placement.Count == important.Count)
                            return true;
                    }
                    continue;
                }
                List<string> newAccessibleAreas = GetNewAreasAvailable(items, new List<string>());

                List<string> possible = newAccessibleAreas.SelectMany(loc => locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, loc, items, newAccessibleAreas))).Distinct().ToList().Shuffle().ToList();
                int count = possible.Count;
                if (possible.Count > 0)
                {
                    Tuple<string, int> nextPlacement = SelectNext(items, possible, rep);
                    string next = nextPlacement.Item1;
                    int depth = nextPlacement.Item2;
                    string hint = null;
                    if (IsHintable(rep))
                        hint = AddHint(items, next, rep, depth);
                    Placement.Add(next, rep);
                    Depths.Add(next, depth);
                    if (Placement.Count == important.Count)
                        return true;
                }
                else
                    return false;
            }

            List<string> remainingOther = remaining.Where(t => !RequiresDepthLogic(t)).ToList().Shuffle().ToList();
            foreach (string rep in remainingOther)
            {
                UpdateProgress(attempt, Placement.Count, important.Count);
                Tuple<string, int> nextItem = GetLocationItem(rep);
                RemoveItems(locations, items, nextItem, rep);
                if (nextItem == null)
                {
                    if (ItemLocations[rep].Traits.Contains("Fake"))
                    {
                        Placement.Add(rep, rep);
                        if (Placement.Count == important.Count)
                            return true;
                    }
                    continue;
                }
                List<string> newAccessibleAreas = GetNewAreasAvailable(items, new List<string>());

                List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsAllowed(t, rep)).ToList();
                if (possible.Count > 0)
                {
                    string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                    string hint = null;
                    if (IsHintable(rep))
                        hint = AddHint(items, next, rep, 0);
                    Placement.Add(next, rep);
                    if (Placement.Count == important.Count)
                        return true;
                }
                else
                    return false;
            }
            return false;
        }

        public virtual void RemoveItems(List<string> locations, Dictionary<string, int> items, Tuple<string, int> nextItem, string rep)
        {
            if (nextItem != null)
                items[nextItem.Item1] -= nextItem.Item2;
        }

        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
            string next = RandomNum.SelectRandomWeighted(possible, s => 1);
            return new Tuple<string, int>(next, possDepths[next]);
        }

        protected override void UpdateProgress(int i, int items, int maxItems)
        {
            SetProgressFunc($"Item Placement Method Attempt {i + 1}" + (maxFailCount == -1 ? "" : $" of {maxFailCount}") + $" ({items} out of {maxItems} items placed)", items, maxItems);
        }
    }
}
