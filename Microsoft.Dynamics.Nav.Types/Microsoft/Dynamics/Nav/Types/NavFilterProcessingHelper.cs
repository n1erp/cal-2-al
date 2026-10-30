using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavFilterProcessingHelper
{
	public const string EscapeSingleCharacter = "'";

	public const char EscapeSingleCharacterChar = '\'';

	public const string EscapeTwoSingleCharacters = "''";

	private const string EscapeExpression = "'{0}'";

	private const string RangeSeparator = "..";

	private const string StartsWithWildCard = "@";

	private const string ContainsWildCardAndIgnoreCase = "@*";

	private const string ContainsWildCard = "*";

	private const string ComplexFilterIndicators = "\\s*'.*'\\s*|[.]{2}|[|*&<>=()]";

	private static readonly Regex complexFilterIndicatorsRegEx = new Regex("\\s*'.*'\\s*|[.]{2}|[|*&<>=()]");

	private static readonly Regex needsQuoting = new Regex("^\\s*$|[.]{2}|[|&<>=()']");

	public static bool IsComplexFilter(string filterValue)
	{
		return complexFilterIndicatorsRegEx.IsMatch(filterValue);
	}

	public static string EscapeFilterValueIfNeeded(string filterValue)
	{
		if (!needsQuoting.IsMatch(filterValue))
		{
			return filterValue;
		}
		return EscapeFilterValue(filterValue);
	}

	public static string GetMaxLimit(string filterValue)
	{
		if (string.IsNullOrEmpty(filterValue))
		{
			throw new ArgumentException(Lang.StringNullOrEmpty, "filterValue");
		}
		if (filterValue.EndsWith("..", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException(Lang.FilterIntervalMaxValueMissing);
		}
		string[] array = filterValue.Split(new string[1] { ".." }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 2)
		{
			throw new ArgumentException(Lang.FilterIntervalWrongFormat);
		}
		if (array.Length == 2)
		{
			return ".." + array[1];
		}
		return ".." + array[0];
	}

	public static string EscapeFilterValue(string filterValue, bool escapeEmptyString = false)
	{
		if (filterValue == null || (!escapeEmptyString && filterValue.Length == 0))
		{
			return filterValue;
		}
		return AddSurroundingEscapeCharacters(ReplaceSingleEscapeCharactersWithTwoSingleEscapeCharacters(filterValue));
	}

	private static string AddSurroundingEscapeCharacters(string inputString)
	{
		return string.Format(CultureInfo.InvariantCulture, "'{0}'", inputString);
	}

	private static string ReplaceSingleEscapeCharactersWithTwoSingleEscapeCharacters(string inputString)
	{
		return inputString.Replace("'", "''");
	}

	public static string AddWildcardForStartsWith(Type columnToFilterDataType, string value)
	{
		return AddWildcards(columnToFilterDataType, value, "@");
	}

	public static string AddWildcardForStartsWithCaseSensitive(Type columnToFilterDataType, string value)
	{
		return AddWildcards(columnToFilterDataType, value, string.Empty);
	}

	public static string AddWildcardForContainsAndEndsWith(Type columnToFilterDataType, string value)
	{
		return AddWildcards(columnToFilterDataType, value, "@*");
	}

	private static string AddWildcards(Type columnToFilterDataType, string value, string startingPrefix)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (CanAddWildcardToFilterValue(columnToFilterDataType, value))
		{
			if (!value.StartsWith(startingPrefix, StringComparison.OrdinalIgnoreCase))
			{
				value = startingPrefix + value;
			}
			if (!value.EndsWith("*", StringComparison.OrdinalIgnoreCase))
			{
				value += "*";
			}
			value = EscapeFilterValueIfNeeded(value);
		}
		return value;
	}

	public static string RemoveWildcardForContainsAndEndsWith(Type columnToFilterDataType, string value)
	{
		return RemoveWildcard(columnToFilterDataType, value, "@*", "*");
	}

	public static string RemoveWildcardForEndsWith(Type columnToFilterDataType, string value)
	{
		return RemoveWildcard(columnToFilterDataType, value, string.Empty, "*");
	}

	private static string RemoveWildcard(Type columnToFilterDataType, string value, string prefix, string postfix)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		string text = value.Trim().Trim("'"[0]);
		if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && text.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
		{
			int num = text.Length - (prefix.Length + postfix.Length);
			if (num <= 0)
			{
				return string.Empty;
			}
			string text2 = text.Substring(prefix.Length, num);
			text2 = text2.Replace("''", "'");
			if (CanAddWildcardToFilterValue(columnToFilterDataType, text2))
			{
				return text2;
			}
		}
		return value;
	}

	public static bool CanAddWildcardToFilterValue(Type columnToFilterDataType, string value)
	{
		Regex regex = new Regex("[.]{2}");
		Regex regex2 = new Regex("[|&<>%]");
		Regex regex3 = new Regex("\\s*'.*'\\s*");
		if (columnToFilterDataType != typeof(string))
		{
			return false;
		}
		if (regex.IsMatch(value) || regex2.IsMatch(value) || regex3.IsMatch(value) || value.Contains("@") || value.Contains("*"))
		{
			return false;
		}
		return true;
	}
}
