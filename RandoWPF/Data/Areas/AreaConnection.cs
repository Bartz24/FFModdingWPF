using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class AreaConnection : CSVDataRow
{
    public SeedGenerator Generator { get; set; }
    [RowIndex(0)]
    public string Name { get; set; }

    [RowIndex(1)]
    public string FromAreaName { get; set; }
    [RowIndex(2)]
    public string ToAreaName { get; set; }

    [RowIndex(3)]
    public ItemReq Requirements { get; set; }
    [RowIndex(4)]
    public List<string> Traits { get; set; }
    [RowIndex(5)]
    public int BaseDifficulty { get; set; }

    public AreaConnection(SeedGenerator generator, string[] row) : base(row)
    {
        Generator = generator;
    }
}
