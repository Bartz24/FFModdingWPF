using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreRPassiveAbility : DataStoreDB3SubEntry
	{
		public int sStringResId_pointer { get; set; }
		public string sStringResId_string { get; set; }
		public int sInfoStResId_pointer { get; set; }
		public string sInfoStResId_string { get; set; }
		public int u8StatusModKind0 { get; set; }
		public int u8StatusModKind1 { get; set; }
		public int u4StatusModType { get; set; }
	}
}
