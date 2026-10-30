using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public static class StringExtensions
{
	private const string IdentifierStart = "\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}";

	private const string IdentifierExtend = "\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}";

	private static ImmutableArray<string> lazyNumerals;

	private const char identifierQuoteChar = '"';

	private const string identifierQuoteCharString = "\"";

	private const string identifierQuoteCharEscape = "\"\"";

	private const char literalQuoteChar = '\'';

	private const string literalQuoteCharString = "'";

	private const string literalQuoteCharEscape = "''";

	private static readonly Regex ClsIdentifierRegex = new Regex("^[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}][\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly Regex ClsExtendingCharacterRegex = new Regex("^[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly string[] OtherReservedWords = new string[1] { "FINALIZE" };

	private static readonly Lazy<ImmutableHashSet<string>> ReservedKeywords = new Lazy<ImmutableHashSet<string>>(() => ReservedKeywordsForParameters.Value.Union(OtherReservedWords));

	private static readonly Lazy<ImmutableHashSet<string>> ReservedKeywordsForParameters = new Lazy<ImmutableHashSet<string>>(() => (from k in Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetReservedKeywordKinds()
		select Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetText(k).ToUpperInvariant()).ToImmutableHashSet());

	public static bool IsValidClsIdentifier(this string identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier))
		{
			return false;
		}
		return ClsIdentifierRegex.Match(identifier).Success;
	}

	public static string GetValidClsIdentifierByUsingUnderscores(string identifier)
	{
		IEnumerable<char> source = identifier.Select((char c) => (!ClsExtendingCharacterRegex.IsMatch(c.ToString())) ? '_' : c);
		source = source.SkipWhile((char c) => c == '_');
		return string.Join(string.Empty, source);
	}

	public static string Concatenate(this string[] values, char delimiter)
	{
		if (values == null)
		{
			return null;
		}
		return string.Join(new string(delimiter, 1), values);
	}

	public static string GetNumeral(int number)
	{
		ImmutableArray<string> value = lazyNumerals;
		if (value.IsDefault)
		{
			value = ImmutableArray.Create<string>("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
			ImmutableInterlocked.InterlockedInitialize(ref lazyNumerals, value);
		}
		DebugAssertHelper.Assert(number >= 0);
		if (number >= value.Length)
		{
			return number.ToString(CultureInfo.InvariantCulture);
		}
		return value[number];
	}

	public static string Join(this IEnumerable<string> source, string separator)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		return string.Join(separator, source);
	}

	public static bool LooksLikeInterfaceName(this string name)
	{
		if (name.Length >= 3 && name[0] == 'I' && char.IsUpper(name[1]))
		{
			return char.IsLower(name[2]);
		}
		return false;
	}

	public static bool LooksLikeTypeParameterName(this string name)
	{
		if (name.Length >= 3 && name[0] == 'T' && char.IsUpper(name[1]))
		{
			return char.IsLower(name[2]);
		}
		return false;
	}

	public static string ToPascalCase(this string shortName, bool trimLeadingTypePrefix = true)
	{
		return shortName.ConvertCase(trimLeadingTypePrefix, char.ToUpper);
	}

	public static string ToCamelCase(this string shortName, bool trimLeadingTypePrefix = true)
	{
		return shortName.ConvertCase(trimLeadingTypePrefix, char.ToLower);
	}

	public static int? GetFirstNonWhitespaceOffset(this string line)
	{
		DebugAssertHelper.Assert(line != null);
		for (int i = 0; i < line.Length; i++)
		{
			if (!char.IsWhiteSpace(line[i]))
			{
				return i;
			}
		}
		return null;
	}

	public static int? GetLastNonWhitespaceOffset(this string line)
	{
		DebugAssertHelper.Assert(line != null);
		for (int num = line.Length - 1; num >= 0; num--)
		{
			if (!char.IsWhiteSpace(line[num]))
			{
				return num;
			}
		}
		return null;
	}

	public static string GetLeadingWhitespace(this string line)
	{
		DebugAssertHelper.Assert(line != null);
		int? firstNonWhitespaceOffset = line.GetFirstNonWhitespaceOffset();
		if (!firstNonWhitespaceOffset.HasValue)
		{
			return line;
		}
		return line.Substring(0, firstNonWhitespaceOffset.Value);
	}

	public static bool HasLeadingWhitespace(this string text)
	{
		return text.GetFirstNonWhitespaceOffset() > 0;
	}

	public static bool HasTrailingWhitespace(this string text)
	{
		return text.GetLastNonWhitespaceOffset() < text.Length - 1;
	}

	public static bool Contains(this string text, char c)
	{
		return text.IndexOf(c) != -1;
	}

	public static bool IsValidAlphaNumeric(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}
		foreach (char c in str)
		{
			if (!char.IsLetter(c) && !char.IsNumber(c))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValidUnicodeString(this string str)
	{
		int num = 0;
		while (num < str.Length)
		{
			char c = str[num++];
			if (char.IsHighSurrogate(c))
			{
				if (num >= str.Length || !char.IsLowSurrogate(str[num]))
				{
					return false;
				}
				num++;
			}
			else if (char.IsLowSurrogate(c))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsNullOrEmptyOrEmptyTextLiteral(this string arg)
	{
		if (!string.IsNullOrEmpty(arg))
		{
			if (arg.Length == 2 && arg[0] == '\'')
			{
				return arg[1] == '\'';
			}
			return false;
		}
		return true;
	}

	public static string UnquoteLiteral(this string arg)
	{
		if (arg.IsLiteralQuoted())
		{
			arg = arg.Substring(1, arg.Length - 2);
			return arg.Replace("''", "'");
		}
		return arg;
	}

	public static string UnquoteIdentifier(this string arg)
	{
		if (arg.IsIdentifierQuoted())
		{
			arg = arg.Substring(1, arg.Length - 2);
			return arg.Replace("\"\"", "\"");
		}
		return arg;
	}

	public static string QuoteLiteral(this string arg)
	{
		return "'" + arg.Replace("'", "''") + "'";
	}

	public static bool IsLiteralQuoted(this string str)
	{
		int length = str.Length;
		if (length >= 2 && str[0] == '\'')
		{
			return str[length - 1] == '\'';
		}
		return false;
	}

	public static bool IsQuotedStringLiteral(this string str)
	{
		if (!str.IsLiteralQuoted())
		{
			return false;
		}
		int length = str.Length;
		for (int i = 1; i < length - 1; i++)
		{
			if (str[i] == '\'' && i < length - 1 && str[i + 1] != '\'')
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsIdentifierQuoted(this string str)
	{
		int length = str.Length;
		if (length < 2 || str[0] != '"' || str[length - 1] != '"')
		{
			return false;
		}
		if (length == 2)
		{
			return true;
		}
		str = str.Replace("\"\"", "");
		length = str.Length;
		if (length < 2 || str[0] != '"' || str[length - 1] != '"')
		{
			return false;
		}
		if (length > 3 && str.IndexOf('"', 1, length - 2) != -1)
		{
			return false;
		}
		return true;
	}

	public static string QuoteIdentifierIfNeeded(this string value)
	{
		if (!NeedsIdentifierQuotes(value))
		{
			return value;
		}
		return value.QuoteIdentifier();
	}

	public static string QuoteIdentifier(this string value)
	{
		if (!value.IsIdentifierQuoted())
		{
			return "\"" + value.Replace("\"", "\"\"") + "\"";
		}
		return value;
	}

	public static bool NeedsIdentifierQuotes(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (value.IsIdentifierQuoted())
		{
			return false;
		}
		if (value[0] != '_' && (value[0] < 'a' || value[0] > 'z') && (value[0] < 'A' || value[0] > 'Z'))
		{
			return true;
		}
		for (int num = value.Length - 1; num > 0; num--)
		{
			if (value[num] != '_' && (value[num] < 'a' || value[num] > 'z') && (value[num] < 'A' || value[num] > 'Z') && !char.IsDigit(value[num]))
			{
				return true;
			}
		}
		SyntaxKind objectKeywordKind = SyntaxFacts.GetObjectKeywordKind(value.ToUpperInvariant());
		if (objectKeywordKind.IsKeyword())
		{
			return !objectKeywordKind.IsKeywordAllowedIdentifier();
		}
		return false;
	}

	public static char First(this string arg)
	{
		return arg[0];
	}

	public static char Last(this string arg)
	{
		return arg[arg.Length - 1];
	}

	public static bool All(this string arg, Predicate<char> predicate)
	{
		foreach (char obj in arg)
		{
			if (!predicate(obj))
			{
				return false;
			}
		}
		return true;
	}

	public static Tuple<string, string> SplitOnFirstOccurrence(this string str, string match)
	{
		int? num = str?.IndexOf(match, StringComparison.OrdinalIgnoreCase);
		if (num == -1)
		{
			return new Tuple<string, string>(str, string.Empty);
		}
		if (!num.HasValue)
		{
			return new Tuple<string, string>(string.Empty, string.Empty);
		}
		string item = str.Substring(0, num.Value);
		string item2 = str.Substring(num.Value + match.Length);
		return new Tuple<string, string>(item, item2);
	}

	internal static string MangleIdentifierName(this string name)
	{
		return GetSafeCSharpIdentifierName(MangleUnquotedIdentifierIfNeeded(name));
	}

	internal static string GetLowercasedMetadataName(this string name)
	{
		name = name.MangleIdentifierName();
		if (!string.IsNullOrEmpty(name))
		{
			return LowercaseFirstLetter(name);
		}
		return string.Empty;
	}

	internal static string GetLowercasedMetadataNameForParameter(this string name)
	{
		name = name.MangleIdentifierNameForParameter();
		if (!string.IsNullOrEmpty(name))
		{
			return LowercaseFirstLetter(name);
		}
		return string.Empty;
	}

	internal static string MangleUnquotedIdentifierName(this string name)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		for (int i = 0; i < name.Length; i++)
		{
			char c = name[i];
			if (c == ' ' || c == '"')
			{
				instance.Builder.Append('_');
			}
			else if (UnicodeCharacterUtilities.IsIdentifierPartCharacter(c))
			{
				if (i == 0 && !UnicodeCharacterUtilities.IsIdentifierStartCharacter(c))
				{
					instance.Builder.Append('_');
				}
				instance.Builder.Append(c);
			}
			else
			{
				instance.Builder.Append('a');
				instance.Builder.Append((int)c);
			}
		}
		return instance.ToStringAndFree();
	}

	internal static string MangleUserDefinedIdentifier(this string name, bool uppercase = true)
	{
		if (uppercase)
		{
			return "\\u03b4" + name;
		}
		return "\\u0251" + name;
	}

	internal static int IndexOfBalancedParenthesis(this string str, int openingOffset, char closing)
	{
		char c = str[openingOffset];
		int num = 1;
		for (int i = openingOffset + 1; i < str.Length; i++)
		{
			char c2 = str[i];
			if (c2 == c)
			{
				num++;
			}
			else if (c2 == closing)
			{
				num--;
				if (num == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static string ConvertCase(this string shortName, bool trimLeadingTypePrefix, Func<char, char> convert)
	{
		if (!string.IsNullOrEmpty(shortName))
		{
			if (trimLeadingTypePrefix && (shortName.LooksLikeInterfaceName() || shortName.LooksLikeTypeParameterName()))
			{
				return convert(shortName[1]) + shortName.Substring(2);
			}
			if (convert(shortName[0]) != shortName[0])
			{
				return convert(shortName[0]) + shortName.Substring(1);
			}
		}
		return shortName;
	}

	private static string GetSafeCSharpIdentifierName(string name)
	{
		if (ReservedKeywords.Value.Contains(name.ToUpperInvariant()))
		{
			return MangleUnsafeName(name);
		}
		return name;
	}

	private static string GetSafeCSharpIdentifierNameForParameter(string name)
	{
		if (ReservedKeywordsForParameters.Value.Contains(name.ToUpperInvariant()))
		{
			return MangleUnsafeName(name);
		}
		return name;
	}

	private static string MangleUnsafeName(string name)
	{
		return "_" + name;
	}

	public static bool OneOf(this string value, StringComparison comparer = StringComparison.Ordinal, params string[] setOfValues)
	{
		return setOfValues.Any((string s) => string.Equals(s, value, comparer));
	}

	private static string LowercaseFirstLetter(string name)
	{
		return char.ToLowerInvariant(name[0]) + name.Substring(1);
	}

	private static string MangleIdentifierNameForParameter(this string name)
	{
		return GetSafeCSharpIdentifierNameForParameter(MangleUnquotedIdentifierIfNeeded(name));
	}

	private static string MangleUnquotedIdentifierIfNeeded(string name)
	{
		string text = name.UnquoteIdentifier();
		if (name.Length != text.Length)
		{
			text = text.MangleUnquotedIdentifierName();
		}
		return text;
	}
}
