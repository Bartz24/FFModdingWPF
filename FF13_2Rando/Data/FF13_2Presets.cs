using Bartz24.RandoWPF;
using System;

namespace FF13_2Rando
{
    public class FF13_2Presets
    {
        public static Preset Diversity, Custom;
        public static void Init()
        {
            Presets.PresetsList.Clear();

            Action baseOnApply = () => {
                Flags.FlagsList.ForEach(f => f.FlagEnabled = false);
            };

            Diversity = new Preset()
            {
                Name = "Diversity (Simple)",
                OnApply = () => {
                    baseOnApply();
                    FF13_2Flags.Other.HistoriaCrux.FlagEnabled = true;
                }
            }.Register();


            Custom = new Preset()
            {
                Name = "Custom (Modified)",
                Custom = true
            }.Register();

            Presets.Selected = Diversity;
        }
    }
}

