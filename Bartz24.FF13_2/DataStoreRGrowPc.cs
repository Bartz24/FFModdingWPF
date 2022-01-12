using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;
using Bartz24.FF13_2_LR;

namespace Bartz24.FF13_2
{
	public class DataStoreRGrowPc : DataStoreDB3SubEntry
	{
		public int sAbilityId_pointer { get; set; }
		public string sAbilityId_string { get; set; }
		public int u4Role { get; set; }
		public int u7Lv { get; set; }
		public int u3Kind { get; set; }
		public int u16Value { get; set; }
	}
}
