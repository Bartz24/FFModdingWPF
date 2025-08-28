using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class ItemReqComponent : ItemLocationReqComponent
{
    private ItemReq Requirements { get; set; }

    public ItemReqComponent(ItemReq req)
    {
        Requirements = req;
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return Requirements.IsValid(items);
    }
}
