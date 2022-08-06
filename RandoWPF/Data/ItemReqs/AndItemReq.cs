﻿using System;
using System.Collections.Generic;
using System.Linq;

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
            foreach (ItemReq req in reqs)
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
        public override int GetPossibleRequirementsCount() { return reqs.Select(r => r.GetPossibleRequirementsCount()).Sum(); }

        public override string GetDisplay(Func<string, string> itemNameFunc)
        {
            return $"({string.Join(" AND ", reqs.Select(r => r.GetDisplay(itemNameFunc)))})";
        }
    }
}
