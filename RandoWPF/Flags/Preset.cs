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
        RandoPresets.PresetsList.Add(this);
        return this;

    }

    public void Apply()
    {
        if (RandoPresets.ApplyingPreset == RandoPresets.PresetSetType.MatchingPreset)
        {
            return;
        }
        RandoPresets.ApplyingPreset = RandoPresets.PresetSetType.FromPreset;
        OnApply?.Invoke();
        RandoPresets.ApplyingPreset = RandoPresets.PresetSetType.Other;
    }

    public string Name { get; set; }
    public string Version { get; set; }
    public bool Default { get; set; } = false;
    public bool CustomModified { get; set; } = false;
    public bool CustomLoaded { get; set; } = false;
    public List<object> FlagSettings { get; set; }
    public string PresetPath { get; set; }
    public string PresetFlagString { get; set; }
}
