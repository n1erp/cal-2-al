using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavBookmarkHelper
{
	public static string ConvertBookmarkToString(byte[] bookmarkBytes)
	{
		if (bookmarkBytes == null)
		{
			return string.Empty;
		}
		if (bookmarkBytes.Length == 0)
		{
			return string.Empty;
		}
		int num = bookmarkBytes.Length - 1;
		while (num > 0 && bookmarkBytes[num] == 0)
		{
			num--;
		}
		if (num == 0)
		{
			return string.Empty;
		}
		return bookmarkBytes.Length.ToString(CultureInfo.InvariantCulture) + ";" + Convert.ToBase64String(bookmarkBytes, 0, num + 1, Base64FormattingOptions.None);
	}
}
