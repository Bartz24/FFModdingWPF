using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LRRando;

public class LRFlags
{
    public enum FlagType
    {
        Debug = RandoFlags.FlagTypeDebug,
        All = RandoFlags.FlagTypeAll,
        Archipelago = RandoFlags.FlagTypeArchipelago,
        StatsAbilities,
        Enemies,
        Items,
        Other
    }
    public class StatsAbilities
    {
        public static Flag EPAbilities, NerfOC, EPCosts;
        public static ToggleFlagProperty EPCostsZero;
        public static DictListBoxFlagProperty<string> EPAbilitiesPool;
        public static NumberFlagProperty EPCostsRange, EPCostMax;
        public static Flag EquipStats, GarbAbilities, EquipPassives, AbilityPassives;
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
                DescriptionFormat = "Shuffles all selected EP abilities between each other."
            }.Register(FlagType.StatsAbilities);

            EPAbilitiesPool = new DictListBoxFlagProperty<string>()
            {
                Text = "",
                ID = "EPAbiPool",
                Description = "",
                DictValues = new BiDictionary<string, string>
                {
                    { "ti000_00", "Curaga" },
                    { "ti020_00", "Arise" },
                    { "ti030_00", "Esunada" },
                    { "ti500_00", "Quake" },
                    { "ti600_00", "Decoy" },
                    { "ti810_00", "Teleport" },
                    { "ti830_00", "Escape" },
                    { "ti840_00", "Chronostasis" },
                    { "at900_00", "Army of One" }
                }
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
                DescriptionFormat = "Randomize the EP costs of EP Abilities except Escape.\n" +
                "Min of 1 on Normal for Esunada, Decoy, Army of One, Chronostasis.\n" +
                "Min of 2 on Normal for Curaga, Arise, Quake, Overclock, Teleport."
            }.Register(FlagType.StatsAbilities);

            EPCostsZero = new ToggleFlagProperty()
            {
                Text = "Lower Min EP Cost",
                ID = "EPZero",
                Description = "Lowers the minimum EP costs to the following on Normal\n." +
                "Allows for EP costs of 0 on Easy or Normal for some abilities:\n" +
                "Min of 0 on Normal for Esunada, Decoy, Army of One, Chronostasis.\n" +
                "Min of 1 on Normal for Curaga, Arise, Quake, Overclock, Teleport.\n"
            }.Register(EPCosts);

            EPCostMax = new NumberFlagProperty()
            {
                Text = "Max EP Cost",
                ID = "EPMax",
                Description = "Set the max EP Cost possible.",
                ValueText = "Max EP Cost: ",
                MinValue = 3,
                MaxValue = 9
            }.Register(EPCosts);

            EPCostsRange = new NumberFlagProperty()
            {
                Text = "EP Cost Range",
                ID = "EPRange",
                Description = "EP Costs can go up or down from their vanilla value by the specified value.",
                ValueText = "EP Cost +/-",
                MinValue = 1,
                MaxValue = 9
            }.Register(EPCosts);

            AbilityPassives = new Flag()
            {
                Text = "Randomize Ability Passives",
                FlagID = "AbiPass",
                DescriptionFormat = "Randomizes the passives on abilities. Includes synthesized passive abilities and rare passive abilities."
            }.Register(FlagType.StatsAbilities);

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
        public static ToggleFlagProperty EnemiesSize, EncounterSize, Prologue;
        public static DictListBoxFlagProperty<string> Bosses;

