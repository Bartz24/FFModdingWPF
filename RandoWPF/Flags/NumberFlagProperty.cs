using Newtonsoft.Json;
using System.ComponentModel;

namespace Bartz24.RandoWPF
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NumberFlagProperty : FlagProperty
    {
        public override NumberFlagProperty Register(Flag parent)
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
        public int StepSize { get; set; } = 1;

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
