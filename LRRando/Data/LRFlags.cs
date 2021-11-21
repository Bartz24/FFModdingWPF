using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public class LRFlags
    {
        public enum FlagType
        {
            Other
        }
        public class Other
        {
            public static Flag EPAbilities, EPAbilitiesEscape;
            public static Flag Enemies, EnemiesSize, EncounterSize, Bosses, Zaltys, Ereshkigal, Prologue, BhuniPlus, MatDrops, AbiDrops;
            public static Flag EquipStats, GarbAbilities, EquipPassives;
            public static Flag Music;
            public static Flag Treasures, Pilgrims, EPLearns, Key, CoP;
            public static Flag Shops;
            public static Flag Quests;
            public static Flag HintsMain, HintsNotes, HintsEP;

            internal static void Init()
            {
                EquipStats = new Flag()
                {
                    Text = "Randomize Equipment Stats",
                    FlagID = "RandEqStat",
                    DescriptionFormat = "Randomize Garb, Weapon, and Shield stats."
                }.Register(FlagType.Other);

                GarbAbilities = new Flag()
                {
                    Text = "Randomize Garb Abilities",
                    FlagID = "RandGarbAbi",
                    DescriptionFormat = "Randomize abilities locked to garbs."
                }.Register(FlagType.Other);

                EquipPassives = new Flag()
                {
                    Text = "Randomize Equipment Passive Abilities",
                    FlagID = "RandPassive",
                    DescriptionFormat = "Randomize passive abilities on garbs, garb abilities, weapons, shields, and accessories."
                }.Register(FlagType.Other);

                EPAbilities = new Flag()
                {
                    Text = "Shuffle EP Abilities",
                    FlagID = "EPAbi",
                    DescriptionFormat = "Shuffles all EP abilities between each other except for Overclock and Escape (Escape requires the below flag)."
                }.Register(FlagType.Other);

                EPAbilitiesEscape = new Flag()
                {
                    Text = "Shuffle EP Abilities - Include Escape",
                    FlagID = "EPAbiEsc",
                    DescriptionFormat = "Requires 'Shuffle EP Abilities'\n" +
                    "Escape will be included in randomization."
                }.Register(FlagType.Other);

                Treasures = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards",
                    FlagID = "Treasures",
                    DescriptionFormat = "Randomize Treasures, Quest Rewards, Non-repeatable Pickups, Soul seed rewards, Non-repeatable Item Appraisal rewards\n" +
                    "Does not include key items"
                }.Register(FlagType.Other);

                Pilgrims = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards - Include Pilgrim's Cruxes",
                    FlagID = "Pilgrims",
                    DescriptionFormat = "Requires 'Randomize Treasures, Quest Item Rewards, and Other Rewards'\n" +
                    "Pilgrim's Cruxes will be included in the pool with treasures, quests, etc.\n" +
                    "Pilgrim's Cruxes will not appear in missable locations or from Day 10 and later."
                }.Register(FlagType.Other);

                Key = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards - Include Key Items",
                    FlagID = "Key",
                    DescriptionFormat = "Requires 'Randomize Treasures, Quest Item Rewards, and Other Rewards'\n" +
                    "Key items will not appear in missable locations or from Day 10 and later.\n" +
                    "The following key items will be included in the pool:\n" +
                    "Fragment of Mischief, Fragment of Smiles, Moogle Fragment, ID Card, Dead Dunes Tablets, Proof of Courage, Violet Amulet, Lapis Lazuli, Power Booster, Moogle Dust, Photo Frame, Etro's Forbidden Tome, Broken Gyroscope, Golden Scarab, Key to the Sand Gate, Key to the Green Gate, Bandit's Bloodseal, Oath of the Merchants Guild, Jade Hair Comb, Bronze Pocket Watch, Nostalgic Scores, Rubber Ball, Thunderclap Cap, Quill Pen, Loupe, Musical Sphere Treasure Key, Supply Sphere Password",
                    Experimental = true
                }.Register(FlagType.Other);

                CoP = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards - Allow Key Items in CoP and Grindy Locations",
                    FlagID = "CoP",
                    DescriptionFormat = "Requires 'Randomize Treasures, Quest Item Rewards, and Other Rewards'\n" +
                    "Key items will be allowed to appear in NON-GLOBAL Canvas of Prayers, 40+ Soul Seeds rewards, and 10+ Unappraised Items.\n" +
                    "Works for all key items including Pilgrim's Cruxes and all the key items in the above flag.",
                    Experimental = true
                }.Register(FlagType.Other);

                EPLearns = new Flag()
                {
                    Text = "Randomize Treasures, Quest Rewards, and Other Rewards - Include Learned EP Abilities",
                    FlagID = "Pilgrims",
                    DescriptionFormat = "Requires 'Randomize Treasures, Quest Item Rewards, and Other Rewards'\n" +
                    "EP Abilities learned at the start of the game will be included in the pool with treasures, quests, etc.\n" +
                    "This includes when Curaga, Escape, Chronostasis, and Teleport are normally learned."
                }.Register(FlagType.Other);

                Shops = new Flag()
                {
                    Text = "Randomize Shops",
                    FlagID = "Shops",
                    DescriptionFormat = "Randomize Shop contents.\n" +
                    "Hard mode items are included."
                }.Register(FlagType.Other);

                Quests = new Flag()
                {
                    Text = "Randomize Quest Stat Rewards",
                    FlagID = "Quests",
                    DescriptionFormat = "Randomize stats rewarded by quests. Includes Strength, Magic, Max HP, Max EP, Max ATB, and Recovery Item Slots."
                }.Register(FlagType.Other);

                Enemies = new Flag()
                {
                    Text = "Randomize Enemy Locations",
                    FlagID = "RandEne",
                    DescriptionFormat = "Randomize normal enemies between each other."
                }.Register(FlagType.Other);

                EnemiesSize = new Flag()
                {
                    Text = "Randomize Enemy Locations - Between Any Size",
                    FlagID = "RandEneSize",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "If turned on, enemies of any size can replace another.\n" +
                    "If turned off, enemies will be randomized with enemies of the same size. Humans are considered mid."
                }.Register(FlagType.Other);

                EncounterSize = new Flag()
                {
                    Text = "Randomize Encounter Size",
                    FlagID = "RandEncCount",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "If turned on, encounters with randomized enemies will be random in size up to +/- 2. A random enemy size will be selected from those already in the encounter.\n" +
                    "If turned off, encounters will remain the same size."
                }.Register(FlagType.Other);

                MatDrops = new Flag()
                {
                    Text = "Randomize Enemy Material Drops",
                    FlagID = "RandMatDrops",
                    DescriptionFormat = "Randomize Enemy material drops"
                }.Register(FlagType.Other);

                AbiDrops = new Flag()
                {
                    Text = "Randomize Enemy Ability Drops",
                    FlagID = "RandAbiDrops",
                    DescriptionFormat = "Randomize Enemy ability drops"
                }.Register(FlagType.Other);

                Prologue = new Flag()
                {
                    Text = "Randomize Enemy Locations - Include Prologue Tutorial",
                    FlagID = "RandProlo",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "Prologue tutorial enemies will be included and randomized.\n" +
                    "VERY HARD or IMPOSSIBLE.",
                    Experimental = true
                }.Register(FlagType.Other);

                Bosses = new Flag()
                {
                    Text = "Shuffle Bosses",
                    FlagID = "RandBoss",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" + 
                    "Shuffle the following bosses between each other:\n" +
                    "Noel Kreiss, Snow Villiers, Caius Ballad, and Grendel.\n" +
                    "Bosses that have + versions will be based on their new random boss of the old boss has + versions.\n"
                }.Register(FlagType.Other);

                Zaltys = new Flag()
                {
                    Text = "Shuffle Bosses - Include Prologue Zaltys",
                    FlagID = "RandZaltys",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "Requires 'Shuffle Bosses'\n" +
                    "Includes Prologue Zaltys in the pool. This boss scales up if randomized to a later boss.\n" +
                    "Later bosses will scale down if replacing Prologue Zaltys."
                }.Register(FlagType.Other);

                Ereshkigal = new Flag()
                {
                    Text = "Shuffle Bosses - Include Ereshkigal",
                    FlagID = "RandEresh",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "Requires 'Shuffle Bosses'\n" + 
                    "Includes Ereshkigal in the pool. This boss scales down if randomized to a story boss.\n" +
                    "Story bosses will scale up if replacing Ereshkigal."
                }.Register(FlagType.Other);

                BhuniPlus = new Flag()
                {
                    Text = "Force Bhunivelze+ on NG",
                    FlagID = "BhuniPlus",
                    DescriptionFormat = "The final boss even on New Game will be Bhunivelze+."
                }.Register(FlagType.Other);

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
            Other.Init();
        }
    }
}

