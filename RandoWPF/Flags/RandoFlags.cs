using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Bartz24.RandoWPF;

public class RandoFlags
{
    public static List<Flag> FlagsList { get; set; } = new List<Flag>();

    public const int FlagTypeDebug = int.MaxValue;
    public const int FlagTypeAll = -1;

    public static Dictionary<int, string> CategoryMap { get; set; } = new Dictionary<int, string>();

    public static List<string> CategoryList => CategoryMap.Keys.OrderBy(i => i).Select(i => CategoryMap[i]).ToList();
    public static string SelectedCategory
    {
        get => selected;
        set
        {
            selected = value;
            SelectedChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool ApplyingPreset = false;
    private static string selected;

    public static event EventHandler SelectedChanged = delegate { };

    public static string Serialize(string seed, string version)
    {
        JObject o = JObject.FromObject(new
        {
            seed,
            version,
            preset = RandoPresets.Selected.Name,
            flags = FlagsList.Where(f => !f.Debug).ToList()
        });
        return o.ToString();
    }
    public static string LoadSeed(string file)
    {
        return Deserialize(File.ReadAllText(file));
    }

    public static (string seed, string version, string preset) GetSeedInfo(string json)
    {
        IDictionary<string, object> data = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

        string seed = (string)data["seed"];
        string version = (string)data["version"];
        string preset = data.ContainsKey("preset") ? (string)data["preset"] : "Unknown from previous version";

        return (seed, version, preset);
    }

    public static string Deserialize(string json)
    {
        IDictionary<string, object> data = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

        string seed = (string)data["seed"];
        string version = (string)data["version"];
        string preset = data.ContainsKey("preset") ? (string)data["preset"] : "Unknown from previous version";

        FlagsList.ForEach(f => f.FlagEnabled = false);

        ((List<object>)data["flags"]).Select(o => (IDictionary<string, object>)o).ToList().ForEach(s =>
        {
            if (FlagsList.Where(f => f.FlagID == (string)s["FlagID"]).Count() == 0)
            {
                return;
            }

            Flag f = FlagsList.First(f => f.FlagID == (string)s["FlagID"]);
            f.FlagEnabled = (bool)s["FlagEnabled"];
            ((List<object>)s["FlagProperties"]).Select(o => (IDictionary<string, object>)o).ToList().ForEach(p =>
            {
            if (f.FlagProperties.Where(fp => fp.ID == (string)p["ID"]).Count() == 0)
                {
                    return;
                }

                FlagProperty prop = f.FlagProperties.First(fp => fp.ID == (string)p["ID"]);
                prop.Deserialize(p);
            });
        });

        return seed;
    }
}
