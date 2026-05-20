using Bartz24.RandoWPF;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF13_2Rando;

public class FF13_2Flags
{
    public enum FlagType
    {
        Debug = RandoFlags.FlagTypeDebug,
        All = RandoFlags.FlagTypeAll,
        Archipelago = RandoFlags.FlagTypeArchipelago,
        Stats,
        Items,
        Enemies,
        Other
    }
    public class Stats
    {
        public static Flag RandCrystAbi;
        public static Flag InitCP;
        public static NumberFlagProperty InitCPAmount, WeightRange, RunSpeedMultValue;
        public static Flag EquipStats, EquipPassives, EquipWeights, RunSpeedMult;

        internal static void Init()
        {
            RandCrystAbi = new Flag(false)
            {
                Text = "Randomize Crystarium Abilities",
                FlagID = "RandCrystAbi",
                DescriptionFormat = "Randomizes the crystarium abilities.",
                Aesthetic = true
            }.Register(FlagType.Stats);

            InitCP = new Flag(false)
            {
                Text = "Start with CP",
                FlagID = "InitCP",
                DescriptionFormat = "Start with a specified amount of CP set below.",
                Aesthetic = true
            }.Register(FlagType.Stats);

            InitCPAmount = new NumberFlagProperty(500)
            {
                Text = "",
                ID = "InitCPAmt",
                Description = "",
                ValueText = "CP:",
                MinValue = 500,
                MaxValue = 10000,
                StepSize = 500
            }.Register(InitCP);

            EquipStats = new Flag(false)
            {
                Text = "Randomize Weapon Stats",
                FlagID = "RandEqStat",
                DescriptionFormat = "Randomize weapon stats."
            }.Register(FlagType.Stats);

            EquipPassives = new Flag(false)
            {
                Text = "Randomize Equipment Passive Abilities",
                FlagID = "RandPassive",
                DescriptionFormat = "Randomize passive abilities on weapons and accessories."
            }.Register(FlagType.Stats);

            EquipWeights = new Flag(false)
            {
                Text = "Randomize Accessory Weights",
                FlagID = "RandAccW",
                DescriptionFormat = "Randomize accessory weights."
            }.Register(FlagType.Stats);

            WeightRange = new NumberFlagProperty(0)
            {
                Text = "",
                ID = "AccWRange",
                Description = "",
                ValueText = "+/-",
                MinValue = 0,
                MaxValue = 100,
                StepSize = 5
            }.Register(EquipWeights);

            RunSpeedMult = new Flag(false)
            {
                Text = "Run Speed Multiplier",
                FlagID = "RunSpeedMult",
                DescriptionFormat = "Increases the run speed all the main party members by the percentage specified.\n" +
                "Hope's run speed will match the others."
            }.Register(FlagType.Stats);

            RunSpeedMultValue = new NumberFlagProperty(100)
            {
                Text = "",
                ID = "RunSpeedVal",
                Description = "",
                ValueText = "(%): ",
                MinValue = 100,
                MaxValue = 150,
                StepSize = 5
            }.Register(RunSpeedMult);
        }
    }
    public class Items
    {
        public static Flag Treasures;
        public static ToggleFlagProperty KeyWild, KeyGraviton, KeyFragment, KeyGateSeal, KeySide, KeyParadox, KeyArtefact;
        public static ToggleFlagProperty KeyPlaceTreasure, KeyPlaceBrainBlast, KeyPlaceThrowCryst, KeyPlaceThrowJunk, KeyPlaceParadox, KeyPlaceAreaBias;
        public static ToggleFlagProperty ReplaceWildArtefacts;
        public static NumberFlagProperty InitialShopLevel;
        public static ComboBoxFlagProperty KeyDepth;

