using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13_2Rando;

public class MogLevelReqComponent: ItemLocationReqComponent
{
    private SeedGenerator generator;
    private int requiredLevel;
    public MogLevelReqComponent(SeedGenerator generator, int requiredLevel) : base()
    {
        this.generator = generator;
        this.requiredLevel = requiredLevel;
    }

    public override bool AreItemReqsMet(ProgressionState state)
    {
        return HasMogLevel(state);
    }

    public bool HasMogLevel(ProgressionState state)
    {
        HistoriaCruxRando cruxRando = generator.Get<HistoriaCruxRando>();
        var availableAreas = state.AreasAccessible.ToList();
        var currMogLevel = cruxRando.GetMogLevel(availableAreas);
        return currMogLevel >= requiredLevel;
    }
}
