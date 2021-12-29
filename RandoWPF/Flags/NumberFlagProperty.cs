using Bartz24.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
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
    [JsonObject(MemberSerialization.OptIn)]
    public class NumberFlagProperty : FlagProperty
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
                Value = MinValue;
            }
        }

        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public string ValueText { get; set; } = "Value:";

        private int value;
        [JsonProperty]
        public int Value
        {
            get => value;
            set
            {
                this.value = value;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }
        public override void Deserialize(dynamic data)
        {
            base.Deserialize((object)data);
            Value = data["Value"];
        }
    }
}
