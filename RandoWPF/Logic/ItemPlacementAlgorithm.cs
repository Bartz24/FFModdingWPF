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

        protected int maxFailCount;

        public ItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations, int maxFail = -1)
        {
            ItemLocations = itemLocations;
            HintsByLocation = hintsByLocations;
            maxFailCount = maxFail;
        }

        public virtual bool Randomize(List<string> defaultAreas)
        {
            Placement.Clear();
            Depths.Clear();
            List<string> allowed = GetKeysAllowed();
            List<string> place = GetKeysToPlace();

            SetHintValues(place);

            if (!DoImportantPlacement(allowed, place.Where(t => RequiresLogic(t)).ToList(), defaultAreas))
                return false;

            DoNonImportantPlacement(allowed, place);

            ApplyPlacement();
            return true;
        }

        public virtual List<string> GetKeysAllowed()
        {
            return ItemLocations.Keys.ToList().Shuffle().ToList();
        }

        public virtual List<string> GetKeysToPlace()
        {
            return ItemLocations.Keys.ToList().Shuffle().Where(l => GetLocationItem(l) != null || ItemLocations[l].Traits.Contains("Fake")).ToList();
        }

        protected virtual void ApplyPlacement()
        {
            foreach (string key in Placement.Keys.Where(l => !ItemLocations[l].Traits.Contains("Fake")))
            {
                string repKey = Placement[key];
                Tuple<string, int> orig = GetLocationItem(repKey);
                SetLocationItem(key, orig.Item1, orig.Item2);
            }
        }

        protected virtual void DoNonImportantPlacement(List<string> allowed, List<string> place)
        {
            List<string> newKeys = place.Where(k => !Placement.ContainsValue(k)).ToList().Shuffle().ToList();
            foreach (string k in allowed.Where(k => !Placement.ContainsKey(k)).ToList().Shuffle().ToList())
            {
                if (newKeys.Count == 0)
                    break;
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

        protected virtual bool DoImportantPlacement(List<string> locations, List<string> important, List<string> defaultAreas)
        {
            for(int i = 0; i < (maxFailCount == -1 ? 1 : maxFailCount); i+=(maxFailCount == -1 ? 0 : 1))
            {
                bool output = TryImportantPlacement(locations, important, new List<string>(defaultAreas));
                if (output)
                    return true;
                Placement.Clear();
                Depths.Clear();
                Clear();
            }
            return false;
        }

        protected virtual bool TryImportantPlacement(List<string> locations, List<string> important, List<string> accessibleAreas)
        {
            Dictionary<string, int> items = GetItemsAvailable();
            List<string> remaining = important.Where(t => !Placement.ContainsValue(t)).ToList().Shuffle().ToList();

            List<string> newAccessibleAreas = GetNewAreasAvailable(items, accessibleAreas);

            List<string> remainingLogic = remaining.Where(t => RequiresDepthLogic(t)).ToList().Shuffle().ToList();
            if (remainingLogic.Count > 0)
            {
                remainingLogic = PrioritizeLockedItems(locations, remainingLogic, important);
                foreach (string rep in remainingLogic)
                {
                    Tuple<string, int> nextItem = GetLocationItem(rep);
                    if (nextItem == null)
                    {
                        if (ItemLocations[rep].Traits.Contains("Fake"))
                        {
                            Placement.Add(rep, rep);
                            if (Placement.Count == important.Count)
                                return true;
                            bool result = TryImportantPlacement(locations, important, newAccessibleAreas);
                            if (result)
                                return result;
                            else
                            {
                                Placement.Remove(rep);
                            }
                        }
                        continue;
                    }
                    List<string> allowedLocations = HintsByLocationsCount.Keys.ToList().Shuffle().ToList();

                    // Remove inaccessible locations
                    allowedLocations = allowedLocations.Where(a => newAccessibleAreas.Contains(a)).ToList();

                    foreach (string loc in allowedLocations)
                    {
                        List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, loc, items, newAccessibleAreas)).ToList().Shuffle().ToList();
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
            }
            else
            {
                List<string> remainingOther = remaining.Where(t => !RequiresDepthLogic(t)).ToList().Shuffle().ToList();
                foreach (string rep in remainingOther)
                {
                    Tuple<string, int> nextItem = GetLocationItem(rep);
                    if (nextItem == null)
                    {
                        if (ItemLocations[rep].Traits.Contains("Fake"))
                        {
                            Placement.Add(rep, rep);
                            if (Placement.Count == important.Count)
                                return true;
                            bool result = TryImportantPlacement(locations, important, newAccessibleAreas);
                            if (result)
                                return result;
                            else
                            {
                                Placement.Remove(rep);
                            }
                        }
                        continue;
                    }
                    List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && IsAllowed(t, rep)).ToList();
                    while (possible.Count > 0)
                    {
                        string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                        string hint = null;
                        if (IsHintable(rep))
                            hint = AddHint(items, next, rep, 0);
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
                            if (IsHintable(rep))
                                RemoveHint(hint, next);
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

        protected virtual Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            placement.ForEach(p =>
            {
                Tuple<string, int> tuple = GetLocationItem(p.Value, false);
                if (tuple != null)
                {
                    string item = tuple.Item1;
                    int amount = tuple.Item2;
                    if (dict.ContainsKey(item))
                        dict[item] += amount;
                    else
                        dict.Add(item, amount);
                }
            });
            return dict;

        }
        public abstract bool RequiresLogic(string location);
        public abstract bool IsValid(string location, string replacement, string area, Dictionary<string, int> items, List<string> areasAvailable);
        public abstract bool IsAllowed(string old, string rep);
        public abstract int GetNextDepth(Dictionary<string, int> items, string location);
        public abstract bool RequiresDepthLogic(string location);
        public abstract bool IsHintable(string location);

        public virtual List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            return soFar.ToList();
        }
        public abstract string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth);
        public abstract void RemoveHint(string hint, string location);
        public abstract void Clear();

        public virtual Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }

        public virtual void SetLocationItem(string key, string item, int count)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }

        protected List<string> PrioritizeLockedItems(List<string> locations, List<string> remaining, List<string> important)
        {
            Dictionary<string, int> locked = new Dictionary<string, int>();
            Dictionary<string, int> items = GetItemsAvailable(important.ToDictionary(l => l, l => l));

            foreach (string rep in remaining)
            {
                Tuple<string, int> nextItem = GetLocationItem(rep);
                if (nextItem == null)
                    continue;
                items[nextItem.Item1] -= nextItem.Item2;
                List<string> newAccessibleAreas = GetNewAreasAvailable(items, new List<string>());

                List<string> possible = newAccessibleAreas.SelectMany(loc => locations.Where(t => !Placement.ContainsKey(t) && IsValid(t, rep, loc, items, newAccessibleAreas))).Distinct().ToList();
                if (possible.Count == 1)
                    locked.Add(rep, ItemLocations[rep].Requirements.GetPossibleRequirementsCount());
                items[nextItem.Item1] += nextItem.Item2;
            }

            return remaining.OrderByDescending(rep => locked.ContainsKey(rep) ? locked[rep] : -1).ToList();
        }
    }
}
