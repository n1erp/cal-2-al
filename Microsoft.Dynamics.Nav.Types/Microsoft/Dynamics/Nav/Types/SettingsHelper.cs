using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class SettingsHelper
{
	public const string MaxValueSetting = "MaxValue";

	public const string NumberFormatSpecifier = "N";

	public const string ClientServicesReconnectPeriodConfigName = "ClientServicesReconnectPeriod";

	public static bool TryParseAsMaxValue(string setting, string settingName, Type type, out object maxValue)
	{
		maxValue = null;
		if (string.Compare(setting, "MaxValue", ignoreCase: true, CultureInfo.CurrentCulture) == 0)
		{
			FieldInfo field = type.GetField("MaxValue", BindingFlags.Static | BindingFlags.Public);
			if (field != null)
			{
				maxValue = field.GetValue(null);
				return true;
			}
			throw new NavConfigurationException(Lang.MaxValueNotValidConfigurationSetting, settingName, type.Name);
		}
		return false;
	}

	public static void ValidateString(string value, string settingName, int maxLength, string validStringDef)
	{
		if (!IsValidStringLength(value, maxLength))
		{
			throw new NavConfigurationException(Lang.ConfigSettingStringTooLong, settingName, value, maxLength);
		}
		if (!IsValidString(value, validStringDef, out var invalidChars))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigSettingStringContainsIllegalChars, settingName, value, new string(invalidChars)));
		}
	}

	public static bool IsValidString(string value, string validCharactersExpression, out char[] invalidChars)
	{
		invalidChars = null;
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (string.IsNullOrEmpty(validCharactersExpression))
		{
			return true;
		}
		MatchCollection matchCollection = new Regex(validCharactersExpression).Matches(value);
		if (matchCollection.Count == 0)
		{
			return true;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Match item in matchCollection)
		{
			stringBuilder.Append(item.Value);
		}
		invalidChars = stringBuilder.ToString().Distinct().ToArray();
		return false;
	}

	public static bool IsValidStringLength(string value, int maxLength)
	{
		if (maxLength < 0)
		{
			return true;
		}
		if (value == null)
		{
			return false;
		}
		if (value.Length > maxLength)
		{
			return false;
		}
		return true;
	}

	public static void ValidateInteger(int value, int minValue, int maxValue, string settingName)
	{
		if (!IsValidInteger(value, minValue, maxValue))
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			numberFormatInfo.NumberDecimalDigits = 0;
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationSettingOutOfRange, settingName, minValue.ToString("N", numberFormatInfo), maxValue.ToString("N", numberFormatInfo)));
		}
	}

	public static bool IsValidInteger(int value, int minValue, int maxValue)
	{
		if (value >= minValue)
		{
			return value <= maxValue;
		}
		return false;
	}
}