        internal static void Init()
        {
            EnemyLocations = new Flag()
            {
                Text = "Randomize Enemy Locations",
                FlagID = "RandEne",
                DescriptionFormat = "Randomize normal enemies between each other."
            }.Register(FlagType.Enemies);

            EnemiesSize = new ToggleFlagProperty()
            {
                Text = "Between Any Size",
                ID = "RandEneSize",
                Description = "If turned on, enemies of any size can replace another.\n" +
                "If turned off, enemies will be randomized with enemies of the same size. Humans are considered mid."
            }.Register(EnemyLocations);

            EncounterSize = new ToggleFlagProperty()
            {
                Text = "Randomize Encounter Size",
                ID = "RandEncCount",
                Description = "If turned on, encounters with randomized enemies will be random in size up to +/- 2. A random enemy size will be selected from those already in the encounter.\n" +
                "If turned off, encounters will remain the same size."
            }.Register(EnemyLocations);

            Prologue = new ToggleFlagProperty()
            {
                Text = "Include Prologue Tutorial",
                ID = "RandProlo",
                Description = "Prologue tutorial enemies will be included and randomized.\n" +
                "Enemies replacing prologue enemies are limited to selection of smaller enemies."
            }.Register(EnemyLocations);

            Bosses = new DictListBoxFlagProperty<string>()
            {
                Text = "Shuffle Bosses",
                ID = "RandBoss",
                Description = "Shuffle the selected bosses between each other.\n" +
                "Bosses will scale up or down depending on their placement.\n" +
                "Bosses that have + versions will be based on their new random boss if the old boss has + versions.",
                DictValues =
                {
                    {"Zaltys", "Prologue Zaltys" },
                    {"Noel", "Noel Kreiss" },
                    {"Snow", "Snow Villiers" },
                    {"Caius", "Caius Ballad" },
                    {"Grendel", "Grendel/Parandus" },
                    {"Ereshkigal", "Ereshkigal" },
                    {"Aeronite", "Aeronite" }
                }
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
        public static Flag Shops;
        public static Flag CoPReqs;
        public static ToggleFlagProperty EPMissable, IDCardBuy, KeyPlaceTreasure, KeyPlaceQuest, KeyPlaceCoP, KeyPlaceGrindy, KeyPlaceSuperboss, ReplaceAny, IncludeDLCItems, IncludeEPAbilities;
        public static ComboBoxFlagProperty KeyDepth;
        public static DictListBoxFlagProperty<string> KeyItems;
        public static NumberFlagProperty ReplaceRank;

        internal static void Init()
        {

            Treasures = new Flag()
            {
                Text = "Randomize Item Locations",
                FlagID = "Treasures",
                DescriptionFormat = "Randomize treasures, quest rewards, battle rewards, non-repeatable pickups, soul seed rewards, non-repeatable item appraisal rewards, and shuffled EP abilities.\n" +
                "Does not include key items unless they are selected."
            }.Register(FlagType.Items);

            KeyItems = new DictListBoxFlagProperty<string>()
            {
                Text = "Include Key Items",
                ID = "KeyItems",
                Description = "Key items to be included in the item pool.\n" +
                "Key items will not appear in missable locations or from Day 10 and later.",
                DictValues =
                {
                    {"key_y_ticket", "Sneaking-In Special Ticket" },
                    {"key_y_id", "ID Card" },
                    {"cos_fa00", "Midnight Mauve" },
                    {"key_y_serap", "Serah's Pendant" },
                    {"key_d_sekiban", "3 Tablets" },
                    {"key_d_base", "Crux Base" },
                    {"key_d_wing", "Crux Body" },
                    {"key_d_top", "Crux Tip" },
                    {"key_w_yasai_t", "Main Story Gysahl Greens" },
                    {"key_soulcd", "Seedhunter Membership Card" },
                    {"key_w_mogsoul", "Moogle Fragment" },
                    {"key_s_okuri", "Beloved's Gift" },
                    {"key_s_kairaku", "Fragment of Mischief" },
                    {"key_s_kanki", "Fragment of Courage" },
                    {"key_s_zyouai", "Fragment of Smiles" },
                    {"key_s_hiai", "Fragment of Radiance" },
                    {"key_s_hunnu", "Fragment of Kindness" },
                    {"key_ball", "Rubber Ball" },
                    {"key_kimochi", "Talbot's Gratitude" },
                    {"key_l_pen", "Quill Pen" },
                    {"key_kyu_pass", "Supply Sphere Password" },
                    {"key_kb_g", "Green Carbuncle Doll" },
                    {"key_kb_r", "Red Carbuncle Doll" },
                    {"key_l_hana", "Phantom Rose" },
                    {"key_j_kino", "Thunderclap Cap" },
                    {"key_niku", "Shaolong Gui Shell" },
                    {"key_ninjin", "Mandragora Root" },
                    {"key_sp_bt", "Spectral Elixir" },
                    {"key_behi_tume", "Cursed Dragon Claw" },
                    {"key_l_kagi", "Service Entrance Key" },
                    {"key_y_kagi1", "Musical Treasure Sphere Key" },
                    {"key_y_kagi2", "Nostalgic Score: Chorus" },
                    {"key_y_kagi3", "Nostalgic Score: Refrain" },
                    {"key_y_rappa", "Nostalgic Score: Coda" },
                    {"key_y_kaban", "Music Satchel" },
                    {"key_y_bashira", "Civet Musk" },
                    {"key_y_recipe", "Gordon Gourmet's Recipe" },
                    {"key_y_cream", "Steak a la Civet" },
                    {"key_y_letter", "Father's Letter" },
                    {"key_d_key", "Pilgrim's Cruxes" },
                    {"key_d_lupe", "Loupe" },
                    {"key_d_keisan", "Arithmometer" },
                    {"key_d_niku", "Monster Flesh" },
                    {"key_w_moji1", "Goddess Glyphs" },
                    {"key_w_moji2", "Chaos Glyphs" },
                    {"key_w_buhin1", "Plate Metal Fragment" },
                    {"key_w_buhin2", "Silvered Metal Fragment" },
                    {"key_w_buhin3", "Golden Metal Fragment" },
                    {"key_w_data", "Data Recorder" },
                    {"key_w_apple", "3 Aryas Apples" },
                    {"key_w_tamago", "Mystery Egg" },
                    {"key_p_toppa", "Proof of Overcoming Limits" },
                    {"key_l_kishin", "Proof of Legendary Title" },
                    {"key_b_00", "Proof of Courage" },
                    {"key_b_01", "Violet Amulet" },
                    {"key_b_20", "Chocobo Girl's Phone No." },
                    {"key_b_02", "Lapis Lazuli" },
                    {"key_b_03", "Power Booster" },
                    {"key_b_16", "Jade Hair Comb" },
                    {"key_b_17", "Bronze Pocket Watch" },
                    {"key_b_08", "Golden Scarab" },
                    {"key_b_04", "Moogle Dust" },
                    {"key_b_05", "Old-Fashioned Photo Frame" },
                    {"key_b_06", "Etro's Forbidden Tome" },
                    {"key_b_07", "Broken Gyroscope" },
                    {"key_b_09", "Key to the Sand Gate" },
                    {"key_b_10", "Key to the Green Gate" },
                    {"key_b_11", "Bandit's Bloodseal" },
                    {"key_b_12", "Oath of the Merchants Guild" },
                }
            }.Register(Treasures);

            IncludeEPAbilities = new ToggleFlagProperty()
            {
                Text = "Include EP Abilities",
                ID = "IncludeEP",
                Description = "Include selected EP abilities in the item pool. Non-selected EP abilities from the 'EP Abilities' option will stay in their normal location."
            }.Register(Treasures);

            EPMissable = new ToggleFlagProperty()
            {
                Text = "Allow EP Abilities in Missable Locations",
                ID = "EPMiss",
                Description = "EP Abilities will be allowed to appear in missable or late game locations Day 10 or later."
            }.Register(Treasures);

            KeyPlaceTreasure = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Treasures",
                ID = "KeyPlaceTreas",
                Description = "Key items are also allowed in treasures and other misc item locations."
            }.Register(Treasures);

            KeyPlaceQuest = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Quests",
                ID = "KeyPlaceQuest",
                Description = "Key items are also allowed in side quests and Global Canvas of Prayers."
            }.Register(Treasures);

            KeyPlaceCoP = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Canvas of Prayers",
                ID = "KeyPlaceCoP",
                Description = "Key items are also allowed in non-global Canvas of Prayers."
            }.Register(Treasures);

