using System;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct LambdaDebugInfo(int syntaxOffset, DebugId lambdaId, int closureOrdinal) : IEquatable<LambdaDebugInfo>
{
	public readonly int SyntaxOffset = syntaxOffset;

	public readonly int ClosureOrdinal = closureOrdinal;

	public readonly DebugId LambdaId = lambdaId;

	public const int StaticClosureOrdinal = -1;

	public const int ThisOnlyClosureOrdinal = -2;

	public const int MinClosureOrdinal = -2;

	public bool Equals(LambdaDebugInfo other)
	{
		if (SyntaxOffset == other.SyntaxOffset && ClosureOrdinal == other.ClosureOrdinal)
		{
			return LambdaId.Equals(other.LambdaId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is LambdaDebugInfo)
		{
			return Equals((LambdaDebugInfo)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ClosureOrdinal, Hash.Combine(SyntaxOffset, LambdaId.GetHashCode()));
	}

	internal string GetDebuggerDisplay()
	{
		if (ClosureOrdinal != -1)
		{
			if (ClosureOrdinal != -2)
			{
				return $"({LambdaId.GetDebuggerDisplay()} @{SyntaxOffset} in {ClosureOrdinal})";
			}
			return $"(#{LambdaId.GetDebuggerDisplay()} @{SyntaxOffset}, this)";
		}
		return $"({LambdaId.GetDebuggerDisplay()} @{SyntaxOffset}, static)";
	}
}
