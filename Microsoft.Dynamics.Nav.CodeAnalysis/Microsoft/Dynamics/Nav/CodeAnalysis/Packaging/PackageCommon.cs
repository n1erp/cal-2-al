using System;
using System.IO.Packaging;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal static class PackageCommon
{
	internal static Uri CreatePackagedUri(string path)
	{
		string text = Uri.EscapeUriString(path.Replace('\\', '/'));
		return PackUriHelper.CreatePartUri(new Uri((path[0] == '/') ? text : ("/" + text), UriKind.Relative));
	}

	internal static string NormalizeRelativeUrlPath(string path)
	{
		path = path.Replace("\\", "/");
		if (path.StartsWith("./", StringComparison.Ordinal))
		{
			path = path.Substring(2);
		}
		return path;
	}
}
