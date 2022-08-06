using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;

namespace FF13Rando
{
    public class EnemyRando : Randomizer
    {
        public DataStoreWDB<DataStoreBtCharaSpec> charaSpec = new DataStoreWDB<DataStoreBtCharaSpec>();
        public DataStoreWDB<DataStoreBtCharaSpec> charaSpecOrig = new DataStoreWDB<DataStoreBtCharaSpec>();

        public EnemyRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetProgressMessage()
        {
            return "Randomizing Enemies...";
        }
        public override string GetID()
        {
            return "Enemies";
        }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Enemy Data...", -1, 100);
            charaSpec.LoadWDB("13", @"\db\resident\bt_chara_spec.wdb");
            charaSpecOrig.LoadWDB("13", @"\db\resident\bt_chara_spec.wdb");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Enemy Data...", -1, 100);
            TextRando textRando = Randomizers.Get<TextRando>("Text");
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Enemy Data...", -1, 100);
            charaSpec.SaveWDB(@"\db\resident\bt_chara_spec.wdb");

        }
    }
}
