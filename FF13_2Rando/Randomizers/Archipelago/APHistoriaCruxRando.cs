using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;

public class APHistoriaCruxRando: HistoriaCruxRando
{
    public APHistoriaCruxRando(SeedGenerator generator) : base(generator)
    {

    }

    public override void Randomize()
    {
        // Not currently supported within AP, coming "soon"
        var apData = RandoFlags.GetArchipelagoData<FF13_2ArchipelagoData>();
        var graph = apData.AreaGraph;
        // etc
    }
}
