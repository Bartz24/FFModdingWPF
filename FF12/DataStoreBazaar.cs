using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreBazaar : DataStore
    {
        public ushort Item1ID
        {
            get { return Data.ReadUShort(0x4); }
            set { Data.SetUShort(0x4, value); }
        }
        public ushort Item1Amount
        {
            get { return Data.ReadUShort(0x6); }
            set { Data.SetUShort(0x6, value); }
        }
        public ushort Item2ID
        {
            get { return Data.ReadUShort(0x8); }
            set { Data.SetUShort(0x8, value); }
        }
        public ushort Item2Amount
        {
            get { return Data.ReadUShort(0xA); }
            set { Data.SetUShort(0xA, value); }
        }
        public ushort Item3ID
        {
            get { return Data.ReadUShort(0xC); }
            set { Data.SetUShort(0xC, value); }
        }
        public ushort Item3Amount
        {
            get { return Data.ReadUShort(0xE); }
            set { Data.SetUShort(0xE, value); }
        }
        public uint Cost
        {
            get { return Data.ReadUInt(0x10); }
            set { Data.SetUInt(0x10, value); }
        }
        public BazaarType Flag
        {
            get { return (BazaarType)Data.ReadByte(0x14); }
            set { Data.SetByte(0x14, (byte)value); }
        }
        public ushort Ingredient1ID
        {
            get { return Data.ReadUShort(0x16); }
            set { Data.SetUShort(0x16, value); }
        }
        public ushort Ingredient1Amount
        {
            get { return Data.ReadUShort(0x18); }
            set { Data.SetUShort(0x18, value); }
        }
        public ushort Ingredient2ID
        {
            get { return Data.ReadUShort(0x1A); }
            set { Data.SetUShort(0x1A, value); }
        }
        public ushort Ingredient2Amount
        {
            get { return Data.ReadUShort(0x1C); }
            set { Data.SetUShort(0x1C, value); }
        }
        public ushort Ingredient3ID
        {
            get { return Data.ReadUShort(0x1E); }
            set { Data.SetUShort(0x1E, value); }
        }
        public ushort Ingredient3Amount
        {
            get { return Data.ReadUShort(0x20); }
            set { Data.SetUShort(0x20, value); }
        }
        public override int GetDefaultLength()
        {
            return 0x24;
        }
    }
}
