using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class LanguageSetting
{
	private static class LanguageSettingValidator
	{
		public const int MinimumValue = 1024;

		public const int MaximumValue = 61439;

		internal static bool IsValidLanguageId(int lcid)
		{
			if (lcid <= 61439 && lcid > 1024)
			{
				return lcid % 1024 != 0;
			}
			return false;
		}
	}

	private string settingValue;

	private CultureInfo cultureInfo;

	private string settingName = string.Empty;

	public int LCID => cultureInfo.LCID;

	public string Value
	{
		get
		{
			return settingValue;
		}
		set
		{
			SetLanguageSetting(value);
		}
	}

	public CultureInfo CultureInfo => cultureInfo;

	public LanguageSetting(string settingValue)
		: this(settingValue, string.Empty)
	{
	}

	public LanguageSetting(string settingValue, string settingName)
	{
		this.settingName = settingName;
		SetLanguageSetting(settingValue);
	}

	private void SetLanguageSetting(string language)
	{
		if (!string.IsNullOrEmpty(language))
		{
			CultureInfo cultureInfo = null;
			try
			{
				cultureInfo = ConfigSettings.ParseCultureInfo(language);
			}
			catch (ArgumentNullException)
			{
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			catch (CultureNotFoundException)
			{
			}
			if (cultureInfo == null)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLanguageCode, language));
			}
			this.cultureInfo = cultureInfo;
			settingValue = language;
			if (!LanguageSettingValidator.IsValidLanguageId(LCID))
			{
				throw new NavConfigurationException(string.Format(CultureInfo.InvariantCulture, "Configuration value {0}:'{1}' contains invalid language Id(s). Language Id should resolve to LCID between {2} and {3}.", settingName, settingValue, 1024, 61439));
			}
		}
	}

	public override string ToString()
	{
		return settingValue;
	}

	public static LanguageSetting Parse(string settingValue, string settingName = "")
	{
		return new LanguageSetting(settingValue, settingName);
	}

	public static bool TryParse(string settingValue, out LanguageSetting result)
	{
		try
		{
			result = Parse(settingValue);
			return true;
		}
		catch (NavConfigurationException)
		{
			result = null;
			return false;
		}
	}
}
