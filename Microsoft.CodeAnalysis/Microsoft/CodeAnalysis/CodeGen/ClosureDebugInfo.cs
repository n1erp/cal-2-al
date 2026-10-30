using System;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct ClosureDebugInfo(int syntaxOffset, DebugId closureId) : IEquatable<ClosureDebugInfo>
{
	public readonly int SyntaxOffset = syntaxOffset;

	public readonly DebugId ClosureId = closureId;

	public bool Equals(ClosureDebugInfo other)
	{
		if (SyntaxOffset == other.SyntaxOffset)
		{
			return ClosureId.Equals(other.ClosureId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ClosureDebugInfo)
		{
			return Equals((ClosureDebugInfo)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(SyntaxOffset, ClosureId.GetHashCode());
	}

	internal string GetDebuggerDisplay()
	{
		return $"({ClosureId.GetDebuggerDisplay()} @{SyntaxOffset})";
	}
}
