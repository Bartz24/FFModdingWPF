﻿using Newtonsoft.Json;
using System.ComponentModel;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class ToggleFlagProperty : FlagProperty
{
    public override ToggleFlagProperty Register(Flag parent)
    {
        base.Register(parent);
        parent.PropertyChanged += Parent_PropertyChanged;
        return this;
    }

    private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Flag flag = (Flag)sender;
        if (e.PropertyName == nameof(flag.FlagEnabled) && !flag.FlagEnabled)
        {
            Enabled = false;
        }
    }

    private bool enabled;
    [JsonProperty]
    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;
            if (enabled && OnEnable != null)
            {
                OnEnable(this, null);
            }
            else if (!enabled && OnDisable != null)
            {
                OnDisable(this, null);
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Enabled)));
        }
    }
    public override void Deserialize(dynamic data)
    {
        base.Deserialize((object)data);
        Enabled = data["Enabled"];
    }
}
