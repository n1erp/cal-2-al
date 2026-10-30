using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class IteratorAndAsyncCaptureWalker : DataFlowPass
{
	private sealed class OutsideVariablesUsedInside : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly HashSet<Symbol> _localsInScope;

		private readonly IteratorAndAsyncCaptureWalker _analyzer;

		private readonly MethodSymbol _topLevelMethod;

		private readonly IteratorAndAsyncCaptureWalker _parent;

		public OutsideVariablesUsedInside(IteratorAndAsyncCaptureWalker analyzer, MethodSymbol topLevelMethod, IteratorAndAsyncCaptureWalker parent)
			: base(parent._recursionDepth)
		{
			_analyzer = analyzer;
			_topLevelMethod = topLevelMethod;
			_localsInScope = new HashSet<Symbol>();
			_parent = parent;
		}

		protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
		{
			return _parent.ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException();
		}

		public override BoundNode VisitBlock(BoundBlock node)
		{
			AddVariables(node.Locals);
			return base.VisitBlock(node);
		}

		private void AddVariables(ImmutableArray<LocalSymbol> locals)
		{
			foreach (LocalSymbol item in locals)
			{
				AddVariable(item);
			}
		}

		public override BoundNode VisitCatchBlock(BoundCatchBlock node)
		{
			AddVariables(node.Locals);
			return base.VisitCatchBlock(node);
		}

		private void AddVariable(Symbol local)
		{
			if ((object)local != null)
			{
				_localsInScope.Add(local);
			}
		}

		public override BoundNode VisitSequence(BoundSequence node)
		{
			AddVariables(node.Locals);
			return base.VisitSequence(node);
		}

		public override BoundNode VisitThisReference(BoundThisReference node)
		{
			Capture(_topLevelMethod.ThisParameter, node.Syntax);
			return base.VisitThisReference(node);
		}

		public override BoundNode VisitBaseReference(BoundBaseReference node)
		{
			Capture(_topLevelMethod.ThisParameter, node.Syntax);
			return base.VisitBaseReference(node);
		}

		public override BoundNode VisitLocal(BoundLocal node)
		{
			Capture(node.LocalSymbol, node.Syntax);
			return base.VisitLocal(node);
		}

		public override BoundNode VisitParameter(BoundParameter node)
		{
			Capture(node.ParameterSymbol, node.Syntax);
			return base.VisitParameter(node);
		}

		private void Capture(Symbol s, SyntaxNode syntax)
		{
			if ((object)s != null && !_localsInScope.Contains(s))
			{
				_analyzer.CaptureVariable(s, syntax);
			}
		}
	}

	private readonly OrderedSet<Symbol> _variablesToHoist;

	private MultiDictionary<Symbol, SyntaxNode> _lazyDisallowedCaptures;

	private bool _seenYieldInCurrentTry;

	private IteratorAndAsyncCaptureWalker(CSharpCompilation compilation, MethodSymbol method, BoundNode node, NeverEmptyStructTypeCache emptyStructCache, HashSet<Symbol> initiallyAssignedVariables)
		: base(compilation, method, node, emptyStructCache, trackUnassignments: true, initiallyAssignedVariables)
	{
		_variablesToHoist = new OrderedSet<Symbol>();
	}

	public static OrderedSet<Symbol> Analyze(CSharpCompilation compilation, MethodSymbol method, BoundNode node, DiagnosticBag diagnostics)
	{
		HashSet<Symbol> hashSet = UnassignedVariablesWalker.Analyze(compilation, method, node, convertInsufficientExecutionStackExceptionToCancelledByStackGuardException: true);
		IteratorAndAsyncCaptureWalker iteratorAndAsyncCaptureWalker = new IteratorAndAsyncCaptureWalker(compilation, method, node, new NeverEmptyStructTypeCache(), hashSet);
		iteratorAndAsyncCaptureWalker._convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = true;
		bool badRegion = false;
		iteratorAndAsyncCaptureWalker.Analyze(ref badRegion);
		if (!method.IsStatic && method.ContainingType.TypeKind == TypeKind.Struct)
		{
			iteratorAndAsyncCaptureWalker.CaptureVariable(method.ThisParameter, node.Syntax);
		}
		OrderedSet<Symbol> variablesToHoist = iteratorAndAsyncCaptureWalker._variablesToHoist;
		MultiDictionary<Symbol, SyntaxNode> lazyDisallowedCaptures = iteratorAndAsyncCaptureWalker._lazyDisallowedCaptures;
		VariableIdentifier[] array = iteratorAndAsyncCaptureWalker.variableBySlot;
		iteratorAndAsyncCaptureWalker.Free();
		if (lazyDisallowedCaptures != null)
		{
			foreach (KeyValuePair<Symbol, MultiDictionary<Symbol, SyntaxNode>.ValueSet> item in lazyDisallowedCaptures)
			{
				Symbol key = item.Key;
				TypeSymbol typeSymbol = ((key.Kind == SymbolKind.Local) ? ((LocalSymbol)key).Type : ((ParameterSymbol)key).Type);
				foreach (CSharpSyntaxNode item2 in item.Value)
				{
					diagnostics.Add(ErrorCode.ERR_SpecialByRefInLambda, item2.Location, typeSymbol);
				}
			}
		}
		if (compilation.Options.OptimizationLevel != OptimizationLevel.Release)
		{
			VariableIdentifier[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Symbol symbol = array2[i].Symbol;
				if ((object)symbol != null && HoistInDebugBuild(symbol))
				{
					variablesToHoist.Add(symbol);
				}
			}
		}
		return variablesToHoist;
	}

	private static bool HoistInDebugBuild(Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Parameter)
		{
			return !((ParameterSymbol)symbol).Type.IsRestrictedType();
		}
		if (symbol.Kind == SymbolKind.Local)
		{
			LocalSymbol localSymbol = (LocalSymbol)symbol;
			if (localSymbol.IsConst || localSymbol.IsPinned)
			{
				return false;
			}
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.UserDefined)
			{
				return !localSymbol.Type.IsRestrictedType();
			}
			return localSymbol.SynthesizedKind.MustSurviveStateMachineSuspension();
		}
		return false;
	}

	private void MarkLocalsUnassigned()
	{
		for (int i = 0; i < nextVariableSlot; i++)
		{
			Symbol symbol = variableBySlot[i].Symbol;
			if ((object)symbol == null)
			{
				continue;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.Local:
				if (!((LocalSymbol)symbol).IsConst)
				{
					SetSlotState(i, assigned: false);
				}
				break;
			case SymbolKind.Parameter:
				SetSlotState(i, assigned: false);
				break;
			case SymbolKind.Field:
				if (!((FieldSymbol)symbol).IsConst)
				{
					SetSlotState(i, assigned: false);
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		base.VisitAwaitExpression(node);
		MarkLocalsUnassigned();
		return null;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		base.VisitYieldReturnStatement(node);
		MarkLocalsUnassigned();
		_seenYieldInCurrentTry = true;
		return null;
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		_variablesToHoist.Clear();
		_lazyDisallowedCaptures?.Clear();
		return base.Scan(ref badRegion);
	}

	private void CaptureVariable(Symbol variable, SyntaxNode syntax)
	{
		if (((variable.Kind == SymbolKind.Local) ? ((LocalSymbol)variable).Type : ((ParameterSymbol)variable).Type).IsRestrictedType())
		{
			if (!(variable is SynthesizedLocal))
			{
				if (_lazyDisallowedCaptures == null)
				{
					_lazyDisallowedCaptures = new MultiDictionary<Symbol, SyntaxNode>();
				}
				_lazyDisallowedCaptures.Add(variable, syntax);
			}
		}
		else if (compilation.Options.OptimizationLevel == OptimizationLevel.Release)
		{
			_variablesToHoist.Add(variable);
		}
	}

	protected override void EnterParameter(ParameterSymbol parameter)
	{
		GetOrCreateSlot(parameter);
	}

	protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		switch (symbol.Kind)
		{
		default:
			return;
		case SymbolKind.Field:
			symbol = GetNonFieldSymbol(slot);
			break;
		case SymbolKind.Local:
		case SymbolKind.Parameter:
			break;
		}
		CaptureVariable(symbol, node);
	}

	protected override void VisitLvalueParameter(BoundParameter node)
	{
		TryHoistTopLevelParameter(node);
		base.VisitLvalueParameter(node);
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		TryHoistTopLevelParameter(node);
		return base.VisitParameter(node);
	}

	private void TryHoistTopLevelParameter(BoundParameter node)
	{
		if (node.ParameterSymbol.ContainingSymbol == topLevelMethod)
		{
			CaptureVariable(node.ParameterSymbol, node.Syntax);
		}
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		if (node.ReceiverOpt != null && node.ReceiverOpt.Kind == BoundKind.ThisReference)
		{
			ParameterSymbol thisParameter = topLevelMethod.ThisParameter;
			CaptureVariable(thisParameter, node.Syntax);
		}
		return base.VisitFieldAccess(node);
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		CaptureVariable(topLevelMethod.ThisParameter, node.Syntax);
		return base.VisitThisReference(node);
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		CaptureVariable(topLevelMethod.ThisParameter, node.Syntax);
		return base.VisitBaseReference(node);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		bool seenYieldInCurrentTry = _seenYieldInCurrentTry;
		_seenYieldInCurrentTry = false;
		base.VisitTryStatement(node);
		_seenYieldInCurrentTry |= seenYieldInCurrentTry;
		return null;
	}

	protected override void VisitFinallyBlock(BoundStatement finallyBlock, ref LocalState unsetInFinally)
	{
		if (_seenYieldInCurrentTry)
		{
			new OutsideVariablesUsedInside(this, topLevelMethod, this).Visit(finallyBlock);
		}
		base.VisitFinallyBlock(finallyBlock, ref unsetInFinally);
	}
}
