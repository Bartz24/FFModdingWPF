using Bartz24.Data;
using Bartz24.FF13_2_LR;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.FF13_2
{
	public class DataStoreBtSTable : DataStoreDB3SubEntry
	{
		public int iBattleSituationId0 { get; set; }
		public int iBattleSituationId1 { get; set; }
		public int iBattleSituationId2 { get; set; }
		public int iBattleSituationId3 { get; set; }
		public int iBattleSituationId4 { get; set; }
		public int iBattleSituationId5 { get; set; }
		public int iBattleSituationId6 { get; set; }
		public int iBattleSituationId7 { get; set; }
		public int iBattleSituationId8 { get; set; }
		public int iBattleSituationId9 { get; set; }
		public int iBattleSituationId10 { get; set; }
		public int iBattleSituationId11 { get; set; }
		public int uiDespair0 { get; set; }
		public int uiDespair1 { get; set; }
		public int uiDespair2 { get; set; }
		public int uiDespair3 { get; set; }
		public int uiDespair4 { get; set; }
		public int uiDespair5 { get; set; }
		public int uiDespair6 { get; set; }
		public int uiDespair7 { get; set; }
		public int uiDespair8 { get; set; }
		public int uiDespair9 { get; set; }
		public int uiDespair10 { get; set; }
		public int uiDespair11 { get; set; }
		public int sAccentTable_pointer { get; set; }
		public string sAccentTable_string { get; set; }
		public int sNext_pointer { get; set; }
		public string sNext_string { get; set; }
		public int i8BattleSituationRandom0 { get; set; }
		public int i8BattleSituationRandom1 { get; set; }
		public int i8BattleSituationRandom2 { get; set; }
		public int i8BattleSituationRandom3 { get; set; }
		public int i8BattleSituationRandom4 { get; set; }
		public int i8BattleSituationRandom5 { get; set; }
		public int i8BattleSituationRandom6 { get; set; }
		public int i8BattleSituationRandom7 { get; set; }
		public int i8BattleSituationRandom8 { get; set; }
		public int i8BattleSituationRandom9 { get; set; }
		public int i8BattleSituationRandom10 { get; set; }
		public int i8BattleSituationRandom11 { get; set; }
		public int u4EntryRepeatNum0 { get; set; }
		public int u4EntryRepeatNum1 { get; set; }
		public int u4EntryRepeatNum2 { get; set; }
		public int u4EntryRepeatNum3 { get; set; }
		public int u4EntryRepeatNum4 { get; set; }
		public int u4EntryRepeatNum5 { get; set; }
		public int u4EntryRepeatNum6 { get; set; }
		public int u4EntryRepeatNum7 { get; set; }
		public int u4EntryRepeatNum8 { get; set; }
		public int u4EntryRepeatNum9 { get; set; }
		public int u4EntryRepeatNum10 { get; set; }
		public int u4EntryRepeatNum11 { get; set; }
		public int i8AccentCount { get; set; }
		public int i8AccentProbability { get; set; }
		public int u1QuickPop { get; set; }
		public int i5attribute { get; set; }
		public int i8flag { get; set; }
		public List<int> GetBattleIDs()
		{
			List<int> list = new List<int>();
			for (int i = 0; i <= 11; i++)
			{
				list.Add(this.GetPropValue<int>($"iBattleSituationId{i}"));
			}
			return list.Where(i => i > 0).ToList();
		}
        public void SetBattleIDs(List<int> list)
        {
            for (int i = 0; i <= 11; i++)
            {
                this.SetPropValue($"iBattleSituationId{i}", i >= list.Count ? 0 : list[i]);
            }
        }
    }
}
