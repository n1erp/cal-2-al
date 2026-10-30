using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public class UriUtilities
{
	public static Uri ParseUri(string s)
	{
		if (string.IsNullOrEmpty(s) || !Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var result))
		{
			return null;
		}
		return result;
	}
}
