using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bartz24.FF12;

public class DataStoreBinText
{
    private Dictionary<int, StringData> Data { get; set; } = new ();

    public string Format { get; set; }

    public DataStoreBinText()
    {
    }

    public StringData this[int id]
    {
        get => Data[id];
        set => Data[id] = value;
    }

    public List<int> Keys => Data.Keys.ToList();
    public List<StringData> Values => Data.Values.ToList();

    public void Add(int id, StringData data)
    {
        Data.Add(id, data);
    }
    public virtual void Load(string path)
    {
        Tools.ConvertBinToTxt(path);
        string[] lines = File.ReadAllLines(path + ".txt");
        File.Delete(path + ".txt");
        LoadFromLines(lines);
    }

    protected void LoadFromLines(string[] lines)
    {
        StringData currentStr = null;

        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                Match match = Regex.Match(lines[i], "{format:(.*)}");
                Format = match.Groups[1].Value;
            }
            else if (i > 1)
            {
                if (currentStr == null)
                {
                    if (lines[i].StartsWith("{dialog"))
                    {
                        Match match = Regex.Match(lines[i], "{dialog (\\d+)(.*)}");
                        string[] parameters = match.Groups[2].Value.TrimStart().Split(", ");
                        int id = int.Parse(match.Groups[1].Value);
                        currentStr = new StringData
                        {
                            Type = StringData.StringType.Dialog
                        };
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(parameters[j]))
                            {
                                string[] split = parameters[j].Split("=");
                                if (split[0] == "singular")
                                {
                                    currentStr.Singular = split[1].Substring(1, split[1].Length - 1);
                                }

                                if (split[0] == "plural")
                                {
                                    currentStr.Plural = split[1].Substring(1, split[1].Length - 1);
                                }

                                if (split[0] == "id")
                                {
                                    currentStr.ID = int.Parse(split[1]);
                                }
                            }
                        }

                        Add(id, currentStr);
                    }
                    else if (lines[i].StartsWith("{symlink"))
                    {
                        Match match = Regex.Match(lines[i], "{symlink (\\d+)(.*)}");
                        string[] parameters = match.Groups[2].Value.TrimStart().Split(", ");
                        int id = int.Parse(match.Groups[1].Value);
                        currentStr = new StringData
                        {
                            Type = StringData.StringType.Symlink
                        };
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(parameters[j]))
                            {
                                string[] split = parameters[j].Split("=");
                                if (split[0] == "link")
                                {
                                    currentStr.Link = int.Parse(split[1]);
                                }

                                if (split[0] == "id")
                                {
                                    currentStr.ID = int.Parse(split[1]);
                                }
                            }
                        }

                        Add(id, currentStr);

                        currentStr = null;
                    }
                }
                else if (lines[i] == "{/dialog}")
                {
                    currentStr = null;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentStr.Text))
                    {
                        currentStr.Text += "\n";
                    }

                    currentStr.Text += lines[i];
                }
            }
        }
    }

    public virtual void Save(string path)
    {
        List<string> lines = GetLinesData();

        File.WriteAllLines(path + ".txt", lines);
        Tools.ConvertTxtToBin(path + ".txt");
    }

    protected List<string> GetLinesData()
    {
        List<string> lines = new()
        {
            "{format:" + Format + "}",
            ""
        };

        foreach (int id in Keys)
        {
            if (Values[id].Type == StringData.StringType.Dialog)
            {
                List<string> attributes = new();
                if (!string.IsNullOrEmpty(Values[id].Singular))
                {
                    attributes.Add("singular=\"" + Values[id].Singular + "\"");
                }

                if (!string.IsNullOrEmpty(Values[id].Plural))
                {
                    attributes.Add("plural=\"" + Values[id].Plural + "\"");
                }

                if (Values[id].ID != -1)
                {
                    attributes.Add("id=" + Values[id].ID);
                }

                string dialogHeader = "{dialog " + id;

                if (attributes.Count > 0)
                {
                    dialogHeader += " " + string.Join(", ", attributes);
                }

                dialogHeader += "}";

                lines.Add(dialogHeader);
                lines.Add(Values[id].Text);
                lines.Add("{/dialog}");
            }
            else if (Values[id].Type == StringData.StringType.Symlink)
            {
                List<string> attributes = new();
                if (Values[id].Link != -1)
                {
                    attributes.Add("link=" + Values[id].Link);
                }

                if (Values[id].ID != -1)
                {
                    attributes.Add("id=" + Values[id].ID);
                }

                lines.Add("{symlink " + id + " " + string.Join(", ", attributes) + "}");
            }

            lines.Add("");
        }

        return lines;
    }

    public class StringData
    {
        public enum StringType
        {
            Dialog,
            Symlink
        }

        public StringType Type { get; set; }

        public string Text { get; set; }
        public string Singular { get; set; }
        public string Plural { get; set; }
        public int ID { get; set; } = -1;
        public int Link { get; set; } = -1;
    }
}
