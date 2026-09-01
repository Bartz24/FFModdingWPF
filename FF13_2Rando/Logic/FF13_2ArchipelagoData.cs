using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;


public struct FF13_2AreaLink
{
    public string link_name { get; set; }
    public string target_area { get; set; }
}

public struct FF13_2AreaNode
{
    public int loc_x { get; set; }
    public int loc_y { get; set; }

    public List<FF13_2AreaLink> links { get; set; }
}

public struct FF13_2WinCondition
{
    public FF13_2WinCondition(int a, int b, bool c)
    {
        condition = a;
        count = b;
        finalBosses = c;
    }
    public int condition { get; set; }
    public int count { get; set; }
    public bool finalBosses { get; set; }
}

public class FF13_2ArchipelagoData: ArchipelagoData
{
    public string Version { get; set; } = string.Empty;

    // All item placements with item names and where they are placed (location name/region)
    public List<(string ID, string Name, string Region, int Address)> ItemPlacements { get; set; } = new();

    // All local item placements with their local string IDs
    public List<(string LocationID, string ItemID, int Amount)> LocalItemPlacements { get; set; } = new();


    public Dictionary<string, FF13_2AreaNode> AreaGraph { get; set; } = new();

    public HashSet<string> UsedItems { get; set; } = new();

    public bool AllowDLCItems { get; set; } = false;

    public List<string> CompatibleAPVersions { get; set; } = new List<string>() { "0.1.1" };

    public List<(string ID, string Item, int Index, int Sphere)> Spheres { get; set; }

    public FF13_2WinCondition WinCondition { get; set; } = new(0, 0, true);

    public override void Parse(IDictionary<string, object> data)
    {
        Version = (string)data["version"];

        // Check if version starts with one of the compatible versions
        if (!CompatibleAPVersions.Any(v => Version.StartsWith(v)))
        {
            throw new RandoException("FF13-2 AP World version " + Version + " is not compatible with this version of the randomizer.", "Incompatible Version");
        }

        UsedItems = ((List<object>)data["used_items"]).Select(o => (string)o).ToHashSet();

        Spheres = ((List<object>)data["spheres"]).Select(o =>
        {
            var sphereData = (IDictionary<string, object>)o;
            return (
            ID: (string)sphereData["id"],
            Item: sphereData.ContainsKey("item") ? (string)sphereData["item"] : "",
            Index: sphereData.ContainsKey("index") ? (int)(long)sphereData["index"] : 0,
            Sphere: (int)(long)sphereData["sphere"]);
        }).ToList();

        // Expected structure similar to FF12's filler placements but generalized:
        // item_placements: [ { id, name, region } ]
        if (data.ContainsKey("item_placements"))
        {
            ItemPlacements = ((List<object>)data["item_placements"]).Select(o =>
            {
                var placement = (IDictionary<string, object>)o;
                return (
                    ID: placement.ContainsKey("id") ? (string)placement["id"] : string.Empty,
                    Name: placement.ContainsKey("name") ? (string)placement["name"] : string.Empty,
                    Region: placement.ContainsKey("region") ? (string)placement["region"] : string.Empty,
                    Address: placement.ContainsKey("address") ? Convert.ToInt32(placement["address"]) : -1
                );
            }).ToList();
        }
        else
        {
            ItemPlacements = new();
        }

        // local_item_placements: [ { location_id, item_id, amount } ]
        if (data.ContainsKey("local_item_placements"))
        {
            LocalItemPlacements = ((List<object>)data["local_item_placements"]).Select(o =>
            {
                var placement = (IDictionary<string, object>)o;
                return (
                    LocationID: placement.ContainsKey("location_id") ? (string)placement["location_id"] : string.Empty,
                    ItemID: placement.ContainsKey("item_id") ? (string)placement["item_id"] : string.Empty,
                    Amount: placement.ContainsKey("amount") ? Convert.ToInt32(placement["amount"]) : 1
                );
            }).ToList();
        }
        else
        {
            LocalItemPlacements = new();
        }

        if (data.ContainsKey("area_graph"))
        {
            // "area_graph": {
            //   "h_bj_AD0005": {
            //    "loc_x": 3, "loc_y": 5,
            //     links = [{link_name:"hs_bjaa01_gy","target_area":"h_gy_AD0010"},
            //            {link_name:"hs_bjaa02_zz","target_area":"h_zz_NA0910"},
            //            {link_name:"hs_bjaa03_bj","target_area":"h_bj_AD0300"}]
            //    },
            // }
            AreaGraph = ((IDictionary<string, object>)data["area_graph"]).ToDictionary(kv => kv.Key, kv =>
            {
                var val = (IDictionary<string, object>)kv.Value;
                var node = new FF13_2AreaNode();
                node.loc_x = Convert.ToInt32(val["loc_x"]);
                node.loc_y = Convert.ToInt32(val["loc_y"]);
                node.links = ((List<object>)val["links"]).Select(link =>
                {
                    var asDict = (IDictionary<string, object>)link;
                    var newLink = new FF13_2AreaLink();
                    newLink.link_name = (string)asDict["link_name"];
                    newLink.target_area = (string)asDict["target_area"];
                    return newLink;
                }).ToList();
                return node;
            });
        }

        if (data.ContainsKey("win_condition"))
        {
            var winData = (IDictionary<string, object>)data["win_condition"];
            int winConditionType = winData.ContainsKey("type") ? Convert.ToInt32(winData["type"]) : 0;
            int fragCount = winData.ContainsKey("fragment_count") ? Convert.ToInt32(winData["fragment_count"]) : 0;
            bool finalBoss = winData.ContainsKey("require_final") ? Convert.ToBoolean(winData["require_final"]) : true;
            WinCondition = new(winConditionType, fragCount, finalBoss);
        }

        AllowDLCItems = data.ContainsKey("allow_dlc_items") && (bool)data["allow_dlc_items"];
    }

    public override IDictionary<string, object> ToJsonObj()
    {
        var itemPlacements = ItemPlacements.Select(p => new Dictionary<string, object>
        {
            { "id", p.ID },
            { "name", p.Name },
            { "region", p.Region },
            { "address", p.Address }
        }).ToList();

        var localItemPlacements = LocalItemPlacements.Select(p => new Dictionary<string, object>
        {
            { "location_id", p.LocationID },
            { "item_id", p.ItemID },
            { "amount", p.Amount }
        }).ToList();

        return new Dictionary<string, object>
        {
            { "version", Version },
            { "used_items", UsedItems.ToList() },
            { "item_placements", itemPlacements },
            { "local_item_placements", localItemPlacements },
            { "allow_dlc_items", AllowDLCItems }
        };
    }
}
