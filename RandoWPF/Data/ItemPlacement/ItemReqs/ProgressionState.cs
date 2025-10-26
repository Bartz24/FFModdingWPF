using System.Collections.Generic;

namespace Bartz24.RandoWPF;
public class ProgressionState
{
    public Dictionary<string, int> ItemsAvailable { get; set; } = new();
    public HashSet<string> AreasAccessible { get; set; } = new();
    public HashSet<string> LocationsCompleted { get; set; } = new();

    public ProgressionState()
    {
    }

    public ProgressionState(ProgressionState orig)
    {
        ItemsAvailable = new Dictionary<string, int>(orig.ItemsAvailable);
        AreasAccessible = new HashSet<string>(orig.AreasAccessible);
        LocationsCompleted = new HashSet<string>(orig.LocationsCompleted);
    }
}
