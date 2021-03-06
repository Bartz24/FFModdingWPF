using Bartz24.Data;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Bartz24.FF13
{
	public class DataStoreItem : DataStoreWDBEntry
	{
		public uint sItemNameStringId_pointer
		{
			get { return Data.ReadUInt(0x0); }
			set { Data.SetUInt(0x0, value); }
		}
		public string sItemNameStringId_string { get; set; }
		public uint sHelpStringId_pointer
		{
			get { return Data.ReadUInt(0x4); }
			set { Data.SetUInt(0x4, value); }
		}
		public string sHelpStringId_string { get; set; }
		public uint sScriptId_pointer
		{
			get { return Data.ReadUInt(0x8); }
			set { Data.SetUInt(0x8, value); }
		}
		public string sScriptId_string { get; set; }

        public override int GetDefaultLength()
        {
            return 0x24;
        }
    }
}
