using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPRando;
public class JobData : CSVDataRow
{
    [RowIndex(0)]
    public int ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    public JobData(string[] row) : base(row)
    {
    }
}
