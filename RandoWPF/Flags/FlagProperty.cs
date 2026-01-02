using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public abstract class FlagProperty : INotifyPropertyChanged
{
    public EventHandler OnEnable { get; set; }
    public EventHandler OnDisable { get; set; }

    private object DefaultValue { get; set; }

    protected Flag ParentFlag { get; set; }

    public FlagProperty(object defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual FlagProperty Register(Flag parent)
    {
        ParentFlag = parent;
        parent.FlagPropertiesDebugIncluded.Add(this);
        PropertyChanged += parent.Flag_PropertyChanged;
        return this;
    }

    public T GetDefaultValue<T>()
    {
        return (T)DefaultValue;
    }

    public string Text { get; set; }
    [JsonProperty]
    public string ID { get; set; }
    public string Description { get; set; }
    public bool Experimental { get; set; }
    public bool Debug { get; set; }

    public bool DisabledByArchipelago { get; set; }

    public Visibility TextVisibility => string.IsNullOrEmpty(Text) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HelpVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
    public Brush HelpColor => Debug ? Brushes.GreenYellow : Experimental ? Brushes.PaleVioletRed : Brushes.SkyBlue;

    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    public abstract void Deserialize(IDictionary<string, object> data);
}
