using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavFormatEvaluateHelper
{
	internal enum FormatAttributeId
	{
		StandardFormat = 24,
		Character1000,
		Comma,
		Overflow,
		FillerCharacter,
		Precision
	}

	private static readonly IList<string> AttributeNames = new List<string>(new string[6] { "STANDARD FORMAT", "1000CHARACTER", "COMMA", "OVERFLOW", "FILLER CHARACTER", "PRECISION" }).AsReadOnly();

	public static string Format(NavValue value)
	{
		return Format(value, 0, 0);
	}

	public static string Format(object value)
	{
		return Format(value, 0, 0);
	}

	public static string Format(NavValue value, int length)
	{
		return Format(value, length, 0);
	}

	public static string Format(object value, int length)
	{
		return Format(value, length, 0);
	}

	public static string Format(object value, int length, int number)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is NavValue value2)
		{
			return Format(value2, length, number);
		}
		return NavGenericFormatter.FormatDotNetObject(value, number);
	}

	public static string Format(object value, int length, string formatSpecifier)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is NavValue value2)
		{
			return Format(value2, length, formatSpecifier);
		}
		return NavGenericFormatter.FormatDotNetObject(value, 0);
	}

	public static string Format(NavValue value, int length, int number)
	{
		return Format(value, length, number, NavCurrentThread.Session?.FormatSettings ?? NavSession.InvariantFormatSettings);
	}

	public static string Format(NavValue value, int length, int number, FormatSettings formatSettings)
	{
		if (Math.Abs(length) > 2048)
		{
			throw new NavNCLStringLengthExceededException(2048);
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is NavRecord.NavFieldRefShallow navFieldRefShallow && value.IsInitialized && navFieldRefShallow.OwnerRecord != null)
		{
			string text = CalculateDecimalFormatString(navFieldRefShallow, number);
			if (!string.IsNullOrEmpty(text) && navFieldRefShallow.ALType == NavType.Decimal)
			{
				return Format(navFieldRefShallow.InnerValue, length, text);
			}
			value = navFieldRefShallow.InnerValue;
		}
		return NavValueFormatter.GetFormatter(value).Format(value, length, number, formatSettings);
	}

	public static string Format(NavValue value, int length, string formatSpecifier)
	{
		if (formatSpecifier == null)
		{
			throw new ArgumentNullException("formatSpecifier");
		}
		if (length > 2048)
		{
			throw new NavNCLStringLengthExceededException(2048);
		}
		if (value is NavRecord.NavFieldRefShallow navFieldRefShallow && value.IsInitialized && navFieldRefShallow.OwnerRecord != null)
		{
			string text = ExtractPrecisionString(navFieldRefShallow);
			if (!string.IsNullOrEmpty(text) && navFieldRefShallow.ALType == NavType.Decimal)
			{
				return Format(navFieldRefShallow.InnerValue, length, text + formatSpecifier);
			}
			value = navFieldRefShallow.InnerValue;
		}
		if ((value.NclType == NavNclType.NavVariant || value.NclType == NavNclType.NavFieldRef) && value.IsInitialized)
		{
			value = value.InnerValue;
		}
		return NavValueFormatter.GetFormatter(value).Format(value, length, formatSpecifier, NavCurrentThread.Session.FormatSettings);
	}

	private static string CalculateDecimalFormatString(NavRecord.NavFieldRefShallow fieldrefValue, int number)
	{
		string text = fieldrefValue.OwnerRecord.GetDecimalString(fieldrefValue.FieldNo);
		if (!string.IsNullOrEmpty(text) && number != 0)
		{
			int num = text.LastIndexOf("<Standard Format,", StringComparison.OrdinalIgnoreCase);
			if (num >= 0)
			{
				int length = "<Standard Format,".Length;
				int num2 = text.IndexOf('>', num);
				if (num2 > 0)
				{
					text = text.Substring(0, num + length) + number.ToString(CultureInfo.InvariantCulture) + text.Substring(num2);
				}
			}
		}
		return text;
	}

	private static string ExtractPrecisionString(NavRecord.NavFieldRefShallow fieldrefValue)
	{
		string decimalString = fieldrefValue.OwnerRecord.GetDecimalString(fieldrefValue.FieldNo);
		if (!string.IsNullOrEmpty(decimalString))
		{
			int num = decimalString.LastIndexOf("<Precision,", StringComparison.OrdinalIgnoreCase);
			if (num >= 0)
			{
				int num2 = decimalString.IndexOf('>', num);
				if (num2 > 0)
				{
					return decimalString.Substring(num, num2 - num + 1);
				}
			}
		}
		return string.Empty;
	}

	private static bool GetKeyValue(string format, ref int pos, out string key, out string value, IList<string> expected)
	{
		key = string.Empty;
		value = string.Empty;
		if (pos == format.Length || format[pos] != '<')
		{
			return false;
		}
		int num = pos;
		while (format[num] != '>' && format[num] != ',')
		{
			if (++num == format.Length)
			{
				throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
			}
		}
		key = format.Substring(pos + 1, num - pos - 1);
		if (expected != null && LookupName(expected, key) < 0)
		{
			return false;
		}
		if (format[num] == ',')
		{
			num++;
			int num2 = num;
			int num3 = 0;
			while (num2 < format.Length && (format[num2] != '>' || num3 > 0))
			{
				switch (format[num2++])
				{
				case '<':
					num3++;
					break;
				case '>':
					num3--;
					break;
				}
			}
			if (num2 == format.Length)
			{
				throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
			}
			value = format.Substring(num, num2 - num);
			num = num2;
		}
		pos = num + 1;
		return true;
	}

	internal static char ParseChar(string value, string format)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
		}
		if (value[0] != '<')
		{
			if (value.Length != 1)
			{
				throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
			}
			return value[0];
		}
		if (!uint.TryParse(value.Substring(1, value.Length - 2), out var result))
		{
			throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
		}
		return Convert.ToChar(result);
	}

	private static char ParseAttributeValueChar(FormatAttributeId attrId, string value, string format)
	{
		switch (attrId)
		{
		case FormatAttributeId.StandardFormat:
		{
			if (!int.TryParse(value, out var result3))
			{
				throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
			}
			return Convert.ToChar(result3);
		}
		case FormatAttributeId.Precision:
		{
			int num = value.IndexOf(':');
			byte result = 0;
			byte result2 = byte.MaxValue;
			if (num < 0)
			{
				if (!byte.TryParse(value, out result))
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				result2 = result;
			}
			else
			{
				string text = value.Substring(0, num).Trim();
				if (text.Length > 0 && !byte.TryParse(value.Substring(0, num), out result))
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				string text2 = value.Substring(num + 1).Trim();
				if (text2.Length > 0 && !byte.TryParse(value.Substring(num + 1), out result2))
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
			}
			return Convert.ToChar((result << 8) | result2);
		}
		default:
			return ParseChar(value, format);
		}
	}

	internal static string EvaluateFormatString(NavValue navValue, string format, out int number)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IList<string> fieldNames = NavValueFormatter.GetFormatter(navValue).GetFieldNames();
		number = -1;
		int pos = 0;
		while (pos < format.Length)
		{
			if (format[pos] != '<')
			{
				if (Convert.ToInt32(format[pos]) < 32)
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				stringBuilder.Append(format[pos++]);
				continue;
			}
			GetKeyValue(format, ref pos, out var key, out var value, null);
			if (string.IsNullOrEmpty(value) && int.TryParse(key, out var result))
			{
				if (result < 32)
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				stringBuilder.Append(char.ConvertFromUtf32(result));
				continue;
			}
			int num = LookupName(fieldNames, key);
			if (num >= 0)
			{
				int result2 = 0;
				if ((!string.IsNullOrEmpty(value) && !int.TryParse(value, out result2)) || result2 < 0 || result2 > 256)
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				stringBuilder.Append(Convert.ToChar(num + 3));
				stringBuilder.Append(Convert.ToChar(result2));
				while (GetKeyValue(format, ref pos, out key, out value, AttributeNames))
				{
					if (string.IsNullOrEmpty(value))
					{
						throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
					}
					int num2 = LookupName(AttributeNames, key);
					FormatAttributeId formatAttributeId = (FormatAttributeId)(num2 + 24);
					stringBuilder.Append(Convert.ToChar((int)formatAttributeId));
					stringBuilder.Append(ParseAttributeValueChar(formatAttributeId, value, format));
				}
			}
			else
			{
				num = LookupName(AttributeNames, key);
				if (num < 0 || string.IsNullOrEmpty(value))
				{
					throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, format);
				}
				FormatAttributeId formatAttributeId2 = (FormatAttributeId)(num + 24);
				stringBuilder.Append(Convert.ToChar((int)formatAttributeId2));
				char c = ParseAttributeValueChar(formatAttributeId2, value, format);
				stringBuilder.Append(c);
				if (formatAttributeId2 == FormatAttributeId.StandardFormat)
				{
					number = c;
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static int LookupName(IList<string> fieldNames, string token)
	{
		int result = -1;
		if (fieldNames != null)
		{
			for (int i = 0; i < fieldNames.Count; i++)
			{
				if (token.Equals(fieldNames[i], StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			for (int j = 0; j < fieldNames.Count; j++)
			{
				if (fieldNames[j].StartsWith(token, StringComparison.OrdinalIgnoreCase))
				{
					result = j;
					break;
				}
			}
		}
		return result;
	}

	private static string FormatUsingOptions(string value, FormatOptions options)
	{
		if (options.Length == 0)
		{
			return value;
		}
		if (options.Length < value.Length)
		{
			if (options.Overflow == '\0')
			{
				return value.Substring(0, options.Length);
			}
			return new string(options.Overflow, options.Length);
		}
		if (options.Length > value.Length)
		{
			if (!options.LeftJustify)
			{
				return value + new string((options.FillerCharacter == '\0') ? ' ' : options.FillerCharacter, options.Length - value.Length);
			}
			return new string((options.FillerCharacter == '\0') ? ' ' : options.FillerCharacter, options.Length - value.Length) + value;
		}
		return value;
	}

	internal static string GetStandardFormat(NavValue value, int formatType)
	{
		return NavValueFormatter.GetFormatter(value).GetStandardFormat(formatType);
	}

	private static string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string text = NavValueFormatter.GetFormatter(value).FormatField(value, options, settings, format, ref pos);
		if (text == null)
		{
			throw NavNCLInvalidFormatStringException.Create(CultureInfo.CurrentCulture, string.Empty);
		}
		return FormatUsingOptions(text, options);
	}

	internal static void GetFieldFormatFromString(FormatOptions options, string format, ref int pos)
	{
		while (pos < format.Length && IsFormatAttribute(format[pos]))
		{
			switch (format[pos])
			{
			case '\u001c':
				options.FillerCharacter = format[++pos];
				pos++;
				break;
			case '\u001b':
				options.Overflow = format[++pos];
				pos++;
				break;
			case '\u001a':
				options.Comma = format[++pos];
				pos++;
				break;
			case '\u0019':
				options.Character1000 = format[++pos];
				pos++;
				break;
			case '\u001d':
				pos += 2;
				break;
			default:
				pos++;
				break;
			}
		}
	}

	private static bool IsFormatAttribute(char ch)
	{
		if (ch <= '\u001e')
		{
			return ch >= '\u0018';
		}
		return false;
	}

	internal static string FormatWithFormatString(NavValue value, int length, string format, FormatSettings settings)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int number = -1;
		format = EvaluateFormatString(value, format ?? string.Empty, out number);
		return FormatWithParsedFormatString(value, length, format, number, settings);
	}

	internal static string FormatWithFormatNumber(NavValue value, int length, int number, FormatSettings settings)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string standardFormat = GetStandardFormat(value, number);
		return FormatWithParsedFormatString(value, length, standardFormat, number, settings);
	}

	internal static string FormatWithParsedFormatString(NavValue value, int length, string format, int number, FormatSettings settings)
	{
		if (value.NclType == NavNclType.NavDateTime)
		{
			if (number != 9)
			{
				DateTime initValue = DateTime.SpecifyKind(((NavDateTime)value).ClientLocalValue, DateTimeKind.Utc);
				value = NavDateTime.Create(initValue, DateTimeReferenceFrame.Client);
			}
			NavDateTime navDateTime = (NavDateTime)value;
			if (navDateTime.IsZeroOrEmpty)
			{
				return NavValueFormatter.AdjustResultLength(string.Empty, length, value);
			}
		}
		else if (value.NclType == NavNclType.NavDecimal)
		{
			NavDecimalHelper.ValidateDecimalRange(value.ToDecimal());
		}
		else if (value.NclType == NavNclType.NavDate)
		{
			NavDate navDate = (NavDate)value;
			if (navDate.IsZeroOrEmpty)
			{
				return NavValueFormatter.AdjustResultLength(string.Empty, length, value);
			}
			if (number == 9 && navDate == NavDateTimeHelper.DateTimeMinimum)
			{
				return "0001-01-02";
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		int pos = 0;
		FormatOptions formatOptions = new FormatOptions();
		while (pos < format.Length)
		{
			switch (Convert.ToInt32(format[pos]))
			{
			case 0:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
				formatOptions.Field = Convert.ToInt32(format[pos++]);
				formatOptions.Length = Convert.ToInt32(format[pos++]);
				stringBuilder.Append(FormatField(value, formatOptions, settings, format, ref pos));
				formatOptions.Clear();
				break;
			case 24:
			{
				string standardFormat = GetStandardFormat(value, Convert.ToInt32(format[++pos]));
				format = standardFormat + format.Substring(pos + 1);
				pos = 0;
				break;
			}
			case 28:
				formatOptions.FillerCharacter = format[++pos];
				pos++;
				break;
			case 26:
				formatOptions.Comma = ((format[++pos] == '\u0001') ? settings.FieldFormatComma : format[pos]);
				pos++;
				break;
			case 25:
				formatOptions.Character1000 = format[++pos];
				pos++;
				break;
			case 27:
				formatOptions.Overflow = format[++pos];
				pos++;
				break;
			case 29:
			{
				int num = format[++pos];
				formatOptions.PrecisionMin = (byte)((num >> 8) & 0xFF);
				formatOptions.PrecisionMax = (byte)(num & 0xFF);
				pos++;
				break;
			}
			default:
				stringBuilder.Append(format[pos++]);
				break;
			}
		}
		return NavValueFormatter.AdjustResultLength(stringBuilder.ToString(), length, value);
	}
}
