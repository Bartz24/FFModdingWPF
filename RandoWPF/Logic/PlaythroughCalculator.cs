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

// Calculates a minimal path from start to victory using spheres calculated earlier.
public class PlaythroughCalculator<T> where T : ItemLocation
{
    public List<(T loc, int sphere)> FinalLocations { get; set; } = new();
    private SphereCalculator<T> SphereCalculator { get; set; }

    public PlaythroughCalculator(SphereCalculator<T> origSphereCalculator)
    {
        SphereCalculator = new SphereCalculator<T>(origSphereCalculator);
    }

    public void CalculatePlaythrough(HashSet<T> locationsIn, bool errorWhenInvalid = true)
    {
        Dictionary<T, int> spheres = new (SphereCalculator.Spheres);
        HashSet<T> locations = [.. locationsIn];

        // Work backwards the max sphere, removing items, and then recalculating the spheres to see if there is still a valid path to victory. If not, put the item back and move on to the next one.

        int victorySphere = spheres.Where(kvp => kvp.Key.GetItem(false)?.Item == "Victory")
                                   .Select(kvp => kvp.Value).FirstOrDefault();

        // Remove any locations that are in spheres greater than the victory sphere, as they cannot be required to reach victory.
        var locationsToRemove = spheres.Where(kvp => kvp.Value > victorySphere).Select(kvp => kvp.Key).ToList();
        foreach (var item in locationsToRemove)
        {
            locations.Remove(item);
        }

        spheres = spheres.Where(kvp => locations.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        int maxSphere = Math.Min(spheres.Values.Max(), victorySphere);
        int progress = 0;
        int maxProgress = locations.Count;
        for (int sphere = maxSphere; sphere >= 0; sphere--)
        {
            var locationsInSphere = spheres.Where(kvp => kvp.Value == sphere).Select(kvp => kvp.Key);

            foreach (var loc in locationsInSphere)
            {
                progress++;
                int percentComplete = (int)((progress / (float)maxProgress) * 100);
                RandoUI.SetUIProgressDeterminate($"Calculating Playthrough... ({percentComplete}%)", progress, maxProgress);

                // Skip the victory item itself
                if (loc.GetItem(false)?.Item == "Victory")
                {
                    continue;
                }

                // Temporarily remove this item from the location
                locations.Remove(loc);
                SphereCalculator.CalculateFromSphere(locations, sphere - 1, false);
                if (!SphereCalculator.FinalProgressionState.ItemsAvailable.ContainsKey("Victory"))
                {
                    // If victory is no longer reachable, add the item back to the location
                    locations.Add(loc);
                }
            }
        }

        // After we've removed all items that aren't necessary, we should have a minimal set of locations that are required to reach victory. We can verify this by calculating the spheres one last time and ensuring that victory is still reachable.
        SphereCalculator.CalculateSpheres(locations, errorWhenInvalid);

        FinalLocations = locations.Select(loc => (loc, SphereCalculator.Spheres.GetValueOrDefault(loc, -1))).OrderBy(kvp => kvp.Item2).ToList();
    }
}
