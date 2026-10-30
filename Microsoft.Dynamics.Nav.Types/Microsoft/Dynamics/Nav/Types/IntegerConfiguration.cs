using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class IntegerConfiguration<T> where T : IntegerConfiguration<T>
{
	private const string NumberFormatSpecifier = "N";

	private readonly int minValue;

	private readonly int maxValue;

	private readonly string settingName;

	private int rawValue;

	public int Value
	{
		get
		{
			return rawValue;
		}
		set
		{
			SettingsHelper.ValidateInteger(value, minValue, maxValue, settingName);
			rawValue = value;
		}
	}

	protected IntegerConfiguration(string value, string settingName, int min, int max)
	{
		minValue = min;
		maxValue = max;
		this.settingName = settingName;
		Value = GetRawValue(value);
	}

	private int GetRawValue(string settingValue)
	{
		try
		{
			return int.Parse(settingValue, CultureInfo.CurrentCulture);
		}
		catch (OverflowException exception)
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			numberFormatInfo.NumberDecimalDigits = 0;
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationSettingOutOfRange, settingName, minValue.ToString("N", numberFormatInfo), maxValue.ToString("N", numberFormatInfo)), exception);
		}
		catch (FormatException exception2)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(int).Name), exception2);
		}
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

	protected static string MaxValueTextToInt(string value, int maxValue)
	{
		if (string.Compare(value, "MaxValue", ignoreCase: true, CultureInfo.CurrentCulture) != 0)
		{
			return value;
		}
		return maxValue.ToString(CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return rawValue.ToString(CultureInfo.CurrentCulture);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		IntegerConfiguration<T> integerConfiguration = obj as IntegerConfiguration<T>;
		if (integerConfiguration == null)
		{
			return false;
		}
		return Value.Equals(integerConfiguration.Value);
	}

	public bool Equals(IntegerConfiguration<T> other)
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

	public static bool operator ==(IntegerConfiguration<T> first, IntegerConfiguration<T> second)
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

	public static bool operator !=(IntegerConfiguration<T> first, IntegerConfiguration<T> second)
	{
		return !(first == second);
	}
}
