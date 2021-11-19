using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
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
    }
}
