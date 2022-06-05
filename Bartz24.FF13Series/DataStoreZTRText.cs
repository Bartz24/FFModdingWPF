using Bartz24.Data;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Bartz24.FF13Series
{
    public class DataStoreZTRText
    {
        Dictionary<string, string> Data;

        private string ztrPath;

        private string doubleBar = System.Text.Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x91 }, 0, 3);

        public string this[string id]
        {
            get { return Data[id]; }
            set { Data[id] = value; }
        }

        public List<string> Keys { get => Data.Keys.ToList(); }
        public List<string> Values { get => Data.Values.ToList(); }

        public void Add(string id, string data)
        {
            Data.Add(id, data);
        }
        public void Load(string path, string novaPath)
        {
            Nova.UnpackZTR(path, novaPath);
            string txtPath = path.Substring(0, path.LastIndexOf(".ztr")) + ".txt";

            Data = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(txtPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                    ztrPath = lines[i];
                else if (i > 1)
                {
                    Match match =  Regex.Match(lines[i], $"\".*{doubleBar}(.*)\" = \"(.*)\";");
                    Data.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
            }
        }

        public void Save(string game, string path, string novaPath)
        {
            string txtPath = path.Substring(0, path.LastIndexOf(".ztr")) + ".txt";
            List<string> lines = new List<string>();
            lines.Add(ztrPath);
            lines.Add($"/*{Data.Count}*/");
            List<string> keysSorted = Data.Keys.OrderBy(s => s).ToList();
            for (int i = 0; i < keysSorted.Count; i++)
            {
                lines.Add($"\"{i}{doubleBar}{keysSorted[i]}\" = \"{Data[keysSorted[i]]}\";");
            }
            File.WriteAllLines(txtPath, lines);

            Nova.InjectZTR(game, path, novaPath);
        }
    }
}
