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
            Stats,
            Items,
            Enemies,
            Other
        }
        public class Stats
        {
            public static Flag RandCrystAbi;
            public static Flag InitCP;
            public static NumberFlagProperty InitCPAmount, WeightRange;
            public static Flag EquipStats, EquipPassives, EquipWeights;

            internal static void Init()
            {
                RandCrystAbi = new Flag()
                {
                    Text = "Randomize Crystarium Abilities",
                    FlagID = "RandCrystAbi",
                    DescriptionFormat = "Randomizes the crystarium abilities.",
                    Aesthetic = true
                }.Register(FlagType.Stats);

                InitCP = new Flag()
                {
                    Text = "Start with CP",
                    FlagID = "InitCP",
                    DescriptionFormat = "Start with a specified amount of CP set below.",
                    Aesthetic = true
                }.Register(FlagType.Stats);

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

                EquipStats = new Flag()
                {
                    Text = "Randomize Weapon Stats",
                    FlagID = "RandEqStat",
                    DescriptionFormat = "Randomize weapon stats."
                }.Register(FlagType.Stats);

                EquipPassives = new Flag()
                {
                    Text = "Randomize Equipment Passive Abilities",
                    FlagID = "RandPassive",
                    DescriptionFormat = "Randomize passive abilities on weapons and accessories."
                }.Register(FlagType.Stats);

                EquipWeights = new Flag()
                {
                    Text = "Randomize Accessory Weights",
                    FlagID = "RandAccW",
                    DescriptionFormat = "Randomize accessory weights."
                }.Register(FlagType.Stats);

                WeightRange = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "",
                    ID = "AccWRange",
                    Description = "",
                    ValueText = "+/-",
                    MinValue = 0,
                    MaxValue = 100,
                    StepSize = 5
                }.Register(EquipWeights);
            }
        }
        public class Items
        {
            public static Flag Treasures;
            public static ToggleFlagProperty KeyWild, KeyGraviton, KeyGateSeal, KeySide, KeyPlaceTreasure, KeyPlaceBrainBlast, KeyPlaceThrowCryst, KeyPlaceThrowJunk;
            public static ComboBoxFlagProperty KeyDepth;

            internal static void Init()
            {
                Treasures = new Flag()
                {
                    Text = "Randomize Item Locations",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize treasure spheres and cubes, Improved Moogle Throw search items, and non-useful fragments.\n" +
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
                    ID = "KeyGraviton",
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

                KeyPlaceThrowCryst = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Improved Moogle Throw Monster Crystals",
                    ID = "KeyPlaceMogCryst",
                    Description = "Key items are also allowed to replace Improve Moogle Throw monster crystal locations."
                }.Register(Treasures);

                KeyPlaceThrowJunk = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Improved Moogle Throw Junk",
                    ID = "KeyPlaceMogJunk",
                    Description = "Key items are also allowed to replace Improve Moogle Throw junk locations."
                }.Register(Treasures);

                KeyDepth = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Item Difficulty Depth",
                    ID = "KeyDepth",
                    Description = "Key items will be more likely to appear in later locations.\n\n" +
                    "Depths:\n" +
                    "    Normal - Each location is equally likely.\n" +
                    "    Hard - Each level of depth/difficulty increases likelihood of that location by 1.10x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelihood of that location by 1.20x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelihood of that location by 1.50x.\n" +
                    "    Hard+++ - Each level of depth/difficulty increases likelihood of that location by 2.00x.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
                }.Register(Treasures);
            }
        }
        public class Enemies
        {
            public static Flag EnemyLocations;
            public static NumberFlagProperty EnemyRank;
            public static ToggleFlagProperty LargeEnc, DLCBosses;
            public static ListBoxFlagProperty Bosses;

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
                    "Allows encounters to have 5 enemies or more. Not recommended as this seems to be cause of some crashes with random enemies.",
                    Experimental = true
                }.Register(EnemyLocations);


                Bosses = (ListBoxFlagProperty)new ListBoxFlagProperty()
                {
                    Text = "Shuffled Bosses",
                    ID = "RandBoss",
                    Description = "Select the bosses to be shuffled. Unselected bosses will stay where they are.",
                    Values =
                    {
                        "Gogmagog Alpha",
                        "Gogmagog Beta",
                        "Aloeidai",
                        "Caius (Oerba)",
                        "Mutantomato",
                        "Caius (Void Beyond)",
                        "Caius (Dying World)",
                        "Gogmagog Gamma",
                        "Pacos Amethyst/Luvulite",
                        "Caius (Deck)",
                        "Caius (Beach)",
                        "Caius (Oerba Paradox)",
                        "Caius (Void Beyond Paradox)",
                        "Caius (Dying World Paradox)",
                        "Lightning",
                        "Amodar",
                        "Snow",
                        "Gilgamesh 1",
                        "Gilgamesh 2",
                        "Kalavinka",
                        "Ugallu",
                        "Gorgyra"
                    }
                }.Register(EnemyLocations);

                EnemyRank = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "Enemy Rank Range",
                    ID = "EnemyRank",
                    Description = "Enemies can be replaced by enemies by enemies within the specified value of its \"Rank\".",
                    ValueText = "Enemy Rank +/-",
                    MinValue = 0,
                    MaxValue = 15
                }.Register(EnemyLocations);
            }
        }
        public class Other
        {
            public static Flag HistoriaCrux;
            public static Flag Music;
            public static ComboBoxFlagProperty ForcedStart;
            public static ToggleFlagProperty RandoDLC;

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

                RandoDLC = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include DLC Areas",
                    ID = "RandDLCCrux",
                    Description = "Includes the Lightning, Sazh, and Coliseum DLC into the pool. Turning this on will also allow 3 additional areas open from the start.\n\n" +
                    "[NOTE]\n" +
                    "This requires a separate mod 'DLC Restoration - Console Content' that is provided as a core mod for Nova. Download this mod from the Core Mods download in the Nova discord server."
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
            Stats.Init();
            Items.Init();
            Enemies.Init();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

