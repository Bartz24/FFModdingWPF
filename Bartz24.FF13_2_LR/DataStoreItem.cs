using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2_LR
{
	public class DataStoreItem : DataStoreDB3SubEntry
	{
		public int sItemNameStringId_pointer { get; set; }
		public string sItemNameStringId_string { get; set; }
		public int sHelpStringId_pointer { get; set; }
		public string sHelpStringId_string { get; set; }
		public int sScriptId_pointer { get; set; }
		public string sScriptId_string { get; set; }
		public int uGpCost { get; set; }
		public int uPurchasePrice { get; set; }
		public int uSellPrice { get; set; }
		public int uItemNum { get; set; }
		public int sRequiredItem_pointer { get; set; }
		public string sRequiredItem_string { get; set; }
		public int sNextItem_pointer { get; set; }
		public string sNextItem_string { get; set; }
		public int iNextItemCount { get; set; }
		public int u8MenuIcon { get; set; }
		public int u8ItemCategory { get; set; }
		public int u1IsUseBattleMenu { get; set; }
		public int u1IsSellable { get; set; }
		public int u1OnlyOne { get; set; }
		public int u1IsTargetFill { get; set; }
		public int u1IsPresentable { get; set; }
		public int u1IsPermanent { get; set; }
		public int u16SortAllByKCategory { get; set; }
		public int u16SortCategoryByCategory { get; set; }
		public int u16SortCategoryByGraphics { get; set; }
		public int u16Padding { get; set; }
	}
}
