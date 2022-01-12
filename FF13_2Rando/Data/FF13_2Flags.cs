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
            public static ToggleFlagProperty LargeEnc, Bosses, DLCBosses;

            internal static void Init()
            {
                EnemyLocations = new Flag()
                {
                    Text = "Randomize Enemy Locations",
                    FlagID = "RandEne",
                    DescriptionFormat = "Randomize normal enemies between each other."
                }.Register(FlagType.Enemies);

                LargeEnc = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Allow Larger Encounters",
                    ID = "LargeEnc",
                    Description = "[EXPERIMENTAL]\n" +
                    "Allows encounters to have 5 enemies or more. Not recommended as this seems to be cause of some crashes with random enemies."
                }.Register(EnemyLocations);


                Bosses = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Shuffle Bosses",
                    ID = "RandBoss",
                    Description = "Shuffle the following bosses between each other:\n" +
                    "Gogmagog (Alpha, Beta and Gamma), Aloedai, Caius (Oerba, Void Beyond, Dying World, Deck, Beach, and Paradoxes)\n"
                }.Register(EnemyLocations);

                DLCBosses = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include DLC Bosses",
                    ID = "RandDLCBoss",
                    Description = "Include the following bosses for boss shuffling:\n" + "Lightning, Amodar, Snow, Gilgamesh (1 and 2)\n"
                }.Register(EnemyLocations);
            }
        }
        public class Other
        {
            public static Flag RandCrystAbi;
            public static Flag HistoriaCrux;
            public static Flag InitCP;
            public static Flag Music;
            public static ComboBoxFlagProperty ForcedStart;
            public static NumberFlagProperty InitCPAmount;

            internal static void Init()
            {
                RandCrystAbi = new Flag()
                {
                    Text = "Randomize Crystarium Abilities",
                    FlagID = "RandCrystAbi",
                    DescriptionFormat = "Randomizes the crystarium abilities.",
                    Aesthetic = true
                }.Register(FlagType.Other);


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

                InitCP = new Flag()
                {
                    Text = "Start with CP",
                    FlagID = "InitCP",
                    DescriptionFormat = "Start with a specified amount of CP set below.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                InitCPAmount = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "",
                    ID = "InitCPAmt",
                    Description = "",
                    ValueText = "CP:",
                    MinValue = 500,
                    MaxValue = 10000,
                    StepSize = 500
                }.Register(InitCP);
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

