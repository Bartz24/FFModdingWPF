using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;

namespace Bartz24.FF13
{
	public class DataStoreCharaSet : DataStoreWDBEntry
	{
		public uint iMemorySizeLimit
		{
			get => Data.ReadUInt(0x0);
			set => Data.SetUInt(0x0, value);
		}
		public uint iVideoMemorySizeLimit
		{
			get => Data.ReadUInt(0x4);
			set => Data.SetUInt(0x4, value);
		}
		public uint sCharaSpecId0_pointer
		{
			get => Data.ReadUInt(0x8 + 0 * 4);
			set => Data.SetUInt(0x8 + 0 * 4, value);
		}
		public string sCharaSpecId0_string { get; set; }
		public uint sCharaSpecId1_pointer
		{
			get => Data.ReadUInt(0x8 + 1 * 4);
			set => Data.SetUInt(0x8 + 1 * 4, value);
		}
		public string sCharaSpecId1_string { get; set; }
		public uint sCharaSpecId2_pointer
		{
			get => Data.ReadUInt(0x8 + 2 * 4);
			set => Data.SetUInt(0x8 + 2 * 4, value);
		}
		public string sCharaSpecId2_string { get; set; }
		public uint sCharaSpecId3_pointer
		{
			get => Data.ReadUInt(0x8 + 3 * 4);
			set => Data.SetUInt(0x8 + 3 * 4, value);
		}
		public string sCharaSpecId3_string { get; set; }
		public uint sCharaSpecId4_pointer
		{
			get => Data.ReadUInt(0x8 + 4 * 4);
			set => Data.SetUInt(0x8 + 4 * 4, value);
		}
		public string sCharaSpecId4_string { get; set; }
		public uint sCharaSpecId5_pointer
		{
			get => Data.ReadUInt(0x8 + 5 * 4);
			set => Data.SetUInt(0x8 + 5 * 4, value);
		}
		public string sCharaSpecId5_string { get; set; }
		public uint sCharaSpecId6_pointer
		{
			get => Data.ReadUInt(0x8 + 6 * 4);
			set => Data.SetUInt(0x8 + 6 * 4, value);
		}
		public string sCharaSpecId6_string { get; set; }
		public uint sCharaSpecId7_pointer
		{
			get => Data.ReadUInt(0x8 + 7 * 4);
			set => Data.SetUInt(0x8 + 7 * 4, value);
		}
		public string sCharaSpecId7_string { get; set; }
		public uint sCharaSpecId8_pointer
		{
			get => Data.ReadUInt(0x8 + 8 * 4);
			set => Data.SetUInt(0x8 + 8 * 4, value);
		}
		public string sCharaSpecId8_string { get; set; }
		public uint sCharaSpecId9_pointer
		{
			get => Data.ReadUInt(0x8 + 9 * 4);
			set => Data.SetUInt(0x8 + 9 * 4, value);
		}
		public string sCharaSpecId9_string { get; set; }
		public uint sCharaSpecId10_pointer
		{
			get => Data.ReadUInt(0x8 + 10 * 4);
			set => Data.SetUInt(0x8 + 10 * 4, value);
		}
		public string sCharaSpecId10_string { get; set; }
		public uint sCharaSpecId11_pointer
		{
			get => Data.ReadUInt(0x8 + 11 * 4);
			set => Data.SetUInt(0x8 + 11 * 4, value);
		}
		public string sCharaSpecId11_string { get; set; }
		public uint sCharaSpecId12_pointer
		{
			get => Data.ReadUInt(0x8 + 12 * 4);
			set => Data.SetUInt(0x8 + 12 * 4, value);
		}
		public string sCharaSpecId12_string { get; set; }
		public uint sCharaSpecId13_pointer
		{
			get => Data.ReadUInt(0x8 + 13 * 4);
			set => Data.SetUInt(0x8 + 13 * 4, value);
		}
		public string sCharaSpecId13_string { get; set; }
		public uint sCharaSpecId14_pointer
		{
			get => Data.ReadUInt(0x8 + 14 * 4);
			set => Data.SetUInt(0x8 + 14 * 4, value);
		}
		public string sCharaSpecId14_string { get; set; }
		public uint sCharaSpecId15_pointer
		{
			get => Data.ReadUInt(0x8 + 15 * 4);
			set => Data.SetUInt(0x8 + 15 * 4, value);
		}
		public string sCharaSpecId15_string { get; set; }
		public uint sCharaSpecId16_pointer
		{
			get => Data.ReadUInt(0x8 + 16 * 4);
			set => Data.SetUInt(0x8 + 16 * 4, value);
		}
		public string sCharaSpecId16_string { get; set; }
		public uint sCharaSpecId17_pointer
		{
			get => Data.ReadUInt(0x8 + 17 * 4);
			set => Data.SetUInt(0x8 + 17 * 4, value);
		}
		public string sCharaSpecId17_string { get; set; }
		public uint sCharaSpecId18_pointer
		{
			get => Data.ReadUInt(0x8 + 18 * 4);
			set => Data.SetUInt(0x8 + 18 * 4, value);
		}
		public string sCharaSpecId18_string { get; set; }
		public uint sCharaSpecId19_pointer
		{
			get => Data.ReadUInt(0x8 + 19 * 4);
			set => Data.SetUInt(0x8 + 19 * 4, value);
		}
		public string sCharaSpecId19_string { get; set; }
		public uint sCharaSpecId20_pointer
		{
			get => Data.ReadUInt(0x8 + 20 * 4);
			set => Data.SetUInt(0x8 + 20 * 4, value);
		}
		public string sCharaSpecId20_string { get; set; }
		public uint sCharaSpecId21_pointer
		{
			get => Data.ReadUInt(0x8 + 21 * 4);
			set => Data.SetUInt(0x8 + 21 * 4, value);
		}
		public string sCharaSpecId21_string { get; set; }
		public uint sCharaSpecId22_pointer
		{
			get => Data.ReadUInt(0x8 + 22 * 4);
			set => Data.SetUInt(0x8 + 22 * 4, value);
		}
		public string sCharaSpecId22_string { get; set; }
		public uint sCharaSpecId23_pointer
		{
			get => Data.ReadUInt(0x8 + 23 * 4);
			set => Data.SetUInt(0x8 + 23 * 4, value);
		}
		public string sCharaSpecId23_string { get; set; }
		public uint sCharaSpecId24_pointer
		{
			get => Data.ReadUInt(0x8 + 24 * 4);
			set => Data.SetUInt(0x8 + 24 * 4, value);
		}
		public string sCharaSpecId24_string { get; set; }
		public uint sCharaSpecId25_pointer
		{
			get => Data.ReadUInt(0x8 + 25 * 4);
			set => Data.SetUInt(0x8 + 25 * 4, value);
		}
		public string sCharaSpecId25_string { get; set; }
		public uint sCharaSpecId26_pointer
		{
			get => Data.ReadUInt(0x8 + 26 * 4);
			set => Data.SetUInt(0x8 + 26 * 4, value);
		}
		public string sCharaSpecId26_string { get; set; }
		public uint sCharaSpecId27_pointer
		{
			get => Data.ReadUInt(0x8 + 27 * 4);
			set => Data.SetUInt(0x8 + 27 * 4, value);
		}
		public string sCharaSpecId27_string { get; set; }
		public uint sCharaSpecId28_pointer
		{
			get => Data.ReadUInt(0x8 + 28 * 4);
			set => Data.SetUInt(0x8 + 28 * 4, value);
		}
		public string sCharaSpecId28_string { get; set; }
		public uint sCharaSpecId29_pointer
		{
			get => Data.ReadUInt(0x8 + 29 * 4);
			set => Data.SetUInt(0x8 + 29 * 4, value);
		}
		public string sCharaSpecId29_string { get; set; }
		public uint sCharaSpecId30_pointer
		{
			get => Data.ReadUInt(0x8 + 30 * 4);
			set => Data.SetUInt(0x8 + 30 * 4, value);
		}
		public string sCharaSpecId30_string { get; set; }
		public uint sCharaSpecId31_pointer
		{
			get => Data.ReadUInt(0x8 + 31 * 4);
			set => Data.SetUInt(0x8 + 31 * 4, value);
		}
		public string sCharaSpecId31_string { get; set; }
		public uint sCharaSpecId32_pointer
		{
			get => Data.ReadUInt(0x8 + 32 * 4);
			set => Data.SetUInt(0x8 + 32 * 4, value);
		}
		public string sCharaSpecId32_string { get; set; }
		public uint sCharaSpecId33_pointer
		{
			get => Data.ReadUInt(0x8 + 33 * 4);
			set => Data.SetUInt(0x8 + 33 * 4, value);
		}
		public string sCharaSpecId33_string { get; set; }
		public uint sCharaSpecId34_pointer
		{
			get => Data.ReadUInt(0x8 + 34 * 4);
			set => Data.SetUInt(0x8 + 34 * 4, value);
		}
		public string sCharaSpecId34_string { get; set; }
		public uint sCharaSpecId35_pointer
		{
			get => Data.ReadUInt(0x8 + 35 * 4);
			set => Data.SetUInt(0x8 + 35 * 4, value);
		}
		public string sCharaSpecId35_string { get; set; }
		public uint sCharaSpecId36_pointer
		{
			get => Data.ReadUInt(0x8 + 36 * 4);
			set => Data.SetUInt(0x8 + 36 * 4, value);
		}
		public string sCharaSpecId36_string { get; set; }
		public uint sCharaSpecId37_pointer
		{
			get => Data.ReadUInt(0x8 + 37 * 4);
			set => Data.SetUInt(0x8 + 37 * 4, value);
		}
		public string sCharaSpecId37_string { get; set; }
		public uint sCharaSpecId38_pointer
		{
			get => Data.ReadUInt(0x8 + 38 * 4);
			set => Data.SetUInt(0x8 + 38 * 4, value);
		}
		public string sCharaSpecId38_string { get; set; }
		public uint sCharaSpecId39_pointer
		{
			get => Data.ReadUInt(0x8 + 39 * 4);
			set => Data.SetUInt(0x8 + 39 * 4, value);
		}
		public string sCharaSpecId39_string { get; set; }
		public uint sCharaSpecId40_pointer
		{
			get => Data.ReadUInt(0x8 + 40 * 4);
			set => Data.SetUInt(0x8 + 40 * 4, value);
		}
		public string sCharaSpecId40_string { get; set; }
		public uint sCharaSpecId41_pointer
		{
			get => Data.ReadUInt(0x8 + 41 * 4);
			set => Data.SetUInt(0x8 + 41 * 4, value);
		}
		public string sCharaSpecId41_string { get; set; }
		public uint sCharaSpecId42_pointer
		{
			get => Data.ReadUInt(0x8 + 42 * 4);
			set => Data.SetUInt(0x8 + 42 * 4, value);
		}
		public string sCharaSpecId42_string { get; set; }
		public uint sCharaSpecId43_pointer
		{
			get => Data.ReadUInt(0x8 + 43 * 4);
			set => Data.SetUInt(0x8 + 43 * 4, value);
		}
		public string sCharaSpecId43_string { get; set; }
		public uint sCharaSpecId44_pointer
		{
			get => Data.ReadUInt(0x8 + 44 * 4);
			set => Data.SetUInt(0x8 + 44 * 4, value);
		}
		public string sCharaSpecId44_string { get; set; }
		public uint sCharaSpecId45_pointer
		{
			get => Data.ReadUInt(0x8 + 45 * 4);
			set => Data.SetUInt(0x8 + 45 * 4, value);
		}
		public string sCharaSpecId45_string { get; set; }
		public uint sCharaSpecId46_pointer
		{
			get => Data.ReadUInt(0x8 + 46 * 4);
			set => Data.SetUInt(0x8 + 46 * 4, value);
		}
		public string sCharaSpecId46_string { get; set; }
		public uint sCharaSpecId47_pointer
		{
			get => Data.ReadUInt(0x8 + 47 * 4);
			set => Data.SetUInt(0x8 + 47 * 4, value);
		}
		public string sCharaSpecId47_string { get; set; }
		public uint sCharaSpecId48_pointer
		{
			get => Data.ReadUInt(0x8 + 48 * 4);
			set => Data.SetUInt(0x8 + 48 * 4, value);
		}
		public string sCharaSpecId48_string { get; set; }
		public uint sCharaSpecId49_pointer
		{
			get => Data.ReadUInt(0x8 + 49 * 4);
			set => Data.SetUInt(0x8 + 49 * 4, value);
		}
		public string sCharaSpecId49_string { get; set; }
		public uint sCharaSpecId50_pointer
		{
			get => Data.ReadUInt(0x8 + 50 * 4);
			set => Data.SetUInt(0x8 + 50 * 4, value);
		}
		public string sCharaSpecId50_string { get; set; }
		public uint sCharaSpecId51_pointer
		{
			get => Data.ReadUInt(0x8 + 51 * 4);
			set => Data.SetUInt(0x8 + 51 * 4, value);
		}
		public string sCharaSpecId51_string { get; set; }
		public uint sCharaSpecId52_pointer
		{
			get => Data.ReadUInt(0x8 + 52 * 4);
			set => Data.SetUInt(0x8 + 52 * 4, value);
		}
		public string sCharaSpecId52_string { get; set; }
		public uint sCharaSpecId53_pointer
		{
			get => Data.ReadUInt(0x8 + 53 * 4);
			set => Data.SetUInt(0x8 + 53 * 4, value);
		}
		public string sCharaSpecId53_string { get; set; }
		public uint sCharaSpecId54_pointer
		{
			get => Data.ReadUInt(0x8 + 54 * 4);
			set => Data.SetUInt(0x8 + 54 * 4, value);
		}
		public string sCharaSpecId54_string { get; set; }
		public uint sCharaSpecId55_pointer
		{
			get => Data.ReadUInt(0x8 + 55 * 4);
			set => Data.SetUInt(0x8 + 55 * 4, value);
		}
		public string sCharaSpecId55_string { get; set; }
		public uint sCharaSpecId56_pointer
		{
			get => Data.ReadUInt(0x8 + 56 * 4);
			set => Data.SetUInt(0x8 + 56 * 4, value);
		}
		public string sCharaSpecId56_string { get; set; }
		public uint sCharaSpecId57_pointer
		{
			get => Data.ReadUInt(0x8 + 57 * 4);
			set => Data.SetUInt(0x8 + 57 * 4, value);
		}
		public string sCharaSpecId57_string { get; set; }
		public uint sCharaSpecId58_pointer
		{
			get => Data.ReadUInt(0x8 + 58 * 4);
			set => Data.SetUInt(0x8 + 58 * 4, value);
		}
		public string sCharaSpecId58_string { get; set; }
		public uint sCharaSpecId59_pointer
		{
			get => Data.ReadUInt(0x8 + 59 * 4);
			set => Data.SetUInt(0x8 + 59 * 4, value);
		}
		public string sCharaSpecId59_string { get; set; }
		public uint sCharaSpecId60_pointer
		{
			get => Data.ReadUInt(0x8 + 60 * 4);
			set => Data.SetUInt(0x8 + 60 * 4, value);
		}
		public string sCharaSpecId60_string { get; set; }
		public uint sCharaSpecId61_pointer
		{
			get => Data.ReadUInt(0x8 + 61 * 4);
			set => Data.SetUInt(0x8 + 61 * 4, value);
		}
		public string sCharaSpecId61_string { get; set; }
		public uint sCharaSpecId62_pointer
		{
			get => Data.ReadUInt(0x8 + 62 * 4);
			set => Data.SetUInt(0x8 + 62 * 4, value);
		}
		public string sCharaSpecId62_string { get; set; }
		public uint sCharaSpecId63_pointer
		{
			get => Data.ReadUInt(0x8 + 63 * 4);
			set => Data.SetUInt(0x8 + 63 * 4, value);
		}
		public string sCharaSpecId63_string { get; set; }


		public override int GetDefaultLength()
		{
			return 0x10C;
		}

		public void SetCharaSpecs(List<string> list)
		{
			if (list.Count > 64)
				throw new Exception("Too many Chara Specs being added");
			for (int i = 0; i < 64; i++)
			{
				if (i < list.Count)
					this.SetPropValue($"sCharaSpecId{i}_string", list[i]);
				else
					this.SetPropValue($"sCharaSpecId{i}_string", "");
			}
		}

		public List<string> GetCharaSpecs()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < 64; i++)
			{
				list.Add(this.GetPropValue<string>($"sCharaSpecId{i}_string"));
			}
			return list.Where(s => s != "").ToList();
		}
	}
}