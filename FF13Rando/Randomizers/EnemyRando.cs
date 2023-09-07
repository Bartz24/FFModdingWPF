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

    public EnemyRando(RandomizerManager randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Enemy Data...", -1, 100);
        charaFamily.LoadWDB("13", @"\db\resident\charafamily.wdb");
        btCharaSpec.LoadWDB("13", @"\db\resident\bt_chara_spec.wdb");
        btCharaSpecOrig.LoadWDB("13", @"\db\resident\bt_chara_spec.wdb");
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Enemy Data...", -1, 100);
        TextRando textRando = Randomizers.Get<TextRando>();
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();

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
        Randomizers.SetUIProgress("Saving Enemy Data...", -1, 100);
        charaFamily.SaveWDB(@"\db\resident\charafamily.wdb");
        btCharaSpec.SaveWDB(@"\db\resident\bt_chara_spec.wdb");
    }
}
