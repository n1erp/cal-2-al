using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class SimpleConfigSettingNoMaxValue<T> : IEquatable<SimpleConfigSettingNoMaxValue<T>>
{
	public T Value { get; set; }

	protected SimpleConfigSettingNoMaxValue()
	{
	}

	public SimpleConfigSettingNoMaxValue(string settingValue)
	{
		Value = Parse(settingValue, string.Empty).Value;
	}

	public static bool TryParse(string settingValue, out SimpleConfigSettingNoMaxValue<T> result)
	{
		result = null;
		if (settingValue == null)
		{
			return false;
		}
		try
		{
			if (!string.IsNullOrWhiteSpace(settingValue) && typeof(T).Equals(typeof(Uri)) && !Uri.IsWellFormedUriString(settingValue, UriKind.Absolute))
			{
				return false;
			}
			result = new SimpleConfigSettingNoMaxValue<T>
			{
				Value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(settingValue)
			};
		}
		catch (NotSupportedException)
		{
			result = null;
			return false;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
		return true;
	}

	public static SimpleConfigSettingNoMaxValue<T> Parse(string settingValue, string settingName)
	{
		if (settingValue == null)
		{
			throw new ArgumentNullException("settingValue");
		}
		try
		{
			if (!string.IsNullOrWhiteSpace(settingValue) && typeof(T).Equals(typeof(Uri)) && !Uri.IsWellFormedUriString(settingValue, UriKind.Absolute))
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, settingName));
			}
			return new SimpleConfigSettingNoMaxValue<T>
			{
				Value = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(settingValue)
			};
		}
		catch (NotSupportedException)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, settingName));
		}
		catch (FormatException)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, settingName));
		}
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
		SimpleConfigSettingNoMaxValue<T> simpleConfigSettingNoMaxValue = obj as SimpleConfigSettingNoMaxValue<T>;
		if (simpleConfigSettingNoMaxValue == null)
		{
			return false;
		}
		return Value.Equals(simpleConfigSettingNoMaxValue.Value);
	}

	public bool Equals(SimpleConfigSettingNoMaxValue<T> other)
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

	public static bool operator ==(SimpleConfigSettingNoMaxValue<T> first, SimpleConfigSettingNoMaxValue<T> second)
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

	public static bool operator !=(SimpleConfigSettingNoMaxValue<T> first, SimpleConfigSettingNoMaxValue<T> second)
	{
		return !(first == second);
	}
}
