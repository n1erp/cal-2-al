using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBooleanFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> BooleanFieldNames = new ReadOnlyCollection<string>(new string[2] { "TEXT", "NUMBER" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		return NavFormatEvaluateHelper.FormatWithFormatNumber(value, length, formatNumber, formatsetting);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.OptionStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Char);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return BooleanFieldNames;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		NavBoolean navBoolean = (NavBoolean)value;
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
			if (!navBoolean.ToBoolean())
			{
				return settings.BooleanOptions[0];
			}
			return settings.BooleanOptions[1];
		case 4:
			return navBoolean.ToInt32().ToString(NavCurrentThread.Session.WindowsCulture);
		case 5:
			if (!navBoolean.ToBoolean())
			{
				return "false";
			}
			return "true";
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}
}
