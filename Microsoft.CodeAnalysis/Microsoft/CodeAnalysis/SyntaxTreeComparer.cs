using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class SyntaxTreeComparer : IEqualityComparer<SyntaxTree>
{
	public static readonly SyntaxTreeComparer Instance = new SyntaxTreeComparer();

	public bool Equals(SyntaxTree x, SyntaxTree y)
	{
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		if (string.Equals(x.FilePath, y.FilePath, StringComparison.OrdinalIgnoreCase))
		{
			return SourceTextComparer.Instance.Equals(x.GetText(), y.GetText());
		}
		return false;
	}

	public int GetHashCode(SyntaxTree obj)
	{
		return Hash.Combine(obj.FilePath.GetHashCode(), SourceTextComparer.Instance.GetHashCode(obj.GetText()));
	}
}
