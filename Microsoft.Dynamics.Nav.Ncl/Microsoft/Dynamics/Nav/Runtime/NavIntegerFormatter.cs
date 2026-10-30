using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavIntegerFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> IntegerFieldNames = new ReadOnlyCollection<string>(new string[3] { "SIGN", "INTEGER", "INTEGER THOUSAND" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return value.ToInt32().ToString("d", NavCurrentThread.Session.WindowsCulture);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Integer);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.IntegerStdFormatStrings;
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Integer);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return IntegerFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string result = null;
		NavInteger navInteger = (NavInteger)value;
		options.Initialize(FormatSettings.IntegerTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch (options.Field)
		{
		case 3:
			result = ((navInteger.Value < 0) ? "-" : string.Empty);
			break;
		case 4:
		case 5:
		{
			string integerStr = navInteger.Value.ToString("d", NavCurrentThread.Session.WindowsCulture);
			return FormatThousandField(integerStr, options, settings);
		}
		}
		return result;
	}

	internal static string FormatThousandField(string integerStr, FormatOptions options, FormatSettings settings)
	{
		int num = integerStr.Length;
		int startIndex = 0;
		if (integerStr[0] == '-')
		{
			startIndex = 1;
			num--;
		}
		string text = integerStr.Substring(startIndex, num);
		num = text.Length;
		char c = ((options.Character1000 == '\u0002') ? settings.FieldFormatThousand : options.Character1000);
		if (c != '\0' && num > 3)
		{
			StringBuilder stringBuilder = new StringBuilder(25);
			for (int i = 0; i < num; i++)
			{
				if ((num - i) % 3 == 0 && i > 0)
				{
					stringBuilder.Append(c);
				}
				stringBuilder.Append(text[i]);
			}
			text = stringBuilder.ToString();
		}
		return text;
	}
}
