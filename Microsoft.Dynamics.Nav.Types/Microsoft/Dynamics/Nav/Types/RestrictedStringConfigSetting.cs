using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

public class RestrictedStringConfigSetting<T> where T : RestrictedStringConfigSetting<T>
{
	private readonly int maxLength;

	private readonly string validStringDefinition;

	private readonly string settingName;

	private string rawValue;

	private bool allowNullOrEmpty;

	public string Value
	{
		get
		{
			return rawValue;
		}
		set
		{
			if (!allowNullOrEmpty || !string.IsNullOrEmpty(value))
			{
				SettingsHelper.ValidateString(value, settingName, maxLength, validStringDefinition);
			}
			rawValue = value;
		}
	}

	protected RestrictedStringConfigSetting(string value, string settingName, bool allowNullOrEmpty, int maxLength, string validStringDefinition)
	{
		this.validStringDefinition = validStringDefinition;
		this.maxLength = maxLength;
		this.settingName = settingName;
		this.allowNullOrEmpty = allowNullOrEmpty;
		Value = value;
	}

	public static bool TryParse(string settingValue, out T result)
	{
		result = null;
		if (settingValue == null)
		{
			return false;
		}
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

	public static T Parse(string settingValue, string settingName)
	{
		ConstructorInfo constructor = typeof(T).GetConstructor(new Type[2]
		{
			typeof(string),
			typeof(string)
		});
		try
		{
			return (T)constructor.Invoke(new object[2] { settingValue, settingName });
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
	}

	public override string ToString()
	{
		return rawValue?.ToString(CultureInfo.CurrentCulture) ?? "";
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		RestrictedStringConfigSetting<T> restrictedStringConfigSetting = obj as RestrictedStringConfigSetting<T>;
		if (restrictedStringConfigSetting == null)
		{
			return false;
		}
		return Value.Equals(restrictedStringConfigSetting.Value);
	}

	public bool Equals(RestrictedStringConfigSetting<T> other)
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

	public static bool operator ==(RestrictedStringConfigSetting<T> first, RestrictedStringConfigSetting<T> second)
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

	public static bool operator !=(RestrictedStringConfigSetting<T> first, RestrictedStringConfigSetting<T> second)
	{
		return !(first == second);
	}

	public static implicit operator string(RestrictedStringConfigSetting<T> setting)
	{
		return setting?.ToString();
	}
}
