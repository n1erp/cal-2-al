namespace Microsoft.CodeAnalysis.Text;

internal static class TextUtilities
{
	internal static int GetLengthOfLineBreak(SourceText text, int index)
	{
		char c = text[index];
		if ((uint)(c - 14) <= 113u)
		{
			return 0;
		}
		return GetLengthOfLineBreakSlow(text, index, c);
	}

	private static int GetLengthOfLineBreakSlow(SourceText text, int index, char c)
	{
		if (c == '\r')
		{
			int num = index + 1;
			if (num >= text.Length || '\n' != text[num])
			{
				return 1;
			}
			return 2;
		}
		if (IsAnyLineBreakCharacter(c))
		{
			return 1;
		}
		return 0;
	}

	public static void GetStartAndLengthOfLineBreakEndingAt(SourceText text, int index, out int startLinebreak, out int lengthLinebreak)
	{
		char c = text[index];
		if (c == '\n')
		{
			if (index > 0 && text[index - 1] == '\r')
			{
				startLinebreak = index - 1;
				lengthLinebreak = 2;
			}
			else
			{
				startLinebreak = index;
				lengthLinebreak = 1;
			}
		}
		else if (IsAnyLineBreakCharacter(c))
		{
			startLinebreak = index;
			lengthLinebreak = 1;
		}
		else
		{
			startLinebreak = index + 1;
			lengthLinebreak = 0;
		}
	}

	internal static bool IsAnyLineBreakCharacter(char c)
	{
		if (c != '\n' && c != '\r' && c != '\u0085' && c != '\u2028')
		{
			return c == '\u2029';
		}
		return true;
	}
}
