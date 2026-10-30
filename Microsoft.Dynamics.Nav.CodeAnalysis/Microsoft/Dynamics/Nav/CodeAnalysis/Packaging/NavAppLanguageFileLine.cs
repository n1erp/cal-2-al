namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppLanguageFileLine
{
	public string LanguageKey { get; }

	private NavAppLanguageFileLine(string rawLine)
	{
		new LanguageFormatConverter().ConvertLineToLangFormat(rawLine);
		int length = rawLine.IndexOf(':');
		LanguageKey = rawLine.Substring(0, length);
	}

	public static NavAppLanguageFileLine Create(string rawLine)
	{
		if (!IsLineWorthConsidering(rawLine))
		{
			return null;
		}
		return new NavAppLanguageFileLine(rawLine);
	}

	private static bool IsLineWorthConsidering(string line)
	{
		if (!string.IsNullOrWhiteSpace(line))
		{
			return line.Contains("-A");
		}
		return false;
	}
}
