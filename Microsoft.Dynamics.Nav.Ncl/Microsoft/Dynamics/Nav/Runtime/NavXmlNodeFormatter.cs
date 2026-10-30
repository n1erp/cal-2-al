using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlNodeFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatNavXmlNodeAsString(value);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, value.NavType);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.DefaultStandardFormatString;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.JsonToken);
	}

	internal override string Format(NavValue value, int length, string format, FormatSettings formatSetting)
	{
		return FormatNavXmlNodeAsString(value);
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return FormatNavXmlNodeAsString(value);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	private static string FormatNavXmlNodeAsString(NavValue value)
	{
		ByRefInstance<NavText> byRefInstance = new ByRefInstance<NavText>(NavText.Empty);
		NavXmlNode navXmlNode = (NavXmlNode)value;
		navXmlNode.ALWriteTo(DataError.ThrowError, byRefInstance);
		return byRefInstance.Value;
	}
}
