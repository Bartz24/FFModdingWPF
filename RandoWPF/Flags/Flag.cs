using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF
{
    public class Flag : INotifyPropertyChanged
    {
        public Flag()
        {
        }

        public virtual Flag Register(object type, bool isTweak = false)
        {
            Flags.FlagsList.Add(this);
            FlagType = (int)type;
            PropertyChanged += Flag_PropertyChanged;
            return this;

        }

        public void Flag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Visibility))
                return;
            Preset custom = Presets.PresetsList.First(p => p.Custom);
            if (!Presets.ApplyingPreset && Presets.Selected != custom)
                Presets.Selected = custom;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int FlagType { get; set; }

        public bool Experimental { get; set; }
        public bool Aesthetic { get; set; }
        public string DescriptionFormat { get; set; } = "";
        public string Description
        {
            get => (Experimental ? "[EXPERIMENTAL]\n" : "") + DescriptionFormatting.Apply(CurrentDescriptionFormat, this);
        }

        public virtual string CurrentDescriptionFormat
        {
            get => DescriptionFormat;
        }

        public virtual FormattingMap DescriptionFormatting
        {
            get => new FormattingMap();
        }

        public string Text { get; set; }

        public bool FlagEnabled
        {
            get => flagEnabled;
            set
            {
                flagEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(FlagEnabled)));
            }
        }
        public string FlagID { get; set; }


        public string GetFlagString()
        {
            if (!this.FlagEnabled)
                return "";
            string output = "-" + FlagID;
            output += GetExtraFlagString();
            return output;
        }

        public virtual string GetExtraFlagString()
        {
            return "";
        }

        public void SetFlagString(string value, bool simulate)
        {
            string input = value;
            string name;
            if (input.Contains("["))
                name = input.Substring(1, input.IndexOf('[') - 1);
            else
                name = input.Substring(1);
            if (name == FlagID)
            {
                if (!simulate)
                    FlagEnabled = true;
                if (input.Contains("[") && input.Contains("]"))
                    SetExtraFlagString(input.Substring(input.IndexOf("[") + 1, input.IndexOf("]") - input.IndexOf("[") - 1), simulate);
            }
        }

        public virtual void SetExtraFlagString(string value, bool simulate)
        {
        }

        public static readonly Flag Empty = EmptyFlag();
        private bool flagEnabled;
        private Visibility visibility;

        private static Flag EmptyFlag()
        {
            Flag flag = new Flag();
            flag.Text = "";
            return flag;
        }

        public Random Random { get; set; }

        public void ResetRandom(int seed)
        {
            Random = new Random(seed + FlagID[0] * FlagID.Length - Description.Length);
        }

        public void SetRand()
        {
            RandomNum.SetRand(Random);
        }
        public List<FlagProperty> FlagProperties { get; set; } = new List<FlagProperty>();
    }
}
