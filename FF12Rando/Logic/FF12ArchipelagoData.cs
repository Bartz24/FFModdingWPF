using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;
public class FF12ArchipelagoData : ArchipelagoData
{
    public readonly record struct TreasureEntry(string MapName, int Index);

    /// <summary>Worlds that predate item_name leave ItemName empty and ItemFlags null.</summary>
    public readonly record struct SphereEntry(string ID, string Item, string ItemDisplay, int Index, int Sphere, string ItemName, string ItemPlayer, int? ItemFlags);

    public readonly record struct FillerItemPlacement(string ID, int Index, string Item, int Amount);

    public string Version { get; set; }
    public HashSet<string> UsedItems { get; set; }
    public List<TreasureEntry> Treasures { get; set; }
    public List<int> CharacterOrder { get; set; }
    public bool AllowSeitengrat { get; set; }
    public int ElixirShop { get; set; } = -1;
    public List<SphereEntry> Spheres { get; set; }
    public List<FillerItemPlacement> FillerItemPlacements { get; set; }

    public List<string> CompatibleAPVersions { get; set; } = new List<string>() { "0.7.2" };

    public FF12ArchipelagoData()
    {
    }

    public override void Parse(IDictionary<string, object> data)
    {
        Version = (string)data["version"];

        // Check if version starts with none of the compatible versions
        if (!CompatibleAPVersions.Any(v => Version.StartsWith(v)))
        {
            throw new RandoException("FF12 AP World version " + Version + " is not compatible with this version of the randomizer.", "Incompatible Version");
        }

        UsedItems = ((List<object>)data["used_items"]).Select(o => (string)o).ToHashSet();
        Treasures = ((List<object>)data["treasures"]).Select(o =>
        {
            var treasureData = (IDictionary<string, object>)o;
            return new TreasureEntry(MapName: (string)treasureData["map"], Index: (int)(long)treasureData["index"]);
        }).ToList();
        CharacterOrder = ((List<object>)data["character_order"]).Select(o => (int)(long)o).ToList();
        AllowSeitengrat = (long)data["allow_seitengrat"] == 1;
        ElixirShop = data.ContainsKey("elixir_shop") ? (int)(long)data["elixir_shop"] : -1;
        Spheres =((List<object>)data["spheres"]).Select(o =>
        {
            var sphereData = (IDictionary<string, object>)o;
            return new SphereEntry(
            ID: (string)sphereData["id"],
            Item: sphereData.ContainsKey("item") ? (string)sphereData["item"] : "",
            ItemDisplay: sphereData.ContainsKey("item_display") ? (string)sphereData["item_display"] : "",
            Index: sphereData.ContainsKey("index") ? (int)(long)sphereData["index"] : 0,
            Sphere: (int)(long)sphereData["sphere"],
            ItemName: sphereData.ContainsKey("item_name") ? (string)sphereData["item_name"] : "",
            ItemPlayer: sphereData.ContainsKey("item_player") ? (string)sphereData["item_player"] : "",
            ItemFlags: sphereData.ContainsKey("item_flags") ? (int?)(long)sphereData["item_flags"] : null);
        }).ToList();
        FillerItemPlacements = ((List<object>)data["filler_item_placements"]).Select(o =>
        {
            var placementData = (IDictionary<string, object>)o;
            return new FillerItemPlacement(
                       ID: (string)placementData["id"],
                       Index: (int)(long)placementData["index"],
                       Item: (string)placementData["item"],
                       Amount: (int)(long)placementData["amount"]);
        }).ToList();
    }

    public override IDictionary<string, object> ToJsonObj()
    {
        var treasures = Treasures.Select(t => new Dictionary<string, object>
        {
            { "map", t.MapName },
            { "index", t.Index }
        }).ToList();

        List<Dictionary<string, object>> spheres = Spheres.Select(s =>
            {
                Dictionary<string, object> sphere = new()
                {
                    { "id", s.ID },
                    { "item", s.Item },
                    { "item_display", s.ItemDisplay },
                    { "index", s.Index },
                    { "sphere", s.Sphere },
                    { "item_name", s.ItemName },
                    { "item_player", s.ItemPlayer }
                };

                // Left out when unknown, so a reload still tells an older world's data apart.
                if (s.ItemFlags != null)
                {
                    sphere["item_flags"] = s.ItemFlags.Value;
                }

                return sphere;
            }).ToList();

        List<Dictionary<string, object>> fillerItemPlacements = FillerItemPlacements.Select(f => new Dictionary<string, object>
        {
                { "id", f.ID },
                { "index", f.Index },
                { "item", f.Item },
                { "amount", f.Amount }
            }).ToList();

        return new Dictionary<string, object>
        {
            { "version", Version },
            { "used_items", UsedItems.ToList() },
            { "treasures", treasures },
            { "character_order", CharacterOrder },
            { "allow_seitengrat", AllowSeitengrat ? 1 : 0 },
            { "elixir_shop", ElixirShop },
            { "spheres", spheres },
            { "filler_item_placements", fillerItemPlacements }
        };
    }
}
