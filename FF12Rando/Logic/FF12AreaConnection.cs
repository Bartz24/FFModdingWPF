using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace FF12Rando;
public class FF12AreaConnection : AreaConnection
{
    public FF12AreaConnection(SeedGenerator generator, string[] row) : base(generator, row)
    {
    }

    public override List<ItemLocationReqComponent> GetComponents()
    {
        var list = base.GetComponents();
        list.Add(new CharReqComponent(BaseDifficulty));
        return list;
    }

    public override AreaConnection CreateReverse()
    {
        FF12AreaConnection reverse = new(Generator, new string[6]);
        reverse.Name = Name + "_Reverse";
        reverse.FromAreaName = ToAreaName;
        reverse.ToAreaName = FromAreaName;
        reverse.Requirements = Requirements;
        reverse.Traits = Traits;
        reverse.BaseDifficulty = BaseDifficulty;
        return reverse;
    }
}
