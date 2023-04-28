using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class Flag : INotifyPropertyChanged
{
    public Flag()
    {
    }

    public virtual Flag Register(object type, bool isTweak = false)
    {
        RandoFlags.FlagsList.Add(this);
        FlagType = (int)type;
        PropertyChanged += Flag_PropertyChanged;
        return this;

    }

    public void Flag_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Visibility))
        {
            return;
        }

        Preset custom = Presets.PresetsList.First(p => p.CustomModified);
        if (!Presets.ApplyingPreset && Presets.Selected != custom)
        {
            Presets.Selected = custom;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public int FlagType { get; set; }

    public bool Experimental { get; set; }
    public bool Aesthetic { get; set; }
    public bool Debug { get; set; }
    public string DescriptionFormat { get; set; } = "";
    public string Description => (Experimental ? "[EXPERIMENTAL]\n" : "") + DescriptionFormatting.Apply(CurrentDescriptionFormat, this);
    public Brush HelpColor => Debug ? Brushes.GreenYellow : Experimental ? Brushes.PaleVioletRed : Brushes.SkyBlue;

    public virtual string CurrentDescriptionFormat => DescriptionFormat;

    public virtual FormattingMap DescriptionFormatting => new();

    public string Text { get; set; }

    [JsonProperty]
    public bool FlagEnabled
    {
        get => flagEnabled;
        set
        {
            flagEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlagEnabled)));
        }
    }
    [JsonProperty]
    public string FlagID { get; set; }

    public string GetFlagString()
    {
        if (!FlagEnabled)
        {
            return "";
        }

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
        string name = input.Contains("[") ? input.Substring(1, input.IndexOf('[') - 1) : input.Substring(1);
        if (name == FlagID)
        {
            if (!simulate)
            {
                FlagEnabled = true;
            }

            if (input.Contains("[") && input.Contains("]"))
            {
                SetExtraFlagString(input.Substring(input.IndexOf("[") + 1, input.IndexOf("]") - input.IndexOf("[") - 1), simulate);
            }
        }
    }

    public virtual void SetExtraFlagString(string value, bool simulate)
    {
    }

    public static readonly Flag Empty = EmptyFlag();
    private bool flagEnabled;
    //private Visibility visibility;

    private static Flag EmptyFlag()
    {
        Flag flag = new()
        {
            Text = ""
        };
        return flag;
    }

    public Random Random { get; set; }

    public void ResetRandom(int seed)
    {
        Random = new Random(seed + (FlagID[0] * FlagID.Length) - Description.Length);
    }

    public void SetRand()
    {
        RandomNum.SetRand(Random);
    }

    [JsonProperty]
    public List<FlagProperty> FlagProperties
    {
        get => FlagPropertiesDebugIncluded.Where(f => !f.Debug).ToList();
        set => FlagPropertiesDebugIncluded = value;
    }
    public List<FlagProperty> FlagPropertiesDebugIncluded { get; set; } = new List<FlagProperty>();
}
