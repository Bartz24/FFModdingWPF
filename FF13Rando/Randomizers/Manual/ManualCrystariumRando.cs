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
}
