using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTestPageFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		switch (value.NclType)
		{
		case NavNclType.NavTestPage:
			return ((NavTestPageBase)value).ToString();
		case NavNclType.NavTestPageReference:
			return ((NavTestPageHandle)value).Target.ToString();
		case NavNclType.NavTestField:
		case NavNclType.NavTestAction:
			return value.ToString();
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}

	internal override string Format(NavValue value, int length, string format, FormatSettings formatSetting)
	{
		return string.Empty;
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		return null;
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}
}
