using System;
using System.Collections.Generic;

namespace Bartz24.RandoWPF
{
    public abstract class ItemLocation
    {
        public abstract string ID { get; }
        public abstract string Name { get; }
        public abstract string LocationImagePath { get; }
        public abstract ItemReq Requirements { get; }
        public abstract List<string> Traits { get; }
        public abstract List<string> Areas { get; }
        public abstract int Difficulty { get; }
        public abstract bool IsValid(Dictionary<string, int> items);

        public abstract void SetData(dynamic obj, string newItem, int newCount);
        public abstract (string, int)? GetData(dynamic obj);
    }
}
