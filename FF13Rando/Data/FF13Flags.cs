using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FF13Rando
{
    public class FF13Flags
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
            public static Flag RandCrystAbi, RandCrystStat, ShuffleCrystRole, ScaledCPCosts, RandInitStats;
            public static ToggleFlagProperty RandCrystAbiAll, ShuffleCrystMisc;

            internal static void Init()
            {
                RandCrystAbi = new Flag()
                {
                    Text = "Randomize Crystarium Abilities",
                    FlagID = "RandCrystAbi",
                    DescriptionFormat = "Randomizes the crystarium abilities in each role."
                }.Register(FlagType.Stats);

                RandCrystAbiAll = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Between All Roles",
                    ID = "AbiAll",
                    Description = "Crystarium abilities can appear for a different role."
                }.Register(RandCrystAbi);

                RandCrystStat = new Flag()
                {
                    Text = "Randomize Crystarium Stats",
                    FlagID = "RandCrystStat",
                    DescriptionFormat = "Randomizes the crystarium HP, Strength, and Magic."
                }.Register(FlagType.Stats);

                ShuffleCrystMisc = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Shuffle Misc Crystarium Nodes",
                    ID = "ShuffleCrystMisc",
                    Description = "Randomizes the crystarium Accessory and ATB Level nodes. These nodes can replace stat nodes."
                }.Register(RandCrystStat);

                ShuffleCrystRole = new Flag()
                {
                    Text = "Shuffle Crystarium Nodes Per Role",
                    FlagID = "ShuffleCrystRole",
                    DescriptionFormat = "Randomizes the order of the crystarium nodes in each role. Abilities may be out of order in terms of pre-required abilities."
                }.Register(FlagType.Stats);

                ScaledCPCosts = new Flag()
                {
                    Text = "Scaled CP Costs",
                    FlagID = "ScaledCPCost",
                    DescriptionFormat = "CP Costs are reduced based on the stage from Stage 1 with 1x multiplier to Stage 9 and 10 0.5x multiplier."
                }.Register(FlagType.Stats);

                RandInitStats = new Flag()
                {
                    Text = "Randomize Initial Stats",
                    FlagID = "RandInitStats",
                    DescriptionFormat = "Randomizes the initial HP, Strength, and Magic."
                }.Register(FlagType.Stats);
            }
        }
        public class Items
        {
            public static Flag Treasures, ShuffleRoles, ShuffleShops, StartingEquip, ShopContents;
            public static ToggleFlagProperty KeyRoles, KeyInitRoles, KeyStages, KeyEidolith, KeyShops, KeyReins, KeyPlaceTreasure, KeyPlaceMissions, ReplaceAny, AnyShop, ShopContentOrder;
            public static ComboBoxFlagProperty KeyDepth;
            public static NumberFlagProperty DifficultyScaling, ReplaceRank;

            internal static void Init()
            {
                Treasures = new Flag()
                {
                    Text = "Randomize Item Locations",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize treasures, mission rewards, and non-important story rewards and drops.\n" +
                    "Any key items in the pool will by default be shuffled between themselves.\n" +
                    "Does not include roles, crystarium expansions, shops, or Gysahl Reins."
                }.Register(FlagType.Items);

                KeyInitRoles = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Starting Roles",
                    ID = "KeyInitRoles",
                    Description = "Starting roles given at the start of the game will be included in the pool of key items."
                }.Register(Treasures);

                KeyRoles = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Other Roles",
                    ID = "KeyRoles",
                    Description = "Other roles given throughout the game will be included in the pool of key items."
                }.Register(Treasures);

                KeyStages = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Crystarium Expansions",
                    ID = "KeyStages",
                    Description = "The 9 crystarium expansions will be included in the pool of key items."
                }.Register(Treasures);

                KeyEidolith = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Eidoliths",
                    ID = "KeyEidolith",
                    Description = "The eidoliths will be included in the pool of key items."
                }.Register(Treasures);

                KeyShops = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Shops",
                    ID = "KeyShops",
                    Description = "The shops, shop upgrades and the Omni-kit will be included in the pool of key items."
                }.Register(Treasures);

                KeyReins = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Gysahl Reins",
                    ID = "KeyReins",
                    Description = "The Gysahl Reins will be included in the pool of key items."
                }.Register(Treasures);

                KeyPlaceTreasure = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Treasures",
                    ID = "KeyPlaceTreas",
                    Description = "Key items are also allowed in treasures and story rewards/drops."
                }.Register(Treasures);

                KeyPlaceMissions = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Key Item Placement - Missions",
                    ID = "KeyPlaceMissions",
                    Description = "Key items are also allowed in first time mission rewards."
                }.Register(Treasures);

                KeyDepth = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Item Difficulty Depth",
                    ID = "KeyDepth",
                    Description = "Key items will be more likely to appear in more difficult locations.\n\n" +
                    "Depths:\n" +
                    "    Normal - Each location is equally likely.\n" +
                    "    Hard - Each level of depth/difficulty increases likelihood of that location by 1.10x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelihood of that location by 1.20x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelihood of that location by 1.50x.\n" +
                    "    Hard+++ - Each level of depth/difficulty increases likelihood of that location by 2.00x.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
                }.Register(Treasures);

                DifficultyScaling = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "Difficulty Handicap",
                    ID = "DiffScale",
                    Description = "Item locations will be given a handicap to their difficulty to decrease the number of required roles and crystarium expansions used in logic. The higher the handicap, the less important key items will be forced to appear in easier locations.",
                    ValueText = "+",
                    MinValue = 0,
                    MaxValue = 5
                }.Register(Treasures);

                ReplaceRank = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "Junk Item Rank Range",
                    ID = "JunkRange",
                    Description = "'Junk' items (consumables, weapons, accessories, and components) will be replaced by items within the specified value of its \"Rank\".",
                    ValueText = "Item Rank +/-",
                    MinValue = 0,
                    MaxValue = 15
                }.Register(Treasures);

                ReplaceAny = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Replace Junk Items From Any Category",
                    ID = "ReplaceJunkAny",
                    Description = "Allow 'Junk' items (consumables, weapons, accessories, and components) to be replaced by items of other types.\n" +
                    "Ex: Potions can be replaced with Turbojets."
                }.Register(Treasures);

                ShuffleRoles = new Flag()
                {
                    Text = "Shuffle Roles",
                    FlagID = "ShuffleRoles",
                    DescriptionFormat = "Shuffle roles for each character. Each character will start with COM or RAV. Sazh always starts with RAV."
                }.Register(FlagType.Items);

                ShuffleShops = new Flag()
                {
                    Text = "Shuffle Shops",
                    FlagID = "ShuffleShops",
                    DescriptionFormat = "Shuffle shops and shop upgrades between themselves."
                }.Register(FlagType.Items);

                StartingEquip = new Flag()
                {
                    Text = "Randomize Starting Equipment",
                    FlagID = "StartingEquip",
                    DescriptionFormat = "Randomizes starting equipment with other weapons found in treasures or rewards."
                }.Register(FlagType.Items);

                ShopContents = new Flag()
                {
                    Text = "Randomize Shop Contents",
                    FlagID = "ShopContents",
                    DescriptionFormat = "Randomizes shop contents within their expected shops. Items required for the Treasure Hunter achievement will be added to shops as well."
                }.Register(FlagType.Items);

                AnyShop = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Items Appear in Any Shop",
                    ID = "AnyShop",
                    Description = "Items can appear in other shops aside from their vanilla shops."
                }.Register(ShopContents);

                ShopContentOrder = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Late Shop Expansions Contain Better Items",
                    ID = "ShopOrder",
                    Description = "Items that are higher 'Rank' will appear in later shop expansions."
                }.Register(ShopContents);
            }
        }
        public class Other
        {
            public static Flag Music, Enemies;
            public static NumberFlagProperty EnemyRank;
            public static ComboBoxFlagProperty EnemyVariety;
            public static ToggleFlagProperty GroupShuffle;

            internal static void Init()
            {
                Music = new Flag()
                {
                    Text = "Shuffle Music",
                    FlagID = "Music",
                    DescriptionFormat = "Shuffle music around.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                Enemies = new Flag()
                {
                    Text = "Randomize Enemies",
                    FlagID = "Enemies",
                    DescriptionFormat = "Randomizes enemies."
                }.Register(FlagType.Other);

                EnemyVariety = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Enemy Variety",
                    ID = "EnemyVariety",
                    Description = "Enemies Stay to the Same Area - Each encounter will only contain enemies from the area.\n" +
                    "Allow Other Enemies - Low - Allows enemies from other areas but limits variety to avoid crashes. Most encounters will not contain new enemies.\n" +
                    "Allow Other Enemies - Medium - Allows enemies from other areas but limits variety in some areas. This may cause crashes in areas less tested so use at your own risk.\n" +
                    "Allow Other Enemies - Max - Allows enemies from other areas but limits variety as least as possible to avoid known crashes. This may cause crashes in areas less tested so use at your own risk.",
                    Values = new string[] { "Enemies Stay to the Same Area", "Allow Other Enemies - Low", "Allow Other Enemies - Medium [EXPERIMENTAL]", "Allow Other Enemies - Max [EXPERIMENTAL]" }.ToList()
                }.Register(Enemies);

                EnemyRank = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "Enemy Rank Range",
                    ID = "EnemyRank",
                    Description = "Enemies can be replaced by enemies by enemies within the specified value of its \"Rank\".",
                    ValueText = "Enemy Rank +/-",
                    MinValue = 0,
                    MaxValue = 15
                }.Register(Enemies);

                GroupShuffle = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Shuffle enemies group-wise",
                    ID = "EnemyGroupShuffle",
                    Description = "Enemies are shuffled using charaspecs as a base rather than per encounter. More likely for individual fights to be randomised away from vanilla but some fights are more likely to have only vanilla enemies from the area",
                }.Register(Enemies);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            Stats.Init();
            Items.Init();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

