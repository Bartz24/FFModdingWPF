using Bartz24.Data;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Bartz24.FF12
{
    public class DataStoreBinText
    {
        Dictionary<int, StringData> Data;

        public string Format { get; set; }

        private string ztrPath;

        public StringData this[int id]
        {
            get { return Data[id]; }
            set { Data[id] = value; }
        }

        public List<int> Keys { get => Data.Keys.ToList(); }
        public List<StringData> Values { get => Data.Values.ToList(); }

        public void Add(int id, StringData data)
        {
            Data.Add(id, data);
        }
        public void Load(string path)
        {
            Data = new Dictionary<int, StringData>();

            string[] lines = File.ReadAllLines(path);
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
                            Match match = Regex.Match(lines[i], "{dialog (.*)}");
                            string[] parameters = match.Groups[1].Value.Split(", ");
                            int id = int.Parse(parameters[0]);
                            currentStr = new StringData();
                            for (int j = 1; j < parameters.Length; j++)
                            {
                                string[] split = parameters[j].Split("=");
                                if (split[0] == "singular")
                                    currentStr.Singular = split[1].Substring(1, split[1].Length - 1);
                                if (split[0] == "plural")
                                    currentStr.Plural = split[1].Substring(1, split[1].Length - 1);
                            }
                            Add(id, currentStr);
                        }
                    }
                    else if(lines[i] == "{/dialog}")
                    {
                        currentStr = null;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentStr.Text))
                            currentStr.Text += "\n";
                        currentStr.Text += lines[i];
                    }
                }
            }
        }

        public void Save(string game, string path, string novaPath)
        {
            throw new NotImplementedException();
        }

        public class StringData
        {
            public string Text { get; set; }
            public string Singular { get; set; }
            public string Plural { get; set; }
        }
    }
}
