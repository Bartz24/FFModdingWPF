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

    public SphereCalculator(SeedGenerator generator)
    {
        Generator = generator;
    }

    public void CalculateSpheres(HashSet<T> locations)
    {
        Spheres.Clear();
        Dictionary<string, int> items = new();

        HashSet<T> remaining = new(locations.Where(l => !l.Traits.Contains("Missable")));

        HashSet<T> used = new();

        for (int sphere = 0; remaining.Count > 0; sphere++)
        {
            RandoUI.SetUIProgressIndeterminate($"Calculating sphere {sphere} items.");
            Generator.Logger.LogDebug($"Calculating sphere {sphere} items.");

            HashSet<T> addedThisSphere = new();
            bool valid = false;
            foreach (T loc in remaining)
            {
                if (loc.AreItemReqsMet(items))
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
                (string itemID, int amount) = loc.GetItem(false).Value;
                if (items.ContainsKey(itemID))
                {
                    items[itemID] += amount;
                }
                else
                {
                    items.Add(itemID, amount);
                }
            }

            if (!valid)
            {
                string msg = "Could not find a path to all items placed. This seed might be unbeatable. Report this to the dev with the seed and flags used. After this seed finishes generating, go to the History tab and share the seed.";
                Generator.Logger.LogError(msg);
                MessageBox.Show(msg);

                return;
            }
        }
    }
}
