using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal class AbstractRegionControlFlowPass : ControlFlowPass
{
	internal AbstractRegionControlFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	public override BoundNode Visit(BoundNode node)
	{
		VisitAlways(node);
		return null;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		return VisitLocalFunctionOrLambda(node.Body);
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		return VisitLocalFunctionOrLambda(node.Body);
	}

	private BoundNode VisitLocalFunctionOrLambda(BoundBlock body)
	{
		SavedPending oldPending = SavePending();
		LocalState self = State;
		State = ReachableState();
		SavedPending oldPending2 = SavePending();
		VisitAlways(body);
		RestorePending(oldPending2);
		ImmutableArray<PendingBranch> immutableArray = RemoveReturns();
		RestorePending(oldPending);
		IntersectWith(ref self, ref State);
		foreach (PendingBranch item in immutableArray)
		{
			State = item.State;
			IntersectWith(ref self, ref State);
		}
		State = self;
		return null;
	}
}