        internal static void Init()
        {
            Treasures = new Flag(false)
            {
                Text = "Randomize Item Locations",
                FlagID = "Treasures",
                DescriptionFormat = "Randomize treasure spheres and cubes, Improved Moogle Throw search items, and non-useful fragments.\n" +
                "Any key items in the pool will by default be shuffled between themselves.\n" +
                "Does not include normal artefacts and event based items and fragments.",
                HasArchipelagoOverride = true
            }.Register(FlagType.Items);

            KeyWild = new ToggleFlagProperty(false)
            {
                Text = "Include Wild Artefacts",
                ID = "KeyWild",
                Description = "Wild Artefacts will be included in the pool of key items."
            }.Register(Treasures);

            KeyGraviton = new ToggleFlagProperty(false)
            {
                Text = "Include Graviton Core Fragments",
                ID = "KeyGraviton",
                Description = "The 7 Graviton Core fragments will be included in the pool of key items."
            }.Register(Treasures);

            KeyFragment = new ToggleFlagProperty(false)
            {
                Text = "Include Additional Fragments",
                ID = "KeyFragment",
                Description = "The additional fragments will be included in the pool of key items.\n" +
                "Checks that require the presence of the vanilla fragments will still be available in the shuffled location (such as Fragment Skill unlock conditions)\n" +
                "Paradox Ending fragments are not included by this flag, see Include Paradox Scope and Endings below."
                // I'm not listing out all of the fragments in the pool there's like 30 of them already and its only going to go up.
                // In the future might want to sub-categorise
            }.Register(Treasures);

            KeyGateSeal = new ToggleFlagProperty(false)
            {
                Text = "Include Gate Seals",
                ID = "KeySeal",
                Description = "The gate seals will be included in the pool of key items."
            }.Register(Treasures);

            KeySide = new ToggleFlagProperty(false)
            {
                Text = "Include Side Key Items",
                ID = "KeySide",
                Description = "The following key items will be included in the key item pool:\n" +
                "Medical Kit, Capsules, Holding Cell Key, Comm Device, Emerald Crystal, Ivory Crystal, Onyx Crystal, Service Manual, Fruit of Fenrir, Tablet of Paddra, Old Battery, Sealed Tablet, Army Comm Device, Recording Device, Picture Frame, Bulb of Hope, Terrorists' Mark, Weapon Material, Outdoor Watch, Personal Notes, Paradox Agent Type A-C, Supply Sphere Access Code"
            }.Register(Treasures);

            KeyParadox = new ToggleFlagProperty(false)
            {
                Text = "Include Paradox Scope and Endings",
                ID = "KeyParadox",
                Description = "The Paradox Scope and all Paradox Ending Fragments (Transcript: {x}) will be included in the key item pool."
            }.Register(Treasures);

            KeyArtefact = new ToggleFlagProperty(false)
            {
                Text = "Include Additional Artefacts",
                ID = "KeyArtefact",
                Description = "The following artefacts will be included in the key item pool:\n" +
                "Vagabond Artefact, Tower Artefact, Artefact of Rebirth, Artefact of Origins, Hollow Artefact, Giant's Artefact"
            }.Register(Treasures);

            KeyPlaceTreasure = new ToggleFlagProperty(false)
            {
                Text = "Key Item Placement - Treasures",
                ID = "KeyPlaceTreas",
                Description = "Key items are also allowed in treasures and fragment/artefact spots."
            }.Register(Treasures);

            KeyPlaceBrainBlast = new ToggleFlagProperty(false)
            {
                Text = "Key Item Placement - Brain Blast",
                ID = "KeyPlaceBrain",
                Description = "Key items are also allowed in Brain Blast rewards."
            }.Register(Treasures);

            KeyPlaceThrowCryst = new ToggleFlagProperty(false)
            {
                Text = "Key Item Placement - Improved Moogle Throw Monster Crystals",
                ID = "KeyPlaceMogCryst",
                Description = "Key items are also allowed to replace Improve Moogle Throw monster crystal locations."
            }.Register(Treasures);

            KeyPlaceThrowJunk = new ToggleFlagProperty(false)
            {
                Text = "Key Item Placement - Improved Moogle Throw Junk",
                ID = "KeyPlaceMogJunk",
                Description = "Key items are also allowed to replace Improve Moogle Throw junk locations."
            }.Register(Treasures);

            KeyPlaceParadox = new ToggleFlagProperty(false)
            {
                Text = "Key Item Placement - Paradox Endings",
                ID = "KeyPlaceParadox",
                Description = "Key items are also allowed to be placed in Paradox Ending fragment locations"
            }.Register(Treasures);

            KeyDepth = new ComboBoxFlagProperty("Normal")
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

            KeyPlaceAreaBias = new ToggleFlagProperty(false)
            {
                Text = "Item Placement Accessibility Bias [EXPERIMENTAL]",
                ID = "KeyAccessibilityBias",
                Description = "Instructs the placement logic to bias unlocking items by area accessibility.\n\n" +
                "The intent of this change is to hopefully downgrade items with wide impacts but low immediate gain (such as mog levels) for more local progression."
            }.Register(Treasures);

            ReplaceWildArtefacts = new ToggleFlagProperty(false)
            {
                Text = "Replace wild artefacts with custom items [EXPERIMENTAL]",
                ID = "WildArtefactReplace",
                Description = "Replaces Wild Artefacts with custom items and updates gates accordingly to have a unique artefact always."
            }.Register(Treasures);

            InitialShopLevel = new NumberFlagProperty(0)
            {
                Text = "Number of initial shop levels",
                ID = "InitialShopLevels",
                Description = "The number of Progressive shop levels to start with.",
                MinValue = 0,
                MaxValue = 11
            }.Register(Treasures);
        }
    }
    public class Enemies
    {
        public static Flag EnemyLocations, EnemyCPMult;
        public static NumberFlagProperty EnemyRank, EnemyCPMultValue;
        public static ToggleFlagProperty LargeEnc, DLCBosses, BossScaling, DeprioritiseCaius, FullRandomShuffleBosses;
        public static ListBoxFlagProperty Bosses;

