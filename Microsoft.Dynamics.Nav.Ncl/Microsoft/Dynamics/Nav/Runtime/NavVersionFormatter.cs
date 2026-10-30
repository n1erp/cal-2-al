using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavVersionFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u)
		{
			return FormatVersionAsString(value);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, value.NavType);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u)
		{
			return FormatSettings.DefaultStandardFormatString;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Version);
	}

	internal override string Format(NavValue value, int length, string format, FormatSettings formatSetting)
	{
		return FormatVersionAsString(value);
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return FormatVersionAsString(value);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	private static string FormatVersionAsString(NavValue value)
	{
		ByRefInstance<NavText> byRefInstance = new ByRefInstance<NavText>(NavText.Empty);
		NavVersion navVersion = (NavVersion)value;
		byRefInstance.Value = new NavText(byRefInstance.Value.MaxLength, navVersion.Value.ToString());
		return byRefInstance.Value;
	}
}
