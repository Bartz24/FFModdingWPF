using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;
public class FlagItemReq : ItemReq
{
    public string FlagID { get; set; }
    public bool Value { get; set; }

    public FlagItemReq(string flagID, bool value)
    {
        FlagID = flagID;
        Value = value;
    }

    protected override bool IsMet(ProgressionState state)
    {
        return RandoFlags.FlagsList.First(f=>f.FlagID == FlagID).FlagEnabled == Value;
    }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        if (Value)
        {
            return $"{RandoFlags.FlagsList.First(f => f.FlagID == FlagID).Text} Enabled";
        }
        else
        {
            return $"{RandoFlags.FlagsList.First(f => f.FlagID == FlagID).Text} Disabled";
        }
    }

    public override bool Equals(object obj)
    {
        return obj is FlagItemReq req &&
               FlagID == req.FlagID &&
               Value == req.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FlagID, Value);
    }

    public override IEnumerable<string> GetArchipelagoPreamble(string gameName)
    {
        return new List<string> { "from rule_builder.options import OptionFilter", $"from .Options import {FlagID}" };
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        if (Value)
        {
            return $"OptionFilter({FlagID}, {FlagID}.option_true)";
        }
        else
        {
            return $"OptionFilter({FlagID}, {FlagID}.option_false)";
        }
    }
    public override List<T> GetOf<T>()
    {
        return new List<T>();
    }
}
