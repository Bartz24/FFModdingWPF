using System;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

public class FileType
{
    public string Extension { get; }
    public string Description { get; }
    public bool AllowMultiple { get; }
    public Action<IReadOnlyList<string>> Load { get; }

    public FileType(string extension, string description, bool allowMultiple, Action<IReadOnlyList<string>> load)
    {
        Extension = "." + extension.ToLower().Trim('.');
        Description = description;
        AllowMultiple = allowMultiple;
        Load = load;
    }
}
