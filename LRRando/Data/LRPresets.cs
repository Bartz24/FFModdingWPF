using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public class LRPresets
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

