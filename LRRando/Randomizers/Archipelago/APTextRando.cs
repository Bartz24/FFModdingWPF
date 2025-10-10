using Bartz24.RandoWPF;
using System.Linq;

namespace LRRando;
public class APTextRando : TextRando
{
    public APTextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        base.Load();

        // Create display names for unique AP items based on LRArchipelagoData
        var apData = RandoFlags.GetArchipelagoData<LRArchipelagoData>();
        for (int i = 0; i < apData.ItemPlacements.Count; i++)
        {
            string idx = (i + 1).ToString("D4");
            string key = $"$zzz_r_ap_{idx}";
            string name = apData.ItemPlacements[i].Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                mainSysUS[key] = name;
            }
            else if (!mainSysUS.Keys.Contains(key))
            {
                mainSysUS.Add(key, $"AP Item {idx}");
            }
            // Unique description per AP item
            string region = apData.ItemPlacements[i].Region;
            string locName = apData.ItemPlacements[i].Name;
            
            string descKey = "$zzz_r_aph_" + idx;
            string fromPart = (!string.IsNullOrWhiteSpace(locName) || !string.IsNullOrWhiteSpace(region))
                ? $" from {region}."
                : string.Empty;

            string desc = $"To be sent via Archipelago: {name}{fromPart}.";
            mainSysUS[descKey] = desc;
        }
    }
}
