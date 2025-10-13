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
        foreach (var placement in apData.ItemPlacements)
        {
            var (ID, Name, Region, Address) = placement;
            string idx = Address.ToString("D4");
            string itemId = $"key_r_ap_{idx}";

            // Find matching location by ID (CSV-defined IDs should match placement.ID)
            if (ItemLocations.TryGetValue(ID, out var loc))
            {
                loc.SetItem(itemId, 1);
            }
            else
            {
                throw new Exception($"AP Item placement ID '{ID}' not found in item locations.");
            }
        }

        // Build a quick lookup for local item placements
        var localMap = apData.LocalItemPlacements.ToDictionary(p => p.LocationID, p => (p.ItemID, p.Amount), StringComparer.Ordinal);
        foreach (var loc in localMap.Keys)
        {
            if (ItemLocations.TryGetValue(loc, out var location))
            {
                var (itemId, amount) = localMap[loc];
                location.SetItem(itemId, amount);
            }
            else
            {
                throw new Exception($"Local item placement ID '{loc}' not found in item locations.");
            }
        }

        HandleIDCardBuyOption();
    }

    public override Dictionary<string, HTMLPage> GetDocumentation()
    {
        // Documentation not supported for AP rando
        return new Dictionary<string, HTMLPage>();
    }
}
