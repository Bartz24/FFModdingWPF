using Bartz24.RandoWPF;

namespace FF13_2Rando
{
    public class FF13_2Presets
    {
        public static Preset Custom;
        public static void Init()
        {
            Presets.PresetsList.Clear();

            Presets.LoadPresets();

            Custom = new Preset()
            {
                Name = "Custom (Modified)",
                CustomModified = true
            }.Register();

            Presets.Selected = Presets.PresetsList[0];
        }
    }
}

