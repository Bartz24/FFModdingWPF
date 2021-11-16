using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{

    public class AmountItemReq : ItemReq
    {
        string item;
        int amount;
        public AmountItemReq(string item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
        public override bool IsValid(Dictionary<string, int> itemsAvailable)
        {
            if (!itemsAvailable.ContainsKey(item))
                return false;
            return itemsAvailable[item] >= amount;
        }

        public override List<string> GetPossibleRequirements()
        {
            return new string[] { item }.ToList();
        }
    }
}
