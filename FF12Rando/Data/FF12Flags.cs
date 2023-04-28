using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF12Rando;

public class FF12Flags
{
    public enum FlagType
    {
        Debug = RandoFlags.FlagTypeDebug,
        All = RandoFlags.FlagTypeAll,
        Stats,
        Items,
        Other
    }
    public class Stats
    {
        public static Flag EquipStats, EquipElements, EquipStatus, EquipAugments;
        public static ToggleFlagProperty EquipHiddenStats;
        internal static void Init()
        {
            EquipStats = new Flag()
            {
                Text = "Randomize Equipment Stats",
                FlagID = "EquipStats",
                DescriptionFormat = "Randomize equipment stats that are visible in menus.\n" +
                "Also affects enemies."
            }.Register(FlagType.Items);

            EquipHiddenStats = new ToggleFlagProperty()
            {
                Text = "Include Hidden Stats",
                ID = "EquipHiddenStat",
                Description = "Randomize the following hidden stats on weapons as well.\n" +
                "Knockback, Combo/Crit Chance, Charge Time"
            }.Register(EquipStats);

            EquipElements = new Flag()
            {
                Text = "Randomize Equipment Elements",
                FlagID = "EquipElem",
                DescriptionFormat = "Randomize equipment on-hit, absorb, immune, weak, and potency elements.\n" +
                "Also affects enemies."
            }.Register(FlagType.Items);

            EquipAugments = new Flag()
            {
                Text = "Randomize Equipment Augments",
                FlagID = "EquipAug",
                DescriptionFormat = "Randomize armor and accessory augments.\n" +
                "Also affects enemies."
            }.Register(FlagType.Items);

            EquipStatus = new Flag()
            {
                Text = "Randomize Equipment Status Effects",
                FlagID = "EquipEff",
                DescriptionFormat = "Randomize equipment on-hit, on-equip, and immunity status effects.\n" +
                "Also affects enemies."
            }.Register(FlagType.Items);
        }
    }

    public class Items
    {
        public static Flag Treasures, Shops, Bazaars, StartingTpStones;
        public static ToggleFlagProperty KeyMain, KeyHunt, KeyGrindy, KeySide, KeyOrb, KeyWrit, KeyTrophy, KeyStartingInv, KeyPlaceTreasure, KeyPlaceHunt, KeyPlaceClanRank, KeyPlaceClanBoss, KeyPlaceClanEsper, KeyPlaceGrindy, KeyPlaceHidden, CharacterScale;
        public static ComboBoxFlagProperty KeyDepth;
        public static NumberFlagProperty ShopSize;
        public static ToggleFlagProperty ShopsShared;
        public static NumberFlagProperty TpStoneCount;

