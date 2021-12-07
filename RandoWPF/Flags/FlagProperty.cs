using Bartz24.Data;
using Microsoft.Win32;
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
        public string ID { get; set; }
        public string Description { get; set; }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