        internal static void Init()
        {
            EnemyLocations = new Flag(false)
            {
                Text = "Randomize Enemy Locations",
                FlagID = "RandEne",
                DescriptionFormat = "Randomize normal enemies between each other."
            }.Register(FlagType.Enemies);

            LargeEnc = new ToggleFlagProperty(false)
            {
                Text = "Allow Larger Encounters",
                ID = "LargeEnc",
                Description = "[EXPERIMENTAL]\n" +
                "Allows encounters to have 5 enemies or more. Not recommended as this seems to be cause of some crashes with random enemies.",
                Experimental = true
            }.Register(EnemyLocations);

            Bosses = new ListBoxFlagProperty([])
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

            EnemyRank = new NumberFlagProperty(0)
            {
                Text = "Enemy Rank Range",
                ID = "EnemyRank",
                Description = "Enemies can be replaced by enemies by enemies within the specified value of its \"Rank\".",
                ValueText = "Enemy Rank +/-",
                MinValue = 0,
                MaxValue = 15
            }.Register(EnemyLocations);

            BossScaling = new ToggleFlagProperty(false)
            {
                Text = "Scale Boss Stats [EXPERIMENTAL]",
                ID = "BossScale",
                Description = "Scales up/down boss stats to be based on the average of the enemy ranks in the area they are placed in.",
                Experimental = true
            }.Register(EnemyLocations);

            DeprioritiseCaius = new ToggleFlagProperty(false)
            {
                Text = "Deprioritise Caius Boss Shuffle [EXPERIMENTAL]",
                ID = "CaiusBossShuffle",
                Description = "When enabled, pushes caius fights later in the shuffle to make them less likely to appear in the main story fights.",
                Experimental = true
            }.Register(EnemyLocations);

            FullRandomShuffleBosses = new ToggleFlagProperty(false)
            {
                Text = "Full random shuffle bosses [EXPERIMENTAL]",
                ID = "FullRandomBosses",
                Description = "Remove some guardrails when shuffling bosses. Silly situations may occur.",
                Experimental = true
            }.Register(EnemyLocations);

            EnemyCPMult = new Flag(false)
            {
                Text = "Enemy CP Multiplier",
                FlagID = "EnemyCPMult",
                DescriptionFormat = "Multiply enemy CP by the specified percentage."
            }.Register(FlagType.Enemies);

            EnemyCPMultValue = new NumberFlagProperty(100)
            {
                Text = "",
                ID = "EnemyCPMultVal",
                Description = "",
                ValueText = "(%): ",
                MinValue = 100,
                MaxValue = 1000,
                StepSize = 5
            }.Register(EnemyCPMult);
        }
    }
    public class Other
    {
        public static Flag HistoriaCrux;
        public static Flag Music;
        public static ComboBoxFlagProperty ForcedStart;
        public static ToggleFlagProperty RandoDLC;
        public static ToggleFlagProperty ForceAcadVoidEndgame;

