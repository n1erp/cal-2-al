using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTextFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> TextFieldNames = new ReadOnlyCollection<string>(new string[1] { "TEXT" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return ((NavStringValue)value).Value;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavNclTypeHelper.GetNavTypeFromNclType(value.NclType));
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.TextStdFormatStrings;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Text);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return TextFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		NavNclType nclType = value.NclType;
		if (nclType == NavNclType.NavOemCode || nclType == NavNclType.NavCode)
		{
			return ((NavCodeBase)value).AlphaIndex != byte.MaxValue;
		}
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		if (value.NclType == NavNclType.NavCode || value.NclType == NavNclType.NavOemCode)
		{
			options.LeftJustify = true;
		}
		return ((NavStringValue)value).Value;
	}
}
