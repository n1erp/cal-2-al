using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class MultiLanguageHelper
{
	private static class NativeMethods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int LCIDToLocaleName(uint locale, StringBuilder localeName, int localeNameSize, int flags);
	}

	private static readonly Lazy<ImmutableArray<CultureInfo>> validCultures = new Lazy<ImmutableArray<CultureInfo>>(GetValidCultures);

	private static readonly Lazy<ImmutableDictionary<string, CultureInfo>> threeLetterToCulture = new Lazy<ImmutableDictionary<string, CultureInfo>>(GetThreeLetterToCulture);

	private const int MakeLanguageId = 1024;

	private const int MaxLanguageId = 61439;

	private static readonly int[] PseudoLanguages = new int[3] { 1281, 1534, 2559 };

	private static IEnumerable<CultureInfo> GetDistinctCultures()
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
		HashSet<string> primaryWindowsLangugeIds = new HashSet<string>();
		foreach (CultureInfo culture in GetDistinctCultures())
		{
			int num = LanguageIdToPrimaryLanguageId(culture.LCID);
			if (!primaryWindowsLangugeIds.Contains(culture.ThreeLetterWindowsLanguageName))
			{
				primaryWindowsLangugeIds.Add(culture.ThreeLetterWindowsLanguageName);
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
							DebugAssertHelper.Assert(condition: false, "The exception should never happen.");
						}
					}
					if (cultureInfo != null && !primaryWindowsLangugeIds.Contains(cultureInfo.ThreeLetterWindowsLanguageName))
					{
						primaryWindowsLangugeIds.Add(cultureInfo.ThreeLetterWindowsLanguageName);
						yield return cultureInfo;
					}
				}
			}
			yield return culture;
		}
	}

	private static ImmutableArray<CultureInfo> GetValidCultures()
	{
		return GetAllCultures().ToImmutableArrayOrEmpty();
	}

	private static ImmutableDictionary<string, CultureInfo> GetThreeLetterToCulture()
	{
		return validCultures.Value.ToImmutableDictionary((CultureInfo c) => c.ThreeLetterWindowsLanguageName, (CultureInfo c) => c);
	}

	internal static bool TryFindCulture(string threeLetterCode, out int lcid)
	{
		lcid = 0;
		if (threeLetterToCulture.Value.TryGetValue(threeLetterCode, out var value))
		{
			lcid = value.LCID;
			return true;
		}
		return false;
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

	internal static bool IsCustomCulture(CultureInfo culture)
	{
		return (culture.CultureTypes & CultureTypes.UserCustomCulture) != 0;
	}

	internal static bool HasCountryRegion(CultureInfo culture)
	{
		return (culture.CultureTypes & CultureTypes.SpecificCultures) != 0;
	}
}
