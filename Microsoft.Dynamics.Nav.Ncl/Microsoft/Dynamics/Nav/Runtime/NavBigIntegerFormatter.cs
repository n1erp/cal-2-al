using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBigIntegerFormatter : NavIntegerFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return value.ToInt64().ToString("d", NavCurrentThread.Session.WindowsCulture);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Integer);
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string result = null;
		NavBigInteger navBigInteger = (NavBigInteger)value;
		options.Initialize(FormatSettings.IntegerTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
			result = ((navBigInteger.Value < 0) ? "-" : string.Empty);
			break;
		case 4:
		case 5:
		{
			string integerStr = navBigInteger.Value.ToString("d", NavCurrentThread.Session.WindowsCulture);
			return NavIntegerFormatter.FormatThousandField(integerStr, options, settings);
		}
		}
		return result;
	}
}
