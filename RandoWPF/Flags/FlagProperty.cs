using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class FlagProperty : INotifyPropertyChanged
{
    public EventHandler OnEnable { get; set; }
    public EventHandler OnDisable { get; set; }

    public FlagProperty()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual FlagProperty Register(Flag parent)
    {
        parent.FlagPropertiesDebugIncluded.Add(this);
        PropertyChanged += parent.Flag_PropertyChanged;
        return this;
    }

    public string Text { get; set; }
    [JsonProperty]
    public string ID { get; set; }
    public string Description { get; set; }
    public bool Experimental { get; set; }
    public bool Debug { get; set; }

    public Visibility TextVisibility => string.IsNullOrEmpty(Text) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HelpVisibility => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
    public Brush HelpColor => Debug ? Brushes.GreenYellow : Experimental ? Brushes.PaleVioletRed : Brushes.SkyBlue;

    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    public virtual void Deserialize(IDictionary<string, object> data)
    {

    }
}
