using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreDB3StringArray : DataStoreDB3Entry
	{
		public int data { get; set; }
		public int higher_pointer { get; set; }
		public int lower_pointer { get; set; }
		public string higher_name { get; set; }
		public string lower_name { get; set; }
		public int strArrayListIndex { get; set; }
	}
}
