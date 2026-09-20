using System.Collections.Generic;
using System.Linq;

namespace Bartz24.RandoWPF;

public class FileDragInfo
{
    public IReadOnlyList<string> Paths { get; }
    public string Path => Paths.FirstOrDefault();
    public FileType Type { get; }
    public bool IsSupported => Type != null;

    public FileDragInfo(IReadOnlyList<string> paths, FileType type)
    {
        Paths = paths;
        Type = type;
    }
}
