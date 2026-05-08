using Bartz24.FF13_2_LR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF13_2;

public class DataStoreZoneScript : DataStoreWDBEntry
{
    public string sClassName { get; set; }
    public string sMethodName { get; set; }
    public int iAdditionalArgCount { get; set; }
    public int iAdditionalArg0 { get; set; }
    public int iAdditionalArg1 { get; set; }
    public int iAdditionalArg2 { get; set; }
    public int iAdditionalArg3 { get; set; }
    public int iAdditionalStringArgCount { get; set; }
    public string sAdditionalStringArg0 { get; set; }
    public string sAdditionalStringArg1 { get; set; }
    public string sAdditionalStringArg2 { get; set; }
}
