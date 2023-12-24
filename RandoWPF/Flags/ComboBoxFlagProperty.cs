using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class ComboBoxFlagProperty : FlagProperty
{
    public override ComboBoxFlagProperty Register(Flag parent)
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
            SelectedValue = Values[0];
        }
    }
    public List<string> Values { get; set; } = new List<string>();

    private string selectedValue;
    [JsonProperty]
    public string SelectedValue
    {
        get => selectedValue;
        set
        {
            selectedValue = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedValue)));
        }
    }

    public int SelectedIndex => IndexOf(SelectedValue);

    public override void Deserialize(IDictionary<string, object> data)
    {
        base.Deserialize(data);
        SelectedValue = (string)data["SelectedValue"];
    }

    public int IndexOf(string val)
    {
        return Values.IndexOf(val);
    }
}
