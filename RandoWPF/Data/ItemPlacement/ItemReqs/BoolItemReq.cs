using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class BoolItemReq : ItemReq
{
    public bool Value { get; set; }

    public BoolItemReq(bool value)
    {
        Value = value;
    }

    protected override bool IsMet(Dictionary<string, int> itemsAvailable)
    {
        return Value;
    }

    public override string GetDisplay(Func<string, string> itemNameFunc)
    {
        return Value ? "Always" : "Never";
    }

    public override bool Equals(object obj)
    {
        return obj is BoolItemReq req &&
               Value == req.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override string GetArchipelagoRule(Func<string, string> itemNameFunc)
    {
        if (Value)
        {
            return "True";
        }
        else
        {
            return "False";
        }
    }
}
