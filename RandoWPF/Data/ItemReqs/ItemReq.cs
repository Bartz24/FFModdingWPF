using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class ItemReq
{
    public static readonly BoolItemReq TRUE = new(true);
    public static readonly BoolItemReq FALSE = new(false);
    public bool IsValid(Dictionary<string, int> itemsAvailable)
    {
        // Return false if this has already been checked
        if (ValidationStack.Any(s => s == this))
        {
            return false;
        }

        ValidationStack.Push(this);
        bool valid = IsValidImpl(itemsAvailable);
        ValidationStack.Pop();

        return valid;
    }
    protected virtual bool IsValidImpl(Dictionary<string, int> itemsAvailable) { return true; }

    public List<string> GetPossibleRequirements()
    {
        // Return empty list if this has already been checked
        if (PossibleStack.Any(s => s == this))
        {
            return new List<string>();
        }

        PossibleStack.Push(this);
        List<string> possible = GetPossibleRequirementsImpl();
        PossibleStack.Pop();

        return possible;
    }

    protected virtual List<string> GetPossibleRequirementsImpl() { return new List<string>(); }
    public virtual int GetPossibleRequirementsCount() { return 0; }

    public static Stack<ItemReq> ValidationStack { get; set; } = new();
    public static Stack<ItemReq> PossibleStack { get; set; } = new();

    static ItemReq()
    {
        parseMapping.Add("AND", args => And(args.Select(s => Parse(s)).ToArray()));
        parseMapping.Add("OR", args => Or(args.Select(s => Parse(s)).ToArray()));
        parseMapping.Add("I", args => args.Count > 1 ? Item(args[0], int.Parse(args[1])) : Item(args[0], 1));
        parseMapping.Add("SELECT", args => Select(int.Parse(args[0]), args.Skip(1).Select(s => Parse(s)).ToArray()));
    }

    public static ItemReq Item(string item, int amount = 1)
    {
        return new AmountItemReq(item, amount);
    }
    public static ItemReq And(params string[] items)
    {
        return And(items.Select(i => Item(i)).ToArray());
    }
    public static ItemReq And(params ItemReq[] reqs)
    {
        return new AndItemReq(reqs.ToList());
    }
    public static ItemReq Or(params string[] items)
    {
        return Or(items.Select(i => Item(i)).ToArray());
    }
    public static ItemReq Or(params ItemReq[] reqs)
    {
        return new OrItemReq(reqs.ToList());
    }
    public static ItemReq Select(int count, params ItemReq[] reqs)
    {
        return new SelectItemReq(count, reqs.ToList());
    }

    private static Dictionary<string, Func<List<string>, ItemReq>> parseMapping = new();

    public static void RegisterMapping(string name, Func<List<string>, ItemReq> func)
    {
        parseMapping.Add(name, func);
    }

    public static ItemReq Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return TRUE;
        }

        string name = s.Substring(0, s.IndexOf("("));
        string argString = s.Substring(s.IndexOf("(") + 1, s.FindClosingBracketIndex('(', ')') - s.IndexOf("(") - 1);
        List<string> args = new();
        int parantheses = 0;
        for (int i = 0; i < argString.Length; i++)
        {
            if (argString[i] == '(')
            {
                parantheses++;
            }

            if (argString[i] == ')')
            {
                parantheses--;
            }

            if (argString[i] == '|' && parantheses == 0)
            {
                args.Add(argString.Substring(0, i));
                argString = argString.Substring(i + 1);
                i = 0;
            }
        }

        args.Add(argString);
        if (parseMapping.ContainsKey(name))
        {
            return parseMapping[name](args);
        }
        else
        {
            throw new RandoException("Item Requirement parsed is not supported: " + s, "Invalid Item requirement");
        }
    }

    public string GetDisplay()
    {
        return GetDisplay(_ => "");
    }

    public virtual string GetDisplay(Func<string, string> itemNameFunc)
    {
        return "None";
    }
}
