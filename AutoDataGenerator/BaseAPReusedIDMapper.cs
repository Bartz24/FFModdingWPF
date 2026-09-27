using System.Text.RegularExpressions;

namespace AutoDataGenerator;

public abstract class BaseAPReusedIDMapper
{
    private readonly Dictionary<string, int> codes = new();
    private readonly HashSet<int> usedCodes = new();
    private int nextCode = 1;

    protected abstract Regex EntryPattern { get; }

    protected BaseAPReusedIDMapper()
    {
    }

    public void LoadAPScript(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach ((string id, int code) in ParseExisting(File.ReadAllText(path)))
        {
            codes[id] = code;
            usedCodes.Add(code);

            // Never go back over an existing code, even one that is now free.
            nextCode = Math.Max(nextCode, code + 1);
        }
    }

    public int GetAPCode(string id)
    {
        // If the ID already has a code, return it.
        if (codes.TryGetValue(id, out int code))
        {
            return code;
        }

        while (!usedCodes.Add(nextCode))
        {
            nextCode++;
        }

        codes[id] = nextCode;
        return nextCode;
    }

    protected abstract string ParseID(Match entry);

    protected virtual int ParseAPCode(Match entry)
    {
        return int.Parse(entry.Groups["code"].Value);
    }

    protected virtual IEnumerable<(string ID, int Code)> ParseExisting(string content)
    {
        foreach (Match entry in EntryPattern.Matches(content))
        {
            string id = ParseID(entry);
            if (id != null)
            {
                yield return (id, ParseAPCode(entry));
            }
        }
    }

    protected static string GetField(Match entry, string name)
    {
        Match field = Regex.Match(entry.Value, $@"\b{name}\s*=\s*""?(?<value>[^"",\r\n)]*)""?");
        return field.Success ? field.Groups["value"].Value : null;
    }
}
