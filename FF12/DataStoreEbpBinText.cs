using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.FF12;
public class DataStoreEbpBinText : DataStoreBinText
{
    public DataStoreEbpBinText() : base()
    {
    }
    public override void Load(string path)
    {
        LoadFromLines(Tools.ReadEbpText(path));
    }

    public override void Save(string path)
    {
        Tools.WriteEbpText(path, GetLinesData().ToArray());
    }
}
