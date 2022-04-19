using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreTreasure : DataStore
    {
        public bool RandomizeGil
        {
            get { return Data.ReadBinary(0x08, 6); }
            set { Data.SetBinary(0x08, 6, value); }
        }
        public byte Respawn
        {
            get { return Data.ReadByte(0x09); }
            set { Data.SetByte(0x09, value); }
        }
        public byte SpawnChance
        {
            get { return Data.ReadByte(0x0A); }
            set { Data.SetByte(0x0A, value); }
        }
        public byte GilChance
        {
            get { return Data.ReadByte(0x0B); }
            set { Data.SetByte(0x0B, value); }
        }
        public ushort CommonItem1ID
        {
            get { return Data.ReadUShort(0x0C); }
            set { Data.SetUShort(0x0C, value); }
        }
        public ushort CommonItem2ID
        {
            get { return Data.ReadUShort(0x0E); }
            set { Data.SetUShort(0x0E, value); }
        }
        public ushort RareItem1ID
        {
            get { return Data.ReadUShort(0x10); }
            set { Data.SetUShort(0x10, value); }
        }
        public ushort RareItem2ID
        {
            get { return Data.ReadUShort(0x12); }
            set { Data.SetUShort(0x12, value); }
        }
        public ushort GilCommon
        {
            get { return Data.ReadUShort(0x14); }
            set { Data.SetUShort(0x14, value); }
        }
        public ushort GilRare
        {
            get { return Data.ReadUShort(0x16); }
            set { Data.SetUShort(0x16, value); }
        }
        public override int GetDefaultLength()
        {
            return 0x18;
        }
    }
}
