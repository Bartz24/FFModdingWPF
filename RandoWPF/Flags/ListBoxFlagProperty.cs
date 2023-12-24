using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class ListBoxFlagProperty : FlagProperty
{
    public override ListBoxFlagProperty Register(Flag parent)
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
            SelectedValues = new List<string>();
        }
    }
    public virtual List<string> Values { get; set; } = new List<string>();

    protected List<string> selectedValues;
    [JsonProperty]
    public virtual IList SelectedValues
    {
        get => selectedValues?.OrderBy(s => s).ToList();
        set
        {
            selectedValues = value.Cast<string>().ToList();

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedValues)));
        }
    }

    public List<int> SelectedIndices => IndicesOf(selectedValues);

    public override void Deserialize(IDictionary<string, object> data)
    {
        base.Deserialize(data);
        SelectedValues = data[nameof(SelectedValues)] == null ? new List<string>() : ((List<object>)data[nameof(SelectedValues)]).Select(o => (string)o).ToList();
    }

    public List<int> IndicesOf(List<string> vals)
    {
        return vals.Select(s => Values.IndexOf(s)).ToList();
    }
}
