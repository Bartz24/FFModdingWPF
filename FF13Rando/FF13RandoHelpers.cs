using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando;

public class FF13RandoHelpers
{
    public static bool AreCrystariumReqsMet(FF13ItemLocation loc, Dictionary<string, int> items)
    {
        int stage = items.GetValueOrDefault("cry_stage", 0) + 1;
        int roleCount = GetRoleCount(loc, items);
        int partySize = loc.Characters.Count;
        return Math.Max(0, loc.BaseDifficulty - FF13Flags.Items.DifficultyScaling.Value) switch
        {
            0 or 1 => stage >= 1 && roleCount >= 0 * partySize,
            2 => stage >= 1 && roleCount >= 1 * partySize,
            3 => stage >= 1 && roleCount >= 1 * partySize,
            4 => stage >= 2 && roleCount >= 1 * partySize,
            5 => stage >= 3 && roleCount >= 2 * partySize,
            6 => stage >= 4 && roleCount >= 2 * partySize,
            7 => stage >= 4 && roleCount >= 3 * partySize,
            8 => stage >= 5 && roleCount >= 3 * partySize,
            9 => stage >= 5 && roleCount >= 3 * partySize,
            10 => stage >= 6 && roleCount >= 3 * partySize,
            11 => stage >= 7 && roleCount >= 4 * partySize,
            12 => stage >= 8 && roleCount >= 5 * partySize,
            13 => stage >= 9 && roleCount >= 6 * partySize,
            _ => stage >= 9 && roleCount >= 6 * partySize,
        };
    }

    public static int GetRoleCount(FF13ItemLocation loc, Dictionary<string, int> items)
    {
        string[] roles = { "com", "rav", "sen", "syn", "sab", "med" };
        int have = loc.Characters.Sum(c => items.Keys.Where(i => i.StartsWith($"rol_{c}_") && items[i] > 0).Count());
        return have;
    }

    public static List<string> ParseReqCharas(string charaStr)
    {
        string[] chars = { "lig", "fan", "hop", "saz", "sno", "van" };
        if (string.IsNullOrEmpty(charaStr))
        {
            return chars.ToList();
        }

        List<string> list = new();
        if (charaStr.ToCharArray().Contains('l'))
        {
            list.Add(chars[0]);
        }

        if (charaStr.ToCharArray().Contains('f'))
        {
            list.Add(chars[1]);
        }

        if (charaStr.ToCharArray().Contains('h'))
        {
            list.Add(chars[2]);
        }

        if (charaStr.ToCharArray().Contains('z'))
        {
            list.Add(chars[3]);
        }

        if (charaStr.ToCharArray().Contains('s'))
        {
            list.Add(chars[4]);
        }

        if (charaStr.ToCharArray().Contains('v'))
        {
            list.Add(chars[5]);
        }

        return list;
    }
}
