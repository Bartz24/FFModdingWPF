namespace FF13Rando
{
    public class Enums
    {
        public enum SynthesisGroup : byte
        {
            None = 0xC4,
            HighHPPowerSurge = 0x82,
            LowHPPowerSurge = 0x84,
            PhysicalWall = 0x86,
            MagicWall = 0x88,
            //PhysicalProofing = 0x8A, Wall but worse
            //MagicProofing = 0x8C, Wall but worse
            DamageWall = 0x8E,
            //DamageProofing = 0x90, Wall but worse
            EtherealMantle = 0x92,
            MagicDamper = 0x94,
            FireDamage = 0x96,
            IceDamage = 0x98,
            LightningDamage = 0x9A,
            WaterDamage = 0x9C,
            WindDamage = 0x9E,
            EarthDamage = 0xA0,
            DebraveDuration = 0xA2,
            DefaithDuration = 0xA4,
            DeprotectDuration = 0xA6,
            DeshellDuration = 0xA8,
            SlowDuration = 0xAA,
            PoisonDuration = 0xAC,
            ImperilDuration = 0xAE,
            CurseDuration = 0xB0,
            PainDuration = 0xB2,
            FogDuration = 0xB4,
            DazeDuration = 0xB6,
            ATBRate = 0xBC,
            RIC_GestaltTPBoost = 0xBE,
            BuffDuration = 0xC0,
            VampiricStrike = 0xC2
        }
    }
}
