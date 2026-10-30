using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemString
{
	public static string ALIncStr(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		int num = value.Length - 1;
		while (num >= 0 && !char.IsDigit(value[num]))
		{
			num--;
		}
		if (num < 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(value);
		while (true)
		{
			if (CharUnicodeInfo.GetDigitValue(stringBuilder[num]) < 9)
			{
				stringBuilder[num] = (stringBuilder[num] += '\u0001');
				break;
			}
			stringBuilder[num--] = '0';
			if (num < 0 || !char.IsDigit(stringBuilder[num]))
			{
				stringBuilder.Insert(num + 1, '1');
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static string ALLowercase(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return value.ToLower(NavCurrentThread.Session.Culture);
	}

	public static string ALUppercase(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return value.ToUpper(NavCurrentThread.Session.Culture);
	}

	public static char GetChar(string value, int index)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		if (index < 0)
		{
			throw new NavNCLOutsidePermittedRangeException("INDEXING", 1, index, 0, int.MaxValue);
		}
		if (index >= value.Length)
		{
			return '\0';
		}
		return value[index];
	}

	public static byte GetCharAsByte(string value, int index)
	{
		return NCLManagedAdapter.TextCharToByte(GetChar(value, index));
	}

	public static string SetChar(string value, int index, byte oemByte)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		if (index < 0 || index > value.Length)
		{
			throw new NavNCLOutsidePermittedRangeException("INDEXING", 1, index, 0, value.Length);
		}
		if (oemByte == 0)
		{
			if (index < value.Length)
			{
				return value.Substring(0, index);
			}
		}
		else
		{
			char c = NCLManagedAdapter.ByteToTextChar(oemByte);
			if (index == value.Length)
			{
				return value + c;
			}
			if (value[index] != c)
			{
				StringBuilder stringBuilder = new StringBuilder(value);
				stringBuilder[index] = c;
				return stringBuilder.ToString();
			}
		}
		return value;
	}

	public static string SetChar(string value, int index, char setValue)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		if (index < 0 || index > value.Length)
		{
			throw new NavNCLOutsidePermittedRangeException("INDEXING", 1, index, 0, value.Length);
		}
		if (setValue == '\0')
		{
			if (index < value.Length)
			{
				return value.Substring(0, index);
			}
		}
		else
		{
			if (index == value.Length)
			{
				return value + setValue;
			}
			if (value[index] != setValue)
			{
				StringBuilder stringBuilder = new StringBuilder(value);
				stringBuilder[index] = setValue;
				return stringBuilder.ToString();
			}
		}
		return value;
	}

	public static int ALStrPos(string source, string substring)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (substring == null)
		{
			throw new ArgumentNullException("substring");
		}
		if (source.Length == 0 || substring.Length == 0)
		{
			return 0;
		}
		return source.IndexOf(substring, StringComparison.Ordinal) + 1;
	}

	public static string ALStrSubstNo(string source, params NavValue[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(source.Length + 100);
		int i = 0;
		while (i < source.Length)
		{
			char c = source[i];
			int value;
			switch (c)
			{
			case '%':
				i++;
				StringHelper.TryParseInt32FromString(source, ref i, out value);
				ALStrSubstNoInsertParam(stringBuilder, values, value, -1);
				break;
			case '#':
			{
				int num = i;
				for (; i < source.Length && source[i] == '#'; i++)
				{
				}
				StringHelper.TryParseInt32FromString(source, ref i, out value);
				for (; i < source.Length && source[i] == '#'; i++)
				{
				}
				ALStrSubstNoInsertParam(stringBuilder, values, value, i - num);
				break;
			}
			default:
				stringBuilder.Append(c);
				i++;
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private static void ALStrSubstNoInsertParam(StringBuilder result, NavValue[] values, int insertParamNo, int insertParamLength)
	{
		_ = string.Empty;
		if (values != null && insertParamNo >= 1 && insertParamNo <= values.Length && values[insertParamNo - 1] != null)
		{
			NavValue navValue = values[insertParamNo - 1];
			string text = navValue.ToString();
			if (insertParamLength < 0)
			{
				result.Append(text);
			}
			else if (text.Length > insertParamLength)
			{
				result.Append(new string('*', insertParamLength));
			}
			else
			{
				result.Append(text.PadRight(insertParamLength, ' '));
			}
		}
		else
		{
			result.Append(CreateParameterSignature(insertParamNo, insertParamLength));
		}
	}

	private static string CreateParameterSignature(int insertParamNo, int insertParamLength)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (insertParamLength == -1)
		{
			stringBuilder.Append('%');
			if (insertParamNo > 0)
			{
				stringBuilder.Append(insertParamNo);
			}
		}
		else
		{
			int num = 1;
			stringBuilder.Append('#');
			if (insertParamNo > 0)
			{
				num += (int)Math.Floor(Math.Log10(insertParamNo)) + 1;
				stringBuilder.Append(insertParamNo);
			}
			if (insertParamLength > 1)
			{
				stringBuilder.Append(string.Empty.PadRight(insertParamLength - num, '#'));
			}
		}
		return stringBuilder.ToString();
	}

	public static string ALCopyStr(string source, int fromPos1Based)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (fromPos1Based < 1)
		{
			throw new NavNCLOutsidePermittedRangeException("COPYSTR", 2, fromPos1Based, 1, 2048);
		}
		if (fromPos1Based > source.Length)
		{
			return string.Empty;
		}
		return source.Substring(fromPos1Based - 1);
	}

	public static string ALCopyStr(string source, int fromPos1Based, int length)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (fromPos1Based < 1)
		{
			throw new NavNCLOutsidePermittedRangeException("COPYSTR", 2, fromPos1Based, 1, int.MaxValue);
		}
		if (length < 0)
		{
			throw new NavNCLOutsidePermittedRangeException("COPYSTR", 3, length, 0, int.MaxValue);
		}
		int num = fromPos1Based - 1;
		if (num > int.MaxValue - length)
		{
			throw new NavNCLOutsidePermittedRangeException("COPYSTR", 3, length, 0, int.MaxValue - num);
		}
		if (length == 0 || fromPos1Based > source.Length)
		{
			return string.Empty;
		}
		if (num + length >= source.Length)
		{
			return source.Substring(num);
		}
		return source.Substring(num, length);
	}

	public static string ALPadStr(string source, int length)
	{
		return ALPadStr(source, length, " ");
	}

	public static string ALPadStr(string source, int length, string padCharacter)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (padCharacter == null)
		{
			throw new ArgumentNullException("padCharacter");
		}
		if (length < 0)
		{
			throw new NavNCLOutsidePermittedRangeException("PADSTR", 2, length, 0, int.MaxValue);
		}
		if (padCharacter.Length != 1)
		{
			throw new NavNCLOutsidePermittedRangeException("PADSTR", 3, padCharacter.Length, 1, 1);
		}
		if (source.Length == 0)
		{
			return new string(padCharacter[0], length);
		}
		if (length > source.Length)
		{
			return source.PadRight(length, padCharacter[0]);
		}
		if (length < source.Length)
		{
			return source.Substring(0, length);
		}
		return source;
	}

	public static string ALDelChr(string source)
	{
		return ALDelChr(source, "=", " ");
	}

	public static string ALDelChr(string source, string where)
	{
		return ALDelChr(source, where, " ");
	}

	public static string ALDelChr(string source, string where, string which)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		if (which == null)
		{
			throw new ArgumentNullException("which");
		}
		if (source.Length == 0)
		{
			return string.Empty;
		}
		if (where.Length == 0 || which.Length == 0)
		{
			return source;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < where.Length; i++)
		{
			switch (where[i])
			{
			case '=':
				flag = true;
				break;
			case '<':
				flag2 = true;
				break;
			case '>':
				flag3 = true;
				break;
			default:
				throw new NavNCLALSystemStringException("Incorrect use of the where parameter.");
			}
		}
		if (flag)
		{
			if (which.Length == 1)
			{
				return source.Replace(which, string.Empty);
			}
			StringBuilder stringBuilder = new StringBuilder(source);
			for (int i = 0; i < which.Length; i++)
			{
				stringBuilder.Replace(which[i].ToString(), string.Empty);
			}
			return stringBuilder.ToString();
		}
		int j = 0;
		int num = source.Length - 1;
		if (flag2)
		{
			for (; j <= num && which.IndexOf(source[j]) >= 0; j++)
			{
			}
		}
		if (flag3)
		{
			while (num >= j && which.IndexOf(source[num]) >= 0)
			{
				num--;
			}
		}
		return source.Substring(j, num - j + 1);
	}

	public static string ALDelStr(string source, int fromPos1Based)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (fromPos1Based < 1)
		{
			throw new NavNCLOutsidePermittedRangeException("DELSTR", 2, fromPos1Based, 1, 2048);
		}
		if (source.Length == 0)
		{
			return string.Empty;
		}
		if (fromPos1Based > source.Length)
		{
			return source;
		}
		return source.Remove(fromPos1Based - 1);
	}

	public static string ALDelStr(string source, int fromPos1Based, int length)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (fromPos1Based < 1)
		{
			throw new NavNCLOutsidePermittedRangeException("DELSTR", 2, fromPos1Based, 1, int.MaxValue);
		}
		if (length < 0)
		{
			throw new NavNCLOutsidePermittedRangeException("DELSTR", 3, length, 0, int.MaxValue);
		}
		if (source.Length == 0)
		{
			return string.Empty;
		}
		if (length == 0 || fromPos1Based > source.Length)
		{
			return source;
		}
		if (fromPos1Based + length > source.Length)
		{
			return source.Remove(fromPos1Based - 1);
		}
		return source.Remove(fromPos1Based - 1, length);
	}

	public static int ALStrCheckSum(string source)
	{
		return ALStrCheckSum(source, string.Empty, 10);
	}

	public static int ALStrCheckSum(string source, string weight)
	{
		return ALStrCheckSum(source, weight, 10);
	}

	public static int ALStrCheckSum(string source, string weight, int modulus)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (weight == null)
		{
			throw new ArgumentNullException("weight");
		}
		if (source.Length == 0)
		{
			return 0;
		}
		if (weight.Length == 0)
		{
			weight = new string('1', source.Length);
		}
		else
		{
			if (weight.Length > source.Length)
			{
				throw new NavNCLALSystemStringException("The weight parameter cannot be longer than the value string.");
			}
			if (weight.Length < source.Length)
			{
				weight = weight.PadRight(source.Length, '1');
			}
		}
		if (modulus < 1)
		{
			throw new NavNCLALSystemStringException("The modulus parameter cannot be less than 1.");
		}
		int num = 0;
		for (int i = 0; i < source.Length; i++)
		{
			int num2 = source[i] - 48;
			int num3 = weight[i] - 48;
			if (num2 < 0 || num2 > 9)
			{
				throw new NavNCLALSystemStringException("The value string must be numeric.");
			}
			if (num3 < 0 || num3 > 9)
			{
				throw new NavNCLALSystemStringException("The weight string must be numeric.");
			}
			num += num2 * num3;
		}
		return (modulus - num % modulus) % modulus;
	}

	public static string ALConvertStr(string source, string fromCharacters, string toCharacters)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (fromCharacters == null)
		{
			throw new ArgumentNullException("fromCharacters");
		}
		if (toCharacters == null)
		{
			throw new ArgumentNullException("toCharacters");
		}
		if (fromCharacters.Length != toCharacters.Length)
		{
			throw NavNCLInvalidStringFormatException.CreateNotEqualLengthForConvert(CultureInfo.InvariantCulture, fromCharacters, toCharacters);
		}
		if (source.Length == 0 || fromCharacters.Length == 0)
		{
			return source;
		}
		if (fromCharacters.Length == 1)
		{
			return source.Replace(fromCharacters[0], toCharacters[0]);
		}
		StringBuilder stringBuilder = new StringBuilder(source);
		for (int i = 0; i < source.Length; i++)
		{
			int num = fromCharacters.IndexOf(source[i]);
			if (num >= 0)
			{
				stringBuilder[i] = toCharacters[num];
			}
		}
		return stringBuilder.ToString();
	}

	public static string ALSelectString(int index1Based, string source)
	{
		string[] array = source.Split(NavOption.OptionDelimiter);
		if (index1Based < 1 || index1Based > array.Length)
		{
			throw NavNCLInvalidStringFormatException.CreateNoValueForIndex(CultureInfo.InvariantCulture, source, index1Based);
		}
		return array[index1Based - 1];
	}

	public static string ALInsStr(string source, string insert, int index1Based)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (insert == null)
		{
			throw new ArgumentNullException("insert");
		}
		if (index1Based < 1)
		{
			throw new NavNCLOutsidePermittedRangeException("INSSTR", 3, index1Based, 1, int.MaxValue);
		}
		if (insert.Length == 0)
		{
			return source;
		}
		if (index1Based > source.Length)
		{
			return source + insert;
		}
		return source.Insert(index1Based - 1, insert);
	}

	public static string ALInsStr(string source, string insert)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ALInsStr(source, insert, source.Length + 1);
	}

	public static int ALStrLen(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Length;
	}

	public static int ALStrLen(NavStringValue source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Value.Length;
	}

	public static int ALStrLen(NavVariant source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return 0;
	}

	public static int ALMaxStrLen(NavStringValue source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.MaxLength;
	}

	public static int ALMaxStrLen(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return int.MaxValue;
	}

	public static int ALMaxStrLen(NavVariant source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		throw new NavNCLArgumentException(source, "MAXSTRLEN", 1);
	}
}
