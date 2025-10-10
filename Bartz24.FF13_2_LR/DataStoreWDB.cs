using Bartz24.Data;
using Bartz24.FF13Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bartz24.FF13_2_LR;
public class DataStoreWDB<T> where T : DataStoreWDBEntry, new()
{
	private readonly Dictionary<string, T> _data = new(StringComparer.Ordinal);
	private static readonly Dictionary<string, PropertyInfo> s_propertyMap = typeof(T)
		.GetProperties(BindingFlags.Public | BindingFlags.Instance)
		.Where(p => p.CanWrite)
		.ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

	// Metadata from the JSON header
	public int RecordCount { get; private set; }
	[JsonPropertyName("!!sheetname")] public string SheetName { get; private set; } = string.Empty;
	public bool HasStrArray { get; private set; }
	public int BitsPerOffset { get; private set; }
	public int OffsetsPerValue { get; private set; }
	public bool IsStrTypelistV1 { get; private set; }
	[JsonPropertyName("!!strtypelistb")] public List<int> StrTypeListB { get; private set; } = new();
	public bool HasTypelist { get; private set; }
	[JsonPropertyName("!!version")] public long Version { get; private set; }
	[JsonPropertyName("!structitem")] public List<string> StructItems { get; private set; } = new();

	public T this[string id] => _data[id];
	public List<string> Keys => _data.Keys.ToList();
	public List<T> Values => _data.Values.ToList();

	public void Add(T entry)
	{
		if (entry == null) throw new ArgumentNullException(nameof(entry));
		string key = entry.record;
		if (_data.ContainsKey(key)) throw new ArgumentException($"Key already exists: {key}");
	_data.Add(key, entry);
	}

	public T Copy(string original, string newName)
	{
		if (!_data.TryGetValue(original, out var source)) throw new KeyNotFoundException(original);
		T clone = new();
		source.CopyPropertiesTo(clone);
		clone.record = newName;
		Add(clone);
		return clone;
	}


	public void Swap(string name1, string name2)
	{
		if (!_data.ContainsKey(name1) || !_data.ContainsKey(name2)) throw new KeyNotFoundException();
		(_data[name1], _data[name2]) = (_data[name2], _data[name1]);
		_data[name1].record = name1;
		_data[name2].record = name2;
	}

