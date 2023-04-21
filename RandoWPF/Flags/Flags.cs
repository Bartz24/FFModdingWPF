using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bartz24.RandoWPF
{
    public class Flags
    {
        public static List<Flag> FlagsList { get; set; } = new List<Flag>();

        public const int FlagTypeDebug = int.MaxValue;
        public const int FlagTypeAll = -1;

        public static Dictionary<int, string> CategoryMap { get; set; } = new Dictionary<int, string>();

        public static List<string> CategoryList { get => CategoryMap.Keys.OrderBy(i => i).Select(i => CategoryMap[i]).ToList(); }
        public static string SelectedCategory
        {
            get => selected;
            set
            {
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(null, EventArgs.Empty);
            }
        }

        public static bool ApplyingPreset = false;
        private static string selected;

        public static event EventHandler SelectedChanged = delegate { };

        public static string Serialize(string seed, string version)
        {
            JObject o = JObject.FromObject(new
            {
                seed = seed,
                version = version,
                flags = FlagsList.Where(f => !f.Debug).ToList()
            });
            return o.ToString();
        }
        public static string LoadSeed(string file)
        {
            dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(file));

            string seed = data["seed"];
            string version = data["version"];

            Flags.FlagsList.ForEach(f => f.FlagEnabled = false);

            ((JArray)data["flags"]).Select(o => (dynamic)o).ToList().ForEach(s =>
            {
                if (Flags.FlagsList.Where(f => f.FlagID == s["FlagID"].Value).Count() == 0)
                    return;
                Flag f = Flags.FlagsList.First(f => f.FlagID == s["FlagID"].Value);
                f.FlagEnabled = s["FlagEnabled"].Value;
                ((JArray)s["FlagProperties"]).Select(o => (dynamic)o).ToList().ForEach(p =>
                {
                    if (f.FlagProperties.Where(fp => fp.ID == p["ID"].Value).Count() == 0)
                        return;
                    FlagProperty prop = f.FlagProperties.First(fp => fp.ID == p["ID"].Value);
                    prop.Deserialize(p);
                });
            });

            return seed;
        }
    }
}
