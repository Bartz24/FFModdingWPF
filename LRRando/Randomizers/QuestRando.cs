using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.LR;
using Bartz24.RandoWPF;
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
        public DataStoreDB3<DataStoreRQuestCtrl> questRequirements = new DataStoreDB3<DataStoreRQuestCtrl>();

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
            FileHelpers.CopyFile(SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb", SetupData.OutputFolder + @"\db\resident\_wdbpack.bin\r_quest.wdb.orig");
            questRequirements.LoadDB3("LR", @"\db\resident\_wdbpack.bin\r_quest_ctrl.wdb", false);

            questRewards["qst_062"].iMaxGp = 2000;
            questRewards["qst_046"].iItemBagSize = 1;
            questRewards["qst_028"].iItemBagSize = 1;
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (LRFlags.StatsAbilities.Quests.FlagEnabled)
            {
                LRFlags.StatsAbilities.Quests.SetRand();

                List<DataStoreRQuest> mainQuests = questRewards.Values.Where(q => q.iMaxGp > 0 || q.iMaxAtb > 0 || q.iItemBagSize > 0).ToList().Shuffle().ToList();
                int ep = mainQuests.Select(q => q.iMaxGp).Sum();
                int atb = mainQuests.Select(q => q.iMaxAtb).Sum();
                int items = mainQuests.Select(q => q.iItemBagSize).Sum();
                mainQuests.ForEach(q =>
                {
                    int count = q.iMaxGp / 2000 + q.iMaxAtb / 10 + q.iItemBagSize;
                    q.iMaxGp = q.iMaxAtb = q.iItemBagSize = 0;
                    for (int n = 0; n < count; n++)
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
                    int[] negs = new int[] { 0, 0, 0 };
                    StatPoints statPoints = new StatPoints(bounds, weights, zeros, negs);
                    statPoints.Randomize(new int[] { q.iMaxHp, q.iAtkPhy, q.iAtkMag });

                    q.iMaxHp = statPoints[0];
                    q.iAtkPhy = statPoints[1];
                    q.iAtkMag = statPoints[2];
                });

                RandomNum.ClearRand();
            }

            if (LRFlags.Items.CoPReqs.FlagEnabled)
            {
                questRequirements.Values.Where(q => q.iQuestIndex >= 1000).ForEach(q =>
                  {
                      if (!q.s9ClearItem_string.StartsWith("key") && q.u7ClearItemNum > 0)
                      {
                          q.u7ClearItemNum = (q.u7ClearItemNum + 2 - 1) / 2;
                      }
                      if (!q.s9ClearItem2_string.StartsWith("key") && q.u7ClearItemNum2 > 0)
                      {
                          q.u7ClearItemNum2 = (q.u7ClearItemNum2 + 2 - 1) / 2;
                      }
                      if (!q.s9ClearItem3_string.StartsWith("key") && q.u7ClearItemNum3 > 0)
                      {
                          q.u7ClearItemNum3 = (q.u7ClearItemNum3 + 2 - 1) / 2;
                      }
                  });
            }
        }

        public override void Save()
        {
            questRewards.SaveDB3(@"\db\resident\_wdbpack.bin\r_quest.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_quest.wdb");
            questRequirements.SaveDB3(@"\db\resident\_wdbpack.bin\r_quest_ctrl.wdb");
            SetupData.WPDTracking[SetupData.OutputFolder + @"\db\resident\wdbpack.bin"].Add("r_quest_ctrl.wdb");
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
