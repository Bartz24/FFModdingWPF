using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF12Rando
{
    public class FF12Flags
    {
        public enum FlagType
        {
            All = -1,
            Items,
            Other
        }
        public class Items
        {
            public static Flag Treasures;
            public static ToggleFlagProperty KeyMain, KeyHunt, KeyGrindy, KeySide, KeyOrb, KeyWrit, KeyPlaceTreasure, KeyPlaceHunt, KeyPlaceClanRank, KeyPlaceClanBoss, KeyPlaceClanEsper, KeyPlaceGrindy;
            public static ComboBoxFlagProperty KeyDepth;

            internal static void Init()
            {
                Treasures = new Flag()
                {
                    Text = "Randomize Item Locations",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize treasures and misc rewards.\n" +
                    "Any key items in the pool will by default be shuffled between themselves."
                }.Register(FlagType.Items);

                KeyMain = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Main Key Items",
                    ID = "KeyMain",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Clan Primer, Shadestone, Sunstone, Crescent Stone, Sword of the Order, No. 1 Brig Key, Systems Access Key, Manufacted Nethicite, Dawn Shard, Goddess's Magicite, Tube Fuse, Lente's Tear, Sword of Kings, Soul Ward Key, First 3 Pinewood Chops, Lab Access Card, Treaty-Blade, First 3 Black Orbs"
                }.Register(Treasures);

                KeySide = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Side Key Items",
                    ID = "KeySide",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Barheim Key, Stone of the Condemner, Wind Globe, Windvane, Sluice Gate Key, Merchant's Armband, Pilika's Diary, Site 3 Key, Site 11 Key, Dragon Scale, Ageworn Key, Dusty Letter, Blackened Fragment, Dull Fragment, Grimy Fragment, Moonsilver Fragment, Medallion of Bravery, Medallion of Love, Lusterless Medallion, Medallion of Might, Ann's Letter"
                }.Register(Treasures);

                KeyGrindy = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Grindy Key Items",
                    ID = "KeyGrindy",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Last 25 Pinewood Chops, Sandalwood Chop"
                }.Register(Treasures);

                KeyHunt = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Hunt Key Items",
                    ID = "KeyHunt",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Cactus Flower, Broken Key, Errmonea Leaf, Rabbit Tail, Ring of the Toad, Rusted Scrap of Armor, Silent Urn, Stolen Articles, Ring of the Light, Serpentwyne Must, Viera Rucksack"
                }.Register(Treasures);

                KeyOrb = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Subterra Black Orbs",
                    ID = "KeyOrb",
                    Description = "The following key items will be included in the key item pool:\n" +
                    "Last 21 Black Orbs in the Subterra.\n" +
                    "For reference, up to 8 can be found in Penumbra, up to 15 in Umbra, and up to 24 in Abyssal."
                }.Register(Treasures);

                KeyWrit = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Writ of Transit",
                    ID = "KeyWrit",
                    Description = "The Writ of Transit will be included in the key item pool.\n" +
                    "If turned off, this will be replaced with potions."
                }.Register(Treasures);

                KeyPlaceTreasure = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Treasures",
                    ID = "KeyPlaceTreas",
                    Description = "Key items are also allowed in treasures and misc rewards."
                }.Register(Treasures);

                KeyPlaceHunt = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Hunts",
                    ID = "KeyPlaceHunt",
                    Description = "Key items are also allowed in hunt rewards."
                }.Register(Treasures);

                KeyPlaceClanRank = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Clan Rank",
                    ID = "KeyPlaceRank",
                    Description = "Key items are also allowed in clan rank rewards."
                }.Register(Treasures);

                KeyPlaceClanBoss = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Clan Boss",
                    ID = "KeyPlaceClanBoss",
                    Description = "Key items are also allowed in clan boss rewards."
                }.Register(Treasures);

                KeyPlaceClanEsper = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Clan Esper",
                    ID = "KeyPlaceClanEsper",
                    Description = "Key items are also allowed in clan esper rewards."
                }.Register(Treasures);

                KeyPlaceGrindy = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Grindy",
                    ID = "KeyPlaceGrindy",
                    Description = "Key items are also allowed in grindy rewards.\n" +
                    "This includes: Ann's Sister Quest reward"
                }.Register(Treasures);

                KeyDepth = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Item Difficulty Depth",
                    ID = "KeyDepth",
                    Description = "Key items will be more likely to appear in later locations.\n\n" +
                    "Depths:\n" +
                    "    Normal - Each location is equally likely.\n" +
                    "    Hard - Each level of depth/difficulty increases likelyhood of that location by 1.10x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelyhood of that location by 1.20x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelyhood of that location by 1.50x.\n" +
                    "    Hard+++ - Locations of the highest depth/difficulty will tend to be preferred.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
                }.Register(Treasures);
            }
        }
        public class Other
        {
            public static Flag Party;
            public static Flag LicenseBoards, StartingBoards;
            public static Flag ExpMult, LPMult;
            public static Flag HintsMain, HintAbilities;
            public static ComboBoxFlagProperty HintsSpecific;
            public static NumberFlagProperty EXPMultAmt, LPMultAmt;

            internal static void Init()
            {
                Party = new Flag()
                {
                    Text = "Randomize Main Party",
                    FlagID = "RandParty",
                    DescriptionFormat = "Randomizes the main party members except for Vaan."
                }.Register(FlagType.Other);


                LicenseBoards = new Flag()
                {
                    Text = "Randomize License Boards",
                    FlagID = "RandBoards",
                    DescriptionFormat = "Randomizes the license boards by selecting a left and right side."
                }.Register(FlagType.Other);


                StartingBoards = new Flag()
                {
                    Text = "Randomize Starting License Boards",
                    FlagID = "StartBoards",
                    DescriptionFormat = "Sets a random starting license boards of each character. Can still be reset at Montblanc."
                }.Register(FlagType.Other);

                HintsMain = new Flag()
                {
                    Text = "Hints from Moogles",
                    FlagID = "HintsMain",
                    DescriptionFormat = "Allows each Sky Pirate's Den award and the Dusty Letter provide hints from the moogle cartographers.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                HintsSpecific = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Specificity",
                    ID = "HintsSpecific",
                    Description = "Set the specificity for the hints from main quests.\n\n" +
                    "Options:\n" +
                    "    Exact - Hints give the exact item in the exact location.\n" +
                    "    Vague Type - Hints give the type ('Main Key Item'/'Side Key Item'/'Hunt Key Item'/'Ability'/'Other') in the exact location.\n" +
                    "    Vague Area - Hints give the exact item in the area.\n" +
                    "    Unknown but Exact Location - Hints will hint that something ('?????') is in the exact location.\n" +
                    "    Random - Each hint will use one of the above rules.",
                    Values = new string[] { "Exact", "Vague Type", "Vague Area", "Unknown but Exact Location", "Random" }.ToList()
                }.Register(HintsMain);

                HintAbilities = new Flag()
                {
                    Text = "Hint Abilities",
                    FlagID = "HintAbi",
                    DescriptionFormat = "Hints will include information on abilities as well."
                }.Register(FlagType.Other);

                ExpMult = new Flag()
                {
                    Text = "EXP Multiplier",
                    FlagID = "EXPMult",
                    DescriptionFormat = "Set to the percentage to scale EXP.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                EXPMultAmt = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "",
                    ID = "EXPMultAmt",
                    Description = "",
                    ValueText = "EXP Mult (%):",
                    MinValue = 0,
                    MaxValue = 1000,
                    StepSize = 10
                }.Register(ExpMult);

                LPMult = new Flag()
                {
                    Text = "LP Multiplier",
                    FlagID = "LPMult",
                    DescriptionFormat = "Set to the percentage to scale LP.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                LPMultAmt = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "",
                    ID = "LPMultAmt",
                    Description = "",
                    ValueText = "LP Mult (%):",
                    MinValue = 0,
                    MaxValue = 1000,
                    StepSize = 10
                }.Register(LPMult);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            Items.Init();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

