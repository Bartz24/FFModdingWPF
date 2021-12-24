using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF13_2Rando
{
    public class FF13_2Flags
    {
        public enum FlagType
        {
            All = -1,
            Other
        }
        public class Other
        {
            public static Flag HistoriaCrux;

            internal static void Init()
            {
                HistoriaCrux = new Flag()
                {
                    Text = "Randomize Historia Crux",
                    FlagID = "HistCrux",
                    DescriptionFormat = "Randomizes the Historia Crux map.",
                    Aesthetic = true
                }.Register(FlagType.Other);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

