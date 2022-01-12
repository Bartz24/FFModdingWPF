using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{

    public class OrItemReq : ItemReq
    {
        List<ItemReq> reqs = new List<ItemReq>();
        public OrItemReq(List<ItemReq> reqs)
        {
            this.reqs = reqs;
        }
        public override bool IsValid(Dictionary<string, int> itemsAvailable)
        {
            foreach (ItemReq req in reqs)
            {
                if (req.IsValid(itemsAvailable))
                    return true;
            }
            return false;
        }

        public override List<string> GetPossibleRequirements()
        {
            return reqs.SelectMany(r => r.GetPossibleRequirements()).Distinct().ToList();
        }
    }
}
