using System;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

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
        OnApply?.Invoke();
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
