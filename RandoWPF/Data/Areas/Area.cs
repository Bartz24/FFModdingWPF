namespace Bartz24.RandoWPF;
public class Area : CSVDataRow
{
    [RowIndex(0)]
    public string Name { get; set; }

    public Area(string[] row) : base(row)
    {
    }
}
