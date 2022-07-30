using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class FF13RandoHelpers
    {
        public static bool AreCrystariumReqsMet(FF13ItemLocation loc, Dictionary<string, int> items)
        {
            int stage = items.GetValueOrDefault("cry_stage", 0) + 1;
            int roleCount = GetRoleCount(loc, items);
            int partySize = loc.Characters.Count();
            switch (loc.Difficulty)
            {
                case 0:
                case 1:
                    return stage >= 1 && roleCount >= 0 * partySize;
                case 2:
                    return stage >= 1 && roleCount >= 1 * partySize;
                case 3:
                    return stage >= 1 && roleCount >= 1 * partySize;
                case 4:
                    return stage >= 2 && roleCount >= 1 * partySize;
                case 5:
                    return stage >= 3 && roleCount >= 2 * partySize;
                case 6:
                    return stage >= 4 && roleCount >= 2 * partySize;
                case 7:
                    return stage >= 4 && roleCount >= 3 * partySize;
                case 8:
                    return stage >= 5 && roleCount >= 3 * partySize;
                case 9:
                    return stage >= 5 && roleCount >= 3 * partySize;
                case 10:
                    return stage >= 6 && roleCount >= 3 * partySize;
                case 11:
                    return stage >= 7 && roleCount >= 4 * partySize;
                case 12:
                    return stage >= 8 && roleCount >= 5 * partySize;
                case 13:
                    return stage >= 9 && roleCount >= 6 * partySize;
                case 14:
                default:
                    return stage >= 9 && roleCount >= 6 * partySize;
            }
        }

        public static int GetRoleCount(FF13ItemLocation loc, Dictionary<string, int> items)
        {
            string[] roles = new string[] { "com", "rav", "sen", "syn", "sab", "med" };
            int have = loc.Characters.Sum(c => roles.Where(r => items.GetValueOrDefault($"rol_{c}_{r}", 0) > 0).Count());
            return have;
        }

        public static List<string> ParseReqCharas(string charaStr)
        {
            string[] chars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            if (string.IsNullOrEmpty(charaStr))
                return chars.ToList();
            List<string> list = new List<string>();
            if (charaStr.ToCharArray().Contains('l'))
                list.Add(chars[0]);
            if (charaStr.ToCharArray().Contains('f'))
                list.Add(chars[1]);
            if (charaStr.ToCharArray().Contains('h'))
                list.Add(chars[2]);
            if (charaStr.ToCharArray().Contains('z'))
                list.Add(chars[3]);
            if (charaStr.ToCharArray().Contains('s'))
                list.Add(chars[4]);
            if (charaStr.ToCharArray().Contains('v'))
                list.Add(chars[5]);
            return list;
        }
    }
}
