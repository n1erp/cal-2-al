using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public abstract class DataFlowAnalysis
{
	public abstract ImmutableArray<ISymbol> VariablesDeclared { get; }

	public abstract ImmutableArray<ISymbol> DataFlowsIn { get; }

	public abstract ImmutableArray<ISymbol> DataFlowsOut { get; }

	public abstract ImmutableArray<ISymbol> AlwaysAssigned { get; }

	public abstract ImmutableArray<ISymbol> ReadInside { get; }

	public abstract ImmutableArray<ISymbol> WrittenInside { get; }

	public abstract ImmutableArray<ISymbol> ReadOutside { get; }

	public abstract ImmutableArray<ISymbol> WrittenOutside { get; }

	public abstract ImmutableArray<ISymbol> Captured { get; }

	public abstract ImmutableArray<ISymbol> CapturedInside { get; }

	public abstract ImmutableArray<ISymbol> CapturedOutside { get; }

	public abstract ImmutableArray<ISymbol> UnsafeAddressTaken { get; }

	public abstract bool Succeeded { get; }
}
