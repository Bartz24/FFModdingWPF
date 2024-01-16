using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF.Logic;
public class ItemReorderer<T, I> where T : ItemLocation where I : IItem
{
    protected HashSet<string> Categories { get; set; } = new();

    protected Dictionary<string, I> Items { get; set; }

    protected SeedGenerator Generator { get; set; }

    public ItemReorderer(SeedGenerator generator, HashSet<string> categories, Dictionary<string, I> items)
    {
        Categories = categories;
        Items = items;
        Generator = generator;
    }

    public void ReorderItems(HashSet<T> locations, SphereCalculator<T> sphereCalculator)
    {
        RandoUI.SetUIProgressIndeterminate($"Reordering items.");
        // Get all the locations with matching items and group by their item type
        var grouping = locations.Where(l =>
        {
            string id = l.GetItem(false)?.Item;
            if (id == null || !Items.ContainsKey(id) || !Categories.Contains(Items[id].Category) || Items[id].Traits.Contains("Ignore"))
            {
                return false;
            }

            return true;
        }).GroupBy(l =>
        {
            string id = l.GetItem(false)?.Item;
            return Items[id].Category;
        });

        // Group by type and sort the items by its item rank
        foreach (var group in grouping)
        {
            List<(string id, int count)> items = group.Shuffle().Select(l => l.GetItem(false).Value).OrderBy(pair =>
            {
                return Items[pair.Item].Rank;
            }).ToList();
            items = RandomNum.ShuffleLocalized(items, 5);

            // Sort the junk locations by sphere
            List<T> junk = group.Shuffle().OrderBy(l => sphereCalculator.Spheres.GetValueOrDefault(l, 0)).ToList();

            // Go in order and set the junk items
            for (int i = 0; i < items.Count; i++)
            {
                (string id, int count) = items[i];
                junk[i].SetItem(id, count);
            }

            RandoUI.SetUIProgressDeterminate($"Reordered {group.Key} items.", grouping.ToList().IndexOf(group) + 1, grouping.Count());
            Generator.Logger.LogDebug($"Reordered {group.Key} items.");
        }
    }
}
