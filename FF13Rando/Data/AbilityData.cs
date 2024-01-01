using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando;

public class AbilityData : CSVDataRow
{
    [RowIndex(0)]
    public string ID { get; set; }
    [RowIndex(1)]
    public string Name { get; set; }
    public Role Role { get; set; }
    public List<string> Characters { get; set; }
    [RowIndex(4)]
    public List<string> Traits { get; set; }
    [RowIndex(5)]
    public ItemReq Requirements { get; set; }
    [RowIndex(6)]
    public List<string> Incompatible { get; set; }

    private readonly string[] chars = { "lightning", "fang", "hope", "sazh", "snow", "vanille" };
    public AbilityData(string[] row) : base(row)
    {
        Role = row[2] == "" ? Role.None : Enum.GetValues(typeof(Role)).Cast<Role>().First(r => r.ToString().Substring(0, 3).ToUpper() == row[2]);
        Characters = row[3] == "" ? chars.ToList() : row[3].ToCharArray().Select(c => ToCharName(c)).ToList();
    }
    public string ToCharName(char id)
    {
        return id switch
        {
            'l' => "lightning",
            's' => "snow",
            'z' => "sazh",
            'h' => "hope",
            'f' => "fang",
            'v' => "vanille",
            _ => "",
        };
    }
}