            KeyPlaceSuperboss = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Superbosses",
                ID = "KeyPlaceSuperboss",
                Description = "Key items are also allowed on Aeronite and Ereshkigal drops and rewards."
            }.Register(Treasures);

            KeyPlaceGrindy = new ToggleFlagProperty()
            {
                Text = "Key Item Placement - Grindy",
                ID = "KeyPlaceGrindy",
                Description = "Key items are also allowed in 20+ Soul Seed rewards and 10+ Unappraised Items."
            }.Register(Treasures);

            KeyDepth = new ComboBoxFlagProperty()
            {
                Text = "Item Difficulty Depth",
                ID = "KeyDepth",
                Description = "Key items and EP abilities will be more likely to appear in more difficult locations. CoP is less likely in general since there are so many.\n\n" +
                "Depths:\n" +
                "    Normal - Each location is equally likely.\n" +
                "    Hard - Each level of depth/difficulty increases likelihood of that location by 1.1x.\n" +
                "    Hard+ - Each level of depth/difficulty increases likelihood of that location by 1.20x.\n" +
                "    Hard++ - Each level of depth/difficulty increases likelihood of that location by 1.50x.\n" +
                "    Hard+++ - Each level of depth/difficulty increases likelihood of that location by 2.00x.",
                Values = ["Normal", "Hard", "Hard+", "Hard++", "Hard+++" ]
            }.Register(Treasures);

            IDCardBuy = new ToggleFlagProperty()
            {
                Text = "Randomize Buyable ID Card",
                ID = "BuyID",
                Description = "The bought item from the tour guide will be the randomized item assigned in the ID Card location."
            }.Register(Treasures);

            Shops = new Flag()
            {
                Text = "Randomize Shops",
                FlagID = "Shops",
                DescriptionFormat = "Randomize Shop contents.\n" +
                "Hard mode items are included."
            }.Register(FlagType.Items);

            CoPReqs = new Flag()
            {
                Text = "Reduce Requirements for Canvas of Prayers",
                FlagID = "CoPReqs",
                DescriptionFormat = "Reduce non-key item requirements for Canvas of Prayers by half (rounds up)."
            }.Register(FlagType.Items);

            ReplaceRank = new NumberFlagProperty()
            {
                Text = "Junk Item Rank Range",
                ID = "JunkRange",
                Description = "'Junk' items (consumables, weapons, shields, garbs, accessories, and materials) will be replaced by items within the specified value of its \"Rank\".",
                ValueText = "Item Rank +/-",
                MinValue = 0,
                MaxValue = 10
            }.Register(Treasures);

            ReplaceAny = new ToggleFlagProperty()
            {
                Text = "Replace Junk Items From Any Category",
                ID = "ReplaceJunkAny",
                Description = "Allow 'Junk' items (consumables, weapons, shields, garbs, accessories, and materials) to be replaced by items of other types.\n" +
                "Ex: Potions can be replaced with Bronze Malistones."
            }.Register(Treasures);

            IncludeDLCItems = new ToggleFlagProperty()
            {
                Text = "Include DLC Items",
                ID = "IncludeDLC",
                Description = "Include DLC equipment and adornments in the item pool.\n" +
                "Note: Be sure to also enable the DLC content in the in-game settings menu."
            }.Register(Treasures);
        }
    }
    public class Other
    {
        public static Flag Music, LoadingText;
        public static Flag HintsMain;
        public static ComboBoxFlagProperty HintsSpecific;
        public static ToggleFlagProperty HintsDepth;
        public static ComboBoxFlagProperty HintsBosses;
        public static ToggleFlagProperty FanfareMusic;

        internal static void Init()
        {
            Music = new Flag()
            {
                Text = "Shuffle Music",
                FlagID = "Music",
                DescriptionFormat = "Shuffle music around.",
                Aesthetic = true
            }.Register(FlagType.Other);

            FanfareMusic = new ToggleFlagProperty()
            {
                Text = "Include Additional Fanfares",
                ID = "Fanfare",
                Description = "Include the additional garb specific fanfares in the pool."
            }.Register(Music);

            LoadingText = new Flag()
            {
                Text = "Randomize Loading Screen Text",
                FlagID = "LoadingText",
                DescriptionFormat = "Randomize the lore on loading screens.",
                Aesthetic = true
            }.Register(FlagType.Other);

            HintsMain = new Flag()
            {
                Text = "Hints by Item",
                FlagID = "HintsMain",
                DescriptionFormat = "Each part of a main quest completed could reveal exact locations of some randomized important key items in the Quests menu.",
                Aesthetic = true
            }.Register(FlagType.Other);

            HintsSpecific = new ComboBoxFlagProperty()
            {
                Text = "Specificity",
                ID = "HintsSpecific",
                Description = "Set the specificity for the hints from main quests.\n\n" +
                "Options:\n" +
                "    Exact - Hints give the exact item in the exact location.\n" +
                "    Vague Type - Hints give the type ('Key Item'/'Pilgrim's Crux'/'EP Ability'/'Other') in the exact location.\n" +
                "    Vague Area - Hints give the exact item in the area.\n" +
                "    Unknown but Exact Location - Hints will hint that something ('?????') is in the exact location.\n" +
                "    Random - Each hint will use one of the above rules.",
                Values = ["Exact", "Vague Type", "Vague Area", "Unknown but Exact Location", "Random"]
            }.Register(HintsMain);

            HintsDepth = new ToggleFlagProperty()
            {
                Text = "Hints at Earlier Locations",
                ID = "HintsDepth",
                Description = "Hints for items will prioritize locations of lower than or equal to depths of the item itself."
            }.Register(HintsMain);

            HintsBosses = new ComboBoxFlagProperty()
            {
                Text = "Boss Hints",
                ID = "HintsBosses",
                Description = "Set whether bosses will be hinted in main quest hints.\n" +
                "Main Quest 0 corresponds to Aeronite, and Main Quest 5 correponds to Ereshkigal.\n\n" +
                "Options:\n" +
                "    None - Do not include boss data in main quest hints.\n" +
                "    Source Initial - The first main quest hint for an area will tell you what boss is at the end of that quest.\n" +
                "    Source Random - The source hint will be in a random hint in this quest).\n" +
                "    Target Initial - The first main quest hint for an area will tell you where the boss from that quest has been placed.\n" +
                "    Target Random - The target hint will be in a random hint in this quest.\n" +
                "    Random - Each hint will use one of the above rules (excluding None).",
                Values = ["None", "Source Initial", "Source Random", "Target Initial", "Target Random", "Random"]
            }.Register(HintsMain);
        }
    }

    public static void Init()
    {
        RandoFlags.FlagsList.Clear();
        StatsAbilities.Init();
        Enemies.Init();
        Items.Init();
        Other.Init();
        RandoFlags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
        RandoFlags.SelectedCategory = "All";
    }
}

