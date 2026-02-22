using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using FF13Rando;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando;

public class ManualCrystariumRando : CrystariumRando
{
    public ManualCrystariumRando(SeedGenerator randomizers) : base(randomizers) { }

    public override string GetFirstRole(string c)
    {
        return primaryRoles[c][0].ToString().Substring(0, 3).ToLower();
    }

    public override void Randomize()
    {
        base.Randomize();

        // Swap Lightning's Blitz with Attack
        var attack = abilityData.Values.Where(a => a.Name == "Attack" && a.Characters.Contains("lightning")).First();
        var blitz = abilityData.Values.Where(a => a.Name == "Blitz" && a.Characters.Contains("lightning")).First();

        // Find the nodes for each ability
        var attackNode = crystariums["lightning"].Values.Where(n => n.sAbility == attack.ID).First();
        var blitzNode = crystariums["lightning"].Values.Where(n => n.sAbility == blitz.ID).First();

        // Swap the abilities
        attackNode.sAbility = blitz.ID;
        blitzNode.sAbility = attack.ID;
    }
}
