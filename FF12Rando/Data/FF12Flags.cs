using Bartz24.RandoWPF;
using MaterialDesignThemes.Wpf;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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
        public static Flag Treasures, Shops, Bazaars, StartingTpStones, AllowSeitengrat;
        public static ToggleFlagProperty KeyStartingInv, KeyPlaceTreasure, KeyPlaceHunt, KeyPlaceClanRank, KeyPlaceClanBoss, KeyPlaceClanEsper, KeyPlaceGrindy, KeyPlaceHidden, CharacterScale, JunkRankScale, ReplaceAny;
        public static ComboBoxFlagProperty KeyDepth;
        public static NumberFlagProperty ShopSize;
        public static ToggleFlagProperty ShopsShared, JunkRankScaleShops;
        public static NumberFlagProperty TpStoneCount, ReplaceRank;
        public static ListBoxFlagProperty WritGoals;
        public const string WritGoalCid2 = "Defeat Cid 2 in Pharos";
        public const string WritGoalAny = "Find in Any Random Location";
        public const string WritGoalMaxSphere = "Find on Random Max Sphere Location";
        public static DictListBoxFlagProperty<string> KeyItems;
        public static NumberFlagProperty KeyChops;
        public static NumberFlagProperty KeyBlackOrbs;

        internal static void Init()
        {
            Treasures = new Flag()
            {
                Text = "Randomize Item Locations",
                FlagID = "Treasures",
                DescriptionFormat = "Randomize treasures and misc rewards.\n" +
                "Any key items in the pool will by default be shuffled between themselves."
            }.Register(FlagType.Items);

            WritGoals = new ListBoxFlagProperty()
            {
                Text = "Bahamut Unlock Conditions",
                ID = "WritGoals",
                Description = "Sets the possible win conditions to unlock the Bahamut. The Writ of Transit allows you to travel to Bahamut.\n" +
                "You only need to find one Writ of Transit, but can set multiple ways to get a Writ of Transit.\n\n" +
                " -Defeat Cid 2 in Pharos: Find 1 Sword (Sword of Kings or Treaty-Blade) and 2 magick stones (Dawn Shard, Goddess's Magicite, Manufacted Nethicite) and defeat Cid 2 to get a Writ of Transit.\n" +
                " -Find in Any Random Location: A randomly placed Writ of Transit can show up in any non-missable location.\n" +
                " -Find on Random Max Sphere Location: A Writ of Transit will be placed on a random location at the maximum sphere. A long chain of key items would be required.",
                Values =
                {
                    WritGoalCid2,
                    WritGoalAny,
                    WritGoalMaxSphere
                }
            }.Register(Treasures);

            KeyItems = new DictListBoxFlagProperty<string>()
            {
                Text = "Include Key Items",
                ID = "KeyItems",
                Description = "Key items to be included in the item pool.\n" +
                "Key items will not appear in missable locations or from Day 10 and later.",
                DictValues =
                {
                    {"80E1", "Rabanastre Aeropass" },
                    {"80E2", "Nalbina Aeropass" },
                    {"8071", "Clan Primer" },
                    {"8079", "Shadestone" },
                    {"807A", "Sunstone" },
                    {"8080", "Crescent Stone" },
                    {"80AC", "Goddess's Magicite" },
                    {"8074", "Tube Fuse" },
                    {"8075", "Sword of the Order" },
                    {"80E3", "Bhujerba Aeropass" },
                    {"8076", "No. 1 Brig Key" },
                    {"8077", "Systems Access Key" },
                    {"8089", "Manufacted Nethicite" },
                    {"8078", "Dawn Shard" },
                    {"80E0", "Rainstone" },
                    {"80B7", "Lente's Tear" },
                    {"808B", "Sword of Kings" },
                    {"80B6", "Soul Ward Key" },
                    {"80E4", "Archades Aeropass" },
                    {"809E", "Lab Access Card" },
                    {"808C", "Treaty-Blade" },
                    {"80E5", "Balfonheim Aeropass" },
                    {"8093", "Barheim Key" },
                    {"8085", "Stone of the Condemner" },
                    {"80B1", "Wind Globe" },
                    {"80B2", "Windvane" },
                    {"80AF", "Sluice Gate Key" },
                    {"8098", "Merchant's Armband" },
                    {"8099", "Pilika's Diary" },
                    {"807E", "Site 3 Key" },
                    {"807F", "Site 11 Key" },
                    {"80B4", "Dragon Scale" },
                    {"80B5", "Ageworn Key" },
                    {"808D", "Dusty Letter" },
                    {"8090", "Blackened Fragment" },
                    {"808F", "Dull Fragment" },
                    {"8091", "Grimy Fragment" },
                    {"80A3", "Moonsilver Medallion" },
                    {"8081", "Medallion of Bravery" },
                    {"8083", "Medallion of Love" },
                    {"8084", "Lusterless Medallion" },
                    {"8082", "Medallion of Might" },
                    {"8088", "Ann's Letter" },
                    {"2112", "Sandalwood Chop" },
                    {"8073", "Cactus Flower" },
                    {"80AE", "Broken Key" },
                    {"8086", "Errmonea Leaf" },
                    {"8087", "Rabbit Tail" },
                    {"807C", "Ring of the Toad" },
                    {"808A", "Rusted Scrap of Armor" },
                    {"807D", "Silent Urn" },
                    {"8092", "Stolen Articles" },
                    {"809A", "Ring of the Light" },
                    {"808E", "Serpentwyne Must" },
                    {"809B", "Viera Rucksack" },
                    {"80B8", "Shelled Trophy" },
                    {"80B9", "Fur-scaled Trophy" },
                    {"80BA", "Bony Trophy" },
                    {"80BB", "Fanged Trophy" },
                    {"80BC", "Hide-covered Trophy" },
                    {"80BD", "Maned Trophy" },
                    {"80BE", "Fell Trophy" },
                    {"80BF", "Accursed Trophy" },
                    {"80C0", "Beaked Trophy" },
                    {"80C1", "Maverick Trophy" },
                    {"80C2", "Soulless Trophy" },
                    {"80C3", "Leathern Trophy" },
                    {"80C4", "Sickle Trophy" },
                    {"80C5", "Vengeful Trophy" },
                    {"80C6", "Gravesoil Trophy" },
                    {"80C7", "Metallic Trophy" },
                    {"80C8", "Slimy Trophy" },
                    {"80C9", "Scythe Trophy" },
                    {"80CA", "Feathered Trophy" },
                    {"80CB", "Skull Trophy" },
                    {"80CC", "Mind Trophy" },
                    {"80CD", "Eternal Trophy" },
                    {"80CE", "Clawed Trophy" },
                    {"80CF", "Odiferous Trophy" },
                    {"80D0", "Whiskered Trophy" },
                    {"80D1", "Frigid Trophy" },
                    {"80D2", "Ensanguined Trophy" },
                    {"80D3", "Cruel Trophy" },
                    {"80D4", "Adamantine Trophy" },
                    {"80D5", "Reptilian Trophy" },
                    {"80D6", "Vile Trophy" },
                    {"C012", "Belias Esper" },
                    {"C013", "Mateus Esper" },
                    {"C014", "Adrammelech Esper" },
                    {"C015", "Hashmal Esper" },
                    {"C016", "Cuchulainn Esper" },
                    {"C017", "Famfrit Esper" },
                    {"C018", "Zalera Esper" },
                    {"C019", "Shemhazai Esper" },
                    {"C01A", "Chaos Esper" },
                    {"C01B", "Zeromus Esper" },
                    {"C01C", "Exodus Esper" },
                    {"C01D", "Ultima Esper" },
                    {"C01E", "Zodiark Esper" },
                    {"C01F", "Second Board" }
                }
            }.Register(Treasures);

            KeyChops = new NumberFlagProperty()
            {
                Text = "Include Pinewood Chops",
                ID = "KeyChops",
                Description = "The number of pinewood chops to include in the pool.\n",
                ValueText = "Pinewood Chops:",
                MinValue = 0,
                MaxValue = 28,
                StepSize = 1
            }.Register(Treasures);

            KeyBlackOrbs = new NumberFlagProperty()
            {
                Text = "Include Black Orbs",
                ID = "KeyBlackOrbs",
                Description = "The number of black orbs to include in the pool.\n" +
                "For reference, up to 3 are found on the first floor, up to 8 can be found in Penumbra, up to 15 in Umbra, and up to 24 in Abyssal.",
                ValueText = "Black Orbs:",
                MinValue = 0,
                MaxValue = 24,
                StepSize = 1
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
                Description = "Key items placed in later locations will require a certain amount of characters unlocked and second board will be required for later locations."
            }.Register(Treasures);

            JunkRankScale = new ToggleFlagProperty()
            {
                Text = "Junk Scaled Depth",
                ID = "PlaceJunkScale",
                Description = "Consumables, equipment, and abilities will generally be placed in order of their sphere unlocks. Better items will appear later."
            }.Register(Treasures);

            ReplaceRank = new NumberFlagProperty()
            {
                Text = "Junk Item Rank Range",
                ID = "JunkRange",
                Description = "'Junk' items (consumables, equipment, and loot) will be replaced by items within the specified value of its \"Rank\". Junk will not be replaced by high end items like Seitengrat or Dark Energy.",
                ValueText = "Item Rank +/-",
                MinValue = 0,
                MaxValue = 8
            }.Register(Treasures);

            ReplaceAny = new ToggleFlagProperty()
            {
                Text = "Replace Junk Items From Any Category",
                ID = "ReplaceJunkAny",
                Description = "Allow 'Junk' items (consumables, equipment, and loot) to be replaced by items of other types.\n" +
                "Ex: Potions can be replaced with Broadsword."
            }.Register(Treasures);

            Shops = new Flag()
            {
                Text = "Randomize Shops",
                FlagID = "Shops",
                DescriptionFormat = "Randomize contents of shops. These items can appear in the item placement pool if that flag is on."
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

            JunkRankScaleShops = new ToggleFlagProperty()
            {
                Text = "Junk Scaled Depth",
                ID = "ShopJunkScale",
                Description = "Consumables, equipment, and abilities will generally be placed in shops in order of their sphere unlocks. Better items will appear later. Unique shops (Clan shop, Nabudis, and Leviathan shops) will be unaffected."
            }.Register(Shops);

            Bazaars = new Flag()
            {
                Text = "Randomize Bazaars",
                FlagID = "Bazaars",
                DescriptionFormat = "Randomize contents of bazaars. They may contain duplicates of items, abilities, and loot found elsewhere. Any monographs not placed in item locations will still show up in bazaars."
            }.Register(FlagType.Items);

            AllowSeitengrat = new Flag()
            {
                Text = "Allow Seitengrat",
                FlagID = "Seitengrat",
                DescriptionFormat = "Allow the Seitengrat to appear in the item pool and bazaar."
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
                "    Vague Type - Hints give the type ('Unique Key Item'/'Chop'/'Black Orb'/'Writ of Transit'/'Useless Trophy'/'Ability'/'Other') in the exact location.\n" +
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

