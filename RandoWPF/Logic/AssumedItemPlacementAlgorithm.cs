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

        public AssumedItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations) : base(itemLocations, hintsByLocations)
        {
        }

        protected override void DoImportantPlacement(List<string> locations, List<string> important, List<string> defaultAreas)
        {
            while(true)
            {
                bool output = TryImportantPlacement(locations, important, new List<string>(defaultAreas));
                if (output)
                    return;
                Placement.Clear();
                Depths.Clear();
            }
            //throw new Exception("Failed to place all important items");
        }

        protected override bool TryImportantPlacement(List<string> locations, List<string> important, List<string> accessibleAreas)
        {
            Dictionary<string, int> items = GetItemsAvailable(important.ToDictionary(l => l, l => l));

            List<string> remaining = important.Shuffle().ToList();

            foreach (string rep in remaining)
            {
                Tuple<string, int> nextItem = GetLocationItem(rep);
                items[nextItem.Item1] -= nextItem.Item2;
                List<string> newAccessibleAreas = GetNewAreasAvailable(items, new List<string>());

                // Only important key items are affected by location/depth logic
                if (RequiresDepthLogic(rep))
                {
                    List<string> possible = newAccessibleAreas.SelectMany(loc => locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, loc, items, newAccessibleAreas))).Distinct().ToList();
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
                }
                else
                {
                    List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, null, items, newAccessibleAreas)).ToList();
                    if (possible.Count > 0)
                    {
                        string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                        Placement.Add(next, rep);
                        if (Placement.Count == important.Count)
                            return true;
                    }
                }
            }
            return false;
        }
        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
            string next = RandomNum.SelectRandomWeighted(possible, s => 1);
            return new Tuple<string, int>(next, possDepths[next]);
        }
    }
}
