using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bartz24.FF13Series;

public class DataStoreZTRText
{
    private Dictionary<string, string> Data;
    private string ztrPath;

    private const string SEPARATOR = "|:|";

    public string this[string id]
    {
        get => Data[id];
        set => Data[id] = value;
    }
    public List<string> Keys => Data.Keys.ToList();
    public List<string> Values => Data.Values.ToList();

    public void Add(string id, string data)
    {
        Data.Add(id, data);
    }
    public void Load(string game, string path, string novaPath)
    {
        Nova.ConvertZTRToTXT(game, path, novaPath);
        string txtPath = path.Substring(0, path.LastIndexOf(".ztr")) + ".txt";

        Data = new Dictionary<string, string>();

        string[] lines = File.ReadAllLines(txtPath);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            int sepIndex = line.IndexOf(SEPARATOR, StringComparison.Ordinal);
            if (sepIndex < 0)
            {
                continue;
            }

            string key = line.Substring(0, sepIndex).Trim();
            string value = line.Substring(sepIndex + SEPARATOR.Length).Trim();

            if (!string.IsNullOrEmpty(key))
            {
                Data[key] = value;
            }
        }
    }

    public void Save(string game, string path, string novaPath)
    {
        string txtPath = path.Substring(0, path.LastIndexOf(".ztr")) + ".txt";
        List<string> lines = new();

        // Write in the new, simpler format: $key |:| value
        List<string> keysSorted = Data.Keys.OrderBy(s => s, StringComparer.Ordinal).ToList();
        foreach (var key in keysSorted)
        {
            // Avoid introducing the separator inside values; write as-is
            lines.Add($"{key} {SEPARATOR} {Data[key]}");
        }

        File.WriteAllLines(txtPath, lines);

        Nova.ConvertTXTToZTR(game, path, novaPath);
    }
}
