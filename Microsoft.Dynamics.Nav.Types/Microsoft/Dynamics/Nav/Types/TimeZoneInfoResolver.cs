using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class TimeZoneInfoResolver
{
	internal const string ServerTimeZoneValue = "Server Time Zone";

	private static readonly Lazy<Dictionary<string, TimeZoneInfo>> validTimeZones = new Lazy<Dictionary<string, TimeZoneInfo>>(CreateValidTimeZoneDictionary);

	public static IDictionary<string, TimeZoneInfo> ValidTimeZones => validTimeZones.Value;

	private static Dictionary<string, TimeZoneInfo> CreateValidTimeZoneDictionary()
	{
		TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
		Dictionary<string, TimeZoneInfo> dictionary = new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase)
		{
			{
				"Server Time Zone",
				TimeZoneInfo.Local
			},
			{
				string.Empty,
				utcTimeZone
			},
			{ utcTimeZone.Id, utcTimeZone }
		};
		foreach (TimeZoneInfo item in from info in TimeZoneInfo.GetSystemTimeZones()
			where !info.Id.Equals(utcTimeZone.Id, StringComparison.OrdinalIgnoreCase)
			orderby info.BaseUtcOffset, info.DisplayName
			select info)
		{
			dictionary.Add(item.Id, item);
		}
		return dictionary;
	}

	public static TimeZoneInfo ResolveFromId(string id)
	{
		TimeZoneInfo utc = TimeZoneInfo.Utc;
		if (string.Equals(id, utc.Id, StringComparison.OrdinalIgnoreCase))
		{
			return utc;
		}
		if (string.Equals(id, "Server Time Zone", StringComparison.OrdinalIgnoreCase))
		{
			return TimeZoneInfo.Local;
		}
		if (validTimeZones.Value.TryGetValue(id, out var value))
		{
			return value;
		}
		IEnumerable<string> source = validTimeZones.Value.Where((KeyValuePair<string, TimeZoneInfo> validValuePair) =>
		{
			KeyValuePair<string, TimeZoneInfo> keyValuePair = validValuePair;
			return !string.IsNullOrEmpty(keyValuePair.Key);
		}).Select((KeyValuePair<string, TimeZoneInfo> validValuePair) =>
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			KeyValuePair<string, TimeZoneInfo> keyValuePair = validValuePair;
			string displayName = keyValuePair.Value.DisplayName;
			keyValuePair = validValuePair;
			return string.Format(currentCulture, "{0}: \"{1}\"", displayName, keyValuePair.Key);
		});
		throw new NavTimeZoneException(string.Format(CultureInfo.CurrentCulture, Lang.UnrecognizedTimeZoneId, id, string.Join(Environment.NewLine, source.ToArray())));
	}
}
