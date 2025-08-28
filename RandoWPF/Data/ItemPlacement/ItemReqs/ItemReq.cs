using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public abstract class ItemReq
{
    public static readonly BoolItemReq TRUE = new(true);
    public static readonly BoolItemReq FALSE = new(false);
    public bool IsValid(Dictionary<string, int> itemsAvailable)
    {
        if (ValidationStack.Count > 100)
        {
            throw new RandoException("Item requirement validation stack overflow", "Item requirement validation stack overflow");
        }

        // Return false if this has already been checked
        if (ValidationStack.Any(s => s == this))
        {
            return false;
        }

        ValidationStack.Push(this);
        bool valid = IsMet(itemsAvailable);
        ValidationStack.Pop();

        return valid;
    }
    protected virtual bool IsMet(Dictionary<string, int> itemsAvailable) { return true; }

    public List<string> GetPossibleRequirements()
    {
        if (ValidationStack.Count > 100)
        {
            throw new RandoException("Item requirement possible stack overflow", "Item requirement possible stack overflow");
        }

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

    protected int BaseDifficulty { get; set; } = 0;

    public virtual int GetDifficulty(Dictionary<string, int> itemsAvailable)
    {
        return IsValid(itemsAvailable) ? BaseDifficulty : -1;
    }

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

        if (s.Count(c=>c == '(') != s.Count(c => c == ')'))
        {
            throw new RandoException("Item Requirement parsed has invalid parentheses syntax: " + s, "Invalid requirement");
        }

        string name = s.Substring(0, s.IndexOf("("));
        string argString = s.Substring(s.IndexOf("(") + 1, s.FindClosingBracketIndex('(', ')') - s.IndexOf("(") - 1);
        if (s.Substring(s.FindClosingBracketIndex('(', ')') + 1).StartsWith("|"))
        {
            throw new Exception("Unexpected ItemReq at the same level in: " + s);
        }

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
            ItemReq req = parseMapping[name](args);
            string end = s.Contains(")") ? s.Substring(s.LastIndexOf(")") + 1) : s;
            if (end.Contains("^"))
            {
                req.BaseDifficulty = int.Parse(end.Substring(end.IndexOf("^") + 1));
            }

            return req;
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

    public override bool Equals(object obj)
    {
        // Needs to be overridden 
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        // Needs to be overridden 
        throw new NotImplementedException();
    }

    public static bool operator ==(ItemReq a, ItemReq b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ItemReq a, ItemReq b)
    {
        return !(a == b);
    }

    public abstract string GetArchipelagoRule(Func<string, string> itemNameFunc);
}
