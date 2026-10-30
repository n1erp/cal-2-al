using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Dynamics.Nav.Model.TypeSystem.Helpers;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class WindowsLanguageHelper
{
	private static class NativeMethods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int LCIDToLocaleName(uint locale, StringBuilder localeName, int localeNameSize, int flags);
	}

	private const int MakeLanguageId = 1024;

	private const int MaxLanguageId = 61439;

	private static readonly int[] PseudoLanguages;

	public static readonly WindowsLanguageHelper Instance;

	public static readonly SortedDictionary<int, CultureInfo> AllCulturesByLanguageId;

	public static readonly SortedDictionary<string, CultureInfo> AllCulturesByAbbreviatedName;

	public IEnumerable<CultureInfo> AllCultures => AllCulturesByLanguageId.Values;

	static WindowsLanguageHelper()
	{
		PseudoLanguages = new int[3] { 1281, 1534, 2559 };
		Instance = new WindowsLanguageHelper();
		AllCulturesByLanguageId = new SortedDictionary<int, CultureInfo>();
		GetAllCultures().ForEach((CultureInfo c) =>
		{
			AllCulturesByLanguageId.Add(c.LCID, c);
		});
		AllCulturesByAbbreviatedName = new SortedDictionary<string, CultureInfo>();
		AllCulturesByLanguageId.Values.ForEach((CultureInfo c) =>
		{
			AllCulturesByAbbreviatedName[c.ThreeLetterWindowsLanguageName] = c;
		});
	}

	private WindowsLanguageHelper()
	{
	}

	public static int PrimaryLanguageId(CultureInfo culture)
	{
		return LanguageIdToPrimaryLanguageId(culture.LCID);
	}

	public static int LanguageId(CultureInfo culture)
	{
		return culture.LCID;
	}

	public static string AbbreviatedName(CultureInfo culture)
	{
		return culture.ThreeLetterWindowsLanguageName;
	}

	public static CultureInfo FindCulture(int languageId)
	{
		if (AllCulturesByLanguageId.TryGetValue(languageId, out var value))
		{
			return value;
		}
		return null;
	}

	public static CultureInfo FindCulture(string threeLetterName)
	{
		if (AllCulturesByAbbreviatedName.TryGetValue(threeLetterName, out var value))
		{
			return value;
		}
		return null;
	}

	internal static IEnumerable<CultureInfo> GetDistinctCultures()
	{
		HashSet<int> lcid = new HashSet<int>();
		HashSet<string> threeLetterWindowsLanguageName = new HashSet<string>();
		HashSet<string> displayName = new HashSet<string>();
		foreach (CultureInfo item in from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
			where ValidLanguageId(c.LCID)
			orderby LanguageIdToPrimaryLanguageId(c.LCID), c.LCID
			select c)
		{
			if (!lcid.Contains(item.LCID) && !threeLetterWindowsLanguageName.Contains(item.ThreeLetterWindowsLanguageName) && !displayName.Contains(item.DisplayName))
			{
				lcid.Add(item.LCID);
				threeLetterWindowsLanguageName.Add(item.ThreeLetterWindowsLanguageName);
				displayName.Add(item.DisplayName);
				yield return item;
			}
		}
	}

	private static IEnumerable<CultureInfo> GetAllCultures()
	{
		HashSet<int> primaryLanguageId = new HashSet<int>();
		foreach (CultureInfo culture in GetDistinctCultures())
		{
			int num = LanguageIdToPrimaryLanguageId(culture.LCID);
			if (!primaryLanguageId.Contains(num))
			{
				primaryLanguageId.Add(num);
				if (num != culture.LCID)
				{
					CultureInfo cultureInfo = null;
					StringBuilder localeName = new StringBuilder();
					if (NativeMethods.LCIDToLocaleName((uint)num, localeName, 0, 0) > 0)
					{
						try
						{
							cultureInfo = CultureInfo.GetCultureInfo(num);
						}
						catch (CultureNotFoundException)
						{
						}
					}
					if (cultureInfo != null)
					{
						yield return cultureInfo;
					}
				}
			}
			yield return culture;
		}
	}

	private static int LanguageIdToPrimaryLanguageId(int languageId)
	{
		return languageId % 1024 + 1024;
	}

	private static bool ValidLanguageId(int languageId)
	{
		if (languageId < 1024 || languageId > 61439 || languageId % 1024 == 0)
		{
			return false;
		}
		if (IsPseudoLanguage(languageId))
		{
			return false;
		}
		return true;
	}

	private static bool IsPseudoLanguage(int languageId)
	{
		return Array.IndexOf(PseudoLanguages, languageId) > 0;
	}
}
