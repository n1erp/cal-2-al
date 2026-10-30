using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class AlwaysAssignedWalker : AbstractRegionDataFlowPass
{
	private LocalState _endOfRegionState;

	private readonly HashSet<LabelSymbol> _labelsInside = new HashSet<LabelSymbol>();

	private AlwaysAssignedWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	internal static IEnumerable<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		AlwaysAssignedWalker alwaysAssignedWalker = new AlwaysAssignedWalker(compilation, member, node, firstInRegion, lastInRegion);
		bool badRegion = false;
		try
		{
			List<Symbol> list = alwaysAssignedWalker.Analyze(ref badRegion);
			IEnumerable<Symbol> result;
			if (!badRegion)
			{
				IEnumerable<Symbol> enumerable = list;
				result = enumerable;
			}
			else
			{
				result = SpecializedCollections.EmptyEnumerable<Symbol>();
			}
			return result;
		}
		finally
		{
			alwaysAssignedWalker.Free();
		}
	}

	private new List<Symbol> Analyze(ref bool badRegion)
	{
		Analyze(ref badRegion, null);
		List<Symbol> list = new List<Symbol>();
		if (_endOfRegionState.Reachable)
		{
			foreach (int item in _endOfRegionState.Assigned.TrueBits())
			{
				if (item < variableBySlot.Length)
				{
					VariableIdentifier variableIdentifier = variableBySlot[item];
					if (variableIdentifier.Exists && !(variableIdentifier.Symbol is FieldSymbol))
					{
						list.Add(variableIdentifier.Symbol);
					}
				}
			}
		}
		return list;
	}

	protected override void WriteArgument(BoundExpression arg, RefKind refKind, MethodSymbol method)
	{
		if (refKind == RefKind.Out)
		{
			Assign(arg, null);
		}
	}

	protected override void ResolveBranch(PendingBranch pending, LabelSymbol label, BoundStatement target, ref bool labelStateChanged)
	{
		if (IsInside && pending.Branch != null && !RegionContains(pending.Branch.Syntax.Span))
		{
			pending.State = (pending.State.Reachable ? ReachableState() : UnreachableState());
		}
		base.ResolveBranch(pending, label, target, ref labelStateChanged);
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		ResolveLabel(node, node.Label);
		return base.VisitLabel(node);
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		ResolveLabel(node, node.Label);
		return base.VisitLabeledStatement(node);
	}

	private void ResolveLabel(BoundNode node, LabelSymbol label)
	{
		if (node.Syntax != null && RegionContains(node.Syntax.Span))
		{
			_labelsInside.Add(label);
		}
	}

	protected override void EnterRegion()
	{
		State = ReachableState();
		base.EnterRegion();
	}

	protected override void LeaveRegion()
	{
		if (IsConditionalState)
		{
			_endOfRegionState = StateWhenTrue.Clone();
			IntersectWith(ref _endOfRegionState, ref StateWhenFalse);
		}
		else
		{
			_endOfRegionState = State.Clone();
		}
		foreach (PendingBranch pendingBranch in PendingBranches)
		{
			if (pendingBranch.Branch != null && RegionContains(pendingBranch.Branch.Syntax.Span) && !_labelsInside.Contains(pendingBranch.Label))
			{
				IntersectWith(ref _endOfRegionState, ref pendingBranch.State);
			}
		}
		base.LeaveRegion();
	}
}
