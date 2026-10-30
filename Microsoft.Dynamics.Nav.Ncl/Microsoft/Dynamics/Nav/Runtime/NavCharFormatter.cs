using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavCharFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> CharFieldNames = new ReadOnlyCollection<string>(new string[3] { "CHAR/NUMBER", "CHAR", "NUMBER" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return value.ToString();
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Char);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.CharStdFormatStrings;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Char);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return CharFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		NavChar navChar = (NavChar)value;
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
			if (navChar.ToInt32() >= 32)
			{
				return navChar.Value.ToString();
			}
			return "<" + navChar.ToInt32().ToString(CultureInfo.InvariantCulture) + ">";
		case 4:
			return navChar.Value.ToString();
		case 5:
			return navChar.ToInt32().ToString(CultureInfo.InvariantCulture);
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}
}
