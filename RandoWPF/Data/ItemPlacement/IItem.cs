using System.Collections.Generic;

namespace Bartz24.RandoWPF;
public interface IItem
{
    public string Category { get; set; }
    public int Rank { get; set; }
    public List<string> Traits { get; set; }
}
