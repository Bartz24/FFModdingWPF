using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF13_2Rando;

public class MogLevelReqComponent : ItemLocationReqComponent
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
        return state.ItemsAvailable.GetValueOrDefault("key_mog_level", 0) >= requiredLevel;
    }
}
