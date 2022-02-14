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
            public static ToggleFlagProperty KeyWild, KeyGraviton, KeyGateSeal, KeySide, KeyPlaceTreasure, KeyPlaceBrainBlast;
            //public static ComboBoxFlagProperty KeyDepth;

            internal static void Init()
            {
                Treasures = new Flag()
                {
                    Text = "Randomize Treasures",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize treasure spheres and cubes, and non-useful fragments.\n" +
                    "Any key items in the pool will by default be shuffled between themselves.\n" +
                    "Does not include normal artefacts and event based items and fragments."
                }.Register(FlagType.Items);

                KeyWild = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Wild Artefacts",
                    ID = "KeyWild",
                    Description = "Wild Artefacts will be included in the pool of key items."
                }.Register(Treasures);

                KeyGraviton = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Graviton Core Fragments",
                    ID = "KeyGrqaviton",
                    Description = "The 7 Graviton Core fragments will be included in the pool of key items."
                }.Register(Treasures);

                KeyGateSeal = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Gate Seals",
                    ID = "KeySeal",
                    Description = "The gate seals will be included in the pool of key items."
                }.Register(Treasures);

                KeySide = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Side Key Items",
                    ID = "KeySide",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Medical Kit, Capsules, Holding Cell Key, Comm Device, Emerald Crystal, Ivory Crystal, Onyx Crystal, Service Manual, Fruit of Fenrir, Tablet of Paddra, Old Battery, Sealed Tablet, Army Comm Device, Recording Device, Picture Frame, Bulb of Hope, Terrorists' Mark, Weapon Material, Outdoor Watch, Personal Notes, Paradox Agent Type A-C, Supply Sphere Access Code"
                }.Register(Treasures);

                KeyPlaceTreasure = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Treasures",
                    ID = "KeyPlaceTreas",
                    Description = "Key items are also allowed in treasures and fragment spots."
                }.Register(Treasures);

                KeyPlaceBrainBlast = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Brain Blast",
                    ID = "KeyPlaceBrain",
                    Description = "Key items are also allowed in Brain Blast rewards."
                }.Register(Treasures);
                /*
                KeyDepth = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Item Difficulty Depth",
                    ID = "KeyDepth",
                    Description = "Key items will be more likely to appear in longer chains of key items and more difficult/time-consuming and later locations.\n\n" +
                    "Depths:\n" +
                    "    Normal - Each location is equally likely.\n" +
                    "    Hard - Each level of depth/difficulty increases likelyhood of that location by 1.05x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelyhood of that location by 1.10x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelyhood of that location by 1.25x.\n" +
                    "    Hard+++ - Locations of the highest depth/difficulty will tend to be preferred.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
                }.Register(Treasures);*/
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