        internal static void Init()
        {
            HistoriaCrux = new Flag(false)
            {
                Text = "Randomize Historia Crux",
                FlagID = "HistCrux",
                DescriptionFormat = "Randomizes the Historia Crux map.",
                Aesthetic = true,
                HasArchipelagoOverride = true
            }.Register(FlagType.Other);

            ForcedStart = new ComboBoxFlagProperty("Bodhum")
            {
                Text = "Forced Start",
                ID = "ForcedStart",
                Description = "Options:\n" +
                "None - Any valid starting area is possible. Highly unstable. Will likely crash generation.\n" +
                "Bodhum - Force starting in New Bodhum 3 AF.\n" +
                "Bodhum & Bresha - Force starting in New Bodhum 3 AF leading to Bresha Ruins 5 AF for guaranteed early branching options.",
                Values = new string[] { "None", "Bodhum", "Bodhum & Bresha" }.ToList()
            }.Register(HistoriaCrux);

            RandoDLC = new ToggleFlagProperty(false)
            {
                Text = "Include DLC Areas",
                ID = "RandDLCCrux",
                Description = "Includes the Lightning, Sazh, and Coliseum DLC into the pool. Turning this on will also allow 3 additional areas open from the start.\n\n" +
                "[NOTE]\n" +
                "This requires a separate mod 'DLC Restoration - Console Content' that is provided as a core mod for Nova. Download this mod from the Core Mods download in the Nova discord server."
            }.Register(HistoriaCrux);

            ForceAcadVoidEndgame = new ToggleFlagProperty(false)
            {
                Text = "Force Acad 4xx / Void Beyond B endgame",
                ID = "CruxForceAcadEndgame",
                Description = "Forces the path to Acad 500 to be Acad 4xx -> Void Beyond -> Acad 500 locked behind the Graviton Core hand-ins.\n\n" +
                "Disabling this flag means that endgame can be placed anywhere in randomisation with potentially low requirements to access."
            }.Register(HistoriaCrux);

            Music = new Flag(false)
            {
                Text = "Shuffle Music",
                FlagID = "Music",
                DescriptionFormat = "Shuffle music around.",
                Aesthetic = true
            }.Register(FlagType.Other);
        }
    }
    public class Debug
    {
        public static Flag HighStats;

        internal static void Init()
        {
            HighStats = new Flag(false)
            {
                Text = "[DEBUG] High Initial Stats",
                FlagID = "DbgStats",
                DescriptionFormat = "[DEBUG]\nSets Serah/Noel initial stats to HP:99999, STR/MAG:9999",
                Debug = true
            }.Register(FlagType.Debug);
        }
    }

    public static void Init()
    {
        RandoFlags.FlagsList.Clear();
        Stats.Init();
        Items.Init();
        Enemies.Init();
        Other.Init();
        Debug.Init();
        RandoFlags.CategoryMap = ((FlagType[])Enum.GetValues(typeof(FlagType))).ToDictionary(f => (int)f, f => string.Join("/", Regex.Split(f.ToString(), @"(?<!^)(?=[A-Z])")));
        RandoFlags.SelectedCategory = "All";
    }
}

