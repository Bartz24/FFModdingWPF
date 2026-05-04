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
    public const int FlagTypeAll = -2;
    public const int FlagTypeArchipelago = -1;

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
        if (Mode == SeedMode.Normal)
        {
            JObject o = JObject.FromObject(new
            {
                seed,
                version,
                preset = RandoPresets.Selected.Name,
                type = "normal",
                flags = FlagsList.Where(f => !f.Debug).ToList()
            });
            return o.ToString();
        }
        else if (Mode == SeedMode.Archipelago)
        {
            JObject o = JObject.FromObject(new
            {
                seed,
                version,
                preset = RandoPresets.Selected.Name,
                type = "archipelago",
                flags = FlagsList.Where(f => !f.Debug).ToList(),
                archipelago = ArchipelagoData.ToJsonObj()
            });
            return o.ToString();
        }
        else
        {
            throw new Exception("Invalid seed mode");
        }
    }
    public static string LoadSeed(string file)
    {
        return Deserialize(File.ReadAllText(file));
    }

    private static SeedMode GetSeedMode(string mode)
    {
        return mode.ToLower() == "archipelago" ? SeedMode.Archipelago : SeedMode.Normal;
    }

    public static (string seed, SeedMode type, ArchipelagoData apData, string version, string preset) GetSeedInfo(string json)
    {
        IDictionary<string, object> data = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

        string seed = (string)data["seed"];
        string version = data.ContainsKey("version") ? (string)data["version"] : "N/A";
        SeedMode type = data.ContainsKey("type") ? GetSeedMode((string)data["type"]) : SeedMode.Normal;
        string preset = data.ContainsKey("preset") ? (string)data["preset"] : "Unknown from previous version";

        ArchipelagoData apData = null;
        if (ArchipelagoDataType != null)
        {
            apData = (ArchipelagoData)Activator.CreateInstance(ArchipelagoDataType);
            if (data.ContainsKey("archipelago"))
            {
                apData.Parse((IDictionary<string, object>)data["archipelago"]);
            }
        }

        return (seed, type, apData, version, preset);
    }

    public static string Deserialize(string json)
    {
        IDictionary<string, object> data = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

        SeedMode previousMode = Mode;
        (string seed, Mode, ArchipelagoData, string version, string preset) = GetSeedInfo(json);

        if (Mode != previousMode)
        {
            FlagsList.ForEach(f =>
            {
                f.ModeChanged();
            });
        }

        if (data.ContainsKey("flags"))
        {
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
        }

        if (Mode == SeedMode.Archipelago)
        {
            SelectedCategory = CategoryMap[FlagTypeArchipelago];
        }
        else
        {
            SelectedCategory = SelectedCategory == CategoryMap[FlagTypeArchipelago] ? CategoryMap[FlagTypeAll] : SelectedCategory;
        }

        return seed;
    }

    public enum SeedMode
    {
        Normal,
        Archipelago
    }

    public static SeedMode Mode { get; set; } = SeedMode.Normal;

    // Store a class type which inherits from ArchipelagoData
    public static Type ArchipelagoDataType { get; set; }

    public static ArchipelagoData ArchipelagoData { get; set; }

    public static T GetArchipelagoData<T>() where T : ArchipelagoData
    {
        if (ArchipelagoData == null)
        {
            throw new Exception("No Archipelago data type loaded");
        }

        if (typeof(T) != ArchipelagoDataType)
        {
            throw new Exception("Invalid type");
        }

        return (T)ArchipelagoData;
    }
}
