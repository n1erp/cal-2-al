using System;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct DebugId(int ordinal, int generation) : IEquatable<DebugId>
{
	public const int UndefinedOrdinal = -1;

	public readonly int Ordinal = ordinal;

	public readonly int Generation = generation;

	public bool Equals(DebugId other)
	{
		if (Ordinal == other.Ordinal)
		{
			return Generation == other.Generation;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is DebugId)
		{
			return Equals((DebugId)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Ordinal, Generation);
	}

	internal string GetDebuggerDisplay()
	{
		if (Generation <= 0)
		{
			int ordinal = Ordinal;
			return ordinal.ToString();
		}
		return $"{Ordinal}#{Generation}";
	}
}
