using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System.Linq;

namespace FF12Rando;
public class APTextRando : TextRando
{
    public APTextRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    protected override void UpdateSeedInfoStr()
    {
        DataStoreBinText.StringData seedInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$VERSION$"));
        seedInfoStr.Text = seedInfoStr.Text.Replace("$SEED$", SetupData.Seed.Clean() + " (Archipelago)");
        base.UpdateSeedInfoStr();
    }

    protected override void UpdateGoalInfoStr()
    {
        DataStoreBinText.StringData goalInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$INFO$"));

        goalInfoStr.Text = goalInfoStr.Text.Replace("$INFO$", "The goal for this seed:\n" +
            "Find a {color:gold}Writ of Transit{rgb:gray} and travel to {italic}Bahamut{/italic}\n" +
            "using the {italic}Strahl{/italic}, accessed with the Systems Access Key");
    }
}
