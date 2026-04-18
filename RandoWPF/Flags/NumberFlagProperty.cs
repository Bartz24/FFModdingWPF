using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class NumberFlagProperty : FlagProperty
{
    public enum NumberScaleType
    {
        Linear,
        Logarithmic
    }

    public NumberFlagProperty(int defaultValue) : base(defaultValue)
    {
        value = defaultValue;
    }

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
    public NumberScaleType ScaleType { get; set; } = NumberScaleType.Linear;
    public int LogarithmicBase { get; set; } = 10;

    public string ValueText { get; set; } = "Value:";

    public double SliderMinimum => ScaleType == NumberScaleType.Logarithmic ? GetLogarithmicPosition(MinValue) : MinValue;
    public double SliderMaximum => ScaleType == NumberScaleType.Logarithmic ? GetLogarithmicPosition(MaxValue) : MaxValue;
    public double SliderTickFrequency => ScaleType == NumberScaleType.Logarithmic ? 1 : StepSize;

    public double SliderValue
    {
        get => ScaleType == NumberScaleType.Logarithmic ? GetLogarithmicPosition(Value) : Value;
        set
        {
            Value = ScaleType == NumberScaleType.Logarithmic ? GetLogarithmicValue(value) : (int)Math.Round(value);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SliderValue)));
        }
    }

    private int value;
    [JsonProperty]
    public int Value
    {
        get
        {
            if (RandoFlags.Mode == RandoFlags.SeedMode.Archipelago && (ParentFlag.HasArchipelagoOverride || DisabledByArchipelago))
            {
                return GetDefaultValue<int>();
            }

            return value;
        }
        set
        {
            this.value = ClampValue(value);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SliderValue)));
        }
    }

    public override void Deserialize(IDictionary<string, object> data)
    {
        Value = (int)(long)data["Value"];
    }

    private int ClampValue(int newValue)
    {
        int clamped = Math.Max(MinValue, Math.Min(MaxValue, newValue));
        if (ScaleType != NumberScaleType.Logarithmic)
        {
            return clamped;
        }

        double position = GetLogarithmicPosition(clamped);
        return GetLogarithmicValue(position);
    }

    private double GetLogarithmicPosition(int currentValue)
    {
        double logBase = Math.Log(LogarithmicBase);
        return Math.Round(Math.Log(currentValue) / logBase);
    }

    private int GetLogarithmicValue(double sliderPosition)
    {
        double roundedPosition = Math.Round(sliderPosition);
        double result = Math.Pow(LogarithmicBase, roundedPosition);
        return (int)Math.Round(result);
    }
}
