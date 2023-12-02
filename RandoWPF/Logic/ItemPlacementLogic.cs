using Bartz24.Data;
using Bartz24.RandoWPF;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public abstract class ItemPlacementLogic<T> where T : ItemLocation
{
    protected ItemPlacementAlgorithm<T> Algorithm { get; set; }
    public ItemPlacementLogic(ItemPlacementAlgorithm<T> algorithm)
    {
        Algorithm = algorithm;
    }

    public Dictionary<string, T> ItemLocations => Algorithm.ItemLocations;
    public Dictionary<string, double> AreaMults => Algorithm.AreaMults;

    public Dictionary<string, string> Placement => Algorithm.Placement;

    public Dictionary<string, int> LocationUsability = new();

    // Used to cache allowed replacements of locations
    public AllowMatrix AllowMatrix { get; set; }

    public int MaxUsability => LocationUsability.Values.Max();

    public virtual void InitializeAllowMatrix()
    {
        AllowMatrix = new(ItemLocations.Count, ItemLocations.Keys.ToList());
    }

    public virtual void CalculateUsability()
    {
        Dictionary<T, List<string>> locPossible = ItemLocations.Values.ToDictionary(l => l, l => l.Requirements.GetPossibleRequirements());

        LocationUsability = new ();
        foreach (string key in ItemLocations.Keys)
        {
            string locItem = GetLocationItem(key)?.Item1;
            if (locItem != null)
            {
                LocationUsability.Add(key, locPossible.Where(l => l.Value.Contains(locItem)).Count());
            }
            else
            {
                LocationUsability.Add(key, 0);
            }
        }
    }

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
    public virtual bool IsAllowed(string old, string rep)
    {
        if (!AllowMatrix.HasAllow(old, rep))
        {
            bool allowed = IsAllowedReplacement(old, rep);
            AllowMatrix.AddAllow(old, rep, allowed);
            return allowed;
        }

        return AllowMatrix.IsAllowed(old, rep);
    }
    protected abstract bool IsAllowedReplacement(string old, string rep);
    public abstract int GetNextDepth(Dictionary<string, int> items, string location);
    public abstract bool RequiresDepthLogic(string location);
    public abstract bool IsHintable(string location);

    public virtual List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
    {
        return soFar.ToList();
    }
    public abstract string AddHint(string location, string replacement, int itemDepth);
    public abstract void RemoveHint(string hint, string location);
    public virtual void RemoveLikeItemsFromRemaining(string replacement, List<string> remaining)
    {
        remaining.Remove(replacement);
    }
    public abstract void Clear();

    public abstract int GetPlacementDifficultyIndex();
    public virtual float GetPlacementDifficultyMultiplier()
    {
        return GetPlacementDifficultyIndex() switch
        {
            1 => 1.05f,
            2 => 1.1f,
            3 => 1.25f,
            4 => 2f,
            _ => 1,
        };
    }

    public virtual (string, int) SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
    {
        float mult = GetPlacementDifficultyMultiplier();
        Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));

        Dictionary<string, long> distribution = possible.ToDictionary(s => ItemLocations[s].Name + " " + s, s => LocWeight(items, s, mult, possDepths, Math.Max(3.2 - mult, 0)));

        string next = RandomNum.SelectRandomWeighted(possible, (Func<string, long>)(s => LocWeight(items, s, mult, possDepths, Math.Max(3.2 - mult, 0))), true);
        if (next == null)
        {
            next = RandomNum.SelectRandomWeighted(possible, (Func<string, long>)(s => LocWeight(items, s, mult, possDepths, 1)), true);
        }

        return (next, possDepths[next]);

        long LocWeight(Dictionary<string, int> items, string s, float mult, Dictionary<string, int> possDepths, double areaMult)
        {
            return (long)(Math.Pow(mult, (possDepths[s] + ItemLocations[s].GetDifficulty(items)) * (1.5 * LocationUsability[rep] / MaxUsability + 1))   
                          + (areaMult * GetAreaMult(s) * 32));
        }
    }
    public virtual double GetAreaMult(string location)
    {
        return Math.Max(0, ItemLocations[location].Areas.Select(a => AreaMults[a]).Average());
    }

    public virtual (string, int)? GetLocationItem(string key, bool orig = true)
    {
        throw new NotImplementedException("The item location type for " + key + " is not implemented.");
    }

    public virtual void SetLocationItem(string key, string item, int count)
    {
        throw new NotImplementedException("The item location type for " + key + " is not implemented.");
    }

    protected virtual void LogSetItem(string key, string item, int count)
    {
        Algorithm.Generator.Logger.LogDebug("Set Item Location \"" + key + "\" to [" + item + " x" + count + "]");
    }

    public Dictionary<string, int> GetItemsAvailable()
    {
        return GetItemsAvailable(Algorithm.Placement);
    }

    public virtual Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
    {
        Dictionary<string, int> dict = new();
        placement.ForEach(p =>
        {
            (string, int)? tuple = GetLocationItem(p.Value, false);
            if (tuple != null)
            {
                string item = tuple.Value.Item1;
                int amount = tuple.Value.Item2;
                if (dict.ContainsKey(item))
                {
                    dict[item] += amount;
                }
                else
                {
                    dict.Add(item, amount);
                }
            }
        });
        return dict;
    }
}
