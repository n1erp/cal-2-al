using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class EnumConfigSetting<T> : IEquatable<EnumConfigSetting<T>>
{
	public T Value { get; set; }

	private EnumConfigSetting()
	{
	}

	public EnumConfigSetting(string value)
	{
		Value = Parse(value, string.Empty).Value;
	}

	public static EnumConfigSetting<T> Parse(string settingValue, string settingName)
	{
		string[] names = Enum.GetNames(typeof(T));
		foreach (string text in names)
		{
			if (string.Compare(settingValue, text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new EnumConfigSetting<T>
				{
					Value = (T)Enum.Parse(typeof(T), text)
				};
			}
		}
		throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, settingName));
	}

	public static bool TryParse(string settingValue, out EnumConfigSetting<T> result)
	{
		result = null;
		try
		{
			result = Parse(settingValue, string.Empty);
		}
		catch (NavConfigurationException)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		EnumConfigSetting<T> enumConfigSetting = obj as EnumConfigSetting<T>;
		if (enumConfigSetting == null)
		{
			return false;
		}
		return Value.Equals(enumConfigSetting.Value);
	}

	public bool Equals(EnumConfigSetting<T> other)
	{
		if (other == null)
		{
			return false;
		}
		return Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(EnumConfigSetting<T> first, EnumConfigSetting<T> second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Value.Equals(second.Value);
	}

	public static bool operator !=(EnumConfigSetting<T> first, EnumConfigSetting<T> second)
	{
		return !(first == second);
	}
}
