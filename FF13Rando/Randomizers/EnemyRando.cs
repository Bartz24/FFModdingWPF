using Bartz24.Data;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using FF13Rando;
using System.Linq;

namespace FF13Rando;

public class EnemyRando : Randomizer
{
    public DataStoreWDB<DataStoreCharaFamily> charaFamily = new();
    public DataStoreWDB<DataStoreBtCharaSpec> btCharaSpec = new();
    public DataStoreWDB<DataStoreBtCharaSpec> btCharaSpecOrig = new();

    public EnemyRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Enemy Data...");
        charaFamily.LoadWDB(Generator, "13", @"\db\resident\charafamily.wdb");
        btCharaSpec.LoadWDB(Generator, "13", @"\db\resident\bt_chara_spec.wdb");
        btCharaSpecOrig.LoadWDB(Generator, "13", @"\db\resident\bt_chara_spec.wdb");
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Enemy Data...");
        TextRando textRando = Generator.Get<TextRando>();
        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        if (FF13Flags.Stats.RunSpeedMult.FlagEnabled)
        {
            string[] chars = { "fam_pc_light", "fam_pc_fang", "fam_pc_hope", "fam_pc_sazz", "fam_pc_snow", "fam_pc_vanira" };
            chars.ForEach(c => charaFamily[c].u8RunSpeed = (byte)(0x60 * FF13Flags.Stats.RunSpeedMultValue.Value / 100));
        }

        if (FF13Flags.Debug.LowEnemyHP.FlagEnabled)
        {
            btCharaSpec.Keys.Where(id => id.StartsWith("m") || id.StartsWith("w")).ForEach(id => btCharaSpec[id].u24MaxHp = 10);            
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Enemy Data...");
        charaFamily.SaveWDB(Generator, @"\db\resident\charafamily.wdb");
        btCharaSpec.SaveWDB(Generator, @"\db\resident\bt_chara_spec.wdb");
    }
}
