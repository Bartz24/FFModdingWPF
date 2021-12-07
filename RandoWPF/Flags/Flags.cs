using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF
{    
    public class Flags
    {
        public static List<Flag> FlagsList { get; set; } = new List<Flag>();

        public static Dictionary<int, string> CategoryMap { get; set; } = new Dictionary<int, string>();

        public static List<string> CategoryList { get => CategoryMap.Keys.OrderBy(i => i).Select(i => CategoryMap[i]).ToList(); }
        public static string SelectedCategory
        {
            get => selected;
            set
            {
                selected = value;
                if (SelectedChanged != null)
                    SelectedChanged(null, EventArgs.Empty);
            }
        }

        public static bool ApplyingPreset = false;
        private static string selected;

        public static event EventHandler SelectedChanged = delegate { };
    }
}
