using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.AL.Common;

public static class NameTransformations
{
	public static string MangleIntoValidXmlIdentifier(string alIdentifierName)
	{
		if (string.IsNullOrEmpty(alIdentifierName))
		{
			return alIdentifierName;
		}
		StringBuilder stringBuilder = new StringBuilder(alIdentifierName.Length);
		char c = '\0';
		foreach (char c2 in alIdentifierName)
		{
			switch (c2)
			{
			case '%':
				stringBuilder.Append("Percent");
				c = 't';
				break;
			case ' ':
			case '"':
			case '(':
			case ')':
			case '-':
			case '.':
			case '/':
			case ':':
			case '@':
			case '\\':
				if (c != '_')
				{
					stringBuilder.Append('_');
					c = '_';
				}
				break;
			default:
				stringBuilder.Append(c2);
				c = c2;
				break;
			}
		}
		string text = stringBuilder.ToString();
		text = text.TrimEnd('_');
		if (string.IsNullOrEmpty(text))
		{
			stringBuilder.Length = 0;
			for (int j = 0; j < alIdentifierName.Length; j++)
			{
				switch (alIdentifierName[j])
				{
				case '@':
					stringBuilder.Append("_at_");
					break;
				case '"':
					stringBuilder.Append("_quote_");
					break;
				case '.':
					stringBuilder.Append("_dot_");
					break;
				case '/':
					stringBuilder.Append("_slash_");
					break;
				case '(':
					stringBuilder.Append("_leftparenthesis_");
					break;
				case ')':
					stringBuilder.Append("_rightparenthesis_");
					break;
				case '\\':
					stringBuilder.Append("_backslash_");
					break;
				case '-':
					stringBuilder.Append("_minus_");
					break;
				case ' ':
					stringBuilder.Append("_blank_");
					break;
				case '_':
					stringBuilder.Append("_underscore_");
					break;
				case ':':
					stringBuilder.Append("_colon_");
					break;
				default:
					stringBuilder.Append(XmlConvert.EncodeName(alIdentifierName[j].ToString(CultureInfo.InvariantCulture)));
					break;
				}
			}
			text = stringBuilder.ToString();
		}
		else
		{
			text = text.TrimEnd('_');
		}
		if (text.EndsWith("Subform", StringComparison.Ordinal))
		{
			text = text.Replace("Subform", "Line");
		}
		return XmlConvert.EncodeName(text);
	}
}
