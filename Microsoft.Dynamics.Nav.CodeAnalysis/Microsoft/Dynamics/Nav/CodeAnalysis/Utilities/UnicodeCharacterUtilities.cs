using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class UnicodeCharacterUtilities
{
	public static bool IsIdentifierStartCharacter(char ch)
	{
		if (ch < 'a')
		{
			if (ch < 'A')
			{
				return false;
			}
			if (ch > 'Z')
			{
				return ch == '_';
			}
			return true;
		}
		if (ch <= 'z')
		{
			return true;
		}
		if (ch <= '\u007f')
		{
			return false;
		}
		return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
	}

	public static bool IsIdentifierPartCharacter(char ch)
	{
		if (ch < 'a')
		{
			if (ch < 'A')
			{
				if (ch >= '0')
				{
					return ch <= '9';
				}
				return false;
			}
			if (ch > 'Z')
			{
				return ch == '_';
			}
			return true;
		}
		if (ch <= 'z')
		{
			return true;
		}
		if (ch <= '\u007f')
		{
			return false;
		}
		UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
		if (!IsLetterChar(unicodeCategory) && !IsDecimalDigitChar(unicodeCategory) && !IsConnectingChar(unicodeCategory) && !IsCombiningChar(unicodeCategory))
		{
			return IsFormattingChar(unicodeCategory);
		}
		return true;
	}

	public static bool IsValidIdentifier(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (!IsIdentifierStartCharacter(name[0]))
		{
			return false;
		}
		int length = name.Length;
		for (int i = 1; i < length; i++)
		{
			if (!IsIdentifierPartCharacter(name[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsLetterChar(UnicodeCategory cat)
	{
		if ((uint)cat <= 4u || cat == UnicodeCategory.LetterNumber)
		{
			return true;
		}
		return false;
	}

	private static bool IsCombiningChar(UnicodeCategory cat)
	{
		if ((uint)(cat - 5) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static bool IsDecimalDigitChar(UnicodeCategory cat)
	{
		return cat == UnicodeCategory.DecimalDigitNumber;
	}

	private static bool IsConnectingChar(UnicodeCategory cat)
	{
		return cat == UnicodeCategory.ConnectorPunctuation;
	}

	internal static bool IsFormattingChar(char ch)
	{
		if (ch > '\u007f')
		{
			return IsFormattingChar(CharUnicodeInfo.GetUnicodeCategory(ch));
		}
		return false;
	}

	private static bool IsFormattingChar(UnicodeCategory cat)
	{
		return cat == UnicodeCategory.Format;
	}
}
