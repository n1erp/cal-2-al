using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal class DiffLineComparer : IEqualityComparer<string>
{
	internal static readonly DiffLineComparer DefaultComparer = new DiffLineComparer(ignoreLeadingAndTrailingWhiteSpace: false);

	internal bool IgnoreLeadingAndTrailingWhiteSpace { get; private set; }

	internal DiffLineComparer(bool ignoreLeadingAndTrailingWhiteSpace)
	{
		IgnoreLeadingAndTrailingWhiteSpace = ignoreLeadingAndTrailingWhiteSpace;
	}

	public bool Equals(string left, string right)
	{
		if (IgnoreLeadingAndTrailingWhiteSpace)
		{
			return StringComparer.Ordinal.Equals(left.Trim(), right.Trim());
		}
		return StringComparer.Ordinal.Equals(left, right);
	}

	public int GetHashCode(string obj)
	{
		return obj.GetHashCode();
	}
}
