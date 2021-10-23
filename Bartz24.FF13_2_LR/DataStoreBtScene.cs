using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreBtScene : DataStoreDB3SubEntry
	{
		public int sDebugStr_pointer { get; set; }
		public string sDebugStr_string { get; set; }
		public int sMapSetId0_pointer { get; set; }
		public string sMapSetId0_string { get; set; }
		public int sMapSetId1_pointer { get; set; }
		public string sMapSetId1_string { get; set; }
		public int sBtSpaceId_pointer { get; set; }
		public string sBtSpaceId_string { get; set; }
		public int sFinCondEntry0_pointer { get; set; }
		public string sFinCondEntry0_string { get; set; }
		public int sDropItem0_pointer { get; set; }
		public string sDropItem0_string { get; set; }
		public int sEntryBtChSpec1_pointer { get; set; }
		public string sEntryBtChSpec1_string { get; set; }
		public int fForcePopPositionX { get; set; }
		public int fForcePopPositionY { get; set; }
		public int fForcePopPositionZ { get; set; }
		public int s10BtSceStrResId_pointer { get; set; }
		public string s10BtSceStrResId_string { get; set; }
		public int s10ExEntrySheet0_pointer { get; set; }
		public string s10ExEntrySheet0_string { get; set; }
		public int u1NoOpening { get; set; }
		public int u1NoEnding { get; set; }
		public int u1Seamless { get; set; }
		public int u1NoDispResult { get; set; }
		public int u1ContFromPrev { get; set; }
		public int u1ContToNext { get; set; }
		public int u6TutNo { get; set; }
		public int u1Flag0 { get; set; }
		public int u1Flag2 { get; set; }
		public int u1Flag3 { get; set; }
		public int u16ResltTimeArg { get; set; }
		public int u4FinCond0 { get; set; }
		public int u4FinType { get; set; }
		public int u1RareDrop0 { get; set; }
		public int u4BtChInitSetNum { get; set; }
		public int i16FinCondArg0 { get; set; }
		public int u8NumDrop0 { get; set; }
		public int s8PartyEntryId_pointer { get; set; }
		public string s8PartyEntryId_string { get; set; }
		public int u16DropProb0 { get; set; }
		public int s8BtChEntryId_pointer { get; set; }
		public string s8BtChEntryId_string { get; set; }
		public int u1EvenNoShift { get; set; }
		public int u1ForceFindPlayer { get; set; }
		public int u1NoMove { get; set; }
		public int u1NoEnhanceDeathZone { get; set; }
		public int s10EntryBtChSpec2_pointer { get; set; }
		public string s10EntryBtChSpec2_string { get; set; }
		public int s10EntryBtChSpec3_pointer { get; set; }
		public string s10EntryBtChSpec3_string { get; set; }
		public int s10EntryBtChSpec4_pointer { get; set; }
		public string s10EntryBtChSpec4_string { get; set; }
		public int s8EntryBtChSpec5_pointer { get; set; }
		public string s8EntryBtChSpec5_string { get; set; }
		public int s8EntryBtChSpec6_pointer { get; set; }
		public string s8EntryBtChSpec6_string { get; set; }
		public int s8EntryBtChSpec7_pointer { get; set; }
		public string s8EntryBtChSpec7_string { get; set; }
		public int s8EntryBtChSpec8_pointer { get; set; }
		public string s8EntryBtChSpec8_string { get; set; }
		public int s8EntryBtChSpec9_pointer { get; set; }
		public string s8EntryBtChSpec9_string { get; set; }
		public int s8EntryBtChSpec10_pointer { get; set; }
		public string s8EntryBtChSpec10_string { get; set; }
		public int s8BgmResourceId_pointer { get; set; }
		public string s8BgmResourceId_string { get; set; }
		public int s8DominantBgmResourceId_pointer { get; set; }
		public string s8DominantBgmResourceId_string { get; set; }

		public override Dictionary<string, int> GetStringArrayMapping()
		{
			Dictionary<string, int> mapping = new Dictionary<string, int>();
			mapping.Add(nameof(s10BtSceStrResId_pointer), 0);
			mapping.Add(nameof(s10ExEntrySheet0_pointer), 1);
			mapping.Add(nameof(s8PartyEntryId_pointer), 2);
			mapping.Add(nameof(s8BtChEntryId_pointer), 3);
			mapping.Add(nameof(s10EntryBtChSpec2_pointer), 4);
			mapping.Add(nameof(s10EntryBtChSpec3_pointer), 5);
			mapping.Add(nameof(s10EntryBtChSpec4_pointer), 6);
			mapping.Add(nameof(s8EntryBtChSpec5_pointer), 7);
			mapping.Add(nameof(s8EntryBtChSpec6_pointer), 8);
			mapping.Add(nameof(s8EntryBtChSpec7_pointer), 9);
			mapping.Add(nameof(s8EntryBtChSpec8_pointer), 10);
			mapping.Add(nameof(s8EntryBtChSpec9_pointer), 11);
			mapping.Add(nameof(s8EntryBtChSpec10_pointer), 12);
			mapping.Add(nameof(s8BgmResourceId_pointer), 13);
			mapping.Add(nameof(s8DominantBgmResourceId_pointer), 14);
			return mapping;
		}

		public void SetCharSpecs(List<string> list)
		{
			sEntryBtChSpec1_string = "";
			s10EntryBtChSpec2_string = "";
			s10EntryBtChSpec3_string = "";
			s10EntryBtChSpec4_string = "";
			s8EntryBtChSpec5_string = "";
			s8EntryBtChSpec6_string = "";
			s8EntryBtChSpec7_string = "";
			s8EntryBtChSpec8_string = "";
			s8EntryBtChSpec9_string = "";
			s8EntryBtChSpec10_string = "";
			if (list.Count > 0)
				sEntryBtChSpec1_string = list[0];
			if (list.Count > 1)
				s10EntryBtChSpec2_string = list[1];
			if (list.Count > 2)
				s10EntryBtChSpec3_string = list[2];
			if (list.Count > 3)
				s10EntryBtChSpec4_string = list[3];
			if (list.Count > 4)
				s8EntryBtChSpec5_string = list[4];
			if (list.Count > 5)
				s8EntryBtChSpec6_string = list[5];
			if (list.Count > 6)
				s8EntryBtChSpec7_string = list[6];
			if (list.Count > 7)
				s8EntryBtChSpec8_string = list[7];
			if (list.Count > 8)
				s8EntryBtChSpec9_string = list[8];
			if (list.Count > 9)
				s8EntryBtChSpec10_string = list[9];
		}

		public List<string> GetCharSpecs()
		{
			List<string> list = new List<string>();
			list.Add(sEntryBtChSpec1_string);
			list.Add(s10EntryBtChSpec2_string);
			list.Add(s10EntryBtChSpec3_string);
			list.Add(s10EntryBtChSpec4_string);
			list.Add(s8EntryBtChSpec5_string);
			list.Add(s8EntryBtChSpec6_string);
			list.Add(s8EntryBtChSpec7_string);
			list.Add(s8EntryBtChSpec8_string);
			list.Add(s8EntryBtChSpec9_string);
			list.Add(s8EntryBtChSpec10_string);
			return list.Where(s => s != "").ToList();
		}
	}
}
