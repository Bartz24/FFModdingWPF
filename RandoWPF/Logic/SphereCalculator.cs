using Bartz24.Data;
using Bartz24.RandoWPF.Data.Areas;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Bartz24.RandoWPF;

// Calculates the spheres by going through the item locations and assigning them to the first sphere they can be in.
public class SphereCalculator<T> where T : ItemLocation
{
    public Dictionary<T, int> Spheres { get; set; } = new();
    public ProgressionState FinalProgressionState { get; set; } = new();

    public SeedGenerator Generator { get; set; }
    public AreaGraph AreaGraph { get; set; }

    private Dictionary<int, (ProgressionState state, HashSet<T> rem, Dictionary<T, int> spheres, HashSet<T> used)> stateCache = new();

    public SphereCalculator(SeedGenerator generator, AreaGraph areaGraph)
    {
        Generator = generator;
        AreaGraph = areaGraph;
    }

    public SphereCalculator(SphereCalculator<T> other)
    {
        Generator = other.Generator;
        AreaGraph = other.AreaGraph;
        Spheres = new Dictionary<T, int>(other.Spheres);
        FinalProgressionState = other.FinalProgressionState;
        stateCache = other.stateCache;
    }

    public void CalculateSpheres(OrderedSet<T> locations, bool errorWhenInvalid = true)
    {
        CalculateFromSphere(locations, 0, errorWhenInvalid);
    }

    public void CalculateFromSphere(OrderedSet<T> locations, int fromSphere, bool errorWhenInvalid = true)
    {
        if (!stateCache.ContainsKey(fromSphere))
        {
            fromSphere = 0;
        }

        // Clear cache at or after the fromSphere
        var keysToRemove = stateCache.Keys.Where(k => k > fromSphere);
        foreach (var key in keysToRemove)
        {
            stateCache.Remove(key);
        }

        ProgressionState state;
        HashSet<T> remaining;
        HashSet<T> used;
        if (fromSphere == 0)
        {
            state = new();
            remaining = [.. locations.Where(l => !l.Traits.Contains("Missable"))];
            Spheres = new();
            used = new HashSet<T>();
        }
        else
        {
            var cached = stateCache[fromSphere];
            state = new ProgressionState(cached.state);
            remaining = new HashSet<T>(cached.rem);
            Spheres = new Dictionary<T, int>(cached.spheres);
            used = new HashSet<T>(cached.used);

            remaining.RemoveWhere(l => !locations.Contains(l));
        }

        for (int sphere = fromSphere; remaining.Count > 0; sphere++)
        {
            // Add the current state to the cache
            if (sphere != fromSphere)
            {
                stateCache[sphere] = (new ProgressionState(state), new HashSet<T>(remaining), new Dictionary<T, int>(Spheres), new HashSet<T>(used));
            }

            bool valid = ProcessSphere(errorWhenInvalid, state, remaining, used, sphere);
            if (!valid)
            {
                return;
            }
        }

        FinalProgressionState = state;
    }

    private bool ProcessSphere(bool errorWhenInvalid, ProgressionState state, HashSet<T> remaining, HashSet<T> used, int sphere)
    {
        // Hide progress cuz spoilers :)
        //RandoUI.SetUIProgressIndeterminate($"Calculating sphere {sphere} items.");
        // Generator.Logger.LogDebug($"Calculating sphere {sphere} items.");
        state.AreasAccessible.UnionWith(AreaGraph.GetAllAccessibleAreas("Initial", state).Select(a => a.Name));

        HashSet<T> addedThisSphere = new();
        bool valid = false;
        foreach (T loc in remaining)
        {
            if (loc.AreItemReqsMet(state))
            {
                valid = true;

                Spheres.Add(loc, sphere);
                used.Add(loc);

                if (loc.GetItem(false) != null)
                {
                    addedThisSphere.Add(loc);
                }
            }
        }

        remaining.RemoveWhere(l => used.Contains(l));

        foreach (var loc in addedThisSphere)
        {
            if (loc.GetItem(false) != null)
            {
                (string itemID, int amount) = loc.GetItem(false).Value;
                if (state.ItemsAvailable.ContainsKey(itemID))
                {
                    state.ItemsAvailable[itemID] += amount;
                }
                else
                {
                    state.ItemsAvailable.Add(itemID, amount);
                }
            }
            state.LocationsCompleted.Add(loc.ID);
        }

        // TODO:
        // Improve validation for "missable" quests like buried passion where it depends on placement but is safe
        // Improve chain checks            

        if (!valid)
        {
            if (errorWhenInvalid)
            {
                Generator.Logger.LogDebug($"Remaining locations: {string.Join(",", remaining.Select(r => r.ID))}");
                string msg = "Could not find a path to all items placed. This seed might be unbeatable. Report this to the dev with the seed and flags used. After this seed finishes generating, go to the History tab and share the seed.";
                Generator.Logger.LogError(msg);
                MessageBox.Show(msg);
            }

            FinalProgressionState = state;

            return false;
        }

        return true;
    }
}
