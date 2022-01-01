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
            Items,
            Enemies,
            Other
        }
        public class Items
        {
            public static Flag Treasures;

            internal static void Init()
            {
                Treasures = new Flag()
                {
                    Text = "Randomize Treasures",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize Treasures.\n" +
                    "Does not include key items, fragments, or artefacts, or live rewards."
                }.Register(FlagType.Items);
            }
        }
        public class Enemies
        {
            public static Flag EnemyLocations;

            internal static void Init()
            {
                EnemyLocations = new Flag()
                {
                    Text = "Randomize Enemy Locations",
                    FlagID = "RandEne",
                    DescriptionFormat = "Randomize normal enemies between each other."
                }.Register(FlagType.Enemies);
            }
        }
        public class Other
        {
            public static Flag HistoriaCrux;
            public static Flag Music;
            public static ComboBoxFlagProperty ForcedStart;

            internal static void Init()
            {
                HistoriaCrux = new Flag()
                {
                    Text = "Randomize Historia Crux",
                    FlagID = "HistCrux",
                    DescriptionFormat = "Randomizes the Historia Crux map.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                ForcedStart = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Forced Start",
                    ID = "ForcedStart",
                    Description = "Options:\n" +
                    "None - Any valid starting area is possible.\n" +
                    "Bodhum - Force starting in New Bodhum 3 AF. (Recommended to avoid softlocks and resets).\n" +
                    "Bodhum & Bresha - Force starting in New Bodhum 3 AF leading to Bresha Ruins 5 AF for branching options.",
                    Values = new string[] { "None", "Bodhum", "Bodhum & Bresha" }.ToList()
                }.Register(HistoriaCrux);

                Music = new Flag()
                {
                    Text = "Shuffle Music",
                    FlagID = "Music",
                    DescriptionFormat = "Shuffle music around.",
                    Aesthetic = true
                }.Register(FlagType.Other);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            Items.Init();
            Enemies.Init();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

