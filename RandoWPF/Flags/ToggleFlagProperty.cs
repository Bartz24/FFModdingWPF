using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class ToggleFlagProperty : FlagProperty
{
    public ToggleFlagProperty(bool defaultValue) : base(defaultValue)
    {
        enabled = defaultValue;
    }

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
        get
        {
            if (RandoFlags.Mode == RandoFlags.SeedMode.Archipelago && (ParentFlag.HasArchipelagoOverride || DisabledByArchipelago))
            {
                return GetDefaultValue<bool>();
            }

            return enabled;
        }
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
    public override void Deserialize(IDictionary<string, object> data)
    {
        Enabled = (bool)data["Enabled"];
    }
}
