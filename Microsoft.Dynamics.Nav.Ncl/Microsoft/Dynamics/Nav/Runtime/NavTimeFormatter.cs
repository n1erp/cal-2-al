using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTimeFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> TimeFieldNames = new ReadOnlyCollection<string>(new string[7] { "HOURS24", "HOURS12", "MINUTES", "SECONDS", "THOUSANDS", "AM/PM", "SECOND DEC." });

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatSetting)
	{
		NavTime navTime = (NavTime)value;
		if (navTime.IsZeroOrEmpty)
		{
			return string.Empty;
		}
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return NavFormatEvaluateHelper.FormatWithFormatNumber(navTime, length, formatNumber, formatSetting);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Time);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return NavCurrentThread.Session.FormatSettings.TimeStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Time);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return TimeFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string text = null;
		NavTime navTime = (NavTime)value;
		options.Initialize(FormatSettings.TimeTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		switch ((char)(ushort)options.Field)
		{
		case '\u0003':
		case '\n':
			text = navTime.Value.Hour.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0004':
		case '\v':
			text = ((navTime.Value.Hour + 11) % 12 + 1).ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0005':
			text = navTime.Value.Minute.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0006':
			text = navTime.Value.Second.ToString(CultureInfo.InvariantCulture);
			break;
		case '\a':
			text = navTime.Value.Millisecond.ToString(CultureInfo.InvariantCulture);
			break;
		case '\b':
			text = ((navTime.Value.Hour < 12) ? settings.TimeAMDesignator : settings.TimePMDesignator);
			break;
		case '\t':
		{
			text = string.Empty;
			int num = navTime.Value.Millisecond;
			int num2 = 100;
			if (num != 0)
			{
				int num3 = 1;
				text += ((options.Comma == '\u0001') ? settings.FieldFormatComma : options.Comma);
				while (num != 0)
				{
					text += (char)(num / num2 + 48);
					num -= (text[num3] - 48) * num2;
					num3++;
					num2 /= 10;
				}
			}
			break;
		}
		}
		return text;
	}
}
