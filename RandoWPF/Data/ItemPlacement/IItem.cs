using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public interface IItem
{
    public string Category { get; set; }
    public int Rank { get; set; }
    public List<string> Traits { get; set; }
}
