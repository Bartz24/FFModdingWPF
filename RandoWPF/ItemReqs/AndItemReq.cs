using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{

    public class AndItemReq : ItemReq
    {
        List<ItemReq> reqs = new List<ItemReq>();
        public AndItemReq(List<ItemReq> reqs)
        {
            this.reqs = reqs;
        }
        public override bool IsValid(Dictionary<string, int> itemsAvailable)
        {
            foreach(ItemReq req in reqs)
            {
                if (!req.IsValid(itemsAvailable))
                    return false;
            }
            return true;
        }

        public override List<string> GetPossibleRequirements()
        {
            return reqs.SelectMany(r => r.GetPossibleRequirements()).Distinct().ToList();
        }
    }
}
