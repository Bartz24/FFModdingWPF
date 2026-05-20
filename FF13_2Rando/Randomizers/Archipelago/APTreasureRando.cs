using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;

public class APTreasureRando: TreasureRando
{
    public APTreasureRando(SeedGenerator generator): base(generator)
    {

    }

    public override void Randomize()
    {
        // Then overwrite with unique AP items according to FF13_2ArchipelagoData order
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();
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
    }
}
