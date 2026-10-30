using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public static class TextUtilities
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

	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
	public static void GetStartAndLengthOfLineBreakEndingAt(SourceText text, int index, out int startLineBreak, out int lengthLineBreak)
	{
		char c = text[index];
		if (c == '\n')
		{
			if (index > 0 && text[index - 1] == '\r')
			{
				startLineBreak = index - 1;
				lengthLineBreak = 2;
			}
			else
			{
				startLineBreak = index;
				lengthLineBreak = 1;
			}
		}
		else if (IsAnyLineBreakCharacter(c))
		{
			startLineBreak = index;
			lengthLineBreak = 1;
		}
		else
		{
			startLineBreak = index + 1;
			lengthLineBreak = 0;
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
