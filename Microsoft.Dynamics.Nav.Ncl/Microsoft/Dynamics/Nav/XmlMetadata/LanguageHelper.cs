using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Runtime;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

internal class LanguageHelper
{
	private readonly Dictionary<int, int> primaryLanguageIds;

	private readonly Dictionary<int, string> languageThreeLetterNames;

	private readonly Dictionary<string, int> languageIdsByAbbreviatedNames;

	private static volatile LanguageHelper instance;

	private static LanguageHelper Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new LanguageHelper();
			}
			return instance;
		}
	}

	private LanguageHelper()
	{
		int capacity = WindowsLanguageDataProvider.AllCultures.Length;
		primaryLanguageIds = new Dictionary<int, int>(capacity);
		languageThreeLetterNames = new Dictionary<int, string>(capacity);
		languageIdsByAbbreviatedNames = new Dictionary<string, int>(capacity);
		CultureInfo[] allCultures = WindowsLanguageDataProvider.AllCultures;
		foreach (CultureInfo culture in allCultures)
		{
			int num = WindowsLanguageDataProvider.LanguageId(culture);
			int value = WindowsLanguageDataProvider.PrimaryLanguageId(culture);
			string text = WindowsLanguageDataProvider.AbbreviatedName(culture);
			if (!languageIdsByAbbreviatedNames.ContainsKey(text))
			{
				primaryLanguageIds.Add(num, value);
				languageThreeLetterNames.Add(num, text);
				languageIdsByAbbreviatedNames.Add(text, num);
			}
		}
	}

	public static int GetPrimaryLanguageId(int languageId)
	{
		if (Instance.primaryLanguageIds.TryGetValue(languageId, out var value))
		{
			return value;
		}
		return languageId;
	}

	public static string GetAbbreviatedLanguageName(int languageId)
	{
		if (Instance.languageThreeLetterNames.TryGetValue(languageId, out var value))
		{
			return value;
		}
		return "ENU";
	}

	public static int GetLanguageIdByAbbreviatedName(string abbreviatedName)
	{
		if (Instance.languageIdsByAbbreviatedNames.TryGetValue(abbreviatedName, out var value))
		{
			return value;
		}
		return NavResourceManager.DefaultLanguageId;
	}
}
