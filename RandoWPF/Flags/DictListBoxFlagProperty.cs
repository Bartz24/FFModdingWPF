using Bartz24.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bartz24.RandoWPF
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DictListBoxFlagProperty<T> : ListBoxFlagProperty
    {
        public override FlagProperty Register(Flag parent)
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
        public BiDictionary<T, string> DictValues { get; set; } = new BiDictionary<T, string>();
        public override List<string> Values { get => DictValues.Forward.Values; }
        public List<T> SelectedKeys
        {
            get => selectedValues.Select(s => DictValues.Reverse[s]).ToList();
        }
    }
}
