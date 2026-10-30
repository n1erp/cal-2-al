using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class ServicesTimeZone
{
	private readonly TimeZoneInfo timeZone;

	private readonly string originalSettingValue;

	public TimeZoneInfo Value => timeZone;

	public ServicesTimeZone(string settingValue)
		: this(settingValue, string.Empty)
	{
	}

	public ServicesTimeZone(string settingValue, string settingName)
	{
		originalSettingValue = settingValue;
		try
		{
			timeZone = TimeZoneInfoResolver.ResolveFromId(settingValue);
		}
		catch (NavTimeZoneException exception)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(TimeZoneInfo).Name), exception);
		}
	}

	public static ServicesTimeZone Parse(string settingValue, string settingName)
	{
		return new ServicesTimeZone(settingValue, settingName);
	}

	public static bool TryParse(string settingValue, out ServicesTimeZone result)
	{
		try
		{
			result = new ServicesTimeZone(settingValue);
			return true;
		}
		catch (NavConfigurationException)
		{
			result = null;
			return false;
		}
	}

	public override string ToString()
	{
		return originalSettingValue;
	}
}
