using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavJsonFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u)
		{
			return FormatJsonAsString(value);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, value.NavType);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u)
		{
			return FormatSettings.DefaultStandardFormatString;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.JsonToken);
	}

	internal override string Format(NavValue value, int length, string format, FormatSettings formatSetting)
	{
		return FormatJsonAsString(value);
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return FormatJsonAsString(value);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	private static string FormatJsonAsString(NavValue value)
	{
		ByRefInstance<NavText> byRefInstance = new ByRefInstance<NavText>(NavText.Empty);
		NavJsonToken navJsonToken = (NavJsonToken)value;
		navJsonToken.ALWriteTo(DataError.ThrowError, byRefInstance);
		return byRefInstance.Value;
	}
}
