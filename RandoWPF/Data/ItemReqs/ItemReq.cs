using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;

namespace Bartz24.RandoWPF
{
    public class ItemReq
    {
        public virtual bool IsValid(Dictionary<string, int> itemsAvailable) { return true; }

        public virtual List<string> GetPossibleRequirements() { return new List<string>(); }
        public virtual int GetPossibleRequirementsCount() { return 0; }

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

        public static ItemReq Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                return new ItemReq();
            string name = s.Substring(0, s.IndexOf("("));
            string argString = s.Substring(s.IndexOf("(") + 1, s.FindClosingBracketIndex('(', ')') - s.IndexOf("(") - 1);
            List<string> args = new List<string>();
            int parantheses = 0;
            for (int i = 0; i < argString.Length; i++)
            {
                if (argString[i] == '(')
                    parantheses++;
                if (argString[i] == ')')
                    parantheses--;
                if (argString[i] == '|' && parantheses == 0)
                {
                    args.Add(argString.Substring(0, i));
                    argString = argString.Substring(i + 1);
                    i = 0;
                }
            }
            args.Add(argString);
            switch (name)
            {
                case "AND":
                    return And(args.Select(s => Parse(s)).ToArray());
                case "OR":
                    return Or(args.Select(s => Parse(s)).ToArray());
                case "I":
                    if (args.Count > 1)
                        return Item(args[0], int.Parse(args[1]));
                    else
                        return Item(args[0], 1);
            }
            throw new Exception("Item Requirement parsed is not supported: " + s);
        }

        public virtual string GetDisplay(Func<string, string> itemNameFunc)
        {
            return "None";
        }
    }
}
