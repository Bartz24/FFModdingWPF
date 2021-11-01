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
            public static Flag Enemies, EnemiesSize, EncounterSize, Bosses, Ereshkigal, Prologue;
            public static Flag EquipStats, GarbAbilities, EquipPassives;
            public static Flag Music;
            public static Flag Treasures, Pilgrims, EPLearns;
            public static Flag Shops;
            public static Flag Quests;

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
                    "Pilgrim's Cruxes will be included in the pool with treasures, quests, etc."
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
                    "Bosses that have + versions will be based on their new random boss of the old boss has + versions."
                }.Register(FlagType.Other);

                Ereshkigal = new Flag()
                {
                    Text = "Shuffle Bosses - Include Ereshkigal",
                    FlagID = "RandEresh",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "Requires 'Shuffle Bosses'\n" + 
                    "Includes Ereshkigal in the pool. This boss will not scale (yet)."
                }.Register(FlagType.Other);

                EncounterSize = new Flag()
                {
                    Text = "Randomize Encounter Size",
                    FlagID = "RandEncCount",
                    DescriptionFormat = "Requires 'Randomize Enemy Locations'\n" +
                    "If turned on, encounters with randomized enemies will be random in size up to +/- 2. A random enemy size will be selected from those already in the encounter.\n" +
                    "If turned off, encounters will remain the same size."
                }.Register(FlagType.Other);

                Music = new Flag()
                {
                    Text = "Shuffle Music",
                    FlagID = "Music",
                    DescriptionFormat = "Shuffle music around."
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

