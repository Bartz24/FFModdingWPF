using System.Collections.Generic;

namespace Bartz24.RandoWPF;
public abstract class ArchipelagoData
{
    public abstract void Parse(IDictionary<string, object> data);

    public abstract IDictionary<string, object> ToJsonObj();
}
