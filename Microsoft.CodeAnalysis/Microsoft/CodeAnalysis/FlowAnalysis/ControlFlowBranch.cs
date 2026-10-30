using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

public sealed class ControlFlowBranch
{
	private ImmutableArray<ControlFlowRegion> _lazyLeavingRegions;

	private ImmutableArray<ControlFlowRegion> _lazyFinallyRegions;

	private ImmutableArray<ControlFlowRegion> _lazyEnteringRegions;

	public BasicBlock Source { get; }

	public BasicBlock Destination { get; }

	public ControlFlowBranchSemantics Semantics { get; }

	public bool IsConditionalSuccessor { get; }

	public ImmutableArray<ControlFlowRegion> LeavingRegions
	{
		get
		{
			if (_lazyLeavingRegions.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(value: (Destination != null) ? CollectRegions(Destination.Ordinal, Source.EnclosingRegion).ToImmutableAndFree() : ImmutableArray<ControlFlowRegion>.Empty, location: ref _lazyLeavingRegions);
			}
			return _lazyLeavingRegions;
		}
	}

	public ImmutableArray<ControlFlowRegion> EnteringRegions
	{
		get
		{
			if (_lazyEnteringRegions.IsDefault)
			{
				ImmutableArray<ControlFlowRegion> value;
				if (Destination == null)
				{
					value = ImmutableArray<ControlFlowRegion>.Empty;
				}
				else
				{
					ArrayBuilder<ControlFlowRegion> arrayBuilder = CollectRegions(Source.Ordinal, Destination.EnclosingRegion);
					arrayBuilder.ReverseContents();
					value = arrayBuilder.ToImmutableAndFree();
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyEnteringRegions, value);
			}
			return _lazyEnteringRegions;
		}
	}

	public ImmutableArray<ControlFlowRegion> FinallyRegions
	{
		get
		{
			if (_lazyFinallyRegions.IsDefault)
			{
				ArrayBuilder<ControlFlowRegion> arrayBuilder = null;
				ImmutableArray<ControlFlowRegion> leavingRegions = LeavingRegions;
				int num = leavingRegions.Length - 1;
				for (int i = 0; i < num; i++)
				{
					if (leavingRegions[i].Kind == ControlFlowRegionKind.Try && leavingRegions[i + 1].Kind == ControlFlowRegionKind.TryAndFinally)
					{
						if (arrayBuilder == null)
						{
							arrayBuilder = ArrayBuilder<ControlFlowRegion>.GetInstance();
						}
						arrayBuilder.Add(leavingRegions[i + 1].NestedRegions.Last());
					}
				}
				ImmutableArray<ControlFlowRegion> value = arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<ControlFlowRegion>.Empty;
				ImmutableInterlocked.InterlockedInitialize(ref _lazyFinallyRegions, value);
			}
			return _lazyFinallyRegions;
		}
	}

	internal ControlFlowBranch(BasicBlock source, BasicBlock destination, ControlFlowBranchSemantics semantics, bool isConditionalSuccessor)
	{
		Source = source;
		Destination = destination;
		Semantics = semantics;
		IsConditionalSuccessor = isConditionalSuccessor;
	}

	private static ArrayBuilder<ControlFlowRegion> CollectRegions(int destinationOrdinal, ControlFlowRegion source)
	{
		ArrayBuilder<ControlFlowRegion> instance = ArrayBuilder<ControlFlowRegion>.GetInstance();
		while (!source.ContainsBlock(destinationOrdinal))
		{
			instance.Add(source);
			source = source.EnclosingRegion;
		}
		return instance;
	}
}
