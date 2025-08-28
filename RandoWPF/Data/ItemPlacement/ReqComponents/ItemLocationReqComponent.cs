using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public abstract class ItemLocationReqComponent
{    public abstract bool AreItemReqsMet(Dictionary<string, int> items);
}
