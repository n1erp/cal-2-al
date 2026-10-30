using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDurationFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		return NavFormatEvaluateHelper.FormatWithFormatNumber(value, length, formatNumber, formatsetting);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.DurationStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Duration);
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return ((NavDuration)value).Format(settings, format[pos++]);
	}
}
