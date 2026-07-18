using Bartz24.FF13_2;
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

    protected override (int, int, int) setupWinCondition()
    {
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();
        var winConditionOptions = apData.WinCondition;
        return (
            winConditionOptions.condition,
            winConditionOptions.count,
            winConditionOptions.finalBosses ? 1 : 2
        );
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
        APHistoriaCruxRando cruxRando = Generator.Get<APHistoriaCruxRando>();
        cruxRando.CalculateAreaSpheres(apData.Spheres.Where(i => i.Item.StartsWith("access_")).ToDictionary(i => i.Item, i => i.Sphere));
    }

    protected override void SaveHints()
    {
        HistoriaCruxRando cruxRando = Generator.Get<HistoriaCruxRando>();
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();
        List<string> gravitonCoreNames = new() { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta" };
        for (var i = 1; i < 8; i++)
        {
            // Graviton core location hints
            var gravitonCoreItemId = $"frg_cmn_gvtn00{i}";
            var gravitonCoreHintTextId = $"$cap_core_0{i}_p1";

            var indexName = gravitonCoreNames[i - 1];
            
            // Lookup from apdata if graviton core is in 13-2 or not
            // if it is, give a local hint
            // if it's not, just give a generic message saying we don't know where it is

            var updatedText = $$"""Due to intense multiversal paradox interference, we have been unable to locate any clear signs of this fragment."""+
                """{Text NewLine}{Text NewLine}It may not even be within our world any more.""";

            textRando.mainSysUS[gravitonCoreHintTextId] = updatedText;
        }
    }
}
