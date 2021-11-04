using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF.Data
{
    public class Flag
    {
        public Flag()
        {
        }

        public virtual Flag Register(object type, bool isTweak = false)
        {
            Flags.FlagsList.Add(this);
            FlagType = (int)type;
            return this;

        }
        public int FlagType { get; set; }

        public bool Experimental { get; set; }
        public bool Aesthetic { get; set; }
        public string DescriptionFormat { get; set; } = "";
        public string Description
        {
            get => (Experimental ? "[EXPERIMENTAL]\n" : "") + DescriptionFormatting.Apply(CurrentDescriptionFormat, this);
        }

        public virtual string CurrentDescriptionFormat
        {
            get => DescriptionFormat;
        }

        public virtual FormattingMap DescriptionFormatting
        {
            get => new FormattingMap();
        }

        public string Text { get; set; }

        public bool FlagEnabled { get; 
            set; }
        public string FlagID { get; set; }

        public string GetFlagString()
        {
            if (!this.FlagEnabled)
                return "";
            string output = "-" + FlagID;
            output += GetExtraFlagString();
            return output;
        }

        public virtual string GetExtraFlagString()
        {
            return "";
        }

        public void SetFlagString(string value, bool simulate)
        {
            string input = value;
            string name;
            if (input.Contains("["))
                name = input.Substring(1, input.IndexOf('[') - 1);
            else
                name = input.Substring(1);
            if (name == FlagID)
            {
                if (!simulate)
                    FlagEnabled = true;
                if (input.Contains("[") && input.Contains("]"))
                    SetExtraFlagString(input.Substring(input.IndexOf("[") + 1, input.IndexOf("]") - input.IndexOf("[") - 1), simulate);
            }
        }

        public virtual void SetExtraFlagString(string value, bool simulate)
        {
        }

        public static readonly Flag Empty = EmptyFlag();

        private static Flag EmptyFlag()
        {
            Flag flag = new Flag();
            flag.Text = "";
            return flag;
        }

        public Random Random { get; set; }

        public void ResetRandom(int seed)
        {
            Random = new Random(seed + FlagID[0] * FlagID.Length - Description.Length);
        }

        public void SetRand()
        {
            RandomNum.SetRand(Random);
        }
    }
}
