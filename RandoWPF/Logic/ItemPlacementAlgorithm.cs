using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF
{
    public class ItemPlacementAlgorithm<T> where T : ItemLocation
    {

        public Dictionary<string, T> ItemLocations { get; set; } = new Dictionary<string, T>();

        public Dictionary<string, string> Placement { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> Depths { get; set; } = new Dictionary<string, int>();

        public List<string> HintsByLocation { get; set; } = new List<string>();
        public Dictionary<string, int> HintsByLocationsCount { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, double> AreaMults { get; set; } = new Dictionary<string, double>();
        public Action<string, int, int> SetProgressFunc { get; set; }
        private GameSpecificItemPlacementLogic<T> logic;
        public GameSpecificItemPlacementLogic<T> Logic { get => logic;
            set
            {
                if (logic != null)
                    throw new Exception("Already set game logic for the algorithm.");
                logic = value;
            }
        }

        protected int maxFailCount;

        public ItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations, int maxFail = -1)
        {
            ItemLocations = itemLocations;
            HintsByLocation = hintsByLocations;
            maxFailCount = maxFail;
        }

        public virtual bool Randomize(List<string> defaultAreas, Dictionary<string, double> areaMults)
        {
            AreaMults = areaMults;
            Placement.Clear();
            Depths.Clear();
            List<string> allowed = Logic.GetKeysAllowed();
            List<string> place = Logic.GetKeysToPlace();

            SetHintValues(place);

            if (!DoImportantPlacement(allowed, place.Where(t => Logic.RequiresLogic(t)).ToList(), defaultAreas))
                return false;

            DoNonImportantPlacement(allowed, place);

            ApplyPlacement();
            return true;
        }

        protected virtual void ApplyPlacement()
        {
            foreach (string key in Placement.Keys.Where(l => !ItemLocations[l].Traits.Contains("Fake")))
            {
                string repKey = Placement[key];
                Tuple<string, int> orig = Logic.GetLocationItem(repKey);
                Logic.SetLocationItem(key, orig.Item1, orig.Item2);
            }
        }

        protected virtual void DoNonImportantPlacement(List<string> allowed, List<string> place)
        {
            List<string> newKeys = place.Where(k => !Placement.ContainsValue(k)).Shuffle();
            foreach (string k in allowed.Where(k => !Placement.ContainsKey(k)).Shuffle())
            {
                if (newKeys.Count == 0)
                    break;
                Placement.Add(k, newKeys[0]);
                newKeys.RemoveAt(0);
            }
        }

        protected virtual void SetHintValues(List<string> keys)
        {
            List<string> locations = ItemLocations.Values.SelectMany(t => t.Areas).Distinct().Shuffle();

            HintsByLocation.ForEach(l =>
            {
                HintsByLocationsCount.Add(l, 0);
            });

            List<string> randomZeros = new List<string>();
            for (int j = 0; j < 10; j++)
            {
                if (RandomNum.RandInt(0, 99) < 10 && HintsByLocationsCount.Keys.Where(l => !randomZeros.Contains(l)).Count() > 0)
                    randomZeros.Add(HintsByLocationsCount.Keys.Where(l => !randomZeros.Contains(l)).Shuffle().First());
            }

            float copMult = RandomNum.RandInt(12, 100) / 100f;

            for (int i = 0; i < keys.Where(t => Logic.IsHintable(t)).Count(); i++)
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
            for (int i = 0; i < (maxFailCount == -1 ? int.MaxValue : maxFailCount); i++)
            {
                bool output = TryImportantPlacement(i, locations, important, new List<string>(defaultAreas));
                if (output)
                    return true;
                Placement.Clear();
                Depths.Clear();
                Logic.Clear();
            }
            return false;
        }

        protected virtual void UpdateProgress(int i, int items, int maxItems)
        {
            SetProgressFunc($"Backup Item Placement Method Attempt {i + 1}" + (maxFailCount == -1 ? "" : $" of {maxFailCount}") + $" ({items} out of {maxItems} items placed)", items, maxItems);
        }

        private List<string> locked = null;

        private List<string> OrderLocked(List<string> locations, List<string> remaining, List<string> important)
        {
            if (Placement.Count == 0)
                locked = null;
            if (locked == null)
                locked = PrioritizeLockedItems(locations, remaining, important);
            return remaining.OrderBy(t => locked.IndexOf(t)).ToList();
        }

        protected virtual bool TryImportantPlacement(int attempt, List<string> locations, List<string> important, List<string> accessibleAreas)
        {
            Dictionary<string, int> items = Logic.GetItemsAvailable();
            List<string> remaining = important.Where(t => !Placement.ContainsValue(t)).Shuffle();
            UpdateProgress(attempt, Placement.Count, important.Count);

            List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, accessibleAreas);

            List<string> remainingLogic = remaining.Where(t => Logic.RequiresDepthLogic(t)).Shuffle();
            if (remainingLogic.Count > 0)
            {
                remainingLogic = OrderLocked(locations, remainingLogic, important);
                while (remainingLogic.Count > 0)
                {
                    string rep = remainingLogic[0];
                    Tuple<string, int> nextItem = Logic.GetLocationItem(rep);
                    if (nextItem == null)
                    {
                        if (ItemLocations[rep].Traits.Contains("Fake"))
                        {
                            Placement.Add(rep, rep);
                            if (Placement.Count == important.Count)
                                return true;
                            bool result = TryImportantPlacement(attempt, locations, important, newAccessibleAreas);
                            if (result)
                                return result;
                            else
                            {
                                Placement.Remove(rep);
                            }
                        }
                        continue;
                    }
                    List<string> allowedLocations = HintsByLocationsCount.Keys.Shuffle();

                    // Remove inaccessible locations
                    allowedLocations = allowedLocations.Where(a => newAccessibleAreas.Contains(a)).ToList();

                    List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, allowedLocations)).Shuffle();
                    while (possible.Count > 0)
                    {
                        Tuple<string, int> nextPlacement = Logic.SelectNext(items, possible, rep);
                        string next = nextPlacement.Item1;
                        int depth = nextPlacement.Item2;
                        string hint = null;
                        if (Logic.IsHintable(rep))
                            hint = Logic.AddHint(items, next, rep, depth);
                        Placement.Add(next, rep);
                        Depths.Add(next, depth);
                        if (Placement.Count == important.Count)
                            return true;
                        bool result = TryImportantPlacement(attempt, locations, important, newAccessibleAreas);
                        if (result)
                            return result;
                        else
                        {
                            possible.Remove(next);
                            Placement.Remove(next);
                            Depths.Remove(next);
                            if (Logic.IsHintable(rep))
                                Logic.RemoveHint(hint, next);
                        }
                    }

                    Logic.RemoveLikeItemsFromRemaining(rep, remainingLogic);
                }
            }
            else
            {
                List<string> remainingOther = remaining.Where(t => !Logic.RequiresDepthLogic(t)).Shuffle();
                while (remainingOther.Count > 0)
                {
                    string rep = remainingOther[0];
                    Tuple<string, int> nextItem = Logic.GetLocationItem(rep);
                    if (nextItem == null)
                    {
                        if (ItemLocations[rep].Traits.Contains("Fake"))
                        {
                            Placement.Add(rep, rep);
                            if (Placement.Count == important.Count)
                                return true;
                            bool result = TryImportantPlacement(attempt, locations, important, newAccessibleAreas);
                            if (result)
                                return result;
                            else
                            {
                                Placement.Remove(rep);
                            }
                        }
                        continue;
                    }

                    List<string> possible = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsAllowed(t, rep)).ToList();
                    while (possible.Count > 0)
                    {
                        string next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                        string hint = null;
                        if (Logic.IsHintable(rep))
                            hint = Logic.AddHint(items, next, rep, 0);
                        Placement.Add(next, rep);
                        if (Placement.Count == important.Count)
                            return true;
                        bool result = TryImportantPlacement(attempt, locations, important, newAccessibleAreas);
                        if (result)
                            return result;
                        else
                        {
                            possible.Remove(next);
                            Placement.Remove(next);
                            if (Logic.IsHintable(rep))
                                Logic.RemoveHint(hint, next);
                            break;
                        }
                    }

                    Logic.RemoveLikeItemsFromRemaining(rep, remainingLogic);
                }
            }
            return false;
        }

        protected List<string> PrioritizeLockedItems(List<string> locations, List<string> remaining, List<string> important)
        {
            Dictionary<string, int> locked = new Dictionary<string, int>();
            Dictionary<string, int> items = Logic.GetItemsAvailable(important.ToDictionary(l => l, l => l));

            foreach (string rep in remaining)
            {
                Tuple<string, int> nextItem = Logic.GetLocationItem(rep);
                if (nextItem == null)
                    continue;
                items[nextItem.Item1] -= nextItem.Item2;
                List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());

                int possibleCount = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Count();
                if (possibleCount == 1)
                    locked.Add(rep, ItemLocations[rep].Requirements.GetPossibleRequirementsCount());
                items[nextItem.Item1] += nextItem.Item2;
            }

            return remaining.OrderByDescending(rep => locked.ContainsKey(rep) ? locked[rep] : -1).ToList();
        }
    }
}
