using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class APTreasureRando : TreasureRando
{
    public APTreasureRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    public override void Randomize()
    {
        // Then overwrite with unique AP items according to LRArchipelagoData order
        var apData = RandoFlags.GetArchipelagoData<LRArchipelagoData>();
        // Build a quick lookup for local item placements
        var localMap = apData.LocalItemPlacements.ToDictionary(p => p.LocationID, p => p.ItemID, StringComparer.Ordinal);
        for (int i = 0; i < apData.ItemPlacements.Count; i++)
        {
            var (ID, Name, Region, Address) = apData.ItemPlacements[i];
            string idx = Address.ToString("D4");
            string itemId = $"key_r_ap_{idx}";

            // Find matching location by ID (CSV-defined IDs should match placement.ID)
            if (ItemLocations.TryGetValue(ID, out var loc))
            {
                // If the location has the 'Same' trait, use the local item placement instead of the AP-specific item
                bool same = loc.Traits != null && loc.Traits.Any(t => t == "Same");
                if (same && localMap.TryGetValue(ID, out var localItem) && !string.IsNullOrWhiteSpace(localItem))
                {
                    loc.SetItem(localItem, 1);
                }
                else
                {
                    loc.SetItem(itemId, 1);
                }
            }
            else
            {
                throw new Exception($"AP Item placement ID '{ID}' not found in item locations.");
            }
        }
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        // Documentation not supported for AP rando
        return new Dictionary<string, HTMLPage>();
    }
}
