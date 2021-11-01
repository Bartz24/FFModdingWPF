using Bartz24.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace Bartz24.FF13_2_LR
{
    public class DataStoreDB3<T> where T : DataStoreDB3SubEntry, new()
    {
        Dictionary<string, T> Data;
        Dictionary<int, DataStoreDB3EntryInfo> EntryInfoList;

        Dictionary<int, DataStoreDB3String> StringList;

        Dictionary<int, List<string>> StringArrayTable;

        Dictionary<int, DataStoreDB3StringArrayList> StringArrayList;

        Dictionary<string, string> StringPointerMapping;

        public T this[string id]
        {
            get { return Data[id]; }
        }

        public List<string> Keys { get => Data.Keys.ToList(); }
        public List<T> Values { get => Data.Values.ToList(); }

        public void Add(string id, T data)
        {
            Data.Add(id, data);
        }
        public void Add(T data)
        {
            data.main_id = Data.Values.Select(d => d.main_id).Max() + 1;
            Data.Add(data.name, data);
        }
        public T Copy(string original, string newName)
        {
            T newData = new T();
            Data[original].CopyPropertiesTo(newData);
            newData.name = newName;
            Add(newData);

            return newData;
        }
        public T InsertCopy(string original, string newName, string after)
        {
            int entryId = after == null ? Data.Values.Select(d => d.main_id).Min() - 1 : Data[after].main_id;
            Data.Values.Where(d => d.main_id > entryId).ForEach(d => d.main_id++);
            EntryInfoList = EntryInfoList.ToDictionary(
                kp => kp.Key <= entryId ? kp.Key : (kp.Key + 1),
                kp =>
            {
                if (kp.Key > entryId)
                    kp.Value.main_id++;
                return kp.Value;
            });
            T newData = new T();
            Data[original].CopyPropertiesTo(newData);
            newData.name = newName;
            newData.main_id = entryId + 1;
            Add(newName, newData);
            DataStoreDB3EntryInfo entry = new DataStoreDB3EntryInfo();
            EntryInfoList[EntryInfoList.Keys.Max()].CopyPropertiesTo(entry);
            entry.main_id = entryId + 1;
            entry.name = newName;
            EntryInfoList.Add(entry.main_id, entry);

            return newData;
        }
        public T InsertCopyAlphabetical(string original, string newName)
        {
            List<string> names = Keys.ToList();
            names.Add(newName);
            names = names.OrderBy(s => s).ToList();
            return InsertCopy(original, newName, names.IndexOf(newName) > 0 ? Data[names[names.IndexOf(newName) - 1]].name : null);
        }


        public void Swap(string name1, string name2)
        {
            Data.Swap(name1, name2);
            int temp = Data[name1].main_id;
            Data[name1].main_id = Data[name2].main_id;
            Data[name2].main_id = temp;
            Data[name1].name = name2;
            Data[name2].name = name1;
        }

        public void Load(string game, string path, string novaPath)
        {
            Nova.ConvertWDBToDB3(game, path, novaPath);
            string db3Path = path.Substring(0, path.LastIndexOf(".wdb")) + ".db3";

            EntryInfoList = DB3Database.GetEntries<DataStoreDB3EntryInfo>(db3Path, "02 WPD Entry Info");

            FindStringMappings();
            Data = DB3Database.GetEntries<T>(db3Path, "03 WPD Sub Entry Info").ToDictionary(t => t.Value.name, t => t.Value);
            StringList = DB3Database.GetEntries<DataStoreDB3String>(db3Path, "08 !!string table");

            Dictionary<int, DataStoreDB3StringArray> stringArray = DB3Database.GetEntries<DataStoreDB3StringArray>(db3Path, "05 !!strArray table");
            StringArrayTable = stringArray.Select(s => s.Value.strArrayListIndex).Distinct().ToDictionary(i => i, i => stringArray.Select(s => s.Value).OrderBy(s => s.main_id).Where(s => s.strArrayListIndex == i).SelectMany(s => new string[] { s.lower_name, s.higher_name }).ToList());
            StringArrayList = DB3Database.GetEntries<DataStoreDB3StringArrayList>(db3Path, "07 !!strArrayList table");
        }

        private void FindStringMappings()
        {
            StringPointerMapping = new Dictionary<string, string>();
            foreach (PropertyInfo p in typeof(T).GetProperties())
            {
                if (p.Name.EndsWith("_pointer"))
                {
                    string prefix = p.Name.Substring(0, p.Name.LastIndexOf("_pointer"));
                    StringPointerMapping.Add(prefix + "_pointer", prefix + "_string");
                }
            }
        }

        public void Save(string path, string novaPath)
        {
            UpdateStringPointers();
            string db3Path = path.Substring(0, path.LastIndexOf(".wdb")) + ".db3";
            UpdateEntryInfo();
            DB3Database.Save(db3Path, "02 WPD Entry Info", EntryInfoList);
            DB3Database.Save(db3Path, "03 WPD Sub Entry Info", Data.ToDictionary(t => t.Value.main_id, t => t.Value));
            DB3Database.Save(db3Path, "08 !!string table", StringList);

            if (StringArrayTable.Count > 0)
            {
                Dictionary<int, DataStoreDB3StringArray> stringArrayData = GetStringArrayData(db3Path);
                DB3Database.Save(db3Path, "05 !!strArray table", stringArrayData);
                StringArrayList.Keys.ForEach(i => StringArrayList[i].strArrayPointer = (i == 0 ? 0 : StringArrayList[i - 1].strArrayPointer + ((StringArrayTable[i - 1].Count - 1) / 2 + 1) * 4));
                DB3Database.Save(db3Path, "07 !!strArrayList table", StringArrayList);
            }
            Nova.ConvertDB3ToWDB(db3Path, novaPath);
        }

        private void UpdateEntryInfo()
        {
            List<string> names = EntryInfoList.Values.Select(e => e.name).ToList();
            int size = EntryInfoList.Values.OrderByDescending(e => e.main_id).First().size;
            Data.Keys.Where(k => !names.Contains(k)).ForEach(k =>
            {
                int max = EntryInfoList.Values.Select(e => e.main_id).Max();
                DataStoreDB3EntryInfo prev = EntryInfoList.Values.First(e => e.main_id == max);
                DataStoreDB3EntryInfo entryInfo = new DataStoreDB3EntryInfo() { main_id = max + 1, name = k, size = size, address = prev.address + size, padding01 = 0, padding02 = 0 };
                EntryInfoList.Add(entryInfo.main_id, entryInfo);
            });
        }

        private Dictionary<int, DataStoreDB3StringArray> GetStringArrayData(string db3Path)
        {
            Dictionary<int, DataStoreDB3StringArray> stringArrayData = new Dictionary<int, DataStoreDB3StringArray>();
            int stringArraySize = DB3Database.GetStringArraySize(db3Path);
            StringArrayTable.Keys.ForEach(i =>
            {
                for (int id = 0; id < StringArrayTable[i].Count; id += 2)
                {
                    int newId = stringArrayData.Count();
                    DataStoreDB3StringArray stringArray = new DataStoreDB3StringArray();
                    stringArray.main_id = newId;
                    stringArray.strArrayListIndex = i;
                    stringArray.lower_name = StringArrayTable[i][id];
                    if (stringArray.lower_name != "")
                        stringArray.lower_pointer = StringList.Values.FirstOrDefault(s => s.name == stringArray.lower_name).pointer;
                    else
                        stringArray.lower_pointer = StringList[1].pointer - 1;
                    if (id + 1 < StringArrayTable[i].Count)
                    {
                        stringArray.higher_name = StringArrayTable[i][id + 1];
                        if (stringArray.higher_name != "")
                            stringArray.higher_pointer = StringList.Values.FirstOrDefault(s => s.name == stringArray.higher_name).pointer;
                        else
                            stringArray.higher_pointer = StringList[1].pointer - 1;
                    }
                    stringArray.data = (int)Math.Pow(2, stringArraySize) * stringArray.higher_pointer + stringArray.lower_pointer;
                    stringArrayData.Add(newId, stringArray);
                }
            });
            return stringArrayData;
        }

        private void UpdateStringPointers()
        {
            Dictionary<string, int> mapping = Values[0].GetStringArrayMapping();
            Values.ForEach(entry =>
            {
                StringPointerMapping.ForEach(p =>
                {
                    string value = entry.GetPropValue<string>(p.Value);
                    int pointer = entry.GetPropValue<int>(p.Key);
                    if (value != "")
                    {
                        DataStoreDB3String match = StringList.Values.FirstOrDefault(s => s.name == value);
                        if (match != null && match.pointer != pointer)
                            entry.SetPropValue(p.Key, match.pointer);
                        else if (match == null)
                        {
                            int maxId = StringList.Keys.Max();
                            StringList.Add(maxId + 1, new DataStoreDB3String()
                            {
                                main_id = maxId + 1,
                                name = value,
                                pointer = StringList[maxId].pointer + StringList[maxId].name.Length + 1
                            });
                            entry.SetPropValue(p.Key, StringList[maxId + 1].pointer);
                        }
                    }
                    else
                    {
                        DataStoreDB3String match = StringList.Values.FirstOrDefault(s => s.pointer - 1 == pointer);
                        if (match == null)
                            entry.SetPropValue(p.Key, StringList[1].pointer - 1);
                    }
                    if (mapping.ContainsKey(p.Key))
                    {
                        int index = mapping[p.Key];
                        if (!StringArrayTable[index].Contains(value))
                            StringArrayTable[index].Add(value);
                        entry.SetPropValue(p.Key, StringArrayTable[index].IndexOf(value));
                    }
                });
            });
        }
    }
}
