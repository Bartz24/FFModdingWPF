using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12
{
    public class DataStoreEquip : DataStore
    {
        public bool Unsellable
        {
            get { return Data.ReadBinary(0x07, 1); }
            set { Data.SetBinary(0x07, 1, value); }
        }
        public bool HitsFlying
        {
            get { return Data.ReadBinary(0x07, 2); }
            set { Data.SetBinary(0x07, 2, value); }
        }
        public bool NoLicenseNeeded
        {
            get { return Data.ReadBinary(0x07, 4); }
            set { Data.SetBinary(0x07, 4, value); }
        }
        private bool IsWeapon
        {
            get { return !IsAccessory && !IsArmor && !IsOffhand; }
        }
        private bool IsArmor
        {
            get { return Data.ReadBinary(0x07, 6); }
        }
        private bool IsOffhand
        {
            get { return Data.ReadBinary(0x07, 5) && !IsArmor; }
        }
        private bool IsAccessory
        {
            get { return Data.ReadBinary(0x07, 7); }
        }
        public EquipType Type
        {
            get
            {
                if (IsWeapon)
                    return EquipType.Weapon;
                else if (IsOffhand)
                    return EquipType.Offhand;
                else if (IsArmor)
                    return EquipType.Armor;
                else
                    return EquipType.Accessory;
            }
            set
            {
                switch (value)
                {
                    case EquipType.Weapon:
                        Data.SetBinary(0x07, 5, false);
                        Data.SetBinary(0x07, 6, false);
                        Data.SetBinary(0x07, 7, false);
                        break;
                    case EquipType.Offhand:
                        Data.SetBinary(0x07, 5, true);
                        Data.SetBinary(0x07, 6, false);
                        Data.SetBinary(0x07, 7, false);
                        break;
                    case EquipType.Armor:
                        Data.SetBinary(0x07, 5, false);
                        Data.SetBinary(0x07, 6, true);
                        Data.SetBinary(0x07, 7, false);
                        break;
                    case EquipType.Accessory:
                        Data.SetBinary(0x07, 5, false);
                        Data.SetBinary(0x07, 6, false);
                        Data.SetBinary(0x07, 7, true);
                        break;
                }
            }
        }
        public EquipCategory Category
        {
            get { return (EquipCategory) Data.ReadByte(0x9); }
            set { Data.SetByte(0x9, (byte)value); }
        }
        public byte Metal
        {
            get { return Data.ReadByte(0x10); }
            set { Data.SetByte(0x10, value); }
        }
        public ushort Price
        {
            get { return Data.ReadUShort(0x12); }
            set { Data.SetUShort(0x12, value); }
        }
        public uint AttributeOffset
        {
            get { return Data.ReadUInt(0x28) / 0x18; }
            set { Data.SetUInt(0x28, value * 0x18); }
        }
        public override int GetDefaultLength()
        {
            return 0x34;
        }
    }
}