        internal static void Init()
        {
            Treasures = new Flag()
            {
                Text = "Randomize Item Locations",
                FlagID = "Treasures",
                DescriptionFormat = "Randomize treasures and misc rewards.\n" +
                "Any key items in the pool will by default be shuffled between themselves."
            }.Register(FlagType.Items);

            KeyMain = new ToggleFlagProperty()
            {
                Text = "Include Main Key Items",
                ID = "KeyMain",
                Description = "The following key items will be included in the key item pool:\n" +
                "Clan Primer, Shadestone, Sunstone, Crescent Stone, Sword of the Order, No. 1 Brig Key, Systems Access Key, Manufacted Nethicite, Dawn Shard, Goddess's Magicite, Tube Fuse, Lente's Tear, Sword of Kings, Soul Ward Key, First 3 Pinewood Chops, Lab Access Card, Treaty-Blade, First 3 Black Orbs"
            }.Register(Treasures);

            KeySide = new ToggleFlagProperty()
            {
                Text = "Include Side Key Items",
                ID = "KeySide",
                Description = "The following key items will be included in the key item pool:\n" +
                "Barheim Key, Stone of the Condemner, Wind Globe, Windvane, Sluice Gate Key, Merchant's Armband, Pilika's Diary, Site 3 Key, Site 11 Key, Dragon Scale, Ageworn Key, Dusty Letter, Blackened Fragment, Dull Fragment, Grimy Fragment, Moonsilver Fragment, Medallion of Bravery, Medallion of Love, Lusterless Medallion, Medallion of Might, Ann's Letter"
            }.Register(Treasures);

            KeyGrindy = new ToggleFlagProperty()
            {
                Text = "Include Grindy Key Items",
                ID = "KeyGrindy",
                Description = "The following key items will be included in the key item pool:\n" +
                "Last 25 Pinewood Chops, Sandalwood Chop"
            }.Register(Treasures);

            KeyHunt = new ToggleFlagProperty()
            {
                Text = "Include Hunt Key Items",
                ID = "KeyHunt",
                Description = "The following key items will be included in the key item pool:\n" +
                "Cactus Flower, Broken Key, Errmonea Leaf, Rabbit Tail, Ring of the Toad, Rusted Scrap of Armor, Silent Urn, Stolen Articles, Ring of the Light, Serpentwyne Must, Viera Rucksack"
            }.Register(Treasures);

            KeyOrb = new ToggleFlagProperty()
            {
                Text = "Include Subterra Black Orbs",
                ID = "KeyOrb",
                Description = "The following key items will be included in the key item pool:\n" +
                "Last 21 Black Orbs in the Subterra.\n" +
                "For reference, up to 8 can be found in Penumbra, up to 15 in Umbra, and up to 24 in Abyssal."
            }.Register(Treasures);

            KeyTrophy = new ToggleFlagProperty()
            {
                Text = "Include Hunt Club Trophies",
                ID = "KeyTrophy",
                Description = "The 31 Hunt Club trophy drops from rare game will be included in the key item pool."
            }.Register(Treasures);

            KeyWrit = new ToggleFlagProperty()
            {
                Text = "Include Writ of Transit",
                ID = "KeyWrit",
                Description = "The Writ of Transit will be included in the key item pool.\n" +
                "If turned off, this will be replaced with potions."
            }.Register(Treasures);

            KeyStartingInv = new ToggleFlagProperty()
            {
                Text = "Include Party Member Starting Items",
                ID = "KeyStartingInv",
                Description = "The items in the main party members' starting inventories will be included in the pool."
            }.Register(Treasures);

            KeyPlaceTreasure = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Treasures",
                ID = "KeyPlaceTreas",
                Description = "Key items are also allowed in treasures and misc rewards."
            }.Register(Treasures);

            KeyPlaceHunt = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Hunts",
                ID = "KeyPlaceHunt",
                Description = "Key items are also allowed in hunt rewards and Jovy's reward."
            }.Register(Treasures);

