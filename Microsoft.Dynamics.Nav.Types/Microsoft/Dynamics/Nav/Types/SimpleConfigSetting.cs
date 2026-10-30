using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

public class SimpleConfigSetting<T> : SimpleConfigSettingNoMaxValue<T>
{
	public static readonly T MaxValue;

	static SimpleConfigSetting()
	{
		FieldInfo field = typeof(T).GetField("MaxValue", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
		if (field != null)
		{
			MaxValue = (T)field.GetValue(null);
		}
	}

	private SimpleConfigSetting()
	{
	}

	public SimpleConfigSetting(string settingValue)
		: base(settingValue)
	{
	}

	public static bool TryParse(string settingValue, out SimpleConfigSetting<T> result)
	{
		if (!SimpleConfigSettingNoMaxValue<T>.TryParse(settingValue, out var result2))
		{
			result = null;
			return false;
		}
		result = new SimpleConfigSetting<T>
		{
			Value = result2.Value
		};
		return true;
	}

	public new static SimpleConfigSetting<T> Parse(string settingValue, string settingName)
	{
		SimpleConfigSettingNoMaxValue<T> simpleConfigSettingNoMaxValue = SimpleConfigSettingNoMaxValue<T>.Parse(settingValue, settingName);
		return new SimpleConfigSetting<T>
		{
			Value = simpleConfigSettingNoMaxValue.Value
		};
	}
}
