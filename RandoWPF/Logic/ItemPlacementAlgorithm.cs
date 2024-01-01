using Bartz24.Data;
using Bartz24.RandoWPF;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Bartz24.RandoWPF;

public class ItemPlacementAlgorithm<T> where T : ItemLocation
{

    public Dictionary<string, T> ItemLocations { get; set; } = new Dictionary<string, T>();

    public Dictionary<string, string> Placement { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, int> Depths { get; set; } = new Dictionary<string, int>();

    public List<string> HintsByLocation { get; set; } = new List<string>();
    public Dictionary<string, int> HintsByLocationsCount { get; set; } = new Dictionary<string, int>();

    public Dictionary<string, double> AreaMults { get; set; } = new Dictionary<string, double>();

    public SeedGenerator Generator { get; set; } = null;

    private ItemPlacementLogic<T> logic;
    public ItemPlacementLogic<T> Logic
    {
        get => logic;
        set
        {
            if (logic != null)
            {
                throw new Exception("Already set game logic for the algorithm.");
            }

            logic = value;
        }
    }

    public int Iterations { get; set; } = 0;

    protected int maxIterations;
    protected int importantCount;

    protected int maxFailCount;

    public ItemPlacementAlgorithm(Dictionary<string, T> itemLocations, List<string> hintsByLocations, SeedGenerator generator, int maxFail = -1)
    {
        ItemLocations = itemLocations;
        HintsByLocation = hintsByLocations;
        Generator = generator;
        maxFailCount = maxFail;
    }

    public virtual bool Randomize(List<string> defaultAreas, Dictionary<string, double> areaMults)
    {
        AreaMults = areaMults;
        List<string> allowed = Logic.GetKeysAllowed();
        List<string> place = Logic.GetKeysToPlace();

        SetHintValues(place);

        if (!DoImportantPlacement(allowed, place.Where(t => Logic.RequiresLogic(t)).ToList(), defaultAreas))
        {
            return false;
        }

        DoNonImportantPlacement(allowed, place);

        ApplyPlacement();
        return true;
    }

    protected virtual void ApplyPlacement()
    {
        foreach (string key in Placement.Keys.Where(l => !ItemLocations[l].Traits.Contains("Fake")))
        {
            string repKey = Placement[key];
            (string, int)? orig = ItemLocations[repKey].GetItem(true);
            ItemLocations[key].SetItem(orig.Value.Item1, orig.Value.Item2);
        }
    }

    protected virtual void DoNonImportantPlacement(List<string> allowed, List<string> place)
    {
        List<string> newKeys = place.Where(k => !Placement.ContainsValue(k)).Shuffle();
        foreach (string k in allowed.Where(k => !Placement.ContainsKey(k)).Shuffle())
        {
            Iterations++;
            if (newKeys.Count == 0)
            {
                break;
            }

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

        List<string> randomZeros = new();
        for (int j = 0; j < 10; j++)
        {
            if (RandomNum.RandInt(0, 99) < 10 && HintsByLocationsCount.Keys.Where(l => !randomZeros.Contains(l)).Count() > 0)
            {
                randomZeros.Add(HintsByLocationsCount.Keys.Where(l => !randomZeros.Contains(l)).Shuffle().First());
            }
        }

        float copMult = RandomNum.RandInt(12, 100) / 100f;

        for (int i = 0; i < keys.Where(t => Logic.IsHintable(t)).Count(); i++)
        {
            long weight(string loc)
            {
                if (randomZeros.Contains(loc))
                {
                    return 0;
                }

                int max = ItemLocations.Keys.Where(t => ItemLocations[t].Areas.Contains(loc) && !ItemLocations[t].Traits.Contains("Missable")).Count();
                if (max == 0)
                {
                    return 0;
                }

                long val = (long)(100 * Math.Max(0, Math.Pow(1 - (HintsByLocationsCount[loc] / (float)max), 4)));

                if (loc.Contains("CoP"))
                {
                    val = (long)(val * copMult);
                }

                return val;
            }

            string next = RandomNum.SelectRandomWeighted(HintsByLocationsCount.Keys.ToList(), weight);
            HintsByLocationsCount[next]++;
        }
    }

    protected virtual bool DoImportantPlacement(List<string> locations, List<string> important, List<string> defaultAreas)
    {
        maxIterations = Math.Max(2500, important.Count * 4);
        importantCount = important.Count;
        Logic.CalculateUsability();
        Logic.InitializeAllowMatrix();

        for (int i = 0; i < (maxFailCount == -1 ? int.MaxValue : maxFailCount); i++)
        {
            Placement.Clear();
            Depths.Clear();
            Logic.Clear();
            Iterations = 0;

            RandoUI.SetUIProgressIndeterminate(i > 0 ? "Retrying item placement..." : "Preparing item placement");
            Generator.Logger.LogInformation($"Starting item placement attempt {i + 1}.");

            bool output = TryImportantPlacement(i, new (locations), important.Shuffle(), new (defaultAreas));
            if (output)
            {
                Generator.Logger.LogInformation($"Item placement attempt {i + 1} succeeded.");
                return true;
            }
        }

        return false;
    }

    protected virtual void UpdateProgress(int i, int items, int maxItems)
    {
        RandoUI.SetUIProgressDeterminate($"Backup Item Placement Attempt {i + 1}" + (maxFailCount == -1 ? "" : $" of {maxFailCount}") + $" ({items} out of {maxItems} items placed, {Iterations} placement attempts made)", items, maxItems);
    }

    private List<string> locked = null;

    private List<string> OrderLocked(List<string> locations, List<string> remaining, List<string> important)
    {
        if (Placement.Count == 0)
        {
            locked = null;
        }

        locked ??= PrioritizeLockedItems(locations, remaining, important);
        return RandomNum.ShuffleLocalized(remaining.OrderBy(t => locked.IndexOf(t)).ToList(), 8);
    }

    protected virtual bool TryImportantPlacement(int attempt, List<string> locations, List<string> remaining, List<string> accessibleAreas)
    {
        Iterations++;
        if (Iterations > maxIterations)
        {
            return false;
        }

        if (Placement.Values.Distinct().Count() != Placement.Values.Count())
        {
            throw new Exception("Duplicate placements.");
        }

        Dictionary<string, int> items = Logic.GetItemsAvailable();
        UpdateProgress(attempt, Placement.Count, importantCount);

        List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, accessibleAreas).Where(a => !accessibleAreas.Contains(a)).ToList();
        accessibleAreas.AddRange(newAccessibleAreas);

        // If we still have depth logic required items to place, finish those.
        List<string> possibleRemaining = remaining.Where(t => Logic.RequiresDepthLogic(t)).Shuffle();

        // Otherwise use the whole list which should be junk
        bool placingJunk = false;
        if (possibleRemaining.Count == 0)
        {
            possibleRemaining = remaining.Shuffle();
            placingJunk = true;
        }

        if (possibleRemaining.Count > 0)
        {
            while (possibleRemaining.Count > 0)
            {
                string rep = possibleRemaining[0];
                (string, int)? nextItem = ItemLocations[rep].GetItem(true);

                List<string> possible = new();
                if (nextItem == null && ItemLocations[rep].Traits.Contains("Fake") && Logic.IsValid(rep, rep, items, accessibleAreas))
                {
                    possible.Add(rep);
                }
                else if (!placingJunk)
                {
                    possible = locations.Where(t => Logic.IsValid(t, rep, items, accessibleAreas)).ToList();
                }
                else
                {
                    possible = locations.Where(t => Logic.IsAllowed(t, rep)).ToList();
                }

                while (possible.Count > 0)
                {
                    string next;
                    int depth = -1;
                    if (!placingJunk)
                    {
                        (next, depth) = Logic.SelectNext(items, possible, rep);
                    }
                    else
                    {
                        next = possible[RandomNum.RandInt(0, possible.Count - 1)];
                    }

                    string hint = AddPlacement(locations, remaining, items, rep, next, depth);

                    if (remaining.Count == 0)
                    {
                        return true;
                    }

                    bool result = TryImportantPlacement(attempt, locations, remaining, accessibleAreas);
                    if (result)
                    {
                        return result;
                    }
                    else
                    {
                        if (Iterations > maxIterations)
                        {
                            return false;
                        }

                        RemovePlacement(locations, remaining, accessibleAreas, newAccessibleAreas, rep, next, possible, hint);
                    }
                }

                possibleRemaining.RemoveAt(0);
                Logic.RemoveLikeItemsFromRemaining(rep, possibleRemaining);
            }
        }

        return false;
    }

    private void RemovePlacement(List<string> locations, List<string> remaining, List<string> accessibleAreas, List<string> newAccessibleAreas, string rep, string next, List<string> possible, string hint = "")
    {
        possible.Remove(next);
        locations.Add(next);
        remaining.Add(rep);
        accessibleAreas.RemoveAll(a => newAccessibleAreas.Contains(a));
        Generator.Logger.LogDebug("Removed location " + Placement[next] + " at " + next + ".");
        Placement.Remove(next);
        Depths.Remove(next);
        if (Logic.IsHintable(rep) && !string.IsNullOrEmpty(hint))
        {
            Logic.RemoveHint(hint, next);
        }
    }

    private string AddPlacement(List<string> locations, List<string> remaining, Dictionary<string, int> items, string rep, string next, int depth = -1)
    {
        Placement.Add(next, rep);
        remaining.Remove(rep);
        locations.Remove(next);
        Generator.Logger.LogDebug($"Set Location {next} ({ItemLocations[next].Name}) to {rep} ({ItemLocations[rep].Name}).");
        if (depth >= 0)
        {
            Depths.Add(next, depth);
        }

        if (Logic.IsHintable(rep))
        {
            return Logic.AddHint( next, rep, depth);
        }
        return null;
    }

    protected List<string> PrioritizeLockedItems(List<string> locations, List<string> remaining, List<string> important)
    {
        Dictionary<string, int> locked = new();
        Dictionary<string, int> items = Logic.GetItemsAvailable(important.ToDictionary(l => l, l => l));

        foreach (string rep in remaining)
        {
            (string, int)? nextItem = ItemLocations[rep].GetItem(true);
            if (nextItem == null)
            {
                continue;
            }

            items[nextItem.Value.Item1] -= nextItem.Value.Item2;
            List<string> newAccessibleAreas = Logic.GetNewAreasAvailable(items, new List<string>());

            int possibleCount = locations.Where(t => !Placement.ContainsKey(t) && Logic.IsValid(t, rep, items, newAccessibleAreas)).Count();
            if (possibleCount == 1)
            {
                locked.Add(rep, ItemLocations[rep].Requirements.GetPossibleRequirementsCount());
            }

            items[nextItem.Value.Item1] += nextItem.Value.Item2;
        }

        return remaining.OrderByDescending(rep => locked.ContainsKey(rep) ? locked[rep] : -1).ToList();
    }
}