            KeyPlaceClanRank = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Clan Rank",
                ID = "KeyPlaceRank",
                Description = "Key items are also allowed in clan rank rewards."
            }.Register(Treasures);

            KeyPlaceClanBoss = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Clan Boss",
                ID = "KeyPlaceClanBoss",
                Description = "Key items are also allowed in clan boss rewards."
            }.Register(Treasures);

            KeyPlaceClanEsper = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Clan Esper",
                ID = "KeyPlaceClanEsper",
                Description = "Key items are also allowed in clan esper rewards."
            }.Register(Treasures);

            KeyPlaceGrindy = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Grindy",
                ID = "KeyPlaceGrindy",
                Description = "Key items are also allowed in grindy rewards.\n" +
                "This includes: Ann's Sister Quest reward, Hunt Club owner rewards"
            }.Register(Treasures);

            KeyPlaceHidden = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Hidden Starting Items",
                ID = "KeyPlaceHidden",
                Description = "Key items are also allowed in main party member starting inventories."
            }.Register(Treasures);

            KeyDepth = new ComboBoxFlagProperty()
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

            CharacterScale = new ToggleFlagProperty()
            {
                Text = "Character Scaled Depth",
                ID = "KeyPlaceCharScale",
                Description = "Key items placed in later locations will require a certain amount of characters unlocked."
            }.Register(Treasures);

            Shops = new Flag()
            {
                Text = "Randomize Shops",
                FlagID = "Shops",
                DescriptionFormat = "Randomize contents of shops. These items can appear in treasures or bazaars if those flags are on."
            }.Register(FlagType.Items);

            ShopsShared = new ToggleFlagProperty()
            {
                Text = "Same Shops in the Same Area",
                ID = "ShopsSameArea",
                Description = "Shops in the same area share the same contents."
            }.Register(Shops);

            ShopSize = new NumberFlagProperty()
            {
                Text = "",
                ID = "ShopSize",
                Description = "Set the maximum number of items per shop. Clan shop upgrades are maxed at 2.",
                ValueText = "Shop Size:",
                MinValue = 0,
                MaxValue = 30,
                StepSize = 1
            }.Register(Shops);

            Bazaars = new Flag()
            {
                Text = "Randomize Bazaars",
                FlagID = "Bazaars",
                DescriptionFormat = "Randomize contents of bazaars. These items can appear in treasures or shops if those flags are on."
            }.Register(FlagType.Items);

            StartingTpStones = new Flag()
            {
                Text = "Add Teleport Stones to Starting Inventory",
                FlagID = "StartingTpStone",
                DescriptionFormat = "Adds the specified number of teleport stones to the starting inventory."
            }.Register(FlagType.Items);

            TpStoneCount = new NumberFlagProperty()
            {
                Text = "",
                ID = "TpStoneCount",
                Description = "",
                ValueText = "",
                MinValue = 1,
                MaxValue = 99,
                StepSize = 1
            }.Register(StartingTpStones);
        }
    }
    public class Other
    {
        public static Flag Party, Music;
        public static Flag LicenseBoards, StartingBoards;
        public static Flag EXPMult, LPMult;
        public static Flag HintsMain, HintAbilities;
        public static ComboBoxFlagProperty HintsSpecific;
        public static NumberFlagProperty EXPMultAmt, LPMultAmt, EXPMultBossAmt, LPMultBossAmt;

        internal static void Init()
        {
            Party = new Flag()
            {
                Text = "Shuffle Main Party",
                FlagID = "RandParty",
                DescriptionFormat = "Shuffles the main party members."
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

            HintsSpecific = new ComboBoxFlagProperty()
            {
                Text = "Specificity",
                ID = "HintsSpecific",
                Description = "Set the specificity for the hints from main quests.\n\n" +
                "Options:\n" +
                "    Exact - Hints give the exact item in the exact location.\n" +
                "    Vague Type - Hints give the type ('Main Key Item'/'Side Key Item'/'Hunt Key Item'/'Chop'/'Black Orb'/'Writ of Transit'/'Trophy'/'Ability'/'Other') in the exact location.\n" +
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

            Music = new Flag()
            {
                Text = "Shuffle Music",
                FlagID = "Music",
                DescriptionFormat = "Shuffle music around using the supplied music packs.\n" +
                "Music packs are required, even for music from the base game.",
                Aesthetic = true
            }.Register(FlagType.Other);

            EXPMult = new Flag()
            {
                Text = "EXP Multipliers",
                FlagID = "EXPMult",
                DescriptionFormat = "Set to the percentages to scale EXP.",
                Aesthetic = true
            }.Register(FlagType.Other);

            EXPMultAmt = new NumberFlagProperty()
            {
                Text = "Normal and Rare Game Enemies",
                ID = "EXPMultAmt",
                Description = "",
                ValueText = "EXP Mult (%):",
                MinValue = 0,
                MaxValue = 1000,
                StepSize = 10
            }.Register(EXPMult);

            EXPMultBossAmt = new NumberFlagProperty()
            {
                Text = "Boss and Mark Enemies",
                ID = "EXPMultBossAmt",
                Description = "",
                ValueText = "EXP Mult (%):",
                MinValue = 0,
                MaxValue = 1000,
                StepSize = 10
            }.Register(EXPMult);

            LPMult = new Flag()
            {
                Text = "LP Multipliers",
                FlagID = "LPMult",
                DescriptionFormat = "Set to the percentages to scale LP.",
                Aesthetic = true
            }.Register(FlagType.Other);

            LPMultAmt = new NumberFlagProperty()
            {
                Text = "Normal and Rare Game Enemies",
                ID = "LPMultAmt",
                Description = "",
                ValueText = "LP Mult (%):",
                MinValue = 0,
                MaxValue = 1000,
                StepSize = 10
            }.Register(LPMult);

            LPMultBossAmt = new NumberFlagProperty()
            {
                Text = "Boss and Mark Enemies",
                ID = "LPMultBossAmt",
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
        RandoFlags.FlagsList.Clear();
        Stats.Init();
        Items.Init();
        Other.Init();
        RandoFlags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
        RandoFlags.SelectedCategory = "All";
    }
}

