using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF
{
    public abstract class GameSpecificItemPlacementLogic<T> where T : ItemLocation
    {
        protected ItemPlacementAlgorithm<T> Algorithm { get; set; }
        public GameSpecificItemPlacementLogic(ItemPlacementAlgorithm<T> algorithm)
        {
            Algorithm = algorithm;
        }

        public Dictionary<string, T> ItemLocations { get => Algorithm.ItemLocations; }
        public Dictionary<string, double> AreaMults { get => Algorithm.AreaMults; }

        public virtual List<string> GetKeysAllowed()
        {
            return ItemLocations.Keys.Shuffle();
        }

        public virtual List<string> GetKeysToPlace()
        {
            return ItemLocations.Keys.Shuffle().Where(l => GetLocationItem(l) != null || ItemLocations[l].Traits.Contains("Fake")).ToList();
        }
        public abstract bool RequiresLogic(string location);
        public abstract bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable);
        public abstract bool IsAllowed(string old, string rep, bool orig = true);
        public abstract int GetNextDepth(Dictionary<string, int> items, string location);
        public abstract bool RequiresDepthLogic(string location);
        public abstract bool IsHintable(string location);

        public virtual List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            return soFar.ToList();
        }
        public abstract string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth);
        public abstract void RemoveHint(string hint, string location);
        public virtual void RemoveLikeItemsFromRemaining(string replacement, List<string> remaining)
        {
            remaining.Remove(replacement);
        }
        public abstract void Clear();

        public abstract bool DoMaxPlacement();

        public abstract int GetPlacementDifficultyIndex();
        public virtual float GetPlacementDifficultyMultiplier()
        {
            switch(GetPlacementDifficultyIndex())
            {
                case 0:
                default:
                    return 1;
                case 1:
                    return 1.05f;
                case 2:
                    return 1.1f;
                case 3:
                    return 1.25f;
            }
        }

        public virtual Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (DoMaxPlacement())
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                float expBase = GetPlacementDifficultyMultiplier();
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)(Math.Pow(expBase, possDepths[s]) + GetAreaMult(s) * 16d));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }
        public virtual double GetAreaMult(string location)
        {
            return Math.Max(0.01, ItemLocations[location].Areas.Select(a => AreaMults[a]).Average());
        }

        public virtual Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }

        public virtual void SetLocationItem(string key, string item, int count)
        {
            throw new NotImplementedException("The item location type for " + key + " is not implemented.");
        }

        public Dictionary<string, int> GetItemsAvailable()
        {
            return GetItemsAvailable(Algorithm.Placement);
        }

        public virtual Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
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
    }
}
