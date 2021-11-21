using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public class LRPresets
    {
        public static Preset Overclock, Chaos, HighVoltage, KO, Custom;

        public static Flag EPAbilities, EPAbilitiesEscape;
        public static Flag Enemies, EnemiesSize, EncounterSize, Bosses, Zaltys, Ereshkigal, Prologue;
        public static Flag EquipStats, GarbAbilities, EquipPassives;
        public static Flag Music;
        public static Flag Treasures, Pilgrims, EPLearns, Key, CoP;
        public static Flag Shops;
        public static Flag Quests;
        public static void Init()
        {
            Presets.PresetsList.Clear();

            Action baseOnApply = () => {
                Flags.FlagsList.ForEach(f => f.FlagEnabled = false);
            };

            Overclock = new Preset()
            {
                Name = "Overclock (Simple)",
                OnApply = () => {
                    baseOnApply();
                    LRFlags.Other.EPAbilities.FlagEnabled = true;
                    LRFlags.Other.EPAbilitiesEscape.FlagEnabled = false;
                    LRFlags.Other.Enemies.FlagEnabled = true;
                    LRFlags.Other.EnemiesSize.FlagEnabled = false;
                    LRFlags.Other.EncounterSize.FlagEnabled = false;
                    LRFlags.Other.Bosses.FlagEnabled = false;
                    LRFlags.Other.Zaltys.FlagEnabled = false;
                    LRFlags.Other.Ereshkigal.FlagEnabled = false;
                    LRFlags.Other.Prologue.FlagEnabled = false;
                    LRFlags.Other.EquipStats.FlagEnabled = true;
                    LRFlags.Other.GarbAbilities.FlagEnabled = true;
                    LRFlags.Other.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Other.Treasures.FlagEnabled = true;
                    LRFlags.Other.Pilgrims.FlagEnabled = false;
                    LRFlags.Other.EPLearns.FlagEnabled = false;
                    LRFlags.Other.Key.FlagEnabled = false;
                    LRFlags.Other.CoP.FlagEnabled = false;
                    LRFlags.Other.Shops.FlagEnabled = true;
                    LRFlags.Other.Quests.FlagEnabled = true;
                    LRFlags.Other.BhuniPlus.FlagEnabled = false;
                    LRFlags.Other.MatDrops.FlagEnabled = false;
                    LRFlags.Other.AbiDrops.FlagEnabled = false;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                }
            }.Register();

            HighVoltage = new Preset()
            {
                Name = "High Voltage (Default)",
                Default = true,
                OnApply = () => {
                    baseOnApply();
                    LRFlags.Other.EPAbilities.FlagEnabled = true;
                    LRFlags.Other.EPAbilitiesEscape.FlagEnabled = true;
                    LRFlags.Other.Enemies.FlagEnabled = true;
                    LRFlags.Other.EnemiesSize.FlagEnabled = true;
                    LRFlags.Other.EncounterSize.FlagEnabled = true;
                    LRFlags.Other.Bosses.FlagEnabled = true;
                    LRFlags.Other.Zaltys.FlagEnabled = true;
                    LRFlags.Other.Ereshkigal.FlagEnabled = false;
                    LRFlags.Other.Prologue.FlagEnabled = false;
                    LRFlags.Other.EquipStats.FlagEnabled = true;
                    LRFlags.Other.GarbAbilities.FlagEnabled = true;
                    LRFlags.Other.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Other.Treasures.FlagEnabled = true;
                    LRFlags.Other.Pilgrims.FlagEnabled = true;
                    LRFlags.Other.EPLearns.FlagEnabled = true;
                    LRFlags.Other.Key.FlagEnabled = false;
                    LRFlags.Other.CoP.FlagEnabled = false;
                    LRFlags.Other.Shops.FlagEnabled = true;
                    LRFlags.Other.Quests.FlagEnabled = true;
                    LRFlags.Other.BhuniPlus.FlagEnabled = false;
                    LRFlags.Other.MatDrops.FlagEnabled = false;
                    LRFlags.Other.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                }
            }.Register();

            Chaos = new Preset()
            {
                Name = "Chaos (Chaotic/Experimental)",
                OnApply = () => {
                    baseOnApply();
                    LRFlags.Other.EPAbilities.FlagEnabled = true;
                    LRFlags.Other.EPAbilitiesEscape.FlagEnabled = true;
                    LRFlags.Other.Enemies.FlagEnabled = true;
                    LRFlags.Other.EnemiesSize.FlagEnabled = true;
                    LRFlags.Other.EncounterSize.FlagEnabled = true;
                    LRFlags.Other.Bosses.FlagEnabled = true;
                    LRFlags.Other.Zaltys.FlagEnabled = true;
                    LRFlags.Other.Ereshkigal.FlagEnabled = true;
                    LRFlags.Other.Prologue.FlagEnabled = false;
                    LRFlags.Other.EquipStats.FlagEnabled = true;
                    LRFlags.Other.GarbAbilities.FlagEnabled = true;
                    LRFlags.Other.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Other.Treasures.FlagEnabled = true;
                    LRFlags.Other.Pilgrims.FlagEnabled = true;
                    LRFlags.Other.EPLearns.FlagEnabled = true;
                    LRFlags.Other.Key.FlagEnabled = true;
                    LRFlags.Other.CoP.FlagEnabled = false;
                    LRFlags.Other.Shops.FlagEnabled = true;
                    LRFlags.Other.Quests.FlagEnabled = true;
                    LRFlags.Other.BhuniPlus.FlagEnabled = true;
                    LRFlags.Other.MatDrops.FlagEnabled = true;
                    LRFlags.Other.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                }
            }.Register();

            KO = new Preset()
            {
                Name = "K.O. (Hopeless/Experimental)",
                OnApply = () => {
                    baseOnApply();
                    LRFlags.Other.EPAbilities.FlagEnabled = true;
                    LRFlags.Other.EPAbilitiesEscape.FlagEnabled = true;
                    LRFlags.Other.Enemies.FlagEnabled = true;
                    LRFlags.Other.EnemiesSize.FlagEnabled = true;
                    LRFlags.Other.EncounterSize.FlagEnabled = true;
                    LRFlags.Other.Bosses.FlagEnabled = true;
                    LRFlags.Other.Zaltys.FlagEnabled = true;
                    LRFlags.Other.Ereshkigal.FlagEnabled = true;
                    LRFlags.Other.Prologue.FlagEnabled = true;
                    LRFlags.Other.EquipStats.FlagEnabled = true;
                    LRFlags.Other.GarbAbilities.FlagEnabled = true;
                    LRFlags.Other.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Other.Treasures.FlagEnabled = true;
                    LRFlags.Other.Pilgrims.FlagEnabled = true;
                    LRFlags.Other.EPLearns.FlagEnabled = true;
                    LRFlags.Other.Key.FlagEnabled = true;
                    LRFlags.Other.CoP.FlagEnabled = true;
                    LRFlags.Other.Shops.FlagEnabled = true;
                    LRFlags.Other.Quests.FlagEnabled = true;
                    LRFlags.Other.BhuniPlus.FlagEnabled = true;
                    LRFlags.Other.MatDrops.FlagEnabled = true;
                    LRFlags.Other.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = false;
                    LRFlags.Other.HintsNotes.FlagEnabled = false;
                    LRFlags.Other.HintsEP.FlagEnabled = false;
                }
            }.Register();


            Custom = new Preset()
            {
                Name = "Custom (Modified)",
                Custom = true
            }.Register();

            Presets.Selected = HighVoltage;
        }
    }
}

