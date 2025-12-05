using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;

public class RequiredAreasComponent: ItemLocationReqComponent
{
    private SeedGenerator generator;
    private List<string> requiredAreas;
    public RequiredAreasComponent(SeedGenerator generator, List<string> requiredAreas): base()
    {
        this.generator = generator;
        this.requiredAreas = requiredAreas;
    }

    public override bool AreItemReqsMet(ProgressionState state)
    {
        return requiredAreas.Intersect(state.AreasAccessible).Count() == requiredAreas.Count;
    }
}
