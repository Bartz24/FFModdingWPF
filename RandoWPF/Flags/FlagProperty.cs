using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Bartz24.RandoWPF
{
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
            parent.FlagProperties.Add(this);
            PropertyChanged += parent.Flag_PropertyChanged;
            return this;
        }

        public string Text { get; set; }
        [JsonProperty]
        public string ID { get; set; }
        public string Description { get; set; }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public virtual void Deserialize(dynamic data)
        {

        }
    }
}
