using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public class Presets
    {
        public static List<Preset> PresetsList { get; set; } = new List<Preset>();

        public static Preset Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(null, EventArgs.Empty);
                selected.Apply();
            }
        }

        public static bool ApplyingPreset = false;
        private static Preset selected;

        public static event EventHandler SelectedChanged = delegate { };

        public static string Serialize(string presetName, string version)
        {            
            JObject o = JObject.FromObject(new
            {
                name = presetName,
                version = version,
                flags = Flags.FlagsList
            });
            return o.ToString();
        }

        public static Preset Deserialize(string content)
        {
            dynamic data = JsonConvert.DeserializeObject(content);

            Preset preset = new Preset()
            {
                Name = data["name"],
                Version = data["version"],
                FlagSettings = ((JArray)data["flags"]).Select(o => (dynamic)o).ToList()
            };

            return preset;
        }

        public static void LoadPresets()
        {
            Directory.GetFiles(@"data\presets", "*.json").ToList().ForEach(f => {
                LoadPreset(f, false);
            });

            PresetsList = PresetsList.OrderBy(p => p.Name).ToList();

            if (!Directory.Exists("presets"))
                Directory.CreateDirectory("presets");
            Directory.GetFiles(@"presets", "*.json").ToList().ForEach(f => {
                LoadPreset(f, true);
            });
        }

        public static void LoadPreset(string file, bool customLoaded)
        {
            Preset preset = Deserialize(File.ReadAllText(file));
            preset.CustomLoaded = customLoaded;
            Action onApply = () => OnApply(preset);
            preset.OnApply = onApply;
            preset.PresetPath = file;
            PresetsList.Add(preset);
            PresetsList = PresetsList.OrderBy(p => p.CustomModified).ToList();
        }

        private static void OnApply(Preset p)
        {
            Flags.FlagsList.ForEach(f => f.FlagEnabled = false);

            p.FlagSettings.ForEach(s =>
            {
                Flag f = Flags.FlagsList.First(f => f.FlagID == s["FlagID"].Value);
                f.FlagEnabled = s["FlagEnabled"].Value;
                ((JArray)s["FlagProperties"]).Select(o => (dynamic)o).ToList().ForEach(p =>
                {
                    FlagProperty prop = f.FlagProperties.First(fp => fp.ID == p["ID"].Value);
                    prop.Deserialize(p);
                });
            });
        }
    }
}
