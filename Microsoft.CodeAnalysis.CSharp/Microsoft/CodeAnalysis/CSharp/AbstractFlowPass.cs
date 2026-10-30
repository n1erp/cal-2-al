namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class AbstractFlowPass<TLocalState> : PreciseAbstractFlowPass<TLocalState> where TLocalState : PreciseAbstractFlowPass<TLocalState>.AbstractLocalState
{
	protected readonly bool trackUnassignments;

	protected AbstractFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, bool trackUnassignments = false)
		: base(compilation, member, node, (BoundNode)null, (BoundNode)null, false, false)
	{
		this.trackUnassignments = trackUnassignments;
	}

	protected AbstractFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, bool trackRegions = true, bool trackUnassignments = false)
		: base(compilation, member, node, firstInRegion, lastInRegion, trackRegions, false)
	{
		this.trackUnassignments = trackUnassignments;
	}

	protected abstract void UnionWith(ref TLocalState self, ref TLocalState other);

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		SavedPending oldPending = SavePending();
		TLocalState tryState = State.Clone();
		SavedPending oldPending2 = SavePending();
		VisitTryBlock(node.TryBlock, node, ref tryState);
		TLocalState finallyState = tryState.Clone();
		TLocalState self = State;
		foreach (BoundCatchBlock catchBlock in node.CatchBlocks)
		{
			SetState(tryState.Clone());
			VisitCatchBlock(catchBlock, ref finallyState);
			IntersectWith(ref self, ref State);
		}
		RestorePending(oldPending2);
		if (node.FinallyBlockOpt != null)
		{
			SetState(finallyState);
			SavedPending oldPending3 = SavePending();
			TLocalState unsetInFinally = AllBitsSet();
			VisitFinallyBlock(node.FinallyBlockOpt, ref unsetInFinally);
			foreach (PendingBranch pendingBranch in oldPending3.PendingBranches)
			{
				if (pendingBranch.Branch != null && pendingBranch.Branch.Kind != BoundKind.YieldReturnStatement)
				{
					UnionWith(ref pendingBranch.State, ref State);
					if (trackUnassignments)
					{
						IntersectWith(ref pendingBranch.State, ref unsetInFinally);
					}
				}
			}
			RestorePending(oldPending3);
			UnionWith(ref self, ref State);
			if (trackUnassignments)
			{
				IntersectWith(ref self, ref unsetInFinally);
			}
		}
		SetState(self);
		RestorePending(oldPending);
		return null;
	}

	protected virtual void VisitTryBlock(BoundStatement tryBlock, BoundTryStatement node, ref TLocalState tryState)
	{
		VisitStatement(tryBlock);
	}

	protected virtual void VisitCatchBlock(BoundCatchBlock catchBlock, ref TLocalState finallyState)
	{
		if (catchBlock.ExceptionSourceOpt != null)
		{
			VisitLvalue(catchBlock.ExceptionSourceOpt);
		}
		if (catchBlock.ExceptionFilterOpt != null)
		{
			VisitCondition(catchBlock.ExceptionFilterOpt);
			SetState(StateWhenTrue);
		}
		VisitStatement(catchBlock.Body);
	}

	protected virtual void VisitFinallyBlock(BoundStatement finallyBlock, ref TLocalState unsetInFinally)
	{
		VisitStatement(finallyBlock);
	}
}
