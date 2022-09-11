using Bartz24.Data;

namespace Bartz24.FF13
{
    public enum CrystariumType : byte
    {
        HP = 1,
        Strength = 2,
        Magic = 3,
        Accessory = 4,
        ATBLevel = 5,
        Ability = 6,
        RoleLevel = 7,
        Unknown = 0
    }

    public enum Role : byte
    {
        None = 0,
        Commando = 2,
        Ravager = 3,
        Sentinel = 1,
        Synergist = 4,
        Medic = 6,
        Saboteur = 5
    }
    public class DataStoreCrystarium : DataStoreWDBEntry
    {
        public uint iCPCost
        {
            get { return Data.ReadUInt(0x0); }
            set { Data.SetUInt(0x0, value); }
        }

        public string sAbility_string { get; set; }

        public uint sAbility_pointer
        {
            get { return Data.ReadUInt(0x4); }
            set { Data.SetUInt(0x4, value); }
        }

        public ushort iValue
        {
            get { return Data.ReadUShort(0x8); }
            set { Data.SetUShort(0x8, value); }
        }

        public CrystariumType iType
        {
            get { return (CrystariumType)Data.ReadByte(0xA); }
            set { Data.SetByte(0xA, (byte)value); }
        }

        public byte iStage
        {
            get { return (byte)(Data.ReadByte(0xB) / 0x10); }
            set { Data.SetByte(0xB, (byte)(value * 0x10 + (byte)iRole)); }
        }

        public Role iRole
        {
            get { return (Role)(Data.ReadByte(0xB) % 0x10); }
            set { Data.SetByte(0xB, (byte)(iStage * 0x10 + (byte)value)); }
        }

        public bool IsSideNode
        {
            get { return ID.Substring(ID.Length - 4) != "0000"; }
        }

        public override int GetDefaultLength()
        {
            return 0xC;
        }

        public void SwapStats(DataStoreCrystarium other)
        {
            CrystariumType type = other.iType;
            ushort value = other.iValue;
            other.iType = this.iType;
            other.iValue = this.iValue;
            this.iType = type;
            this.iValue = value;
        }

        public void SwapStatsAbilities(DataStoreCrystarium other)
        {
            SwapStats(other);

            string abilityName = other.sAbility_string;
            other.sAbility_string = this.sAbility_string;
            this.sAbility_string = abilityName;
        }
    }
}
