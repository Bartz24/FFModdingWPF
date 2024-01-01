using Bartz24.RandoWPF;
using System.Collections.Generic;

namespace LRRando;

public class MusicData : CSVDataRow
{
    [RowIndex(0)]
    public string Path { get; set; }
    [RowIndex(1)]
    public List<string> Traits { get; set; }
    public MusicData(string[] row) : base(row)
    {
    }
}