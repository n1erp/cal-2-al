using System;
using System.Collections.Specialized;

namespace Microsoft.Dynamics.Nav.Types;

public static class UriHelper
{
	public const string PathDelimiter = "/";

	public static NameValueCollection ParseQuery(this Uri uri)
	{
		string text = uri.Query;
		if (text.Length > 0 && text[0] == '?')
		{
			text = text.Substring(1);
		}
		return ParseQuery(text, urlencoded: true);
	}

	private static NameValueCollection ParseQuery(string queryString, bool urlencoded)
	{
		NameValueCollection nameValueCollection = new NameValueCollection();
		int length = queryString.Length;
		for (int i = 0; i < length; i++)
		{
			int num = i;
			int num2 = -1;
			for (; i < length; i++)
			{
				switch (queryString[i])
				{
				case '=':
					if (num2 < 0)
					{
						num2 = i;
					}
					continue;
				default:
					continue;
				case '&':
					break;
				}
				break;
			}
			string text = string.Empty;
			string text2;
			if (num2 >= 0)
			{
				text = queryString.Substring(num, num2 - num);
				text2 = queryString.Substring(num2 + 1, i - num2 - 1);
			}
			else
			{
				text2 = queryString.Substring(num, i - num);
			}
			if (urlencoded)
			{
				nameValueCollection.Add(Uri.UnescapeDataString(text), Uri.UnescapeDataString(text2));
			}
			else
			{
				nameValueCollection.Add(text, text2);
			}
			if (i == length - 1 && queryString[i] == '&')
			{
				nameValueCollection.Add(string.Empty, string.Empty);
			}
		}
		return nameValueCollection;
	}
}
