using System.Collections.Generic;

namespace Bartz24.Docs;

public class TableCellMultiple
{
    public List<string> Elements { get; set; }
    public TableCellMultiple(List<string> elements)
    {
        Elements = elements;
    }

    public override string ToString()
    {
        return $"<div style=\"display: flex; justify-content: flex-end\">{string.Join("", Elements)}</div>";
    }
}
