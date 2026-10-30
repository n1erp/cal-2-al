using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal struct LocalSlotDebugInfo(SynthesizedLocalKind synthesizedKind, LocalDebugId id) : IEquatable<LocalSlotDebugInfo>
{
	public readonly SynthesizedLocalKind SynthesizedKind = synthesizedKind;

	public readonly LocalDebugId Id = id;

	public bool Equals(LocalSlotDebugInfo other)
	{
		if (SynthesizedKind == other.SynthesizedKind)
		{
			return Id.Equals(other.Id);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is LocalSlotDebugInfo)
		{
			return Equals((LocalSlotDebugInfo)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine((int)SynthesizedKind, Id.GetHashCode());
	}

	public override string ToString()
	{
		return SynthesizedKind.ToString() + " " + Id.ToString();
	}
}
