using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class ObjectDisplay
{
	internal static string NullLiteral => "null";

	public static string FormatPrimitive(object obj, ObjectDisplayOptions options)
	{
		if (obj == null)
		{
			return NullLiteral;
		}
		Type type = obj.GetType();
		if (type.GetTypeInfo().IsEnum)
		{
			type = Enum.GetUnderlyingType(type);
		}
		if ((object)type == typeof(int))
		{
			return FormatLiteral((int)obj, options);
		}
		if ((object)type == typeof(string))
		{
			return FormatLiteral((string)obj, options);
		}
		if ((object)type == typeof(bool))
		{
			return FormatLiteral((bool)obj);
		}
		if ((object)type == typeof(char))
		{
			return FormatLiteral((char)obj, options);
		}
		if ((object)type == typeof(byte))
		{
			return FormatLiteral((byte)obj, options);
		}
		if ((object)type == typeof(short))
		{
			return FormatLiteral((short)obj, options);
		}
		if ((object)type == typeof(long))
		{
			return FormatLiteral((long)obj, options);
		}
		if ((object)type == typeof(double))
		{
			return FormatLiteral((double)obj, options);
		}
		if ((object)type == typeof(ulong))
		{
			return FormatLiteral((ulong)obj, options);
		}
		if ((object)type == typeof(uint))
		{
			return FormatLiteral((uint)obj, options);
		}
		if ((object)type == typeof(ushort))
		{
			return FormatLiteral((ushort)obj, options);
		}
		if ((object)type == typeof(sbyte))
		{
			return FormatLiteral((sbyte)obj, options);
		}
		if ((object)type == typeof(float))
		{
			return FormatLiteral((float)obj, options);
		}
		if ((object)type == typeof(decimal))
		{
			return FormatLiteral((decimal)obj, options);
		}
		return null;
	}

	internal static string FormatLiteral(bool value)
	{
		if (!value)
		{
			return "false";
		}
		return "true";
	}

	private static bool TryReplaceChar(char c, out string replaceWith)
	{
		replaceWith = null;
		switch (c)
		{
		case '\\':
			replaceWith = "\\\\";
			break;
		case '\0':
			replaceWith = "\\0";
			break;
		case '\a':
			replaceWith = "\\a";
			break;
		case '\b':
			replaceWith = "\\b";
			break;
		case '\f':
			replaceWith = "\\f";
			break;
		case '\n':
			replaceWith = "\\n";
			break;
		case '\r':
			replaceWith = "\\r";
			break;
		case '\t':
			replaceWith = "\\t";
			break;
		case '\v':
			replaceWith = "\\v";
			break;
		}
		if (replaceWith != null)
		{
			return true;
		}
		if (NeedsEscaping(CharUnicodeInfo.GetUnicodeCategory(c)))
		{
			int num = c;
			replaceWith = "\\u" + num.ToString("x4");
			return true;
		}
		return false;
	}

	private static bool NeedsEscaping(UnicodeCategory category)
	{
		if ((uint)(category - 12) <= 2u || category == UnicodeCategory.Surrogate || category == UnicodeCategory.OtherNotAssigned)
		{
			return true;
		}
		return false;
	}

	public static string FormatLiteral(string value, ObjectDisplayOptions options)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		bool flag = options.IncludesOption(ObjectDisplayOptions.UseQuotes);
		bool flag2 = options.IncludesOption(ObjectDisplayOptions.EscapeNonPrintableCharacters);
		bool flag3 = flag && !flag2 && ContainsNewLine(value);
		if (flag)
		{
			if (flag3)
			{
				builder.Append('@');
			}
			builder.Append('"');
		}
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			string replaceWith;
			if (flag2 && CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Surrogate)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(value, i);
				if (unicodeCategory == UnicodeCategory.Surrogate)
				{
					int num = c;
					builder.Append("\\u" + num.ToString("x4"));
				}
				else if (NeedsEscaping(unicodeCategory))
				{
					builder.Append("\\U" + char.ConvertToUtf32(value, i).ToString("x8"));
					i++;
				}
				else
				{
					builder.Append(c);
					builder.Append(value[++i]);
				}
			}
			else if (flag2 && TryReplaceChar(c, out replaceWith))
			{
				builder.Append(replaceWith);
			}
			else if (flag && c == '"')
			{
				if (flag3)
				{
					builder.Append('"');
					builder.Append('"');
				}
				else
				{
					builder.Append('\\');
					builder.Append('"');
				}
			}
			else
			{
				builder.Append(c);
			}
		}
		if (flag)
		{
			builder.Append('"');
		}
		return instance.ToStringAndFree();
	}

	private static bool ContainsNewLine(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (SyntaxFacts.IsNewLine(s[i]))
			{
				return true;
			}
		}
		return false;
	}

	internal static string FormatLiteral(char c, ObjectDisplayOptions options)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (options.IncludesOption(ObjectDisplayOptions.IncludeCodePoints))
		{
			string value;
			if (!options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
			{
				int num = c;
				value = num.ToString();
			}
			else
			{
				int num = c;
				value = "0x" + num.ToString("x4");
			}
			builder.Append(value);
			builder.Append(" ");
		}
		bool flag = options.IncludesOption(ObjectDisplayOptions.UseQuotes);
		bool flag2 = options.IncludesOption(ObjectDisplayOptions.EscapeNonPrintableCharacters);
		if (flag)
		{
			builder.Append('\'');
		}
		if (flag2 && TryReplaceChar(c, out var replaceWith))
		{
			builder.Append(replaceWith);
		}
		else if (flag && c == '\'')
		{
			builder.Append('\\');
			builder.Append('\'');
		}
		else
		{
			builder.Append(c);
		}
		if (flag)
		{
			builder.Append('\'');
		}
		return instance.ToStringAndFree();
	}

	internal static string FormatLiteral(sbyte value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			string text;
			if (value < 0)
			{
				int num = value;
				text = num.ToString("x8");
			}
			else
			{
				text = value.ToString("x2");
			}
			return "0x" + text;
		}
		return value.ToString(GetFormatCulture(cultureInfo));
	}

	internal static string FormatLiteral(byte value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			return "0x" + value.ToString("x2");
		}
		return value.ToString(GetFormatCulture(cultureInfo));
	}

	internal static string FormatLiteral(short value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			string text;
			if (value < 0)
			{
				int num = value;
				text = num.ToString("x8");
			}
			else
			{
				text = value.ToString("x4");
			}
			return "0x" + text;
		}
		return value.ToString(GetFormatCulture(cultureInfo));
	}

	internal static string FormatLiteral(ushort value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			return "0x" + value.ToString("x4");
		}
		return value.ToString(GetFormatCulture(cultureInfo));
	}

	internal static string FormatLiteral(int value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			return "0x" + value.ToString("x8");
		}
		return value.ToString(GetFormatCulture(cultureInfo));
	}

	internal static string FormatLiteral(uint value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			builder.Append("0x");
			builder.Append(value.ToString("x8"));
		}
		else
		{
			builder.Append(value.ToString(GetFormatCulture(cultureInfo)));
		}
		if (options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			builder.Append('U');
		}
		return instance.ToStringAndFree();
	}

	internal static string FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			builder.Append("0x");
			builder.Append(value.ToString("x16"));
		}
		else
		{
			builder.Append(value.ToString(GetFormatCulture(cultureInfo)));
		}
		if (options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			builder.Append('L');
		}
		return instance.ToStringAndFree();
	}

	internal static string FormatLiteral(ulong value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
		{
			builder.Append("0x");
			builder.Append(value.ToString("x16"));
		}
		else
		{
			builder.Append(value.ToString(GetFormatCulture(cultureInfo)));
		}
		if (options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			builder.Append("UL");
		}
		return instance.ToStringAndFree();
	}

	internal static string FormatLiteral(double value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		string text = value.ToString("R", GetFormatCulture(cultureInfo));
		if (!options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			return text;
		}
		return text + "D";
	}

	internal static string FormatLiteral(float value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		string text = value.ToString("R", GetFormatCulture(cultureInfo));
		if (!options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			return text;
		}
		return text + "F";
	}

	internal static string FormatLiteral(decimal value, ObjectDisplayOptions options, CultureInfo cultureInfo = null)
	{
		string text = value.ToString(GetFormatCulture(cultureInfo));
		if (!options.IncludesOption(ObjectDisplayOptions.IncludeTypeSuffix))
		{
			return text;
		}
		return text + "M";
	}

	private static CultureInfo GetFormatCulture(CultureInfo cultureInfo)
	{
		return cultureInfo ?? CultureInfo.InvariantCulture;
	}
}
