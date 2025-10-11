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
        foreach (var placement in apData.ItemPlacements)
        {
            var (id, name, region, address) = placement;
            string idx = address.ToString("D4");
            string key = $"$zzz_r_ap_{idx}";
            if (!string.IsNullOrWhiteSpace(name))
            {
                mainSysUS[key] = name;
            }
            else if (!mainSysUS.Keys.Contains(key))
            {
                mainSysUS.Add(key, $"AP Item {idx}");
            }
            // Unique description per AP item            
            string descKey = "$zzz_r_aph_" + idx;
            string fromPart = (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(region))
                ? $" from {region}"
                : string.Empty;

            string desc = $"To be sent via Archipelago: {name}{fromPart}.";
            mainSysUS[descKey] = desc;
        }

        // Add text for key_r_multi_# items
        // Treated as being base-50 for each "digit"
        var multiItems = Generator.Get<EquipRando>().items.Values.Where(i => i.record.StartsWith("key_r_multi_"));
        foreach (var item in multiItems)
        {
            string idx = item.record.Split('_').Last();
            string key = $"$zzz_r_multi_{idx}";
            if (!mainSysUS.Keys.Contains(key))
            {
                mainSysUS.Add(key, $"AP Item Count Tracker #{idx}");
            }
            string descKey = "$zzz_r_multih_" + idx;
            string desc = $"Tracks the number of items received from Archipelago in base-50 counting (digit=count - 1).";
            mainSysUS[descKey] = desc;
        }

        // Add text for key_r_added
        if (!mainSysUS.Keys.Contains("$zzz_r_added"))
        {
            mainSysUS.Add("$zzz_r_added", "AP Item Added");
            mainSysUS.Add("$zzz_r_addedh", "Indicates an item has been received and added to your inventory.");
        }
    }
}
