using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class PropertyValueTextHelper
{
	internal static string GetMultilanguageStringItem(string language, string unescapedLanguageValue)
	{
		return language + "=" + GetEscapedMultilanguageValue(unescapedLanguageValue);
	}

	internal static string GetMultilanguageString(IEnumerable<string> values)
	{
		return string.Join(";", values);
	}

	private static string GetEscapedMultilanguageValue(string value)
	{
		if (value.HasLeadingWhitespace() || value.HasTrailingWhitespace() || value.Contains(";") || value.Contains("\""))
		{
			string text = value.Replace("\"", "\"\"");
			return "\"" + text + "\"";
		}
		return value;
	}
}
