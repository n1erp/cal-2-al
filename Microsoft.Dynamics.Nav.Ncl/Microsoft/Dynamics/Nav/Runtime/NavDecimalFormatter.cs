using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDecimalFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> DecimalFieldNames = new ReadOnlyCollection<string>(new string[4] { "SIGN", "INTEGER", "DECIMALS", "INTEGER THOUSAND" });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 4u || formatNumber == 9)
		{
			return NavFormatEvaluateHelper.FormatWithFormatNumber(value, length, formatNumber, formatsetting);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Decimal);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 4u || formatNumber == 9)
		{
			return FormatSettings.DecimalStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Decimal);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return DecimalFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string text = null;
		NavDecimal navDecimal = (NavDecimal)value;
		options.Initialize(FormatSettings.DecimalTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch ((char)(ushort)options.Field)
		{
		case '\u0003':
			text = ((navDecimal.Value < 0) ? "-" : string.Empty);
			break;
		case '\u0004':
		case '\u0006':
		{
			string text2 = ((options.PrecisionMax < byte.MaxValue) ? Decimal18.Round(navDecimal.ToDecimal(), options.PrecisionMax) : navDecimal.ToDecimal()).ToString("f18", CultureInfo.InvariantCulture).TrimEnd('0');
			int num3 = text2.IndexOf('.');
			if (num3 == -1)
			{
				num3 = text2.Length;
			}
			int startIndex = 0;
			if (text2[0] == '-')
			{
				startIndex = 1;
				num3--;
			}
			text = text2.Substring(startIndex, num3);
			num3 = text.Length;
			char c = ((options.Character1000 == '\u0002') ? settings.FieldFormatThousand : options.Character1000);
			if (c == '\0' || num3 <= 3)
			{
				break;
			}
			StringBuilder stringBuilder = new StringBuilder(25);
			for (int i = 0; i < num3; i++)
			{
				if ((num3 - i) % 3 == 0 && i > 0)
				{
					stringBuilder.Append(c);
				}
				stringBuilder.Append(text[i]);
			}
			text = stringBuilder.ToString();
			break;
		}
		case '\u0005':
		{
			text = string.Empty;
			string text2 = ((options.PrecisionMax < byte.MaxValue) ? Decimal18.Round(navDecimal.ToDecimal(), options.PrecisionMax) : navDecimal.ToDecimal()).ToString("f18", CultureInfo.InvariantCulture).TrimEnd('0');
			int num = text2.IndexOf('.');
			int num2 = text2.Length - (num + 1);
			if (num2 != 0 || options.PrecisionMin != 0 || options.Length > 0)
			{
				text += ((options.Comma == '\u0001') ? settings.FieldFormatComma : options.Comma);
				if (num2 != 0)
				{
					text += text2.Substring(num + 1, num2);
				}
			}
			if (options.Length == 0 && num2 < options.PrecisionMin + 1 && options.FillerCharacter != '\0' && options.PrecisionMin != 0)
			{
				options.Length = options.PrecisionMin + 1;
			}
			break;
		}
		}
		return text;
	}
}
