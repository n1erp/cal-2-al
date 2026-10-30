using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavByteFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> ByteFieldNames = new ReadOnlyCollection<string>(new string[3] { "CHAR/NUMBER", "CHAR", "NUMBER" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return NCLManagedAdapter.ByteToTextChar(value.ToByte()).ToString();
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Byte);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.CharStdFormatStrings;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Byte);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return ByteFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		char c = NCLManagedAdapter.ByteToTextChar(value.ToByte());
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
		{
			if (c >= ' ')
			{
				return c.ToString();
			}
			int num = c;
			return "<" + num.ToString(CultureInfo.InvariantCulture) + ">";
		}
		case 4:
			return c.ToString();
		case 5:
		{
			int num = c;
			return num.ToString(CultureInfo.InvariantCulture);
		}
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}
}
