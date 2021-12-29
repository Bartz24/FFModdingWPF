using Bartz24.Data;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public class Preset
    {
        public Action OnApply { get; set; }

        public Preset()
        {
        }

        public virtual Preset Register()
        {
            Presets.PresetsList.Add(this);
            return this;

        }

        public void Apply()
        {
            Presets.ApplyingPreset = true;
            if (OnApply != null)
                OnApply();
            Presets.ApplyingPreset = false;
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public bool Default { get; set; } = false;
        public bool CustomModified { get; set; } = false;
        public bool CustomLoaded { get; set; } = false;
        public List<dynamic> FlagSettings { get; set; }
        public string PresetPath { get; set; }
    }
}
