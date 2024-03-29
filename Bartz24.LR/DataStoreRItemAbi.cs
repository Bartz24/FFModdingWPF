﻿using Bartz24.FF13_2_LR;

namespace Bartz24.LR;

public class DataStoreRItemAbi : DataStoreDB3SubEntry
{
    public int sAbilityId_pointer { get; set; }
    public string sAbilityId_string { get; set; }
    public int iPower { get; set; }
    public int sPasvAbility_pointer { get; set; }
    public string sPasvAbility_string { get; set; }
    public int u4Lv { get; set; }
    public int u1Fixed { get; set; }
    public int i8AtbDec { get; set; }
}
