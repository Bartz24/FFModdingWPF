using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public abstract class FF12ItemLocation : ItemLocation
{
    protected FF12ItemLocation(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public override bool AreItemReqsMet(Dictionary<string, int> items)
    {
        return HasEnoughChars(items);
    }

    private int GetCharCount(Dictionary<string, int> items)
    {
        int count = 0;
        if (items.ContainsKey("Vaan") && items["Vaan"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Ashe") && items["Ashe"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Fran") && items["Fran"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Balthier") && items["Balthier"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Basch") && items["Basch"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Penelo") && items["Penelo"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Guest") && items["Guest"] > 0)
        {
            count++;
        }

        return count;
    }

    private bool HasEnoughChars(Dictionary<string, int> items)
    {
        if (FF12Flags.Items.CharacterScale.Enabled)
        {
            int charCount = GetCharCount(items);
            int diff = BaseDifficulty;

            if (diff >= 7)
            {
                return charCount >= 6;
            }

            if (diff >= 5)
            {
                return charCount >= 5;
            }

            if (diff >= 4)
            {
                return charCount >= 4;
            }

            if (diff >= 3)
            {
                return charCount >= 3;
            }
        }

        return true;
    }
}
