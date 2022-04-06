using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public abstract class ItemLocation
    {
        public abstract string ID { get; }
        public abstract string Name { get; }
        public abstract ItemReq Requirements { get; }
        public abstract List<string> Traits { get; }
        public abstract List<string> Areas { get; }
        public abstract bool IsValid(Dictionary<string, int> items);

        public abstract void SetData(dynamic obj, string newItem, int newCount);
        public abstract Tuple<string, int> GetData(dynamic obj);
    }
}
