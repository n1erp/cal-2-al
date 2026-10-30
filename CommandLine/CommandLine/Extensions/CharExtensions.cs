namespace CommandLine.Extensions;

internal static class CharExtensions
{
	public static bool IsWhiteSpace(this char c)
	{
		switch (c)
		{
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			return true;
		default:
			if (c > '\u007f')
			{
				return char.IsWhiteSpace(c);
			}
			return false;
		}
	}

	public static bool IsLineTerminator(this char c)
	{
		switch (c)
		{
		case '\n':
		case '\r':
		case '\u2028':
		case '\u2029':
			return true;
		default:
			return false;
		}
	}
}
