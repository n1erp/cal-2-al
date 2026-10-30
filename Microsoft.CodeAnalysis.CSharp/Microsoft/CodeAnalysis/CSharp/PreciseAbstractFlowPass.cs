using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class PreciseAbstractFlowPass<LocalState> : BoundTreeVisitor where LocalState : PreciseAbstractFlowPass<LocalState>.AbstractLocalState
{
	internal interface AbstractLocalState
	{
		bool Reachable { get; }

		LocalState Clone();
	}

	protected enum RegionPlace
	{
		Before,
		Inside,
		After
	}

	internal class PendingBranch
	{
		public readonly BoundNode Branch;

		public LocalState State;

		public LabelSymbol Label
		{
			get
			{
				if (Branch == null)
				{
					return null;
				}
				return Branch.Kind switch
				{
					BoundKind.GotoStatement => ((BoundGotoStatement)Branch).Label, 
					BoundKind.ConditionalGoto => ((BoundConditionalGoto)Branch).Label, 
					BoundKind.BreakStatement => ((BoundBreakStatement)Branch).Label, 
					BoundKind.ContinueStatement => ((BoundContinueStatement)Branch).Label, 
					BoundKind.PatternSwitchLabel => ((BoundPatternSwitchLabel)Branch).Label, 
					_ => null, 
				};
			}
		}

		public PendingBranch(BoundNode branch, LocalState state)
		{
			Branch = branch;
			State = state.Clone();
		}
	}

	protected struct SavedPending(ArrayBuilder<PendingBranch> pendingBranches, PooledHashSet<BoundStatement> labelsSeen)
	{
		public readonly ArrayBuilder<PendingBranch> PendingBranches = pendingBranches;

		public readonly PooledHashSet<BoundStatement> LabelsSeen = labelsSeen;
	}

	protected int _recursionDepth;

	protected readonly CSharpCompilation compilation;

	private readonly Symbol _member;

	protected readonly BoundNode methodMainNode;

	private readonly PooledDictionary<LabelSymbol, LocalState> _labels;

	protected bool stateChangedAfterUse;

	private ArrayBuilder<PendingBranch> _pendingBranches;

	private PooledHashSet<BoundStatement> _labelsSeen;

	private readonly bool _trackExceptions;

	protected LocalState State;

	protected LocalState StateWhenTrue;

	protected LocalState StateWhenFalse;

	protected bool IsConditionalState;

	protected RegionPlace regionPlace;

	protected readonly BoundNode firstInRegion;

	protected readonly BoundNode lastInRegion;

	private readonly bool _trackRegions;

	private readonly Dictionary<BoundLoopStatement, LocalState> _loopHeadState;

	protected readonly TextSpan RegionSpan;

	protected ArrayBuilder<PendingBranch> PendingBranches => _pendingBranches;

	protected DiagnosticBag Diagnostics { get; }

	protected bool IsInside => regionPlace == RegionPlace.Inside;

	protected ImmutableArray<ParameterSymbol> MethodParameters
	{
		get
		{
			if (_member is MethodSymbol methodSymbol)
			{
				return methodSymbol.Parameters;
			}
			return ImmutableArray<ParameterSymbol>.Empty;
		}
	}

	protected ParameterSymbol MethodThisParameter
	{
		get
		{
			if (_member is MethodSymbol methodSymbol)
			{
				return methodSymbol.ThisParameter;
			}
			return null;
		}
	}

	protected abstract bool IntersectWith(ref LocalState self, ref LocalState other);

	protected void SetConditionalState(LocalState whenTrue, LocalState whenFalse)
	{
		IsConditionalState = true;
		State = default;
		StateWhenTrue = whenTrue;
		StateWhenFalse = whenFalse;
	}

	protected void SetState(LocalState newState)
	{
		StateWhenTrue = (StateWhenFalse = default);
		IsConditionalState = false;
		State = newState;
	}

	protected void Split()
	{
		if (!IsConditionalState)
		{
			SetConditionalState(State, State.Clone());
		}
	}

	protected void Unsplit()
	{
		if (IsConditionalState)
		{
			IntersectWith(ref StateWhenTrue, ref StateWhenFalse);
			SetState(StateWhenTrue);
		}
	}

	protected PreciseAbstractFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion = null, BoundNode lastInRegion = null, bool trackRegions = false, bool trackExceptions = false)
	{
		if (firstInRegion != null && lastInRegion != null)
		{
			trackRegions = true;
		}
		if (trackRegions)
		{
			int spanStart = firstInRegion.Syntax.SpanStart;
			int length = lastInRegion.Syntax.Span.End - spanStart;
			RegionSpan = new TextSpan(spanStart, length);
		}
		_pendingBranches = ArrayBuilder<PendingBranch>.GetInstance();
		_labelsSeen = PooledHashSet<BoundStatement>.GetInstance();
		_labels = PooledDictionary<LabelSymbol, LocalState>.GetInstance();
		Diagnostics = DiagnosticBag.GetInstance();
		this.compilation = compilation;
		_member = member;
		methodMainNode = node;
		this.firstInRegion = firstInRegion;
		this.lastInRegion = lastInRegion;
		_loopHeadState = new Dictionary<BoundLoopStatement, LocalState>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		_trackRegions = trackRegions;
		_trackExceptions = trackExceptions;
	}

	protected abstract string Dump(LocalState state);

	protected string Dump()
	{
		if (!IsConditionalState)
		{
			return Dump(State);
		}
		return "true: " + Dump(StateWhenTrue) + " false: " + Dump(StateWhenFalse);
	}

	protected virtual void EnterRegion()
	{
		regionPlace = RegionPlace.Inside;
	}

	protected virtual void LeaveRegion()
	{
		regionPlace = RegionPlace.After;
	}

	protected bool RegionContains(TextSpan span)
	{
		if (span.Length == 0)
		{
			return RegionSpan.Contains(span.Start);
		}
		return RegionSpan.Contains(span);
	}

	public override BoundNode Visit(BoundNode node)
	{
		return VisitAlways(node);
	}

	protected BoundNode VisitAlways(BoundNode node)
	{
		BoundNode result = null;
		if (node != null)
		{
			if (_trackRegions)
			{
				if (node == firstInRegion && regionPlace == RegionPlace.Before)
				{
					EnterRegion();
				}
				result = VisitWithStackGuard(node);
				if (node == lastInRegion && regionPlace == RegionPlace.Inside)
				{
					LeaveRegion();
				}
			}
			else
			{
				result = VisitWithStackGuard(node);
			}
		}
		return result;
	}

	private BoundNode VisitWithStackGuard(BoundNode node)
	{
		if (node is BoundExpression node2)
		{
			return VisitExpressionWithStackGuard(ref _recursionDepth, node2);
		}
		return base.Visit(node);
	}

	protected override BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
	{
		return (BoundExpression)base.Visit(node);
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return false;
	}

	protected abstract LocalState ReachableState();

	protected abstract LocalState UnreachableState();

	protected virtual ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		SavedPending oldPending = SavePending();
		Visit(methodMainNode);
		Unsplit();
		RestorePending(oldPending);
		if (_trackRegions && regionPlace != RegionPlace.After)
		{
			badRegion = true;
		}
		return RemoveReturns();
	}

	protected ImmutableArray<PendingBranch> Analyze(ref bool badRegion)
	{
		ImmutableArray<PendingBranch> result;
		do
		{
			regionPlace = RegionPlace.Before;
			State = ReachableState();
			_pendingBranches.Clear();
			if (_trackExceptions)
			{
				_pendingBranches.Add(new PendingBranch(null, ReachableState()));
			}
			stateChangedAfterUse = false;
			Diagnostics.Clear();
			result = Scan(ref badRegion);
		}
		while (stateChangedAfterUse);
		return result;
	}

	protected virtual void Free()
	{
		Diagnostics.Free();
		_pendingBranches.Free();
		_labelsSeen.Free();
		_labels.Free();
	}

	protected bool ShouldAnalyzeOutParameters(out Location location)
	{
		if (!(_member is MethodSymbol { Locations: { Length: 1 } } methodSymbol))
		{
			location = null;
			return false;
		}
		location = methodSymbol.Locations[0];
		return true;
	}

	protected virtual LocalState LabelState(LabelSymbol label)
	{
		if (_labels.TryGetValue(label, out var value))
		{
			return value;
		}
		value = UnreachableState();
		_labels.Add(label, value);
		return value;
	}

	protected virtual ImmutableArray<PendingBranch> RemoveReturns()
	{
		ImmutableArray<PendingBranch> result;
		if (_trackExceptions)
		{
			result = _pendingBranches.Where((PendingBranch b) => b.Branch != null).AsImmutableOrNull();
			PendingBranch item = _pendingBranches[0];
			_pendingBranches.Clear();
			_pendingBranches.Add(item);
		}
		else
		{
			result = _pendingBranches.ToImmutable();
			_pendingBranches.Clear();
		}
		return result;
	}

	protected void SetUnreachable()
	{
		State = UnreachableState();
	}

	protected void VisitLvalue(BoundExpression node)
	{
		if (_trackRegions && node == firstInRegion && regionPlace == RegionPlace.Before)
		{
			EnterRegion();
		}
		switch (node?.Kind)
		{
		case BoundKind.Parameter:
			VisitLvalueParameter((BoundParameter)node);
			break;
		case BoundKind.Local:
			VisitLvalue((BoundLocal)node);
			break;
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node;
			if (Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, _member))
			{
				SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (boundPropertyAccess.PropertySymbol as SourcePropertySymbol)?.BackingField;
				if (synthesizedBackingFieldSymbol != null)
				{
					VisitFieldAccessInternal(boundPropertyAccess.ReceiverOpt, synthesizedBackingFieldSymbol);
					break;
				}
			}
			goto default;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			VisitFieldAccessInternal(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol);
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)node;
			VisitFieldAccessInternal(boundEventAccess.ReceiverOpt, boundEventAccess.EventSymbol.AssociatedField);
			break;
		}
		case BoundKind.TupleLiteral:
			((BoundTupleExpression)node).VisitAllElements((BoundExpression x, PreciseAbstractFlowPass<LocalState> self) =>
			{
				self.VisitLvalue(x);
			}, this);
			break;
		default:
			VisitRvalue(node);
			break;
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			break;
		}
		if (_trackRegions && node == lastInRegion && regionPlace == RegionPlace.Inside)
		{
			LeaveRegion();
		}
	}

	protected virtual void VisitLvalue(BoundLocal node)
	{
	}

	protected void VisitCondition(BoundExpression node)
	{
		Visit(node);
		AdjustConditionalState(node);
	}

	private void AdjustConditionalState(BoundExpression node)
	{
		if (IsConstantTrue(node))
		{
			Unsplit();
			SetConditionalState(State, UnreachableState());
		}
		else if (IsConstantFalse(node))
		{
			Unsplit();
			SetConditionalState(UnreachableState(), State);
		}
		else if ((object)node.Type == null || node.Type.SpecialType != SpecialType.System_Boolean)
		{
			Unsplit();
		}
		Split();
	}

	protected BoundNode VisitRvalue(BoundExpression node)
	{
		BoundNode result = Visit(node);
		Unsplit();
		return result;
	}

	[DebuggerHidden]
	protected virtual void VisitStatement(BoundStatement statement)
	{
		Visit(statement);
	}

	private static bool IsConstantTrue(BoundExpression node)
	{
		return node.ConstantValue == ConstantValue.True;
	}

	private static bool IsConstantFalse(BoundExpression node)
	{
		return node.ConstantValue == ConstantValue.False;
	}

	private static bool IsConstantNull(BoundExpression node)
	{
		return node.ConstantValue == ConstantValue.Null;
	}

	private void LoopHead(BoundLoopStatement node)
	{
		if (_loopHeadState.TryGetValue(node, out var value))
		{
			IntersectWith(ref State, ref value);
		}
		_loopHeadState[node] = State.Clone();
	}

	private void LoopTail(BoundLoopStatement node)
	{
		LocalState self = _loopHeadState[node];
		if (IntersectWith(ref self, ref State))
		{
			_loopHeadState[node] = self;
			stateChangedAfterUse = true;
		}
	}

	private void ResolveBreaks(LocalState breakState, LabelSymbol label)
	{
		ArrayBuilder<PendingBranch> pendingBranches = _pendingBranches;
		int count = pendingBranches.Count;
		if (count != 0)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				PendingBranch pendingBranch = pendingBranches[i];
				if (pendingBranch.Label == label)
				{
					IntersectWith(ref breakState, ref pendingBranch.State);
					continue;
				}
				if (num != i)
				{
					pendingBranches[num] = pendingBranch;
				}
				num++;
			}
			pendingBranches.Clip(num);
		}
		SetState(breakState);
	}

	private void ResolveContinues(LabelSymbol continueLabel)
	{
		ArrayBuilder<PendingBranch> pendingBranches = _pendingBranches;
		int count = pendingBranches.Count;
		if (count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			PendingBranch pendingBranch = pendingBranches[i];
			if (pendingBranch.Label == continueLabel)
			{
				IntersectWith(ref State, ref pendingBranch.State);
				continue;
			}
			if (num != i)
			{
				pendingBranches[num] = pendingBranch;
			}
			num++;
		}
		pendingBranches.Clip(num);
	}

	protected virtual void NoteBranch(PendingBranch pending, BoundNode gotoStmt, BoundStatement target)
	{
	}

	protected virtual void NotePossibleException(BoundNode node)
	{
		IntersectWith(ref _pendingBranches[0].State, ref State);
	}

	private bool ResolveBranches(LabelSymbol label, BoundStatement target)
	{
		bool labelStateChanged = false;
		ArrayBuilder<PendingBranch> pendingBranches = _pendingBranches;
		int count = pendingBranches.Count;
		if (count != 0)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				PendingBranch pendingBranch = pendingBranches[i];
				if (pendingBranch.Label == label)
				{
					ResolveBranch(pendingBranch, label, target, ref labelStateChanged);
					continue;
				}
				if (num != i)
				{
					pendingBranches[num] = pendingBranch;
				}
				num++;
			}
			pendingBranches.Clip(num);
		}
		return labelStateChanged;
	}

	protected virtual void ResolveBranch(PendingBranch pending, LabelSymbol label, BoundStatement target, ref bool labelStateChanged)
	{
		LocalState self = LabelState(label);
		if (target != null)
		{
			NoteBranch(pending, pending.Branch, target);
		}
		if (IntersectWith(ref self, ref pending.State))
		{
			labelStateChanged = true;
			_labels[label] = self;
		}
	}

	protected SavedPending SavePending()
	{
		SavedPending result = new SavedPending(_pendingBranches, _labelsSeen);
		_pendingBranches = ArrayBuilder<PendingBranch>.GetInstance();
		_labelsSeen = PooledHashSet<BoundStatement>.GetInstance();
		if (_trackExceptions)
		{
			_pendingBranches.Add(new PendingBranch(null, State));
		}
		return result;
	}

	protected void RestorePending(SavedPending oldPending)
	{
		foreach (BoundStatement item in _labelsSeen)
		{
			switch (item.Kind)
			{
			case BoundKind.LabeledStatement:
			{
				BoundLabeledStatement boundLabeledStatement = (BoundLabeledStatement)item;
				stateChangedAfterUse |= ResolveBranches(boundLabeledStatement.Label, boundLabeledStatement);
				break;
			}
			case BoundKind.LabelStatement:
			{
				BoundLabelStatement boundLabelStatement = (BoundLabelStatement)item;
				stateChangedAfterUse |= ResolveBranches(boundLabelStatement.Label, boundLabelStatement);
				break;
			}
			case BoundKind.SwitchSection:
			{
				BoundSwitchSection boundSwitchSection = (BoundSwitchSection)item;
				foreach (BoundSwitchLabel switchLabel in boundSwitchSection.SwitchLabels)
				{
					stateChangedAfterUse |= ResolveBranches(switchLabel.Label, boundSwitchSection);
				}
				break;
			}
			case BoundKind.PatternSwitchSection:
			{
				BoundPatternSwitchSection boundPatternSwitchSection = (BoundPatternSwitchSection)item;
				foreach (BoundPatternSwitchLabel switchLabel2 in boundPatternSwitchSection.SwitchLabels)
				{
					stateChangedAfterUse |= ResolveBranches(switchLabel2.Label, boundPatternSwitchSection);
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(item.Kind);
			}
		}
		if (_trackExceptions)
		{
			IntersectWith(ref oldPending.PendingBranches[0].State, ref PendingBranches[0].State);
			int i = 1;
			for (int count = PendingBranches.Count; i < count; i++)
			{
				oldPending.PendingBranches.Add(PendingBranches[i]);
			}
		}
		else
		{
			oldPending.PendingBranches.AddRange(PendingBranches);
		}
		_pendingBranches.Free();
		_pendingBranches = oldPending.PendingBranches;
		_labelsSeen.Free();
		_labelsSeen = oldPending.LabelsSeen;
	}

	public override BoundNode DefaultVisit(BoundNode node)
	{
		return null;
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		return null;
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		VisitRvalue(node.Expression);
		SetUnreachable();
		return node;
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		VisitRvalue(node.Expression);
		return node;
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		VisitRvalue(node.Expression);
		VisitPattern(node.Expression, node.Pattern);
		return null;
	}

	public virtual void VisitPattern(BoundExpression expression, BoundPattern pattern)
	{
		Split();
		if (expression == null)
		{
			return;
		}
		bool? flag = CheckRefutations(expression, pattern);
		if (flag.HasValue)
		{
			switch (flag == true)
			{
			case true:
				SetState(StateWhenTrue);
				SetConditionalState(State, UnreachableState());
				break;
			case false:
				SetState(StateWhenFalse);
				SetConditionalState(UnreachableState(), State);
				break;
			}
		}
	}

	public override BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private bool? CheckRefutations(BoundExpression expression, BoundPattern pattern)
	{
		switch (pattern.Kind)
		{
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern boundDeclarationPattern = (BoundDeclarationPattern)pattern;
			if (!boundDeclarationPattern.IsVar)
			{
				BoundTypeExpression declaredType = boundDeclarationPattern.DeclaredType;
				if (declaredType == null || declaredType.Type?.IsValueType != true || (object)boundDeclarationPattern.DeclaredType.Type != expression.Type)
				{
					return expression.ConstantValue?.IsNull switch
					{
						true => (bool?)false, 
						false => true, 
						_ => null, 
					};
				}
			}
			return true;
		}
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern boundConstantPattern = (BoundConstantPattern)pattern;
			if (expression.ConstantValue == null || boundConstantPattern.ConstantValue == null)
			{
				return null;
			}
			return object.Equals(expression.ConstantValue.Value, boundConstantPattern.ConstantValue.Value);
		}
		default:
			return null;
		}
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	private BoundNode VisitTupleExpression(BoundTupleExpression node)
	{
		VisitArguments(node.Arguments, default, null);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null);
		VisitRvalue(node.InitializerExpressionOpt);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		VisitRvalue(node.ReceiverOpt);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		VisitRvalue(node.Receiver);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		VisitRvalue(node.Expression);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		foreach (BoundExpression part in node.Parts)
		{
			VisitRvalue(part);
		}
		return null;
	}

	public override BoundNode VisitStringInsert(BoundStringInsert node)
	{
		VisitRvalue(node.Value);
		if (node.Alignment != null)
		{
			VisitRvalue(node.Alignment);
		}
		if (node.Format != null)
		{
			VisitRvalue(node.Format);
		}
		return null;
	}

	public override BoundNode VisitArgList(BoundArgList node)
	{
		return null;
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, null);
		return null;
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitRefValueOperator(BoundRefValueOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		VisitStatement(node.Statement);
		return null;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		return null;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		return null;
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		if (node.InitializerOpt != null)
		{
			VisitRvalue(node.InitializerOpt);
			if (node.LocalSymbol.RefKind != RefKind.None)
			{
				WriteArgument(node.InitializerOpt, node.LocalSymbol.RefKind, null);
			}
		}
		return null;
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		VisitStatements(node.Statements);
		return null;
	}

	private void VisitStatements(ImmutableArray<BoundStatement> statements)
	{
		foreach (BoundStatement item in statements)
		{
			VisitStatement(item);
		}
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		VisitStatements(node.Statements);
		return null;
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		bool flag = node.Method.CallsAreOmitted(node.SyntaxTree);
		LocalState state = default;
		if (flag)
		{
			state = State.Clone();
			SetUnreachable();
		}
		VisitReceiverBeforeCall(node.ReceiverOpt, node.Method);
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, node.Method);
		UpdateStateForCall(node);
		VisitReceiverAfterCall(node.ReceiverOpt, node.Method);
		if (flag)
		{
			State = state;
		}
		return null;
	}

	protected virtual void UpdateStateForCall(BoundCall node)
	{
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
	}

	private void VisitReceiverBeforeCall(BoundExpression receiverOpt, MethodSymbol method)
	{
		if ((object)method == null || method.MethodKind != MethodKind.Constructor)
		{
			VisitRvalue(receiverOpt);
		}
	}

	private void VisitReceiverAfterCall(BoundExpression receiverOpt, MethodSymbol method)
	{
		NamedTypeSymbol containingType;
		if (receiverOpt != null && ((object)method == null || method.MethodKind == MethodKind.Constructor || ((object)(containingType = method.ContainingType) != null && !method.IsStatic && !containingType.IsReferenceType && !TypeIsImmutable(containingType))))
		{
			WriteArgument(receiverOpt, ((object)method == null || method.MethodKind != MethodKind.Constructor) ? RefKind.Ref : RefKind.Out, method);
		}
	}

	private static bool TypeIsImmutable(TypeSymbol t)
	{
		SpecialType specialType = t.SpecialType;
		if ((uint)(specialType - 7) <= 12u || specialType == SpecialType.System_DateTime)
		{
			return true;
		}
		return t.IsNullableType();
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		VisitRvalue(node.ReceiverOpt);
		MethodSymbol methodSymbol = node.Indexer.GetOwnOrInheritedGetMethod() ?? node.Indexer.SetMethod;
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, methodSymbol);
		if (_trackExceptions && (object)methodSymbol != null)
		{
			NotePossibleException(node);
		}
		if ((object)methodSymbol != null)
		{
			VisitReceiverAfterCall(node.ReceiverOpt, methodSymbol);
		}
		return null;
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		VisitRvalue(node.ReceiverOpt);
		VisitRvalue(node.Argument);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	private void VisitArguments(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, MethodSymbol method)
	{
		if (refKindsOpt.IsDefault)
		{
			for (int i = 0; i < arguments.Length; i++)
			{
				VisitRvalue(arguments[i]);
			}
			return;
		}
		for (int j = 0; j < arguments.Length; j++)
		{
			if (((refKindsOpt.Length > j) ? refKindsOpt[j] : RefKind.None) != RefKind.Out)
			{
				VisitRvalue(arguments[j]);
			}
			else
			{
				VisitLvalue(arguments[j]);
			}
		}
		for (int k = 0; k < arguments.Length; k++)
		{
			RefKind refKind = ((refKindsOpt.Length > k) ? refKindsOpt[k] : RefKind.None);
			if (refKind != RefKind.None)
			{
				WriteArgument(arguments[k], refKind, method);
			}
		}
	}

	protected virtual void WriteArgument(BoundExpression arg, RefKind refKind, MethodSymbol method)
	{
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		foreach (BoundExpression childBoundNode in node.ChildBoundNodes)
		{
			VisitRvalue(childBoundNode);
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		foreach (BoundNode childBoundNode in node.ChildBoundNodes)
		{
			if (childBoundNode is BoundStatement)
			{
				VisitStatement(childBoundNode as BoundStatement);
			}
			else
			{
				VisitRvalue(childBoundNode as BoundExpression);
			}
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		foreach (BoundExpression initializer in node.Initializers)
		{
			VisitRvalue(initializer);
		}
		return null;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument is BoundMethodGroup boundMethodGroup)
		{
			if ((object)node.MethodOpt != null && !node.MethodOpt.IsStatic)
			{
				if (_trackRegions)
				{
					if (boundMethodGroup == firstInRegion && regionPlace == RegionPlace.Before)
					{
						EnterRegion();
					}
					VisitRvalue(boundMethodGroup.ReceiverOpt);
					if (boundMethodGroup == lastInRegion && IsInside)
					{
						LeaveRegion();
					}
				}
				else
				{
					VisitRvalue(boundMethodGroup.ReceiverOpt);
				}
			}
		}
		else
		{
			VisitRvalue(node.Argument);
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		return null;
	}

	public override BoundNode VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		return Visit(node.Data.ValueExpression);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		return null;
	}

	public override BoundNode VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionId(BoundModuleVersionId node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		return null;
	}

	public override BoundNode VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		return null;
	}

	public override BoundNode VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		return null;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		if (node.ConversionKind == ConversionKind.MethodGroup)
		{
			if (node.IsExtensionMethod || ((object)node.SymbolOpt != null && !node.SymbolOpt.IsStatic))
			{
				BoundExpression receiverOpt = ((BoundMethodGroup)node.Operand).ReceiverOpt;
				if (_trackRegions)
				{
					if (node.Operand == firstInRegion && regionPlace == RegionPlace.Before)
					{
						EnterRegion();
					}
					Visit(receiverOpt);
					if (node.Operand == lastInRegion && IsInside)
					{
						LeaveRegion();
					}
				}
				else
				{
					Visit(receiverOpt);
				}
			}
		}
		else
		{
			Visit(node.Operand);
			if (_trackExceptions && node.HasExpressionSymbols())
			{
				NotePossibleException(node);
			}
		}
		return null;
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		VisitCondition(node.Condition);
		LocalState stateWhenTrue = StateWhenTrue;
		LocalState stateWhenFalse = StateWhenFalse;
		SetState(stateWhenTrue);
		VisitStatement(node.Consequence);
		stateWhenTrue = State;
		SetState(stateWhenFalse);
		if (node.AlternativeOpt != null)
		{
			VisitStatement(node.AlternativeOpt);
		}
		IntersectWith(ref State, ref stateWhenTrue);
		return null;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		SavedPending oldPending = SavePending();
		LocalState val = State.Clone();
		VisitStatement(node.TryBlock);
		LocalState self = State;
		SavedPending oldPending2 = SavePending();
		if (!node.CatchBlocks.IsEmpty)
		{
			LocalState self2 = val.Clone();
			foreach (PendingBranch pendingBranch in oldPending2.PendingBranches)
			{
				IntersectWith(ref self2, ref pendingBranch.State);
			}
			foreach (BoundCatchBlock catchBlock in node.CatchBlocks)
			{
				SetState(self2.Clone());
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
				IntersectWith(ref self, ref State);
			}
		}
		if (node.FinallyBlockOpt != null)
		{
			SetState(self);
			SavedPending oldPending3 = SavePending();
			VisitStatement(node.FinallyBlockOpt);
			self = State;
			foreach (PendingBranch pendingBranch2 in oldPending2.PendingBranches)
			{
				if (pendingBranch2.Branch != null && pendingBranch2.Branch.Kind != BoundKind.YieldReturnStatement)
				{
					SetState(pendingBranch2.State);
					VisitStatement(node.FinallyBlockOpt);
					pendingBranch2.State = State;
				}
			}
			foreach (PendingBranch pendingBranch3 in oldPending3.PendingBranches)
			{
				if (pendingBranch3.Branch != null && pendingBranch3.Branch.Kind != BoundKind.YieldReturnStatement)
				{
					SetState(pendingBranch3.State);
					VisitStatement(node.FinallyBlockOpt);
					pendingBranch3.State = State;
				}
			}
			SetState(self);
			RestorePending(oldPending3);
		}
		else
		{
			SetState(self);
		}
		RestorePending(oldPending2);
		RestorePending(oldPending);
		return null;
	}

	protected virtual LocalState AllBitsSet()
	{
		return default;
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundNode result = VisitRvalue(node.ExpressionOpt);
		if (node.RefKind != RefKind.None)
		{
			WriteArgument(node.ExpressionOpt, node.RefKind, null);
		}
		_pendingBranches.Add(new PendingBranch(node, State));
		SetUnreachable();
		return result;
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		return null;
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		return null;
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		return null;
	}

	protected virtual void VisitLvalueParameter(BoundParameter node)
	{
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		VisitArguments(node.Arguments, node.ArgumentRefKindsOpt, node.Constructor);
		VisitRvalue(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		VisitRvalue(node.InitializerExpressionOpt);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		VisitRvalue(node.InitializerExpressionOpt);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	protected virtual void PropertySetter(BoundExpression node, BoundExpression receiver, MethodSymbol setter, BoundExpression value = null)
	{
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		VisitReceiverAfterCall(receiver, setter);
	}

	private bool RegularPropertyAccess(BoundExpression expr)
	{
		if (expr.Kind != BoundKind.PropertyAccess)
		{
			return false;
		}
		return !Binder.AccessingAutoPropertyFromConstructor((BoundPropertyAccess)expr, _member);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (RegularPropertyAccess(node.Left))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Left;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol methodSymbol = propertySymbol.GetOwnOrInheritedSetMethod() ?? propertySymbol.GetMethod;
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, methodSymbol);
				VisitRvalue(node.Right);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, methodSymbol, node.Right);
				return null;
			}
		}
		VisitLvalue(node.Left);
		VisitRvalue(node.Right);
		if (node.IsRef)
		{
			RefKind refKind = ((node.Left.Kind == BoundKind.BadExpression) ? RefKind.Ref : node.Left.GetRefKind());
			WriteArgument(node.Right, refKind, null);
		}
		return null;
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		VisitLvalue(node.Left);
		VisitRvalue(node.Right);
		return null;
	}

	public sealed override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		if (RegularPropertyAccess(node.Left))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Left;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol method = propertySymbol.GetOwnOrInheritedGetMethod() ?? propertySymbol.SetMethod;
				MethodSymbol methodSymbol = propertySymbol.GetOwnOrInheritedSetMethod() ?? propertySymbol.GetMethod;
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, method);
				if (_trackExceptions)
				{
					NotePossibleException(node);
				}
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, method);
				VisitRvalue(node.Right);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, methodSymbol);
				if (_trackExceptions)
				{
					NotePossibleException(node);
				}
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, methodSymbol);
				return null;
			}
		}
		VisitRvalue(node.Left);
		VisitRvalue(node.Right);
		if (_trackExceptions && node.HasExpressionSymbols())
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		VisitFieldAccessInternal(node.ReceiverOpt, node.FieldSymbol);
		return null;
	}

	private void VisitFieldAccessInternal(BoundExpression receiverOpt, FieldSymbol fieldSymbol)
	{
		if (MayRequireTracking(receiverOpt, fieldSymbol) || ((object)fieldSymbol != null && fieldSymbol.IsFixed))
		{
			VisitLvalue(receiverOpt);
		}
		else
		{
			VisitRvalue(receiverOpt);
		}
	}

	public override BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		return null;
	}

	public override BoundNode VisitMethodInfo(BoundMethodInfo node)
	{
		return null;
	}

	protected static bool MayRequireTracking(BoundExpression receiverOpt, FieldSymbol fieldSymbol)
	{
		if ((object)fieldSymbol != null && receiverOpt != null && !fieldSymbol.IsStatic && !fieldSymbol.IsFixed && receiverOpt.Type.IsStructType())
		{
			return !receiverOpt.Type.IsPrimitiveRecursiveStruct();
		}
		return false;
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		PropertySymbol propertySymbol = node.PropertySymbol;
		if (Binder.AccessingAutoPropertyFromConstructor(node, _member))
		{
			SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (propertySymbol as SourcePropertySymbol)?.BackingField;
			if (synthesizedBackingFieldSymbol != null)
			{
				VisitFieldAccessInternal(node.ReceiverOpt, synthesizedBackingFieldSymbol);
				return null;
			}
		}
		MethodSymbol method = propertySymbol.GetOwnOrInheritedGetMethod() ?? propertySymbol.SetMethod;
		VisitReceiverBeforeCall(node.ReceiverOpt, method);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		VisitReceiverAfterCall(node.ReceiverOpt, method);
		return null;
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		VisitFieldAccessInternal(node.ReceiverOpt, node.EventSymbol.AssociatedField);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		return null;
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		VisitRvalue(node.UnoptimizedForm ?? node.Value);
		return null;
	}

	public override BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		foreach (BoundLocalDeclaration localDeclaration in node.LocalDeclarations)
		{
			Visit(localDeclaration);
		}
		return null;
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		LoopHead(node);
		VisitCondition(node.Condition);
		LocalState stateWhenTrue = StateWhenTrue;
		LocalState stateWhenFalse = StateWhenFalse;
		SetState(stateWhenTrue);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		LoopTail(node);
		ResolveBreaks(stateWhenFalse, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		VisitRvalue(node.Expression);
		foreach (BoundExpression index in node.Indices)
		{
			VisitRvalue(index);
		}
		if (_trackExceptions && node.HasExpressionSymbols())
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		if (node.OperatorKind.IsLogical())
		{
			VisitBinaryLogicalOperatorChildren(node);
		}
		else
		{
			VisitBinaryOperatorChildren(node);
		}
		return null;
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		VisitBinaryLogicalOperatorChildren(node);
		return null;
	}

	private void VisitBinaryLogicalOperatorChildren(BoundExpression node)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		BoundExpression boundExpression = node;
		while (true)
		{
			BoundExpression boundExpression2;
			switch (boundExpression.Kind)
			{
			case BoundKind.BinaryOperator:
			{
				BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)boundExpression;
				if (boundBinaryOperator.OperatorKind.IsLogical())
				{
					boundExpression2 = boundExpression;
					boundExpression = boundBinaryOperator.Left;
					goto IL_0049;
				}
				break;
			}
			case BoundKind.UserDefinedConditionalLogicalOperator:
				boundExpression2 = boundExpression;
				boundExpression = ((BoundUserDefinedConditionalLogicalOperator)boundExpression2).Left;
				goto IL_0049;
			}
			break;
			IL_0049:
			instance.Push(boundExpression2);
		}
		VisitCondition(boundExpression);
		while (true)
		{
			BoundExpression boundExpression2 = instance.Pop();
			BinaryOperatorKind operatorKind;
			BoundExpression right;
			switch (boundExpression2.Kind)
			{
			case BoundKind.BinaryOperator:
			{
				BoundBinaryOperator boundBinaryOperator2 = (BoundBinaryOperator)boundExpression2;
				operatorKind = boundBinaryOperator2.OperatorKind;
				right = boundBinaryOperator2.Right;
				break;
			}
			case BoundKind.UserDefinedConditionalLogicalOperator:
			{
				BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)boundExpression2;
				operatorKind = boundUserDefinedConditionalLogicalOperator.OperatorKind;
				right = boundUserDefinedConditionalLogicalOperator.Right;
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(boundExpression2.Kind);
			}
			bool flag = operatorKind.Operator() == BinaryOperatorKind.And;
			bool flag2 = operatorKind.OperandTypes() == BinaryOperatorKind.Bool;
			LocalState other = StateWhenTrue;
			LocalState other2 = StateWhenFalse;
			SetState(flag ? other : other2);
			VisitCondition(right);
			if (!flag2)
			{
				Unsplit();
				Split();
			}
			LocalState self = StateWhenTrue;
			LocalState self2 = StateWhenFalse;
			if (flag)
			{
				IntersectWith(ref self2, ref other2);
			}
			else
			{
				IntersectWith(ref self, ref other);
			}
			SetConditionalState(self, self2);
			if (!flag2)
			{
				Unsplit();
			}
			if (_trackExceptions && boundExpression2.HasExpressionSymbols())
			{
				NotePossibleException(boundExpression2);
			}
			if (instance.Count == 0)
			{
				break;
			}
			AdjustConditionalState(boundExpression2);
		}
		instance.Free();
	}

	private void VisitBinaryOperatorChildren(BoundBinaryOperator node)
	{
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		instance.Push(node);
		BoundExpression left = node.Left;
		while (left is BoundBinaryOperator boundBinaryOperator && !boundBinaryOperator.OperatorKind.IsLogical())
		{
			instance.Push(boundBinaryOperator);
			left = boundBinaryOperator.Left;
		}
		VisitRvalue(left);
		while (true)
		{
			BoundBinaryOperator boundBinaryOperator2 = instance.Pop();
			VisitRvalue(boundBinaryOperator2.Right);
			if (_trackExceptions && boundBinaryOperator2.HasExpressionSymbols())
			{
				NotePossibleException(boundBinaryOperator2);
			}
			if (instance.Count == 0)
			{
				break;
			}
			Unsplit();
		}
		instance.Free();
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		if (node.OperatorKind == UnaryOperatorKind.BoolLogicalNegation)
		{
			VisitCondition(node.Operand);
			SetConditionalState(StateWhenFalse, StateWhenTrue);
		}
		else
		{
			VisitRvalue(node.Operand);
			if (_trackExceptions && node.HasExpressionSymbols())
			{
				NotePossibleException(node);
			}
		}
		return null;
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		VisitRvalue(node.Expression);
		_pendingBranches.Add(new PendingBranch(node, State));
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		if (RegularPropertyAccess(node.Operand))
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node.Operand;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind == RefKind.None)
			{
				MethodSymbol method = propertySymbol.GetOwnOrInheritedGetMethod() ?? propertySymbol.SetMethod;
				MethodSymbol setter = propertySymbol.GetOwnOrInheritedSetMethod() ?? propertySymbol.GetMethod;
				VisitReceiverBeforeCall(boundPropertyAccess.ReceiverOpt, method);
				if (_trackExceptions)
				{
					NotePossibleException(node);
				}
				VisitReceiverAfterCall(boundPropertyAccess.ReceiverOpt, method);
				PropertySetter(node, boundPropertyAccess.ReceiverOpt, setter);
				return null;
			}
		}
		VisitRvalue(node.Operand);
		if (_trackExceptions && node.HasExpressionSymbols())
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		foreach (BoundExpression bound in node.Bounds)
		{
			VisitRvalue(bound);
		}
		if (node.InitializerOpt != null && !node.InitializerOpt.Initializers.IsDefault)
		{
			foreach (BoundExpression initializer in node.InitializerOpt.Initializers)
			{
				VisitRvalue(initializer);
			}
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		if (node.Initializer != null)
		{
			VisitStatement(node.Initializer);
		}
		LoopHead(node);
		LocalState state;
		LocalState breakState;
		if (node.Condition != null)
		{
			VisitCondition(node.Condition);
			state = StateWhenTrue;
			breakState = StateWhenFalse;
		}
		else
		{
			state = State;
			breakState = UnreachableState();
		}
		SetState(state);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		if (node.Increment != null)
		{
			VisitStatement(node.Increment);
		}
		LoopTail(node);
		ResolveBreaks(breakState, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		VisitRvalue(node.Expression);
		LocalState breakState = State.Clone();
		LoopHead(node);
		VisitForEachIterationVariables(node);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		LoopTail(node);
		ResolveBreaks(breakState, node.BreakLabel);
		return null;
	}

	public virtual void VisitForEachIterationVariables(BoundForEachStatement node)
	{
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		if (node.ReceiverOpt != null)
		{
			VisitRvalue(node.ReceiverOpt);
		}
		return null;
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		VisitRvalue(node.LeftOperand);
		if (IsConstantNull(node.LeftOperand))
		{
			VisitRvalue(node.RightOperand);
		}
		else
		{
			LocalState other = State.Clone();
			if (node.LeftOperand.ConstantValue != null)
			{
				SetUnreachable();
			}
			VisitRvalue(node.RightOperand);
			IntersectWith(ref State, ref other);
		}
		return null;
	}

	public override BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		VisitRvalue(node.Receiver);
		if (node.Receiver.ConstantValue != null && !IsConstantNull(node.Receiver))
		{
			VisitRvalue(node.AccessExpression);
		}
		else
		{
			LocalState other = State.Clone();
			if (IsConstantNull(node.Receiver))
			{
				SetUnreachable();
			}
			VisitRvalue(node.AccessExpression);
			IntersectWith(ref State, ref other);
		}
		return null;
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		VisitRvalue(node.Receiver);
		LocalState other = State.Clone();
		VisitRvalue(node.WhenNotNull);
		IntersectWith(ref State, ref other);
		if (node.WhenNullOpt != null)
		{
			other = State.Clone();
			VisitRvalue(node.WhenNullOpt);
			IntersectWith(ref State, ref other);
		}
		return null;
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		return null;
	}

	public override BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		LocalState other = State.Clone();
		VisitRvalue(node.ValueTypeReceiver);
		IntersectWith(ref State, ref other);
		other = State.Clone();
		VisitRvalue(node.ReferenceTypeReceiver);
		IntersectWith(ref State, ref other);
		return null;
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		ImmutableArray<BoundExpression> sideEffects = node.SideEffects;
		if (!sideEffects.IsEmpty)
		{
			foreach (BoundExpression item in sideEffects)
			{
				VisitRvalue(item);
			}
		}
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitSequencePoint(BoundSequencePoint node)
	{
		if (node.StatementOpt != null)
		{
			VisitStatement(node.StatementOpt);
		}
		return null;
	}

	public override BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		if (node.StatementOpt != null)
		{
			VisitStatement(node.StatementOpt);
		}
		return null;
	}

	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		return VisitStatementListWorker(node);
	}

	private BoundNode VisitStatementListWorker(BoundStatementList node)
	{
		foreach (BoundStatement statement in node.Statements)
		{
			VisitStatement(statement);
		}
		return null;
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		return VisitStatementListWorker(node);
	}

	public override BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		return VisitLambda(node.BindForErrorRecovery());
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		_pendingBranches.Add(new PendingBranch(node, State));
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		_pendingBranches.Add(new PendingBranch(node, State));
		SetUnreachable();
		return null;
	}

	public sealed override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		bool isRef = node.IsRef;
		VisitCondition(node.Condition);
		LocalState stateWhenTrue = StateWhenTrue;
		LocalState stateWhenFalse = StateWhenFalse;
		if (IsConstantTrue(node.Condition))
		{
			VisitConditionalOperand(stateWhenFalse, node.Alternative, isRef);
			VisitConditionalOperand(stateWhenTrue, node.Consequence, isRef);
		}
		else if (IsConstantFalse(node.Condition))
		{
			VisitConditionalOperand(stateWhenTrue, node.Consequence, isRef);
			VisitConditionalOperand(stateWhenFalse, node.Alternative, isRef);
		}
		else
		{
			VisitConditionalOperand(stateWhenTrue, node.Consequence, isRef);
			Unsplit();
			stateWhenTrue = State;
			VisitConditionalOperand(stateWhenFalse, node.Alternative, isRef);
			Unsplit();
			IntersectWith(ref State, ref stateWhenTrue);
		}
		return null;
	}

	private void VisitConditionalOperand(LocalState state, BoundExpression operand, bool isByRef)
	{
		SetState(state);
		if (isByRef)
		{
			VisitLvalue(operand);
			WriteArgument(operand, RefKind.Ref, null);
		}
		else
		{
			Visit(operand);
		}
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		return null;
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		LoopHead(node);
		VisitStatement(node.Body);
		ResolveContinues(node.ContinueLabel);
		VisitCondition(node.Condition);
		LocalState stateWhenFalse = StateWhenFalse;
		SetState(StateWhenTrue);
		LoopTail(node);
		ResolveBreaks(stateWhenFalse, node.BreakLabel);
		return null;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		_pendingBranches.Add(new PendingBranch(node, State));
		SetUnreachable();
		return null;
	}

	private void VisitLabel(LabelSymbol label, BoundStatement node)
	{
		ResolveBranches(label, node);
		LocalState other = LabelState(label);
		IntersectWith(ref State, ref other);
		_labels[label] = State.Clone();
		_labelsSeen.Add(node);
	}

	protected virtual void VisitLabel(BoundLabeledStatement node)
	{
		VisitLabel(node.Label, node);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		VisitLabel(node.Label, node);
		return null;
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		VisitLabel(node);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		VisitRvalue(node.Argument);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return null;
	}

	public override BoundNode VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		return null;
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		if (node.ExpressionOpt != null)
		{
			VisitRvalue(node.ExpressionOpt);
		}
		if (node.DeclarationsOpt != null)
		{
			VisitStatement(node.DeclarationsOpt);
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		VisitStatement(node.Declarations);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundExpression expressionOpt = node.ExpressionOpt;
		VisitRvalue(expressionOpt);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		_pendingBranches.Add(new PendingBranch(node, State));
		SetUnreachable();
		return null;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		VisitRvalue(node.Expression);
		_pendingBranches.Add(new PendingBranch(node, State));
		return null;
	}

	public override BoundNode VisitDefaultExpression(BoundDefaultExpression node)
	{
		return null;
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		VisitTypeExpression(node.SourceType);
		return null;
	}

	public override BoundNode VisitNameOfOperator(BoundNameOfOperator node)
	{
		LocalState state = State;
		SetState(UnreachableState());
		Visit(node.Argument);
		SetState(state);
		return null;
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		VisitAddressOfOperand(node.Operand, shouldReadOperand: false);
		return null;
	}

	protected void VisitAddressOfOperand(BoundExpression operand, bool shouldReadOperand)
	{
		if (shouldReadOperand)
		{
			VisitRvalue(operand);
		}
		else
		{
			VisitLvalue(operand);
		}
		WriteArgument(operand, RefKind.Out, null);
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		VisitRvalue(node.Expression);
		VisitRvalue(node.Index);
		return null;
	}

	public override BoundNode VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		return null;
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		VisitRvalue(node.Count);
		if (node.InitializerOpt != null && !node.InitializerOpt.Initializers.IsDefault)
		{
			foreach (BoundExpression initializer in node.InitializerOpt.Initializers)
			{
				VisitRvalue(initializer);
			}
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		VisitStackAllocArrayCreation(node);
		return null;
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		VisitArguments(node.Arguments, default, node.Constructor);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		VisitRvalue(node.Expression);
		return null;
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		VisitCondition(node.Condition);
		if (node.JumpIfTrue)
		{
			_pendingBranches.Add(new PendingBranch(node, StateWhenTrue));
			SetState(StateWhenFalse);
		}
		else
		{
			_pendingBranches.Add(new PendingBranch(node, StateWhenFalse));
			SetState(StateWhenTrue);
		}
		return null;
	}

	public override BoundNode VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		return VisitObjectOrCollectionInitializerExpression(node.Initializers);
	}

	public override BoundNode VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		return VisitObjectOrCollectionInitializerExpression(node.Initializers);
	}

	private BoundNode VisitObjectOrCollectionInitializerExpression(ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			VisitRvalue(item);
			if (_trackExceptions)
			{
				NotePossibleException(item);
			}
		}
		return null;
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		if (!arguments.IsDefaultOrEmpty)
		{
			foreach (BoundExpression item in arguments)
			{
				VisitRvalue(item);
			}
		}
		return null;
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return null;
	}

	public override BoundNode VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		if (node.AddMethod.CallsAreOmitted(node.SyntaxTree))
		{
			LocalState state = State.Clone();
			SetUnreachable();
			VisitArguments(node.Arguments, default, node.AddMethod);
			State = state;
		}
		else
		{
			VisitArguments(node.Arguments, default, node.AddMethod);
		}
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		VisitArguments(node.Arguments, default, null);
		if (_trackExceptions)
		{
			NotePossibleException(node);
		}
		return null;
	}

	public override BoundNode VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		return null;
	}

	public override BoundNode VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return null;
	}

	public sealed override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public sealed override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitDiscardExpression(BoundDiscardExpression node)
	{
		return null;
	}

	public override BoundNode VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		Visit(node.Initializer);
		VisitMethodBodies(node.BlockBody, node.ExpressionBody);
		return null;
	}

	public override BoundNode VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		VisitMethodBodies(node.BlockBody, node.ExpressionBody);
		return null;
	}

	private void VisitMethodBodies(BoundBlock blockBody, BoundBlock expressionBody)
	{
		if (blockBody == null)
		{
			Visit(expressionBody);
			return;
		}
		if (expressionBody == null)
		{
			Visit(blockBody);
			return;
		}
		LocalState state = State.Clone();
		Visit(blockBody);
		LocalState other = State;
		SetState(state);
		Visit(expressionBody);
		IntersectWith(ref State, ref other);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		LocalState self = VisitSwitchHeader(node);
		SetUnreachable();
		VisitSwitchBlock(node);
		IntersectWith(ref self, ref State);
		ResolveBreaks(self, node.BreakLabel);
		return null;
	}

	private LocalState VisitSwitchHeader(BoundSwitchStatement node)
	{
		VisitRvalue(node.Expression);
		LocalState result = State;
		LabelSymbol constantTargetOpt = node.ConstantTargetOpt;
		if ((object)constantTargetOpt == null)
		{
			bool flag = false;
			foreach (BoundSwitchSection switchSection in node.SwitchSections)
			{
				foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
				{
					LabelSymbol label = switchLabel.Label;
					flag = flag || switchLabel.ConstantValueOpt == null;
					SetState(result.Clone());
					BoundGotoStatement node2 = new BoundGotoStatement(node.Syntax, label);
					VisitGotoStatement(node2);
				}
			}
			if (flag)
			{
				result = UnreachableState();
			}
		}
		else if (!node.BreakLabel.Equals(constantTargetOpt))
		{
			SetState(result.Clone());
			BoundGotoStatement node3 = new BoundGotoStatement(node.Syntax, constantTargetOpt);
			VisitGotoStatement(node3);
			result = UnreachableState();
		}
		return result;
	}

	private void VisitSwitchBlock(BoundSwitchStatement node)
	{
		LocalState self = UnreachableState();
		ImmutableArray<BoundSwitchSection> switchSections = node.SwitchSections;
		int num = switchSections.Length - 1;
		for (int i = 0; i <= num; i++)
		{
			VisitSwitchSection(switchSections[i], i == num);
			IntersectWith(ref self, ref State);
		}
		SetState(self);
	}

	public virtual BoundNode VisitSwitchSection(BoundSwitchSection node, bool lastSection)
	{
		return VisitSwitchSection(node);
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		foreach (BoundSwitchLabel switchLabel in node.SwitchLabels)
		{
			VisitRvalue(switchLabel.ExpressionOpt);
			VisitSwitchSectionLabel(switchLabel.Label, node);
		}
		VisitStatementList(node);
		return null;
	}

	private void VisitSwitchSectionLabel(LabelSymbol label, BoundSwitchSection node)
	{
		VisitLabel(label, node);
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		LocalState self = VisitPatternSwitchHeader(node);
		VisitPatternSwitchBlock(node);
		IntersectWith(ref self, ref State);
		ResolveBreaks(self, node.BreakLabel);
		return null;
	}

	private void VisitPatternSwitchBlock(BoundPatternSwitchStatement node)
	{
		LocalState state = State;
		LocalState self = UnreachableState();
		ImmutableArray<BoundPatternSwitchSection> switchSections = node.SwitchSections;
		int num = switchSections.Length - 1;
		foreach (BoundPatternSwitchSection item in switchSections)
		{
			foreach (BoundPatternSwitchLabel switchLabel in item.SwitchLabels)
			{
				if (switchLabel.IsReachable && switchLabel != node.DefaultLabel)
				{
					SetState(state.Clone());
					VisitPattern(null, switchLabel.Pattern);
					SetState(StateWhenTrue);
					if (switchLabel.Guard != null)
					{
						VisitCondition(switchLabel.Guard);
						SetState(StateWhenTrue);
					}
					_pendingBranches.Add(new PendingBranch(switchLabel, State));
				}
			}
		}
		if (node.DefaultLabel != null)
		{
			if (node.SomeLabelAlwaysMatches)
			{
				SetUnreachable();
			}
			else
			{
				SetState(state.Clone());
			}
			_pendingBranches.Add(new PendingBranch(node.DefaultLabel, State));
		}
		for (int i = 0; i <= num; i++)
		{
			VisitPatternSwitchSection(switchSections[i], node.Expression, i == num);
			IntersectWith(ref self, ref State);
		}
		SetState(self);
	}

	private LocalState VisitPatternSwitchHeader(BoundPatternSwitchStatement node)
	{
		VisitRvalue(node.Expression);
		if (FullyHandlesItsInput(node))
		{
			return UnreachableState();
		}
		return State;
	}

	private bool FullyHandlesItsInput(BoundPatternSwitchStatement node)
	{
		if (node.IsComplete)
		{
			return true;
		}
		if (node.Expression.ConstantValue == null)
		{
			return false;
		}
		foreach (BoundPatternSwitchSection switchSection in node.SwitchSections)
		{
			foreach (BoundPatternSwitchLabel switchLabel in switchSection.SwitchLabels)
			{
				if ((switchLabel.Guard == null || !(switchLabel.Guard.ConstantValue != ConstantValue.True)) && switchLabel.Pattern.Kind == BoundKind.ConstantPattern && ((BoundConstantPattern)switchLabel.Pattern).ConstantValue == node.Expression.ConstantValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void VisitPatternSwitchSection(BoundPatternSwitchSection node, BoundExpression switchExpression, bool isLastSection)
	{
		SetState(UnreachableState());
		foreach (BoundPatternSwitchLabel switchLabel in node.SwitchLabels)
		{
			VisitLabel(switchLabel.Label, node);
		}
		VisitStatementList(node);
	}
}
