using Bartz24.RandoWPF.Data.Areas;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF;

// Calculates the spheres by going through the item locations and assigning them to the first sphere they can be in.
public class SphereCalculator<T> where T : ItemLocation
{
    public Dictionary<T, int> Spheres { get; set; } = new();

    private SeedGenerator Generator { get; set; }
    private AreaGraph AreaGraph { get; set; }

    public SphereCalculator(SeedGenerator generator, AreaGraph areaGraph)
    {
        Generator = generator;
        AreaGraph = areaGraph;
    }

    public void CalculateSpheres(HashSet<T> locations, bool errorWhenInvalid = true)
    {
        Spheres.Clear();
        ProgressionState state = new();

        HashSet<T> remaining = new(locations.Where(l => !l.Traits.Contains("Missable")));

        HashSet<T> used = new();

        for (int sphere = 0; remaining.Count > 0; sphere++)
        {
            RandoUI.SetUIProgressIndeterminate($"Calculating sphere {sphere} items.");
            Generator.Logger.LogDebug($"Calculating sphere {sphere} items.");
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

                return;
            }
        }
    }
}
