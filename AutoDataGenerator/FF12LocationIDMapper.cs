using System.Text.RegularExpressions;

namespace AutoDataGenerator;

public class FF12LocationIDMapper : BaseAPReusedIDMapper
{
    protected override Regex EntryPattern { get; } = new(@"FF12OpenWorldLocationData\([^)]*\)", RegexOptions.Compiled);

    protected override string ParseID(Match entry)
    {
        string type = GetField(entry, "type");
        string strId = GetField(entry, "str_id");
        if (type == null || strId == null)
        {
            return null;
        }

        return $"{strId}|{GetField(entry, "secondary_index") ?? "0"}";
    }

    protected override int ParseAPCode(Match entry)
    {
        return int.Parse(GetField(entry, "address"));
    }
}
