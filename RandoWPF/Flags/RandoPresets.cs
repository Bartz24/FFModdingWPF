using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Bartz24.RandoWPF;

public class RandoPresets
{
    public static List<Preset> PresetsList { get; set; } = new List<Preset>();

    public static Preset Selected
    {
        get => selected;
        set
        {
            if (selected != value)
            {
                selected = value;
                SelectedChanged?.Invoke(null, EventArgs.Empty);
                selected?.Apply();
            }
        }
    }

    public enum PresetSetType
    {
        FromPreset,
        MatchingPreset,
        Other
    }

    public static PresetSetType ApplyingPreset = PresetSetType.Other;
    private static Preset selected;

    public static event EventHandler SelectedChanged = delegate { };

    public static string Serialize(string presetName, string version)
    {
        JObject o = JObject.FromObject(new
        {
            name = presetName,
            version,
            flags = RandoFlags.FlagsList
        });
        return o.ToString();
    }

    public static Preset Deserialize(string content)
    {
        IDictionary<string, object> data = JsonConvert.DeserializeObject<ExpandoObject>(content, new ExpandoObjectConverter());

        Preset preset = new()
        {
            Name = (string)data["name"],
            Version = (string)data["version"],
            FlagSettings = (List<object>)data["flags"]
        };

        OnApply(preset);
        FlagStringCompressor compressor = new();
        preset.PresetFlagString = compressor.Compress(JObject.FromObject(new
        {
            flags = RandoFlags.FlagsList
        }).ToString());

        return preset;
    }
    public static void Init()
    {
        PresetsList.Clear();
        LoadPresets();

        new Preset()
        {
            Name = "Custom (Modified)",
            CustomModified = true
        }.Register();

        Selected = PresetsList[0];
    }

    private static void LoadPresets()
    {
        Directory.GetFiles(@"data\presets", "*.json").ToList().ForEach(f =>
        {
            LoadPreset(f, false);
        });

        PresetsList = PresetsList.OrderBy(p => p.Name).ToList();

        if (!Directory.Exists("presets"))
        {
            Directory.CreateDirectory("presets");
        }

        Directory.GetFiles(@"presets", "*.json").ToList().ForEach(f =>
        {
            LoadPreset(f, true);
        });
    }

    public static void LoadPreset(string file, bool customLoaded)
    {
        Preset preset = Deserialize(File.ReadAllText(file));
        preset.CustomLoaded = customLoaded;
        void onApply()
        {
            OnApply(preset);
        }

        preset.OnApply = onApply;
        preset.PresetPath = file;
        PresetsList.Add(preset);
        PresetsList = PresetsList.OrderBy(p => p.CustomModified).ToList();
    }

    private static void OnApply(Preset p)
    {
        RandoFlags.FlagsList.ForEach(f => f.FlagEnabled = false);

        p.FlagSettings.Select(o => (IDictionary<string, object>)o).ToList().ForEach(s =>
        {
            if (RandoFlags.FlagsList.Where(f => f.FlagID == (string)s["FlagID"]).Count() == 0)
            {
                return;
            }

            Flag f = RandoFlags.FlagsList.First(f => f.FlagID == (string)s["FlagID"]);
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
}
