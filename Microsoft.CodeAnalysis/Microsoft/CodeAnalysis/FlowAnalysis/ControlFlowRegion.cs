using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

public sealed class ControlFlowRegion
{
	public ControlFlowRegionKind Kind { get; }

	public ControlFlowRegion EnclosingRegion { get; private set; }

	public ITypeSymbol ExceptionType { get; }

	public int FirstBlockOrdinal { get; }

	public int LastBlockOrdinal { get; }

	public ImmutableArray<ControlFlowRegion> NestedRegions { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public ImmutableArray<IMethodSymbol> LocalFunctions { get; }

	public ImmutableArray<CaptureId> CaptureIds { get; }

	internal ControlFlowRegion(ControlFlowRegionKind kind, int firstBlockOrdinal, int lastBlockOrdinal, ImmutableArray<ControlFlowRegion> nestedRegions, ImmutableArray<ILocalSymbol> locals, ImmutableArray<IMethodSymbol> methods, ImmutableArray<CaptureId> captureIds, ITypeSymbol exceptionType, ControlFlowRegion enclosingRegion)
	{
		Kind = kind;
		FirstBlockOrdinal = firstBlockOrdinal;
		LastBlockOrdinal = lastBlockOrdinal;
		ExceptionType = exceptionType;
		Locals = locals.NullToEmpty();
		LocalFunctions = methods.NullToEmpty();
		CaptureIds = captureIds.NullToEmpty();
		NestedRegions = nestedRegions.NullToEmpty();
		EnclosingRegion = enclosingRegion;
		foreach (ControlFlowRegion nestedRegion in NestedRegions)
		{
			nestedRegion.EnclosingRegion = this;
		}
	}

	internal bool ContainsBlock(int destinationOrdinal)
	{
		if (FirstBlockOrdinal <= destinationOrdinal)
		{
			return LastBlockOrdinal >= destinationOrdinal;
		}
		return false;
	}
}
