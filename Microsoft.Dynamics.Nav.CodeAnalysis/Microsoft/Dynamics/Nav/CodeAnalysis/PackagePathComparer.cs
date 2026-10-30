using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PackagePathComparer : StringComparer
{
	private static readonly Lazy<PackagePathComparer> instance = new Lazy<PackagePathComparer>(() => new PackagePathComparer());

	internal static PackagePathComparer Instance => instance.Value;

	private PackagePathComparer()
	{
	}

	public override int Compare(string x, string y)
	{
		return string.Compare(GetCleanPath(x), GetCleanPath(y), StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(string x, string y)
	{
		return Compare(x, y) == 0;
	}

	public override int GetHashCode(string obj)
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(GetCleanPath(obj));
	}

	private static string GetCleanPath(string path)
	{
		if (path == null)
		{
			return null;
		}
		path = Path.GetFullPath(path);
		return path;
	}
}
