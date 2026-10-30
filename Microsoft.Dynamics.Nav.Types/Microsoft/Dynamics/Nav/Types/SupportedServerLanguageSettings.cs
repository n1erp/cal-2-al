using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

public class SupportedServerLanguageSettings
{
	private readonly List<LanguageSetting> languages;

	public string Value { get; }

	public IEnumerable<int> SupportedLanguageIds => languages?.Select((LanguageSetting lang) => lang.LCID) ?? Enumerable.Empty<int>();

	public IEnumerable<string> SupportedLanguagesStrings => languages?.Select((LanguageSetting lang) => lang.Value) ?? Enumerable.Empty<string>();

	public SupportedServerLanguageSettings(string settingValue, string settingName)
	{
		languages = new List<LanguageSetting>();
		string[] array = settingValue.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string settingValue2 in array)
		{
			languages.Add(LanguageSetting.Parse(settingValue2, settingName));
		}
		Value = settingValue;
	}

	public static SupportedServerLanguageSettings Parse(string settingsValue, string settingName)
	{
		return new SupportedServerLanguageSettings(settingsValue, settingName);
	}

	public override string ToString()
	{
		return Value;
	}
}
