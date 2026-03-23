using Bartz24.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bartz24.RandoWPF;

[JsonObject(MemberSerialization.OptIn)]
public class DictComboBoxFlagProperty<T> : ComboBoxFlagProperty
{
	public DictComboBoxFlagProperty(string defaultValue) : base(defaultValue)
	{
		selectedKey = DictValues.Reverse.Contains(defaultValue)
			? DictValues.Reverse[defaultValue]
			: default;
	}

	public override DictComboBoxFlagProperty<T> Register(Flag parent)
	{
		base.Register(parent);
		parent.PropertyChanged += Parent_PropertyChanged;
		return this;
	}

	private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		Flag flag = (Flag)sender;
		if (e.PropertyName == nameof(flag.FlagEnabled) && !flag.FlagEnabled)
		{
			SelectedValue = Values[0];
		}
	}

	public BiDictionary<T, string> DictValues { get; set; } = new BiDictionary<T, string>();
	public override List<string> Values => DictValues.Forward.Values.ToList();

	private T selectedKey;
	public T SelectedKey
	{
		get => selectedKey;
		set
		{
			selectedKey = value;
			if (DictValues.Forward.Contains(selectedKey))
			{
				SelectedValue = DictValues.Forward[selectedKey];
			}
		}
	}

	[JsonProperty]
	public override string SelectedValue
	{
		get => base.SelectedValue;
		set
		{
			base.SelectedValue = value;
			if (DictValues.Reverse.Contains(value))
			{
				selectedKey = DictValues.Reverse[value];
			}
		}
	}
}
