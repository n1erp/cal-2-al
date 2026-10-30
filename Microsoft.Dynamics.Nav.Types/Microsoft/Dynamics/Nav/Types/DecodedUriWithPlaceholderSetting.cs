using System;
using System.Web;

namespace Microsoft.Dynamics.Nav.Types;

public class DecodedUriWithPlaceholderSetting : SimpleConfigSettingNoMaxValue<Uri>
{
	private static readonly string[] unencodedCharsThatNeedsEncoding = new string[2] { "{", "}" };

	private DecodedUriWithPlaceholderSetting()
	{
	}

	public DecodedUriWithPlaceholderSetting(string settingValue)
		: base(GetEncodedSettingsValue(settingValue))
	{
	}

	public static bool TryParse(string settingValue, out DecodedUriWithPlaceholderSetting result)
	{
		settingValue = GetEncodedSettingsValue(settingValue);
		if (!SimpleConfigSettingNoMaxValue<Uri>.TryParse(settingValue, out var result2))
		{
			result = null;
			return false;
		}
		result = new DecodedUriWithPlaceholderSetting
		{
			Value = result2.Value
		};
		return true;
	}

	public new static DecodedUriWithPlaceholderSetting Parse(string settingValue, string settingName)
	{
		settingValue = GetEncodedSettingsValue(settingValue);
		SimpleConfigSettingNoMaxValue<Uri> simpleConfigSettingNoMaxValue = SimpleConfigSettingNoMaxValue<Uri>.Parse(settingValue, settingName);
		return new DecodedUriWithPlaceholderSetting
		{
			Value = simpleConfigSettingNoMaxValue.Value
		};
	}

	private static string GetEncodedSettingsValue(string settingValue)
	{
		string[] array = unencodedCharsThatNeedsEncoding;
		foreach (string text in array)
		{
			settingValue = settingValue.Replace(text, HttpUtility.UrlEncode(text));
		}
		return settingValue;
	}
}
