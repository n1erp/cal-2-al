using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ObjectDisplay
{
	internal static string FormatLiteral(int value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	internal static string FormatLiteral(string value)
	{
		return value.QuoteLiteral();
	}

	internal static string FormatDate(int date)
	{
		if (date == 0)
		{
			return "0D";
		}
		return DateTimeUtilities.FormatCSideDate(date, "yyyyMMdd") + "D";
	}

	internal static string FormatTime(int time)
	{
		if (time == 0)
		{
			return "0T";
		}
		return DateTimeUtilities.FormatCSideTime(time, "HHmmss") + "T";
	}
}
