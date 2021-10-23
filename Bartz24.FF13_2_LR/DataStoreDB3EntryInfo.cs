using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreDB3EntryInfo : DataStoreDB3SubEntry
	{
		public int address { get; set; }
		public int size { get; set; }
		public int padding01 { get; set; }
		public int padding02 { get; set; }
	}
}
