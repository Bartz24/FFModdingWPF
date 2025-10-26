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

    public void CalculatePlaythrough(HashSet<T> progressionLocations, bool errorWhenInvalid = true)
    {
        Dictionary<T, int> spheres = new (SphereCalculator.Spheres);
        HashSet<T> locations = new(progressionLocations);

        // Get possible requirements of all locations and connections to determine items that may be needed.
        HashSet<string> possibleRequiredItems = new();
        foreach (var loc in locations)
        {
            possibleRequiredItems.UnionWith(loc.Requirements.GetPossibleRequirements());
        }
        SphereCalculator.AreaGraph.Connections.ForEach(conn =>
        {
            possibleRequiredItems.UnionWith(conn.Requirements.GetPossibleRequirements());
        });

        // Remove locations that do not have any possible required items, as they cannot be necessary for progression.
        locations.RemoveWhere(loc => !possibleRequiredItems.Contains(loc.GetItem(false)?.Item));

        // Work backwards the max sphere, removing items, and then recalculating the spheres to see if there is still a valid path to victory. If not, put the item back and move on to the next one.

        int victorySphere = spheres.Where(kvp => kvp.Key.GetItem(false)?.Item == "Victory").Select(kvp => kvp.Value).FirstOrDefault();

        // Remove any locations that are in spheres greater than the victory sphere, as they cannot be required to reach victory.
        var locationsToRemove = spheres.Where(kvp => kvp.Value > victorySphere).Select(kvp => kvp.Key).ToList();
        foreach (var item in locationsToRemove)
        {
            locations.Remove(item);
        }

        spheres = spheres.Where(kvp => locations.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        int maxSphere = Math.Min(spheres.Values.Max(), victorySphere);
        for (int sphere = maxSphere; sphere >= 0; sphere--)
        {
            int percentComplete = (maxSphere - sphere) * 100 / maxSphere;
            RandoUI.SetUIProgressDeterminate($"Calculating Playthrough... ({percentComplete}%)", percentComplete, 100);
            var locationsInSphere = spheres.Where(kvp => kvp.Value == sphere).Select(kvp => kvp.Key);

            // Group the locations in this sphere by their item
            var groupedByItem = locationsInSphere.GroupBy(loc => loc.GetItem(false)?.Item);

            // Test each group, stop removing items from the group if we find that removing them causes victory to become unreachable. We can do this by temporarily removing the items from the locations, recalculating the spheres, and checking if victory is still reachable.
            foreach (var group in groupedByItem)
            {
                // Temporarily remove the items from the locations in this group
                foreach (var loc in group)
                {
                    locations.Remove(loc);
                }

                SphereCalculator.CalculateSpheres(locations, false);
                if (!SphereCalculator.FinalProgressionState.ItemsAvailable.ContainsKey("Victory"))
                {
                    // If victory is no longer reachable, add the items back to the locations and try again by removing individual items.
                    locations.UnionWith(group);

                    if (group.Count() == 1)
                    {
                        // If there is only one location in this group, we can't remove it, so we can skip it.
                        continue;
                    }

                    foreach (var loc in group)
                    {
                        // Temporarily remove this item from the location
                        locations.Remove(loc);
                        SphereCalculator.CalculateSpheres(locations, false);
                        if (!SphereCalculator.FinalProgressionState.ItemsAvailable.ContainsKey("Victory"))
                        {
                            // If victory is no longer reachable, add the item back to the location
                            locations.Add(loc);
                        }
                    }
                }
            }
        }

        // After we've removed all items that aren't necessary, we should have a minimal set of locations that are required to reach victory. We can verify this by calculating the spheres one last time and ensuring that victory is still reachable.
        SphereCalculator.CalculateSpheres(locations, errorWhenInvalid);

        FinalLocations = locations.Select(loc => (loc, SphereCalculator.Spheres.GetValueOrDefault(loc, -1))).OrderBy(kvp => kvp.Item2).ToList();
    }
}
