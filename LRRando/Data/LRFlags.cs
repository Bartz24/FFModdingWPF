using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LRRando
{
    public class LRFlags
    {
        public enum FlagType
        {
            All = -1,
            StatsAbilities,
            Enemies,
            Items,
            Other
        }
        public class StatsAbilities
        {
            public static Flag EPAbilities, NerfOC, EPCosts;
            public static ToggleFlagProperty EPAbilitiesEscape, EPCostsZero;
            public static NumberFlagProperty EPCostsRange;
            public static Flag EquipStats, GarbAbilities, EquipPassives;
            public static Flag Quests;

            internal static void Init()
            {
                EquipStats = new Flag()
                {
                    Text = "Randomize Equipment Stats",
                    FlagID = "RandEqStat",
                    DescriptionFormat = "Randomize Garb, Weapon, and Shield stats."
                }.Register(FlagType.StatsAbilities);

                GarbAbilities = new Flag()
                {
                    Text = "Randomize Garb Abilities",
                    FlagID = "RandGarbAbi",
                    DescriptionFormat = "Randomize abilities locked to garbs."
                }.Register(FlagType.StatsAbilities);

                EquipPassives = new Flag()
                {
                    Text = "Randomize Equipment Passive Abilities",
                    FlagID = "RandPassive",
                    DescriptionFormat = "Randomize passive abilities on garbs, garb abilities, weapons, shields, and accessories."
                }.Register(FlagType.StatsAbilities);

                EPAbilities = new Flag()
                {
                    Text = "Shuffle EP Abilities",
                    FlagID = "EPAbi",
                    DescriptionFormat = "Shuffles all EP abilities between each other except for Overclock and Escape (Escape requires the below flag)."
                }.Register(FlagType.StatsAbilities);

                EPAbilitiesEscape = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Escape",
                    ID = "EPAbiEsc",
                    Description = "Escape will be included in randomization."
                }.Register(EPAbilities);

                Quests = new Flag()
                {
                    Text = "Randomize Quest Stat Rewards",
                    FlagID = "Quests",
                    DescriptionFormat = "Randomize stats rewarded by quests. Includes Strength, Magic, Max HP, Max EP, Max ATB, and Recovery Item Slots."
                }.Register(FlagType.StatsAbilities);

                EPCosts = new Flag()
                {
                    Text = "Randomize EP Costs",
                    FlagID = "RandEPCost",
                    DescriptionFormat = "Randomize the EP costs of EP Abilities.\n" +
                    "Min of 0 on Normal for Escape.\n" +
                    "Min of 1 on Normal for Esunada, Decoy, Army of One, Chronostasis.\n" +
                    "Min of 2 on Normal for Curaga, Arise, Quake, Overclock, Teleport.\n" +
                    "Max of 5 on Normal for all."
                }.Register(FlagType.StatsAbilities);

                EPCostsZero = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Lower Min EP Cost",
                    ID = "EPZero",
                    Description = "Lowers the minimum EP costs to the following on Normal\n." +
                    "Allows for EP costs of 0 on Easy or Normal for some abilities aside from Escape:\n" +
                    "Min of 0 on Normal for Esunada, Decoy, Army of One, Chronostasis.\n" +
                    "Min of 1 on Normal for Curaga, Arise, Quake, Overclock, Teleport.\n"
                }.Register(EPCosts);

                EPCostsRange = (NumberFlagProperty)new NumberFlagProperty()
                {
                    Text = "EP Cost Range",
                    ID = "EPRange",
                    Description = "EP Costs can go up or down from their vanilla value by the specified value.",
                    ValueText = "EP Cost +/-",
                    MinValue = 1,
                    MaxValue = 5
                }.Register(EPCosts);

                NerfOC = new Flag()
                {
                    Text = "Increase Overclock EP Cost",
                    FlagID = "NerfOC",
                    DescriptionFormat = "Increases the EP cost of Overclock to 5 EP (or 4 EP on Easy). Takes effect before randomization."
                }.Register(FlagType.StatsAbilities);
            }
        }
        public class Enemies
        {
            public static Flag EnemyLocations, BhuniPlus, MatDrops, AbiDrops;
            public static ToggleFlagProperty EnemiesSize, EncounterSize, Bosses, Zaltys, Ereshkigal, Prologue;

            internal static void Init()
            {
                EnemyLocations = new Flag()
                {
                    Text = "Randomize Enemy Locations",
                    FlagID = "RandEne",
                    DescriptionFormat = "Randomize normal enemies between each other."
                }.Register(FlagType.Enemies);

                EnemiesSize = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Between Any Size",
                    ID = "RandEneSize",
                    Description = "If turned on, enemies of any size can replace another.\n" +
                    "If turned off, enemies will be randomized with enemies of the same size. Humans are considered mid."
                }.Register(EnemyLocations);

                EncounterSize = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Randomize Encounter Size",
                    ID = "RandEncCount",
                    Description = "If turned on, encounters with randomized enemies will be random in size up to +/- 2. A random enemy size will be selected from those already in the encounter.\n" +
                    "If turned off, encounters will remain the same size."
                }.Register(EnemyLocations);

                Prologue = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Prologue Tutorial",
                    ID = "RandProlo",
                    Description = "Prologue tutorial enemies will be included and randomized.\n" +
                    "Enemies replacing prologue enemies are limited to selection of smaller enemies."
                }.Register(EnemyLocations);

                Bosses = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Shuffle Bosses",
                    ID = "RandBoss",
                    Description = "Shuffle the following bosses between each other:\n" +
                    "Noel Kreiss, Snow Villiers, Caius Ballad, and Grendel.\n" +
                    "Bosses that have + versions will be based on their new random boss if the old boss has + versions.\n"
                }.Register(EnemyLocations);

                Zaltys = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Prologue Zaltys",
                    ID = "RandZaltys",
                    Description = "Includes Prologue Zaltys in the pool. This boss scales up if randomized to a later boss.\n" +
                    "Later bosses will scale down if replacing Prologue Zaltys."
                }.Register(EnemyLocations);

                Ereshkigal = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Ereshkigal",
                    ID = "RandEresh",
                    Description = "Includes Ereshkigal in the pool. This boss scales down if randomized to a story boss.\n" +
                    "Story bosses will scale up if replacing Ereshkigal."
                }.Register(EnemyLocations);

                MatDrops = new Flag()
                {
                    Text = "Randomize Enemy Material Drops",
                    FlagID = "RandMatDrops",
                    DescriptionFormat = "Enemy material drops will be spread evenly between enemies."
                }.Register(FlagType.Enemies);

                AbiDrops = new Flag()
                {
                    Text = "Randomize Enemy Ability Drops",
                    FlagID = "RandAbiDrops",
                    DescriptionFormat = "Ability drops will be spread evenly between enemies."
                }.Register(FlagType.Enemies);

                BhuniPlus = new Flag()
                {
                    Text = "Force Bhunivelze+ on NG",
                    FlagID = "BhuniPlus",
                    DescriptionFormat = "The final boss even on New Game will be Bhunivelze+."
                }.Register(FlagType.Enemies);
            }
        }
        public class Items
        {
            public static Flag Treasures;
            public static ToggleFlagProperty Pilgrims, EPLearns;
            public static ComboBoxFlagProperty Key, KeyDepth;
            public static Flag Shops;

            internal static void Init()
            {

                Treasures = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize Treasures, Quest Rewards, Non-repeatable Pickups, Soul seed rewards, Non-repeatable Item Appraisal rewards\n" +
                    "Does not include key items"
                }.Register(FlagType.Items);

                Pilgrims = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Pilgrim's Cruxes",
                    ID = "Pilgrims",
                    Description = "Pilgrim's Cruxes will be included in the pool with treasures, quests, etc.\n" +
                    "Pilgrim's Cruxes will not appear in missable locations or from Day 10 and later."
                }.Register(Treasures);

                EPLearns = (ToggleFlagProperty)new ToggleFlagProperty()
                {
                    Text = "Include Learned EP Abilities",
                    ID = "LearnEP",
                    Description = "EP Abilities learned at the start of the game will be included in the pool with treasures, quests, etc.\n" +
                    "This includes when Curaga, Escape, Chronostasis, and Teleport are normally learned."
                }.Register(Treasures);

                Key = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Randomize Key Items",
                    ID = "Key",
                    Description = "Key items will not appear in missable locations or from Day 10 and later.\n" +
                    "The following key items will be included in the pool based on the set level:\n" +
                    "Fragment of Mischief, Fragment of Smiles, Fragment of Courage, Moogle Fragment, ID Card, Midnight Mauve, Dead Dunes Tablets, Dr. Ghysahl's Ghysahl Greens, Proof of Courage, Violet Amulet, Lapis Lazuli, Power Booster, Moogle Dust, Photo Frame, Etro's Forbidden Tome, Broken Gyroscope, Golden Scarab, Key to the Sand Gate, Key to the Green Gate, Bandit's Bloodseal, Oath of the Merchants Guild, Jade Hair Comb, Bronze Pocket Watch, Nostalgic Scores, Rubber Ball, Thunderclap Cap, Quill Pen, Loupe, Musical Sphere Treasure Key, Supply Sphere Password, Chocobo Girl's Phone No.\n\n" +
                    "Levels:\n" +
                    "    None - Key items are not randomized.\n" +
                    "    Key Items Only - Key items are shuffled between themselves.\n" +
                    "    Treasures - Key items are also allowed in treasures/Learned EP ability spots.\n" +
                    "    Quests - Key items are also allowed in side quests and Non-Global Canvas of Prayers.\n" +
                    "    CoP - Key items are also allowed in all Canvas of Prayers.\n" +
                    "    Grindy - Key items are also allowed in 40+ Soul Seed rewards and 10+ Unappraised Items.\n",
                    Values = new string[] { "None", "Key Items Only", "Treasures", "Quests", "CoP", "Grindy" }.ToList()
                }.Register(Treasures);

                KeyDepth = (ComboBoxFlagProperty)new ComboBoxFlagProperty()
                {
                    Text = "Key Item Difficulty Depth",
                    ID = "KeyDepth",
                    Description = "Key items will be more likely to appear in longer chains of key items and more difficult/time-consuming locations.\n\n" +
                    "Depths:\n" +
                    "    Normal - Each location is equally likely.\n" +
                    "    Hard - Each level of depth/difficulty increases likelyhood of that location by 1.5x.\n" +
                    "    Hard+ - Each level of depth/difficulty increases likelyhood of that location by 2x.\n" +
                    "    Hard++ - Each level of depth/difficulty increases likelyhood of that location by 3x.\n" +
                    "    Hard+++ - Locations of the highest depth/difficulty will tend to be preferred.",
                    Values = new string[] { "Normal", "Hard", "Hard+", "Hard++", "Hard+++" }.ToList()
                }.Register(Treasures);

                Shops = new Flag()
                {
                    Text = "Randomize Shops",
                    FlagID = "Shops",
                    DescriptionFormat = "Randomize Shop contents.\n" +
                    "Hard mode items are included."
                }.Register(FlagType.Items);
            }
        }
        public class Other
        {
            public static Flag Music;
            public static Flag HintsMain, HintsNotes, HintsEP;

            internal static void Init()
            {
                Music = new Flag()
                {
                    Text = "Shuffle Music",
                    FlagID = "Music",
                    DescriptionFormat = "Shuffle music around.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                HintsMain = new Flag()
                {
                    Text = "Hints by Specific Item",
                    FlagID = "HintsMain",
                    DescriptionFormat = "Each part of a main quest completed could reveal exact locations of some randomized important key items in the Quests menu.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                HintsNotes = new Flag()
                {
                    Text = "Hints by Area",
                    FlagID = "HintsArea",
                    DescriptionFormat = "Each randomized Enemy Libra Notes item will hint towards the number of randomized important key items by area.",
                    Aesthetic = true
                }.Register(FlagType.Other);

                HintsEP = new Flag()
                {
                    Text = "Hint EP Abilities",
                    FlagID = "HintsEP",
                    DescriptionFormat = "Randomized EP abilities will be included in hints.",
                    Aesthetic = true
                }.Register(FlagType.Other);
            }
        }

        public static void Init()
        {
            Flags.FlagsList.Clear();
            StatsAbilities.Init();
            Enemies.Init();
            Items.Init();
            Other.Init();
            Flags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
            Flags.SelectedCategory = "All";
        }
    }
}

