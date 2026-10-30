using System;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal struct LoadDirective(string resolvedPath, ImmutableArray<Diagnostic> diagnostics) : IEquatable<LoadDirective>
{
	public readonly string ResolvedPath = resolvedPath;

	public readonly ImmutableArray<Diagnostic> Diagnostics = diagnostics;

	public bool Equals(LoadDirective other)
	{
		if (ResolvedPath == other.ResolvedPath)
		{
			return Diagnostics.SequenceEqual(other.Diagnostics);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is LoadDirective)
		{
			return Equals((LoadDirective)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Diagnostics.GetHashCode(), ResolvedPath.GetHashCode());
	}
}
