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
                    LRFlags.StatsAbilities.EPAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled = false;
                    LRFlags.Enemies.EnemyLocations.FlagEnabled = true;
                    LRFlags.Enemies.EnemiesSize.Enabled = false;
                    LRFlags.Enemies.EncounterSize.Enabled = false;
                    LRFlags.Enemies.Bosses.Enabled = false;
                    LRFlags.Enemies.Zaltys.Enabled = false;
                    LRFlags.Enemies.Ereshkigal.Enabled = false;
                    LRFlags.Enemies.Prologue.Enabled = false;
                    LRFlags.StatsAbilities.EquipStats.FlagEnabled = true;
                    LRFlags.StatsAbilities.GarbAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Items.Treasures.FlagEnabled = true;
                    LRFlags.Items.Pilgrims.Enabled = false;
                    LRFlags.Items.EPLearns.Enabled = false;
                    LRFlags.Items.EPMissable.Enabled = false;
                    LRFlags.Items.Key.SelectedValue = "None";
                    LRFlags.Items.KeyDepth.SelectedValue = "Normal";
                    LRFlags.Items.Shops.FlagEnabled = true;
                    LRFlags.StatsAbilities.Quests.FlagEnabled = true;
                    LRFlags.Enemies.BhuniPlus.FlagEnabled = false;
                    LRFlags.Enemies.MatDrops.FlagEnabled = false;
                    LRFlags.Enemies.AbiDrops.FlagEnabled = false;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsSpecific.SelectedValue = "Exact";
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                    LRFlags.Other.HintsPilgrim.FlagEnabled = false;
                    LRFlags.StatsAbilities.EPCosts.FlagEnabled = false;
                    LRFlags.StatsAbilities.EPCostsZero.Enabled = false;
                    LRFlags.StatsAbilities.EPCostsRange.Value = 1;
                    LRFlags.StatsAbilities.NerfOC.FlagEnabled = false;
                }
            }.Register();

            HighVoltage = new Preset()
            {
                Name = "High Voltage (Default)",
                Default = true,
                OnApply = () => {
                    baseOnApply();
                    LRFlags.StatsAbilities.EPAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled = true;
                    LRFlags.Enemies.EnemyLocations.FlagEnabled = true;
                    LRFlags.Enemies.EnemiesSize.Enabled = true;
                    LRFlags.Enemies.EncounterSize.Enabled = true;
                    LRFlags.Enemies.Bosses.Enabled = true;
                    LRFlags.Enemies.Zaltys.Enabled = true;
                    LRFlags.Enemies.Ereshkigal.Enabled = false;
                    LRFlags.Enemies.Prologue.Enabled = false;
                    LRFlags.StatsAbilities.EquipStats.FlagEnabled = true;
                    LRFlags.StatsAbilities.GarbAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Items.Treasures.FlagEnabled = true;
                    LRFlags.Items.Pilgrims.Enabled = true;
                    LRFlags.Items.EPLearns.Enabled = true;
                    LRFlags.Items.EPMissable.Enabled = false;
                    LRFlags.Items.Key.SelectedValue = "None";
                    LRFlags.Items.KeyDepth.SelectedValue = "Normal";
                    LRFlags.Items.Shops.FlagEnabled = true;
                    LRFlags.StatsAbilities.Quests.FlagEnabled = true;
                    LRFlags.Enemies.BhuniPlus.FlagEnabled = false;
                    LRFlags.Enemies.MatDrops.FlagEnabled = false;
                    LRFlags.Enemies.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsSpecific.SelectedValue = "Exact";
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                    LRFlags.Other.HintsPilgrim.FlagEnabled = false;
                    LRFlags.StatsAbilities.EPCosts.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPCostsZero.Enabled = false;
                    LRFlags.StatsAbilities.EPCostsRange.Value = 1;
                    LRFlags.StatsAbilities.NerfOC.FlagEnabled = false;
                }
            }.Register();

            Chaos = new Preset()
            {
                Name = "Chaos (Chaotic/Experimental)",
                OnApply = () => {
                    baseOnApply();
                    LRFlags.StatsAbilities.EPAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled = true;
                    LRFlags.Enemies.EnemyLocations.FlagEnabled = true;
                    LRFlags.Enemies.EnemiesSize.Enabled = true;
                    LRFlags.Enemies.EncounterSize.Enabled = true;
                    LRFlags.Enemies.Bosses.Enabled = true;
                    LRFlags.Enemies.Zaltys.Enabled = true;
                    LRFlags.Enemies.Ereshkigal.Enabled = true;
                    LRFlags.Enemies.Prologue.Enabled = false;
                    LRFlags.StatsAbilities.EquipStats.FlagEnabled = true;
                    LRFlags.StatsAbilities.GarbAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Items.Treasures.FlagEnabled = true;
                    LRFlags.Items.Pilgrims.Enabled = true;
                    LRFlags.Items.EPLearns.Enabled = true;
                    LRFlags.Items.EPMissable.Enabled = false;
                    LRFlags.Items.Key.SelectedValue = "Quests";
                    LRFlags.Items.KeyDepth.SelectedValue = "Hard";
                    LRFlags.Items.Shops.FlagEnabled = true;
                    LRFlags.StatsAbilities.Quests.FlagEnabled = true;
                    LRFlags.Enemies.BhuniPlus.FlagEnabled = true;
                    LRFlags.Enemies.MatDrops.FlagEnabled = true;
                    LRFlags.Enemies.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsSpecific.SelectedValue = "Vague Type";
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                    LRFlags.Other.HintsPilgrim.FlagEnabled = false;
                    LRFlags.StatsAbilities.EPCosts.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPCostsZero.Enabled = true;
                    LRFlags.StatsAbilities.EPCostsRange.Value = 3;
                    LRFlags.StatsAbilities.NerfOC.FlagEnabled = false;
                }
            }.Register();

            KO = new Preset()
            {
                Name = "K.O. (Hopeless/Experimental)",
                OnApply = () => {
                    baseOnApply();
                    LRFlags.StatsAbilities.EPAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPAbilitiesEscape.Enabled = true;
                    LRFlags.Enemies.EnemyLocations.FlagEnabled = true;
                    LRFlags.Enemies.EnemiesSize.Enabled = true;
                    LRFlags.Enemies.EncounterSize.Enabled = true;
                    LRFlags.Enemies.Bosses.Enabled = true;
                    LRFlags.Enemies.Zaltys.Enabled = true;
                    LRFlags.Enemies.Ereshkigal.Enabled = true;
                    LRFlags.Enemies.Prologue.Enabled = true;
                    LRFlags.StatsAbilities.EquipStats.FlagEnabled = true;
                    LRFlags.StatsAbilities.GarbAbilities.FlagEnabled = true;
                    LRFlags.StatsAbilities.EquipPassives.FlagEnabled = true;
                    LRFlags.Other.Music.FlagEnabled = true;
                    LRFlags.Items.Treasures.FlagEnabled = true;
                    LRFlags.Items.Pilgrims.Enabled = true;
                    LRFlags.Items.EPLearns.Enabled = true;
                    LRFlags.Items.EPMissable.Enabled = true;
                    LRFlags.Items.Key.SelectedValue = "Grindy";
                    LRFlags.Items.KeyDepth.SelectedValue = "Hard+++";
                    LRFlags.Items.Shops.FlagEnabled = true;
                    LRFlags.StatsAbilities.Quests.FlagEnabled = true;
                    LRFlags.Enemies.BhuniPlus.FlagEnabled = true;
                    LRFlags.Enemies.MatDrops.FlagEnabled = true;
                    LRFlags.Enemies.AbiDrops.FlagEnabled = true;
                    LRFlags.Other.HintsMain.FlagEnabled = true;
                    LRFlags.Other.HintsSpecific.SelectedValue = "Random";
                    LRFlags.Other.HintsNotes.FlagEnabled = true;
                    LRFlags.Other.HintsEP.FlagEnabled = true;
                    LRFlags.Other.HintsPilgrim.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPCosts.FlagEnabled = true;
                    LRFlags.StatsAbilities.EPCostsZero.Enabled = true;
                    LRFlags.StatsAbilities.EPCostsRange.Value = 5;
                    LRFlags.StatsAbilities.NerfOC.FlagEnabled = true;
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

