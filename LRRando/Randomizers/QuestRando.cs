using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bartz24.FF13_2_LR.Enums;

namespace LRRando
{
    public class QuestRando : Randomizer
    {
        public DataStoreDB3<DataStoreRQuest> questRewards = new DataStoreDB3<DataStoreRQuest>();

        public QuestRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Quest Rewards...";
        }
        public override string GetID()
        {
            return "Quests";
        }

        public override void Load()
        {
            questRewards.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_quest.wdb", false);
            FileExtensions.CopyFile(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb", SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb.orig");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (LRFlags.Other.Quests.FlagEnabled)
            {
                LRFlags.Other.Quests.SetRand();

                List<DataStoreRQuest> mainQuests = questRewards.Values.Where(q => q.iMaxGp > 0 || q.iMaxAtb > 0 || q.iItemBagSize > 0).ToList().Shuffle().ToList();
                int ep = mainQuests.Select(q => q.iMaxGp).Sum();
                int atb = mainQuests.Select(q => q.iMaxAtb).Sum();
                int items = mainQuests.Select(q => q.iItemBagSize).Sum();
                mainQuests.ForEach(q =>
                {
                    int count = q.iMaxGp / 2000 + q.iMaxAtb / 10 + q.iItemBagSize;
                    q.iMaxGp = q.iMaxAtb = q.iItemBagSize = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int next = RandomNum.SelectRandomWeighted(new int[] { 0, 1, 2 }.ToList(), i =>
                        {
                            switch (i)
                            {
                                case 0:
                                    return ep > 0 ? 1 : 0;
                                case 1:
                                    return atb > 0 ? 1 : 0;
                                case 2:
                                    return items > 0 ? 1 : 0;
                                default:
                                    return 0;
                            }
                        });
                        switch (next)
                        {
                            case 0:
                                q.iMaxGp += 2000;
                                ep -= 2000;
                                break;
                            case 1:
                                q.iMaxAtb += 10;
                                atb -= 10;
                                break;
                            case 2:
                                q.iItemBagSize += 1;
                                items -= 1;
                                break;
                        }
                    }
                });

                questRewards.Values.ForEach(q =>
                {
                    Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                    new Tuple<int, int>(0, 10000),
                    new Tuple<int, int>(0, 1000),
                    new Tuple<int, int>(0, 1000)
                    };
                    float[] weights = new float[] { 1, 10, 10 };
                    int[] zeros = new int[] { 15, 15, 15 };
                    StatPoints statPoints = new StatPoints(bounds, weights, zeros);
                    statPoints.Randomize(new int[] { q.iMaxHp, q.iAtkPhy, q.iAtkMag });

                    q.iMaxHp = statPoints[0];
                    q.iAtkPhy = statPoints[1];
                    q.iAtkMag = statPoints[2];
                });

                RandomNum.ClearRand();
            }
        }

        public override void Save()
        {
            questRewards.SaveDB3(@"\db\resident\_wdbpack.bin\r_quest.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_quest.wdb");
            TempSaveFix();
        }

        private void TempSaveFix()
        {
            byte[] origData = File.ReadAllBytes(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb.orig");
            byte[] data = File.ReadAllBytes(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb");

            if (data.Length < origData.Length)
            {

                int startQstData = (int)origData.ReadUInt(0xE0);
                int newStartQstData = (int)data.ReadUInt(0xE0);
                data = origData.SubArray(0, startQstData).Concat(data.SubArray(newStartQstData, data.Length - newStartQstData));
                List<DataStoreRQuest> values = questRewards.Values.ToList();
                for (int i = 0; i < values.Count; i++)
                {
                    byte[] itembag2 = new byte[4];
                    itembag2.SetUInt(0, (uint)values[i].iItemBagSize2);
                    data = data.SubArray(0, startQstData + 72 * i + 68).Concat(itembag2).Concat(data.SubArray(startQstData + 72 * i + 68, data.Length - (startQstData + 72 * i + 68)));
                }

                File.WriteAllBytes(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb", data);
            }
            File.Delete(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb.orig");
        }
    }
}
