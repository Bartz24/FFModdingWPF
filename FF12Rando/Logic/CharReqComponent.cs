using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class CharReqComponent : ItemLocationReqComponent
{
    private int BaseDifficulty { get; set; }

    public CharReqComponent(int baseDifficulty)
    {
        BaseDifficulty = baseDifficulty;
    }

    public override bool AreItemReqsMet(ProgressionState state)
    {
        return HasEnoughChars(state);
    }

    private int GetCharCount(ProgressionState state)
    {
        int count = 0;
        var items = state.ItemsAvailable;
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

    private bool HasEnoughChars(ProgressionState state)
    {
        if (FF12Flags.Items.CharacterScale.Enabled)
        {
            int charCount = GetCharCount(state);
            var items = state.ItemsAvailable;
            int diff = BaseDifficulty;

            if (diff >= 7)
            {
                return charCount >= 6 && items.GetValueOrDefault("C01F") > 0;
            }

            if (diff >= 5)
            {
                return charCount >= 5 && items.GetValueOrDefault("C01F") > 0;
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
