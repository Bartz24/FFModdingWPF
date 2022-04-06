using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public abstract class ItemPlacementAlgorithm<T> where T : ItemLocation
    {

        public Dictionary<string, T> ItemLocations = new Dictionary<string, T>();

        public Dictionary<string, string> Placement = new Dictionary<string, string>();
        public Dictionary<string, int> Depths = new Dictionary<string, int>();

        public List<string> HintsByLocation = new List<string>();
        public Dictionary<string, int> HintsByLocationsCount = new Dictionary<string, int>();

        public ItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations)
        {
            ItemLocations = itemLocations;
            HintsByLocation = hintsByLocations;
        }

        public virtual void Randomize(List<string> defaultAreas)
        {
            Placement.Clear();
            Depths.Clear();
            List<string> keys = ItemLocations.Keys.ToList().Shuffle().ToList();

            SetHintValues(keys);

            DoImportantPlacement(keys, keys.Where(t => RequiresLogic(t)).ToList(), defaultAreas);

            DoNonImportantPlacement(keys);

            ApplyPlacement(keys);
        }

        protected virtual void ApplyPlacement(List<string> keys)
        {
            foreach (string key in keys)
            {
                string repKey = Placement[key];
                Tuple<string, int> orig = GetLocationItem(repKey);
                SetLocationItem(key, orig.Item1, orig.Item2);
            }
        }

        protected virtual void DoNonImportantPlacement(List<string> keys)
        {
            List<string> newKeys = keys.Where(k => !Placement.ContainsValue(k)).ToList().Shuffle().ToList();
            foreach (string k in keys.Where(k => !Placement.ContainsKey(k)))
            {
                Placement.Add(k, newKeys[0]);
                newKeys.RemoveAt(0);
            }
        }

        protected virtual void SetHintValues(List<string> keys)
        {
            List<string> locations = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList().Shuffle().ToList();

            HintsByLocation.ForEach(l =>
            {
                HintsByLocationsCount.Add(l, 0);
            });

            List<string> randomZeros = new List<string>();
            for (int j = 0; j < 10; j++)
            {
                if (RandomNum.RandInt(0, 99) < 10)
                    randomZeros.Add(HintsByLocationsCount.Keys.Where(l => !randomZeros.Contains(l)).ToList().Shuffle().First());
            }

            float copMult = RandomNum.RandInt(12, 100) / 100f;

            for (int i = 0; i < keys.Where(t => IsHintable(t)).Count(); i++)
            {
                Func<string, long> weight = loc =>
                {
                    if (randomZeros.Contains(loc))
                        return 0;

                    int max = ItemLocations.Keys.Where(t => ItemLocations[t].Areas.Contains(loc) && !ItemLocations[t].Traits.Contains("Missable")).Count();
                    long val = (long)(100 * Math.Pow(1 - (HintsByLocationsCount[loc] / (float)max), 4));

                    if (loc.Contains("CoP"))
                        val = (long)(val * copMult);

                    return val;
                };
                string next = RandomNum.SelectRandomWeighted(HintsByLocationsCount.Keys.ToList(), weight);
                HintsByLocationsCount[next]++;
            }
        }

        protected virtual void DoImportantPlacement(List<string> locations, List<string> important, List<string> defaultAreas)
        {
            bool output = TryImportantPlacement(locations, important, new List<string>(defaultAreas));
            if (!output)
                throw new Exception("Failed to place all important items");
        }

        protected virtual bool TryImportantPlacement(List<string> locations, List<string> important, List<string> accessibleAreas)
        {
            Dictionary<string, int> items = GetItemsAvailable();
            List<string> remaining = important.Where(t => !Placement.ContainsValue(t)).ToList().Shuffle().ToList();

            List<string> newAccessibleAreas = GetNewAreasAvailable(items, accessibleAreas);

            // Used to determine when a non-important item is placed and leads to a dead end, therefore all non-important items are dead ends
            bool nonImportantDeadEnd = false;

            foreach (string rep in remaining)
            {
                // Only important key items are affected by location/depth logic
                if (RequiresDepthLogic(rep))
                {
                    List<string> allowedLocations = new List<string>();
                    allowedLocations.AddRange(HintsByLocationsCount.Keys.Where(l => !IsHintable(rep) || (HintsByLocationsCount[l] > 0 && IsHintable(rep))).ToList().Shuffle());
                    // If there are no more locations with available spots, just add to any location
                    allowedLocations.AddRange(HintsByLocationsCount.Keys.Where(l => !allowedLocations.Contains(l)).ToList().Shuffle());

                    // Remove inaccessible locations
                    allowedLocations = allowedLocations.Where(a => newAccessibleAreas.Contains(a)).ToList();

                    foreach (string loc in allowedLocations)
                    {
                        List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, loc, items, newAccessibleAreas)).ToList();
                        while (possible.Count > 0)
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
                            bool result = TryImportantPlacement(locations, important, newAccessibleAreas);
                            if (result)
                                return result;
                            else
                            {
                                possible.Remove(next);
                                Placement.Remove(next);
                                Depths.Remove(next);
                                if (IsHintable(rep))
                                    RemoveHint(hint, next);
                            }
                        }
                    }
                }
                else if (!nonImportantDeadEnd)
                {
                    List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, null, items, newAccessibleAreas)).ToList();
                    while (possible.Count > 0)
                    {
                        string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                        Placement.Add(next, rep);
                        if (Placement.Count == important.Count)
                            return true;
                        bool result = TryImportantPlacement(locations, important, newAccessibleAreas);
                        if (result)
                            return result;
                        else
                        {
                            possible.Remove(next);
                            Placement.Remove(next);
                            nonImportantDeadEnd = true;
                            break;
                        }
                    }
                }
            }
            return false;
        }
        public virtual Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
            string next = RandomNum.SelectRandomWeighted(possible, s => 1);
            return new Tuple<string, int>(next, possDepths[next]);
        }

        protected Dictionary<string, int> GetItemsAvailable()
        {
            return GetItemsAvailable(Placement);
        }

        protected Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            placement.ForEach(p =>
            {
                string item = GetLocationItem(p.Value, false).Item1;
                int amount = GetLocationItem(p.Value, false).Item2;
                if (dict.ContainsKey(item))
                    dict[item] += amount;
                else
                    dict.Add(item, amount);
            });
            return dict;

        }
        public abstract bool RequiresLogic(string location);
        public abstract bool IsValid(string location, string replacement, string area, Dictionary<string, int> items, List<string> areasAvailable);
        public abstract int GetNextDepth(Dictionary<string, int> items, string location);
        public abstract bool RequiresDepthLogic(string location);
        public abstract bool IsHintable(string location);

        public virtual List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            return soFar.ToList();
        }
        public abstract string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth);
        public abstract void RemoveHint(string hint, string location);

        public virtual Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }

        public virtual void SetLocationItem(string key, string item, int count)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }
    }
}
