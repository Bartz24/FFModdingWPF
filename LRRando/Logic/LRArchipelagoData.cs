using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando;
public class LRArchipelagoData : ArchipelagoData
{
	public string Version { get; set; } = string.Empty;

	// All item placements with item names and where they are placed (location name/region)
	public List<(string ID, string Name, string Region)> ItemPlacements { get; set; } = new();

	public List<string> CompatibleAPVersions { get; set; } = new List<string>() { "0.1.0" };

	public override void Parse(IDictionary<string, object> data)
	{
		Version = (string)data["version"];

		// Check if version starts with one of the compatible versions
		if (!CompatibleAPVersions.Any(v => Version.StartsWith(v)))
		{
			throw new RandoException("LR AP World version " + Version + " is not compatible with this version of the randomizer.", "Incompatible Version");
		}

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
					Region: placement.ContainsKey("region") ? (string)placement["region"] : string.Empty
				);
			}).ToList();
		}
		else
		{
			ItemPlacements = new();
		}
	}

	public override IDictionary<string, object> ToJsonObj()
	{
		var itemPlacements = ItemPlacements.Select(p => new Dictionary<string, object>
		{
			{ "id", p.ID },
			{ "name", p.Name },
			{ "region", p.Region }
		}).ToList();

		return new Dictionary<string, object>
		{
			{ "version", Version },
			{ "item_placements", itemPlacements }
		};
	}
}