	private class WdbRoot
	{
		public int recordCount { get; set; }
		[JsonPropertyName("!!sheetname")] public string sheetname { get; set; } = string.Empty;
		public bool hasStrArray { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int bitsPerOffset { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int offsetsPerValue { get; set; }
		public bool isStrTypelistV1 { get; set; }
		[JsonPropertyName("!!strtypelistb")] public List<int> strtypelistb { get; set; } = new();
		public bool hasTypelist { get; set; }
		[JsonPropertyName("!!version")] public long version { get; set; }
		[JsonPropertyName("!structitem")] public List<string> structitem { get; set; } = new();
		public List<JsonElement> records { get; set; } = new();
	}

	public void Load(string game, string path, string novaPath)
	{
		// Convert the WDB file to JSON using Nova only if needed
		string jsonPath = path.Substring(0, path.LastIndexOf('.')) + ".json";
		Nova.ConvertWDBToJSON(game, path, novaPath);
		if (!File.Exists(jsonPath)) throw new FileNotFoundException(jsonPath);

		_data.Clear();
		using FileStream fs = File.OpenRead(jsonPath);
		using var doc = JsonDocument.Parse(fs, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
		var root = doc.RootElement;
		if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Invalid WDB JSON");

		// Capture metadata
		RecordCount = root.TryGetProperty("recordCount", out var rc) ? rc.GetInt32() : 0;
		SheetName = root.TryGetProperty("!!sheetname", out var sn) ? (sn.GetString() ?? string.Empty) : string.Empty;
		HasStrArray = root.TryGetProperty("hasStrArray", out var hsa) && (hsa.ValueKind == JsonValueKind.True || (hsa.ValueKind == JsonValueKind.Number && hsa.GetInt32() != 0));
		BitsPerOffset = root.TryGetProperty("bitsPerOffset", out var bpo) && bpo.ValueKind == JsonValueKind.Number ? bpo.GetInt32() : 0;
		OffsetsPerValue = root.TryGetProperty("offsetsPerValue", out var opv) && opv.ValueKind == JsonValueKind.Number ? opv.GetInt32() : 0;
		IsStrTypelistV1 = root.TryGetProperty("isStrTypelistV1", out var stv1) && (stv1.ValueKind == JsonValueKind.True || (stv1.ValueKind == JsonValueKind.Number && stv1.GetInt32() != 0));
		StrTypeListB = root.TryGetProperty("!!strtypelistb", out var stlb) && stlb.ValueKind == JsonValueKind.Array ? stlb.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Number).Select(e => e.GetInt32()).ToList() : new List<int>();
		HasTypelist = root.TryGetProperty("hasTypelist", out var ht) && (ht.ValueKind == JsonValueKind.True || (ht.ValueKind == JsonValueKind.Number && ht.GetInt32() != 0));
		Version = root.TryGetProperty("!!version", out var ver) && ver.ValueKind == JsonValueKind.Number ? ver.GetInt64() : 0;
		StructItems = root.TryGetProperty("!structitem", out var si) && si.ValueKind == JsonValueKind.Array ? si.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList() : new List<string>();

		if (root.TryGetProperty("records", out var records) && records.ValueKind == JsonValueKind.Array)
		{
			foreach (var rec in records.EnumerateArray())
			{
				if (rec.ValueKind != JsonValueKind.Object) continue;
				T entry = new();
				foreach (var prop in rec.EnumerateObject())
				{
					if (!s_propertyMap.TryGetValue(prop.Name, out var pi)) continue;
					try
					{
						AssignProperty(entry, pi, prop.Value);
					}
					catch { }
				}
				if (string.IsNullOrEmpty(entry.record)) continue;
				_data[entry.record] = entry;
			}
		}
	}

	private static void AssignProperty(T entry, PropertyInfo pi, JsonElement value)
	{
		if (value.ValueKind == JsonValueKind.Null)
		{
			if (pi.PropertyType == typeof(string))
			{
				pi.SetValue(entry, null);
			}
			return;
		}

		object? v = null;
		var t = pi.PropertyType;
		if (t == typeof(string)) v = value.GetString();
		else if (t == typeof(int)) v = value.ValueKind == JsonValueKind.Number ? value.GetInt32() : 0;
		else if (t == typeof(uint)) v = value.ValueKind == JsonValueKind.Number ? value.GetUInt32() : 0u;
		else if (t == typeof(short)) v = (short)(value.ValueKind == JsonValueKind.Number ? value.GetInt32() : 0);
		else if (t == typeof(ushort)) v = (ushort)(value.ValueKind == JsonValueKind.Number ? value.GetUInt32() : 0);
		else if (t == typeof(byte)) v = (byte)(value.ValueKind == JsonValueKind.Number ? value.GetByte() : 0);
		else if (t == typeof(sbyte)) v = (sbyte)(value.ValueKind == JsonValueKind.Number ? value.GetSByte() : 0);
		else if (t == typeof(float)) v = value.ValueKind == JsonValueKind.Number ? value.GetSingle() : 0f;
		else if (t == typeof(double)) v = value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0d;
		else if (t == typeof(bool)) v = value.ValueKind == JsonValueKind.Number ? value.GetInt32() != 0 : value.GetBoolean();
		else
		{
			// Fallback deserialize
			v = JsonSerializer.Deserialize(value.GetRawText(), t);
		}
		pi.SetValue(entry, v);
	}

	public void Save(string game, string path, string novaPath)
	{
		// Prepare struct item header list
		var structItems = StructItems?.ToList() ?? new List<string>();

		string jsonPath = path.Substring(0, path.LastIndexOf(".wdb")) + ".json";
		using (FileStream fs = File.Create(jsonPath))
		{
			using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
			{
				writer.WriteStartObject();
				// Header in strict order expected by Nova
				writer.WriteNumber("recordCount", _data.Count);
				writer.WriteString("!!sheetname", SheetName ?? string.Empty);
				writer.WriteBoolean("hasStrArray", HasStrArray);
				if (HasStrArray)
				{
					writer.WriteNumber("bitsPerOffset", BitsPerOffset);
					writer.WriteNumber("offsetsPerValue", OffsetsPerValue);
				}
				writer.WriteBoolean("isStrTypelistV1", IsStrTypelistV1);
				writer.WritePropertyName("!!strtypelistb");
				writer.WriteStartArray();
				foreach (var v in StrTypeListB ?? new List<int>()) writer.WriteNumberValue(v);
				writer.WriteEndArray();
				writer.WriteBoolean("hasTypelist", HasTypelist);
				writer.WriteNumber("!!version", Version);
				writer.WritePropertyName("!structitem");
				writer.WriteStartArray();
				foreach (var col in structItems) writer.WriteStringValue(col);
				writer.WriteEndArray();

				// Records
				writer.WritePropertyName("records");
				writer.WriteStartArray();
				foreach (var key in _data.Keys.OrderBy(s => s, StringComparer.Ordinal))
				{
					var entry = _data[key];
					writer.WriteStartObject();
					writer.WriteString("record", entry.record ?? string.Empty);
					foreach (var col in structItems)
					{
						if (string.Equals(col, "record", StringComparison.Ordinal)) continue;
						if (!s_propertyMap.TryGetValue(col, out var pi)) continue;
						WriteJsonValue(writer, col, pi.PropertyType, pi.GetValue(entry));
					}
					writer.WriteEndObject();
				}
				writer.WriteEndArray();

				writer.WriteEndObject();
				writer.Flush();
			}
		}

		// Convert back to WDB
		Nova.ConvertJSONToWDB(game, path, novaPath);
	}

	private static void WriteJsonValue(Utf8JsonWriter writer, string name, Type type, object? value)
	{
		if (type == typeof(string))
		{
			writer.WriteString(name, value as string ?? string.Empty);
			return;
		}
		if (type == typeof(bool))
		{
			writer.WriteBoolean(name, value is bool b && b);
			return;
		}
		// Numeric types
		try
		{
			if (type == typeof(int) || type == typeof(short) || type == typeof(sbyte))
			{
				writer.WriteNumber(name, Convert.ToInt32(value ?? 0));
				return;
			}
			if (type == typeof(uint) || type == typeof(ushort) || type == typeof(byte))
			{
				writer.WriteNumber(name, Convert.ToUInt32(value ?? 0));
				return;
			}
			if (type == typeof(long))
			{
				writer.WriteNumber(name, (long)(value ?? 0L));
				return;
			}
			if (type == typeof(ulong))
			{
				writer.WriteNumber(name, Convert.ToUInt64(value ?? 0UL));
				return;
			}
			if (type == typeof(float))
			{
				writer.WriteNumber(name, (float)(value ?? 0f));
				return;
			}
			if (type == typeof(double))
			{
				writer.WriteNumber(name, (double)(value ?? 0d));
				return;
			}
		}
		catch
		{
			// Fallback to string representation on conversion issues
		}
		writer.WriteString(name, value?.ToString() ?? string.Empty);
	}

    public void Clear()
    {
		_data.Clear();
    }
}
