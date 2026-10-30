using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateFormulaFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		return NavFormatEvaluateHelper.FormatWithFormatNumber(value, length, formatNumber, formatsetting);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.DateFormulaStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Duration);
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return Format((NavDateFormula)value, settings, format[pos++]);
	}

	public static string Format(NavDateFormula dateFormula, FormatSettings formatSettings, int standardFormatNumber)
	{
		return Format(dateFormula.Value, formatSettings, standardFormatNumber);
	}

	public static string Format(string dateFormula, FormatSettings formatSettings, int standardFormatNumber)
	{
		bool flag = standardFormatNumber == 2 || standardFormatNumber == 9;
		bool flag2 = standardFormatNumber == 2;
		string[] array = (flag ? FormatSettings.GlobalDateQuantor : formatSettings.DateQuantor);
		StringBuilder stringBuilder = new StringBuilder(dateFormula.Length + 10);
		if (flag2)
		{
			stringBuilder.Append('<');
		}
		foreach (char c in dateFormula)
		{
			if (c > '\0' && c <= array.Length)
			{
				stringBuilder.Append(array[c - 1]);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		if (flag2)
		{
			stringBuilder.Append('>');
		}
		return stringBuilder.ToString();
	}
}
