using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal struct MethodImplKey : IEquatable<MethodImplKey>
{
	internal readonly int ImplementingMethod;

	internal readonly int Index;

	internal MethodImplKey(int implementingMethod, int index)
	{
		ImplementingMethod = implementingMethod;
		Index = index;
	}

	public override bool Equals(object obj)
	{
		if (obj is MethodImplKey)
		{
			return Equals((MethodImplKey)obj);
		}
		return false;
	}

	public bool Equals(MethodImplKey other)
	{
		if (ImplementingMethod == other.ImplementingMethod)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ImplementingMethod, Index);
	}
}
