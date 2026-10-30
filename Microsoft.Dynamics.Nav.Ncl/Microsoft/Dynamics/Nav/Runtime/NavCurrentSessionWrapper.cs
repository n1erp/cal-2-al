using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavCurrentSessionWrapper
{
	private class AppInitFallbackValues
	{
		public FormatSettings DefaultFormatSettings { get; }

		public ClientSettings DefaultRegionalSettings { get; }

		public CultureInfo DefaultWindowsCulture { get; }

		public TimeZoneInfo DefaultClientTimeZone { get; }

		public AppInitFallbackValues()
		{
			int defaultLanguage = NavEnvironment.DefaultLanguage;
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(defaultLanguage);
			ClientSettings clientSettings = default;
			clientSettings.Refresh(cultureInfo, cultureInfo);
			FormatSettings formatSettings = FormatSettings.Create(defaultLanguage, clientSettings);
			DefaultFormatSettings = formatSettings;
			DefaultRegionalSettings = clientSettings;
			DefaultWindowsCulture = cultureInfo;
			DefaultClientTimeZone = TimeZoneInfo.Utc;
		}
	}

	private static readonly Lazy<AppInitFallbackValues> appInitFallbackValues = new Lazy<AppInitFallbackValues>(() => new AppInitFallbackValues(), isThreadSafe: true);

	public static FormatSettings FormatSettings => NavCurrentThread.Session?.FormatSettings ?? appInitFallbackValues.Value.DefaultFormatSettings;

	public static ClientSettings RegionalSettings => NavCurrentThread.Session?.RegionalSettings ?? appInitFallbackValues.Value.DefaultRegionalSettings;

	public static CultureInfo WindowsCulture => NavCurrentThread.Session?.WindowsCulture ?? appInitFallbackValues.Value.DefaultWindowsCulture;

	public static TimeZoneInfo ClientTimeZone => NavCurrentThread.Session?.ClientTimeZone ?? appInitFallbackValues.Value.DefaultClientTimeZone;
}
