using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
            public static Flag RandCrystAbi, RandCrystStat, ShuffleCrystRole, RandInitStats;
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
            public static Flag Treasures, ShuffleRoles, ShuffleShops, StartingEquip;
            public static ToggleFlagProperty KeyRoles, KeyInitRoles, KeyStages, KeyEidolith, KeyShops, KeyReins, KeyPlaceTreasure, KeyPlaceMissions;
            public static ComboBoxFlagProperty KeyDepth;

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
                    "    Hard - Each level of depth/difficulty increases likelyhood of that location by 1.10x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelyhood of that location by 1.20x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelyhood of that location by 1.50x.\n" +
                    "    Hard+++ - Locations of the highest depth/difficulty will tend to be preferred.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
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
            }
        }
        public class Other
        {
            public static Flag Music, Enemies;
            public static NumberFlagProperty EnemyRank;
            public static ToggleFlagProperty EnemyNoLimit;

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

                EnemyNoLimit = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Increase Enemy Variety",
                    ID = "EnemyVariety",
                    Description = "[EXPERIMENTAL] Enemy variety will be increased in areas that allow it. This may cause crashes in areas less tested so use at your own risk. If the crashes become an issue, turn this off and regenerate the seed."
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

