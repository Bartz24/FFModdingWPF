using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreRItemAbi : DataStoreDB3SubEntry
	{
		public int sAbilityId_pointer { get; set; }
		public string sAbilityId_string { get; set; }
		public int iPower { get; set; }
		public int sPasvAbility_pointer { get; set; }
		public string sPasvAbility_string { get; set; }
		public int u4Lv { get; set; }
		public int u1Fixed { get; set; }
		public int i8AtbDec { get; set; }
	}
}
