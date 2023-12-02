using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF;

// Calculates the spheres by going through the item locations and assigning them to the first sphere they can be in.
public class SphereCalculator
{
    public static Dictionary<string, int> CalculateSpheres<T>(ItemPlacementLogic<T> logic) where T : ItemLocation
    {
        Dictionary<string, int> spheres = new();

        Dictionary<string, int> items = new();

        List<string> remaining = new(logic.ItemLocations.Keys.Where(k => !logic.ItemLocations[k].Traits.Contains("Missable")));

        List<string> used = new();

        for (int sphere = 0; remaining.Count > 0; sphere++)
        {
            bool valid = false;
            foreach (string loc in remaining)
            {
                if (logic.ItemLocations[loc].IsValid(items))
                {
                    valid = true;

                    spheres.Add(loc, sphere);
                    used.Add(loc);
                }
            }

            remaining.RemoveAll(l => used.Contains(l));

            items = logic.GetItemsAvailable(used.ToDictionary(l => l, l => l));

            if (!valid)
            {
                MessageBox.Show("Could not find a path to all items placed. This seed might be unbeatable. Report this to the dev with the seed and flags used. After this seed finishes generating, you can click on \"Share current seed\" and send the JSON seed file.");

                return spheres;
            }
        }

        return spheres;
    }
}
