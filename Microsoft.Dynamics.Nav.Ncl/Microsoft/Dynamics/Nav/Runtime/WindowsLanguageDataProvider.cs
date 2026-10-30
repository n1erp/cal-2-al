using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class WindowsLanguageDataProvider : EagerVirtualDataProvider
{
	private static class NativeMethods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int LCIDToLocaleName(uint locale, StringBuilder localeName, int localeNameSize, int flags);
	}

	private class CultureInfoComparer : IComparer<CultureInfo>
	{
		public int Compare(CultureInfo x, CultureInfo y)
		{
			if (x.LCID < y.LCID)
			{
				return -1;
			}
			if (x.LCID > y.LCID)
			{
				return 1;
			}
			return 0;
		}
	}

	private static readonly CultureInfoComparer cultureInfoComparer = new CultureInfoComparer();

	private static ConcurrentDictionary<int, bool> InstalledLanguages = new ConcurrentDictionary<int, bool>();

	private const int MakeLanguageId = 1024;

	private const int MaxLanguageId = 61439;

	private const string HelpFileName = "master.hh";

	private static readonly int[] PseudoLanguages = new int[3] { 1281, 1534, 2559 };

	public static readonly WindowsLanguageDataProvider Instance = new WindowsLanguageDataProvider();

	public static readonly CultureInfo[] AllCultures = GetSortedAllCultures();

	public override int TableId => 2000000045;

	private WindowsLanguageDataProvider()
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

	public static bool IsAvailableLanguage(int languageId)
	{
		CultureInfo cultureInfo = FindValidCulture(languageId);
		if (cultureInfo == null)
		{
			return false;
		}
		if (LocalizationExists(cultureInfo) && GloballyEnabled(NavCurrentThread.Session.Database.SecurityAndLicense.License, cultureInfo))
		{
			return true;
		}
		return false;
	}

	public static CultureInfo FindValidCulture(int languageId)
	{
		CultureInfo cultureInfo;
		try
		{
			cultureInfo = CultureInfo.GetCultureInfo(languageId);
		}
		catch (CultureNotFoundException)
		{
			return null;
		}
		int num = Array.BinarySearch(AllCultures, cultureInfo, cultureInfoComparer);
		return (num >= 0) ? AllCultures[num] : null;
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		return GetAllWindowsLanguageRecords();
	}

	internal static bool LoadLanguage(CultureInfo culture)
	{
		bool value = false;
		if (InstalledLanguages.TryGetValue(culture.LCID, out value))
		{
			return InstalledLanguages[culture.LCID];
		}
		using (DiagnosticsResolver.CreateTimingScope("000012K", Category.Runtime, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.CurrentCulture, "IsSatelliteAssemblyInstalled : {0}", culture))))
		{
			if (CustomTranslationResolver.IsSatelliteAssemblyInstalled((_AppDomain)AppDomain.CurrentDomain, culture))
			{
				value = true;
			}
		}
		InstalledLanguages[culture.LCID] = value;
		return InstalledLanguages[culture.LCID];
	}

	internal static List<int> LoadAndVerifyLanguages(List<int> supportedLanguages)
	{
		List<int> list = new List<int>();
		foreach (int supportedLanguage in supportedLanguages)
		{
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(supportedLanguage);
			if (LoadLanguage(cultureInfo))
			{
				list.Add(supportedLanguage);
			}
		}
		return list;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetAllWindowsLanguageRecords()
	{
		INavLicense licence = NavCurrentThread.Session.Database.SecurityAndLicense.License;
		CultureInfo[] allCultures = AllCultures;
		foreach (CultureInfo cultureInfo in allCultures)
		{
			bool flag = GloballyEnabled(licence, cultureInfo);
			bool flag2 = licence.HasLanguagePermission(cultureInfo.LCID, NavSession.LanguagePermission.Form);
			bool flag3 = licence.HasLanguagePermission(cultureInfo.LCID, NavSession.LanguagePermission.Report);
			bool flag4 = licence.HasLanguagePermission(cultureInfo.LCID, NavSession.LanguagePermission.DataPort);
			bool flag5 = licence.HasLanguagePermission(cultureInfo.LCID, NavSession.LanguagePermission.XmlPort);
			bool value = flag | flag2 | flag3 | flag4 | flag5;
			bool value2 = LocalizationExists(cultureInfo);
			yield return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(LanguageId(cultureInfo)), NavInteger.Create(PrimaryLanguageId(cultureInfo)), GetTruncatedTextValue(10, cultureInfo.DisplayName), GetTruncatedTextValue(11, AbbreviatedName(cultureInfo)), NavBoolean.Create(value), NavBoolean.Create(flag), NavBoolean.Create(flag2), NavBoolean.Create(flag3), NavBoolean.Create(flag4), NavBoolean.Create(flag5), GetTruncatedTextValue(30, cultureInfo.TextInfo.OEMCodePage.ToString(CultureInfo.InvariantCulture)), NavBoolean.Create(value2), NavBoolean.Create(value2), NavBoolean.Create(GetHelpFileExist(cultureInfo)), NavBoolean.Create(value2));
		}
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

	private static CultureInfo[] GetSortedAllCultures()
	{
		CultureInfo[] array = GetAllCultures().ToArray();
		Array.Sort(array, cultureInfoComparer);
		return array;
	}

	private static bool GloballyEnabled(INavLicense licence, CultureInfo culture)
	{
		return licence.HasLanguagePermission(culture.LCID, NavSession.LanguagePermission.Global);
	}

	private static bool LocalizationExists(CultureInfo culture)
	{
		if (!LoadLanguage(culture) && culture.LCID != 1033)
		{
			return false;
		}
		List<int> list = LoadAndVerifyLanguages(NavEnvironment.SupportedLanguages.ToList());
		if (!InstalledLanguages.Any() && culture.LCID == 1033)
		{
			return true;
		}
		if (list.Any())
		{
			return list.Contains(culture.LCID);
		}
		return true;
	}

	private static bool GetHelpFileExist(CultureInfo cultureInfo)
	{
		string path = Path.Combine(NavEnvironment.ApplicationAssemblyPath, cultureInfo.ThreeLetterWindowsLanguageName, "master.hh");
		return File.Exists(path);
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
