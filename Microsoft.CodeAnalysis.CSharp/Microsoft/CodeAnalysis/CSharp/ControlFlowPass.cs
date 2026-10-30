using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ControlFlowPass : AbstractFlowPass<ControlFlowPass.LocalState>
{
	internal struct LocalState : AbstractLocalState
	{
		internal bool Alive;

		internal bool Reported;

		public bool Reachable => Alive;

		internal LocalState(bool live, bool reported)
		{
			Alive = live;
			Reported = reported;
		}

		public LocalState Clone()
		{
			return this;
		}
	}

	private readonly PooledHashSet<LabelSymbol> _labelsDefined = PooledHashSet<LabelSymbol>.GetInstance();

	private readonly PooledHashSet<LabelSymbol> _labelsUsed = PooledHashSet<LabelSymbol>.GetInstance();

	protected bool _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;

	protected override void Free()
	{
		_labelsDefined.Free();
		_labelsUsed.Free();
		base.Free();
	}

	internal ControlFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node)
		: base(compilation, member, node, false)
	{
	}

	internal ControlFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion, true, false)
	{
	}

	protected override void UnionWith(ref LocalState self, ref LocalState other)
	{
		self.Alive &= other.Alive;
		self.Reported &= other.Reported;
	}

	protected override bool IntersectWith(ref LocalState self, ref LocalState other)
	{
		LocalState localState = self;
		self.Alive |= other.Alive;
		self.Reported &= other.Reported;
		return self.Alive != localState.Alive;
	}

	protected override string Dump(LocalState state)
	{
		return "[alive: " + state.Alive + "; reported: " + state.Reported + "]";
	}

	protected override LocalState ReachableState()
	{
		return new LocalState(live: true, reported: false);
	}

	protected override LocalState UnreachableState()
	{
		return new LocalState(live: false, State.Reported);
	}

	protected override LocalState LabelState(LabelSymbol label)
	{
		LocalState result = base.LabelState(label);
		result.Reported = false;
		return result;
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (!(node is BoundExpression))
		{
			return base.Visit(node);
		}
		return null;
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		Diagnostics.Clear();
		ImmutableArray<PendingBranch> result = base.Scan(ref badRegion);
		foreach (LabelSymbol item in _labelsDefined)
		{
			if (!_labelsUsed.Contains(item))
			{
				Diagnostics.Add(ErrorCode.WRN_UnreferencedLabel, item.Locations[0]);
			}
		}
		return result;
	}

	public static bool Analyze(CSharpCompilation compilation, Symbol member, BoundBlock block, DiagnosticBag diagnostics)
	{
		ControlFlowPass controlFlowPass = new ControlFlowPass(compilation, member, block);
		if (diagnostics != null)
		{
			controlFlowPass._convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = true;
		}
		try
		{
			bool badRegion = false;
			return controlFlowPass.Analyze(ref badRegion, diagnostics);
		}
		catch (CancelledByStackGuardException ex) when (diagnostics != null)
		{
			ex.AddAnError(diagnostics);
			return true;
		}
		finally
		{
			controlFlowPass.Free();
		}
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;
	}

	protected bool Analyze(ref bool badRegion, DiagnosticBag diagnostics)
	{
		Analyze(ref badRegion);
		diagnostics?.AddRange(Diagnostics);
		return State.Alive;
	}

	protected override ImmutableArray<PendingBranch> RemoveReturns()
	{
		ImmutableArray<PendingBranch> result = base.RemoveReturns();
		foreach (PendingBranch item in result)
		{
			switch (item.Branch.Kind)
			{
			case BoundKind.GotoStatement:
			{
				SourceLocation location2 = new SourceLocation(item.Branch.Syntax.GetFirstToken());
				Diagnostics.Add(ErrorCode.ERR_LabelNotFound, location2, ((BoundGotoStatement)item.Branch).Label.Name);
				break;
			}
			case BoundKind.BreakStatement:
			case BoundKind.ContinueStatement:
			{
				SourceLocation location = new SourceLocation(item.Branch.Syntax.GetFirstToken());
				Diagnostics.Add(ErrorCode.ERR_BadDelegateLeave, location);
				break;
			}
			}
		}
		return result;
	}

	protected override void VisitStatement(BoundStatement statement)
	{
		switch (statement.Kind)
		{
		case BoundKind.Block:
		case BoundKind.LocalFunctionStatement:
		case BoundKind.NoOpStatement:
		case BoundKind.ThrowStatement:
		case BoundKind.LabeledStatement:
			base.VisitStatement(statement);
			break;
		case BoundKind.StatementList:
			base.VisitStatementList((BoundStatementList)statement);
			break;
		default:
			CheckReachable(statement);
			base.VisitStatement(statement);
			break;
		}
	}

	private void CheckReachable(BoundStatement statement)
	{
		if (!State.Alive && !State.Reported && !statement.WasCompilerGenerated && statement.Syntax.Span.Length != 0)
		{
			SyntaxToken token = statement.Syntax.GetFirstToken();
			Diagnostics.Add(ErrorCode.WRN_UnreachableCode, new SourceLocation(in token));
			State.Reported = true;
		}
	}

	protected override void VisitTryBlock(BoundStatement tryBlock, BoundTryStatement node, ref LocalState tryState)
	{
		if (node.CatchBlocks.IsEmpty)
		{
			base.VisitTryBlock(tryBlock, node, ref tryState);
			return;
		}
		SavedPending oldPending = SavePending();
		base.VisitTryBlock(tryBlock, node, ref tryState);
		RestorePending(oldPending);
	}

	protected override void VisitCatchBlock(BoundCatchBlock catchBlock, ref LocalState finallyState)
	{
		SavedPending oldPending = SavePending();
		base.VisitCatchBlock(catchBlock, ref finallyState);
		RestorePending(oldPending);
	}

	protected override void VisitFinallyBlock(BoundStatement finallyBlock, ref LocalState endState)
	{
		SavedPending oldPending = SavePending();
		SavedPending oldPending2 = SavePending();
		base.VisitFinallyBlock(finallyBlock, ref endState);
		RestorePending(oldPending2);
		foreach (PendingBranch pendingBranch in PendingBranches)
		{
			if (pendingBranch.Branch != null)
			{
				SourceLocation location = new SourceLocation(pendingBranch.Branch.Syntax.GetFirstToken());
				BoundKind kind = pendingBranch.Branch.Kind;
				if (kind - 61 > BoundKind.PropertyEqualsValue)
				{
					Diagnostics.Add(ErrorCode.ERR_BadFinallyLeave, location);
				}
			}
		}
		RestorePending(oldPending);
	}

	protected override void VisitLabel(BoundLabeledStatement node)
	{
		_labelsDefined.Add(node.Label);
		base.VisitLabel(node);
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		VisitLabel(node);
		CheckReachable(node);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		_labelsUsed.Add(node.Label);
		return base.VisitGotoStatement(node);
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node, bool lastSection)
	{
		base.VisitSwitchSection(node);
		if (State.Alive)
		{
			BoundSwitchLabel boundSwitchLabel = node.SwitchLabels.Last();
			Diagnostics.Add(lastSection ? ErrorCode.ERR_SwitchFallOut : ErrorCode.ERR_SwitchFallThrough, new SourceLocation(boundSwitchLabel.Syntax), boundSwitchLabel.Label.Name);
		}
		return null;
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		return base.VisitPatternSwitchStatement(node);
	}

	protected override void VisitPatternSwitchSection(BoundPatternSwitchSection node, BoundExpression switchExpression, bool isLastSection)
	{
		base.VisitPatternSwitchSection(node, switchExpression, isLastSection);
		if (State.Alive)
		{
			SyntaxNode syntax = node.SwitchLabels.Last().Pattern.Syntax;
			Diagnostics.Add(isLastSection ? ErrorCode.ERR_SwitchFallOut : ErrorCode.ERR_SwitchFallThrough, new SourceLocation(syntax), syntax.ToString());
		}
	}
}
