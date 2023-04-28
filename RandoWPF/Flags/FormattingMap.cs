using System;
using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class FormattingMap
{
    private readonly Dictionary<string, Func<object, string>> map = new();

    public void AddMapping(string name, Func<object, string> replace)
    {
        map.Add(name, replace);
    }

    public string Apply(string format, object obj)
    {
        map.Keys.ToList().ForEach(key => format = format.Replace($"${{{key}}}", map[key].Invoke(obj)));
        return format;
    }
}
