using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AsyncExceptionHandlerRewriter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private sealed class AwaitInFinallyAnalysis : LabelCollector
	{
		private Dictionary<BoundTryStatement, HashSet<LabelSymbol>> _labelsInInterestingTry;

		private HashSet<BoundCatchBlock> _awaitContainingCatches;

		private bool _seenAwait;

		public AwaitInFinallyAnalysis(BoundStatement body)
		{
			_seenAwait = false;
			Visit(body);
		}

		public bool FinallyContainsAwaits(BoundTryStatement statement)
		{
			if (_labelsInInterestingTry != null)
			{
				return _labelsInInterestingTry.ContainsKey(statement);
			}
			return false;
		}

		internal bool CatchContainsAwait(BoundCatchBlock node)
		{
			if (_awaitContainingCatches != null)
			{
				return _awaitContainingCatches.Contains(node);
			}
			return false;
		}

		public bool ContainsAwaitInHandlers()
		{
			if (_labelsInInterestingTry == null)
			{
				return _awaitContainingCatches != null;
			}
			return true;
		}

		internal HashSet<LabelSymbol> Labels(BoundTryStatement statement)
		{
			return _labelsInInterestingTry[statement];
		}

		public override BoundNode VisitTryStatement(BoundTryStatement node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			currentLabels = null;
			Visit(node.TryBlock);
			VisitList(node.CatchBlocks);
			bool seenAwait = _seenAwait;
			_seenAwait = false;
			Visit(node.FinallyBlockOpt);
			if (_seenAwait)
			{
				Dictionary<BoundTryStatement, HashSet<LabelSymbol>> dictionary = _labelsInInterestingTry;
				if (dictionary == null)
				{
					dictionary = (_labelsInInterestingTry = new Dictionary<BoundTryStatement, HashSet<LabelSymbol>>());
				}
				dictionary.Add(node, currentLabels);
				currentLabels = hashSet;
			}
			else if (currentLabels == null)
			{
				currentLabels = hashSet;
			}
			else if (hashSet != null)
			{
				currentLabels.UnionWith(hashSet);
			}
			_seenAwait |= seenAwait;
			return null;
		}

		public override BoundNode VisitCatchBlock(BoundCatchBlock node)
		{
			bool seenAwait = _seenAwait;
			_seenAwait = false;
			BoundNode result = base.VisitCatchBlock(node);
			if (_seenAwait)
			{
				HashSet<BoundCatchBlock> awaitContainingCatches = _awaitContainingCatches;
				if (awaitContainingCatches == null)
				{
					_awaitContainingCatches = new HashSet<BoundCatchBlock>();
				}
				_awaitContainingCatches.Add(node);
			}
			_seenAwait |= seenAwait;
			return result;
		}

		public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
		{
			_seenAwait = true;
			return base.VisitAwaitExpression(node);
		}

		public override BoundNode VisitLambda(BoundLambda node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			bool seenAwait = _seenAwait;
			currentLabels = null;
			_seenAwait = false;
			base.VisitLambda(node);
			currentLabels = hashSet;
			_seenAwait = seenAwait;
			return null;
		}

		public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			HashSet<LabelSymbol> hashSet = currentLabels;
			bool seenAwait = _seenAwait;
			currentLabels = null;
			_seenAwait = false;
			base.VisitLocalFunctionStatement(node);
			currentLabels = hashSet;
			_seenAwait = seenAwait;
			return null;
		}
	}

	private sealed class AwaitFinallyFrame
	{
		public readonly AwaitFinallyFrame ParentOpt;

		public readonly HashSet<LabelSymbol> LabelsOpt;

		private readonly TryStatementSyntax _tryStatementSyntaxOpt;

		public Dictionary<LabelSymbol, LabelSymbol> proxyLabels;

		public List<LabelSymbol> proxiedLabels;

		public GeneratedLabelSymbol returnProxyLabel;

		public SynthesizedLocal returnValue;

		public AwaitFinallyFrame()
		{
		}

		public AwaitFinallyFrame(AwaitFinallyFrame parent, HashSet<LabelSymbol> labelsOpt, TryStatementSyntax tryStatementSyntax)
		{
			ParentOpt = parent;
			LabelsOpt = labelsOpt;
			_tryStatementSyntaxOpt = tryStatementSyntax;
		}

		public bool IsRoot()
		{
			return ParentOpt == null;
		}

		public LabelSymbol ProxyLabelIfNeeded(LabelSymbol label)
		{
			if (IsRoot() || (LabelsOpt != null && LabelsOpt.Contains(label)))
			{
				return label;
			}
			Dictionary<LabelSymbol, LabelSymbol> dictionary = proxyLabels;
			List<LabelSymbol> list = proxiedLabels;
			if (dictionary == null)
			{
				dictionary = (proxyLabels = new Dictionary<LabelSymbol, LabelSymbol>());
				list = (proxiedLabels = new List<LabelSymbol>());
			}
			if (!dictionary.TryGetValue(label, out var value))
			{
				value = new GeneratedLabelSymbol("proxy" + label.Name);
				dictionary.Add(label, value);
				list.Add(label);
			}
			return value;
		}

		public LabelSymbol ProxyReturnIfNeeded(MethodSymbol containingMethod, BoundExpression valueOpt, out SynthesizedLocal returnValue)
		{
			returnValue = null;
			if (IsRoot())
			{
				return null;
			}
			GeneratedLabelSymbol generatedLabelSymbol = returnProxyLabel;
			if (generatedLabelSymbol == null)
			{
				generatedLabelSymbol = (returnProxyLabel = new GeneratedLabelSymbol("returnProxy"));
			}
			if (valueOpt != null)
			{
				returnValue = this.returnValue;
				if (returnValue == null)
				{
					this.returnValue = (returnValue = new SynthesizedLocal(containingMethod, valueOpt.Type, SynthesizedLocalKind.AsyncMethodReturnValue, _tryStatementSyntaxOpt));
				}
			}
			return generatedLabelSymbol;
		}
	}

	private sealed class AwaitCatchFrame
	{
		public readonly SynthesizedLocal pendingCaughtException;

		public readonly SynthesizedLocal pendingCatch;

		public readonly List<BoundBlock> handlers;

		private readonly Dictionary<LocalSymbol, LocalSymbol> _hoistedLocals;

		private readonly List<LocalSymbol> _orderedHoistedLocals;

		public AwaitCatchFrame(SyntheticBoundNodeFactory F, TryStatementSyntax tryStatementSyntax)
		{
			pendingCaughtException = new SynthesizedLocal(F.CurrentFunction, F.SpecialType(SpecialType.System_Object), SynthesizedLocalKind.TryAwaitPendingCaughtException, tryStatementSyntax);
			pendingCatch = new SynthesizedLocal(F.CurrentFunction, F.SpecialType(SpecialType.System_Int32), SynthesizedLocalKind.TryAwaitPendingCatch, tryStatementSyntax);
			handlers = new List<BoundBlock>();
			_hoistedLocals = new Dictionary<LocalSymbol, LocalSymbol>();
			_orderedHoistedLocals = new List<LocalSymbol>();
		}

		public void HoistLocal(LocalSymbol local, SyntheticBoundNodeFactory F)
		{
			if (!_hoistedLocals.Keys.Any((LocalSymbol l) => l.Name == local.Name && l.Type == local.Type))
			{
				_hoistedLocals.Add(local, local);
				_orderedHoistedLocals.Add(local);
			}
			else
			{
				LocalSymbol localSymbol = F.SynthesizedLocal(local.Type, pendingCatch.SyntaxOpt, isPinned: false, RefKind.None, SynthesizedLocalKind.ExceptionFilterAwaitHoistedExceptionLocal);
				_hoistedLocals.Add(local, localSymbol);
				_orderedHoistedLocals.Add(localSymbol);
			}
		}

		public IEnumerable<LocalSymbol> GetHoistedLocals()
		{
			return _orderedHoistedLocals;
		}

		public bool TryGetHoistedLocal(LocalSymbol originalLocal, out LocalSymbol hoistedLocal)
		{
			return _hoistedLocals.TryGetValue(originalLocal, out hoistedLocal);
		}
	}

	private readonly SyntheticBoundNodeFactory _F;

	private readonly DiagnosticBag _diagnostics;

	private readonly AwaitInFinallyAnalysis _analysis;

	private AwaitCatchFrame _currentAwaitCatchFrame;

	private AwaitFinallyFrame _currentAwaitFinallyFrame = new AwaitFinallyFrame();

	private AsyncExceptionHandlerRewriter(MethodSymbol containingMethod, NamedTypeSymbol containingType, SyntheticBoundNodeFactory factory, DiagnosticBag diagnostics, AwaitInFinallyAnalysis analysis)
	{
		_F = factory;
		_F.CurrentFunction = containingMethod;
		_diagnostics = diagnostics;
		_analysis = analysis;
	}

	public static BoundStatement Rewrite(MethodSymbol containingSymbol, NamedTypeSymbol containingType, BoundStatement statement, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		AwaitInFinallyAnalysis awaitInFinallyAnalysis = new AwaitInFinallyAnalysis(statement);
		if (!awaitInFinallyAnalysis.ContainsAwaitInHandlers())
		{
			return statement;
		}
		SyntheticBoundNodeFactory factory = new SyntheticBoundNodeFactory(containingSymbol, statement.Syntax, compilationState, diagnostics);
		return (BoundStatement)new AsyncExceptionHandlerRewriter(containingSymbol, containingType, factory, diagnostics, awaitInFinallyAnalysis).Visit(statement);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		SyntaxNode syntax = node.Syntax;
		BoundStatement boundStatement;
		BoundBlock boundBlock;
		if (!_analysis.FinallyContainsAwaits(node))
		{
			boundStatement = RewriteFinalizedRegion(node);
			boundBlock = (BoundBlock)Visit(node.FinallyBlockOpt);
			if (boundBlock == null)
			{
				return boundStatement;
			}
			if (boundStatement is BoundTryStatement boundTryStatement)
			{
				return boundTryStatement.Update(boundTryStatement.TryBlock, boundTryStatement.CatchBlocks, boundBlock, boundTryStatement.PreferFaultHandler);
			}
			return _F.Try((BoundBlock)boundStatement, ImmutableArray<BoundCatchBlock>.Empty, boundBlock);
		}
		AwaitFinallyFrame awaitFinallyFrame = PushFrame(node);
		boundStatement = RewriteFinalizedRegion(node);
		boundBlock = (BoundBlock)VisitBlock(node.FinallyBlockOpt);
		PopFrame();
		NamedTypeSymbol type = _F.SpecialType(SpecialType.System_Object);
		SynthesizedLocal synthesizedLocal = new SynthesizedLocal(_F.CurrentFunction, type, SynthesizedLocalKind.TryAwaitPendingException, syntax);
		GeneratedLabelSymbol generatedLabelSymbol = _F.GenerateLabel("finallyLabel");
		SynthesizedLocal synthesizedLocal2 = new SynthesizedLocal(_F.CurrentFunction, _F.SpecialType(SpecialType.System_Int32), SynthesizedLocalKind.TryAwaitPendingBranch, syntax);
		BoundCatchBlock item = _F.Catch(_F.Local(synthesizedLocal), _F.Block());
		BoundStatement item2 = _F.Try(_F.Block(boundStatement, _F.HiddenSequencePoint(), _F.Goto(generatedLabelSymbol), PendBranches(awaitFinallyFrame, synthesizedLocal2, generatedLabelSymbol)), ImmutableArray.Create(item));
		BoundBlock item3 = _F.Block(_F.HiddenSequencePoint(), _F.Label(generatedLabelSymbol), boundBlock, _F.HiddenSequencePoint(), UnpendException(synthesizedLocal), UnpendBranches(awaitFinallyFrame, synthesizedLocal2, synthesizedLocal));
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		instance2.Add(_F.HiddenSequencePoint());
		instance.Add(synthesizedLocal);
		instance2.Add(_F.Assignment(_F.Local(synthesizedLocal), _F.Default(synthesizedLocal.Type)));
		instance.Add(synthesizedLocal2);
		instance2.Add(_F.Assignment(_F.Local(synthesizedLocal2), _F.Default(synthesizedLocal2.Type)));
		LocalSymbol returnValue = awaitFinallyFrame.returnValue;
		if (returnValue != null)
		{
			instance.Add(returnValue);
		}
		instance2.Add(item2);
		instance2.Add(item3);
		return _F.Block(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	private BoundBlock PendBranches(AwaitFinallyFrame frame, LocalSymbol pendingBranchVar, LabelSymbol finallyLabel)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		List<LabelSymbol> proxiedLabels = frame.proxiedLabels;
		Dictionary<LabelSymbol, LabelSymbol> proxyLabels = frame.proxyLabels;
		int i = 1;
		if (proxiedLabels != null)
		{
			for (int count = proxiedLabels.Count; i <= count; i++)
			{
				LabelSymbol key = proxiedLabels[i - 1];
				LabelSymbol proxy = proxyLabels[key];
				PendBranch(instance, proxy, i, pendingBranchVar, finallyLabel);
			}
		}
		GeneratedLabelSymbol returnProxyLabel = frame.returnProxyLabel;
		if (returnProxyLabel != null)
		{
			PendBranch(instance, returnProxyLabel, i, pendingBranchVar, finallyLabel);
		}
		return _F.Block(instance.ToImmutableAndFree());
	}

	private void PendBranch(ArrayBuilder<BoundStatement> bodyStatements, LabelSymbol proxy, int i, LocalSymbol pendingBranchVar, LabelSymbol finallyLabel)
	{
		bodyStatements.Add(_F.Label(proxy));
		bodyStatements.Add(_F.Assignment(_F.Local(pendingBranchVar), _F.Literal(i)));
		bodyStatements.Add(_F.Goto(finallyLabel));
	}

	private BoundStatement UnpendBranches(AwaitFinallyFrame frame, SynthesizedLocal pendingBranchVar, SynthesizedLocal pendingException)
	{
		AwaitFinallyFrame parentOpt = frame.ParentOpt;
		List<LabelSymbol> proxiedLabels = frame.proxiedLabels;
		int i = 1;
		ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance();
		if (proxiedLabels != null)
		{
			for (int count = proxiedLabels.Count; i <= count; i++)
			{
				LabelSymbol label = proxiedLabels[i - 1];
				LabelSymbol label2 = parentOpt.ProxyLabelIfNeeded(label);
				BoundSwitchSection item = _F.SwitchSection(i, _F.Goto(label2));
				instance.Add(item);
			}
		}
		if (frame.returnProxyLabel != null)
		{
			BoundLocal boundLocal = null;
			if (frame.returnValue != null)
			{
				boundLocal = _F.Local(frame.returnValue);
			}
			LabelSymbol labelSymbol = parentOpt.ProxyReturnIfNeeded(_F.CurrentFunction, boundLocal, out var returnValue);
			BoundStatement boundStatement;
			if (labelSymbol == null)
			{
				boundStatement = new BoundReturnStatement(_F.Syntax, RefKind.None, boundLocal);
			}
			else
			{
				boundStatement = ((boundLocal != null) ? ((BoundStatement)_F.Block(_F.Assignment(_F.Local(returnValue), boundLocal), _F.Goto(labelSymbol))) : ((BoundStatement)_F.Goto(labelSymbol)));
			}
			BoundSwitchSection item2 = _F.SwitchSection(i, boundStatement);
			instance.Add(item2);
		}
		return _F.Switch(_F.Local(pendingBranchVar), instance.ToImmutableAndFree());
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		LabelSymbol label = _currentAwaitFinallyFrame.ProxyLabelIfNeeded(node.Label);
		return node.Update(label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		return base.VisitConditionalGoto(node);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		LabelSymbol labelSymbol = _currentAwaitFinallyFrame.ProxyReturnIfNeeded(_F.CurrentFunction, node.ExpressionOpt, out var returnValue);
		if (labelSymbol == null)
		{
			return base.VisitReturnStatement(node);
		}
		BoundExpression boundExpression = (BoundExpression)Visit(node.ExpressionOpt);
		if (boundExpression != null)
		{
			return _F.Block(_F.Assignment(_F.Local(returnValue), boundExpression), _F.Goto(labelSymbol));
		}
		return _F.Goto(labelSymbol);
	}

	private BoundStatement UnpendException(LocalSymbol pendingExceptionLocal)
	{
		LocalSymbol localSymbol = _F.SynthesizedLocal(_F.SpecialType(SpecialType.System_Object));
		BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(localSymbol), _F.Local(pendingExceptionLocal));
		BoundStatement thenClause = Rethrow(localSymbol);
		return _F.Block(ImmutableArray.Create(localSymbol), boundExpressionStatement, _F.If(_F.ObjectNotEqual(_F.Local(localSymbol), _F.Null(localSymbol.Type)), thenClause));
	}

	private BoundStatement Rethrow(LocalSymbol obj)
	{
		BoundStatement boundStatement = _F.Throw(_F.Local(obj));
		MethodSymbol methodSymbol = _F.WellKnownMethod(WellKnownMember.System_Runtime_ExceptionServices_ExceptionDispatchInfo__Capture, isOptional: true);
		MethodSymbol methodSymbol2 = _F.WellKnownMethod(WellKnownMember.System_Runtime_ExceptionServices_ExceptionDispatchInfo__Throw, isOptional: true);
		if (methodSymbol != null && methodSymbol2 != null)
		{
			LocalSymbol localSymbol = _F.SynthesizedLocal(_F.WellKnownType(WellKnownType.System_Exception));
			BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(localSymbol), _F.As(_F.Local(obj), localSymbol.Type));
			boundStatement = _F.Block(ImmutableArray.Create(localSymbol), boundExpressionStatement, _F.If(_F.ObjectEqual(_F.Local(localSymbol), _F.Null(localSymbol.Type)), boundStatement), _F.ExpressionStatement(_F.Call(_F.StaticCall(methodSymbol.ContainingType, methodSymbol, _F.Local(localSymbol)), methodSymbol2)));
		}
		return boundStatement;
	}

	private BoundStatement RewriteFinalizedRegion(BoundTryStatement node)
	{
		BoundBlock boundBlock = (BoundBlock)VisitBlock(node.TryBlock);
		if (node.CatchBlocks.IsDefaultOrEmpty)
		{
			return boundBlock;
		}
		AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
		_currentAwaitCatchFrame = null;
		ImmutableArray<BoundCatchBlock> catchBlocks = VisitList(node.CatchBlocks);
		BoundStatement boundStatement = _F.Try(boundBlock, catchBlocks);
		AwaitCatchFrame currentAwaitCatchFrame2 = _currentAwaitCatchFrame;
		if (currentAwaitCatchFrame2 != null)
		{
			GeneratedLabelSymbol label = _F.GenerateLabel("handled");
			List<BoundBlock> handlers = currentAwaitCatchFrame2.handlers;
			ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance(handlers.Count);
			int i = 0;
			for (int count = handlers.Count; i < count; i++)
			{
				instance.Add(_F.SwitchSection(i + 1, _F.Block(handlers[i], _F.Goto(label))));
			}
			boundStatement = _F.Block(ImmutableArray.Create((LocalSymbol)currentAwaitCatchFrame2.pendingCaughtException, (LocalSymbol)currentAwaitCatchFrame2.pendingCatch).AddRange(currentAwaitCatchFrame2.GetHoistedLocals()), _F.HiddenSequencePoint(), _F.Assignment(_F.Local(currentAwaitCatchFrame2.pendingCatch), _F.Default(currentAwaitCatchFrame2.pendingCatch.Type)), boundStatement, _F.HiddenSequencePoint(), _F.Switch(_F.Local(currentAwaitCatchFrame2.pendingCatch), instance.ToImmutableAndFree()), _F.HiddenSequencePoint(), _F.Label(label));
		}
		_currentAwaitCatchFrame = currentAwaitCatchFrame;
		return boundStatement;
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		if (!_analysis.CatchContainsAwait(node))
		{
			AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
			_currentAwaitCatchFrame = null;
			BoundNode result = base.VisitCatchBlock(node);
			_currentAwaitCatchFrame = currentAwaitCatchFrame;
			return result;
		}
		AwaitCatchFrame awaitCatchFrame = _currentAwaitCatchFrame;
		if (awaitCatchFrame == null)
		{
			TryStatementSyntax tryStatementSyntax = (TryStatementSyntax)node.Syntax.Parent;
			awaitCatchFrame = (_currentAwaitCatchFrame = new AwaitCatchFrame(_F, tryStatementSyntax));
		}
		TypeSymbol typeSymbol = node.ExceptionTypeOpt ?? _F.SpecialType(SpecialType.System_Object);
		LocalSymbol localSymbol = _F.SynthesizedLocal(typeSymbol);
		BoundAssignmentOperator boundAssignmentOperator = _F.AssignmentExpression(_F.Local(awaitCatchFrame.pendingCaughtException), _F.Convert(awaitCatchFrame.pendingCaughtException.Type, _F.Local(localSymbol)));
		BoundExpressionStatement boundExpressionStatement = _F.Assignment(_F.Local(awaitCatchFrame.pendingCatch), _F.Literal(awaitCatchFrame.handlers.Count + 1));
		BoundExpression exceptionFilterOpt = node.ExceptionFilterOpt;
		BoundCatchBlock result2;
		ImmutableArray<LocalSymbol> locals;
		if (exceptionFilterOpt == null)
		{
			result2 = node.Update(ImmutableArray.Create(localSymbol), _F.Local(localSymbol), typeSymbol, null, _F.Block(_F.HiddenSequencePoint(), _F.ExpressionStatement(boundAssignmentOperator), boundExpressionStatement), node.IsSynthesizedAsyncCatchAll);
			locals = node.Locals;
		}
		else
		{
			locals = ImmutableArray<LocalSymbol>.Empty;
			foreach (LocalSymbol local in node.Locals)
			{
				awaitCatchFrame.HoistLocal(local, _F);
			}
			BoundExpression exceptionSourceOpt = node.ExceptionSourceOpt;
			BoundExpression boundExpression = (BoundExpression)Visit(exceptionFilterOpt);
			BoundExpression exceptionFilterOpt2 = ((exceptionSourceOpt == null) ? _F.MakeSequence(boundAssignmentOperator, boundExpression) : _F.MakeSequence(boundAssignmentOperator, AssignCatchSource((BoundExpression)Visit(exceptionSourceOpt), awaitCatchFrame), boundExpression));
			result2 = node.Update(ImmutableArray.Create(localSymbol), _F.Local(localSymbol), typeSymbol, exceptionFilterOpt2, _F.Block(_F.HiddenSequencePoint(), boundExpressionStatement), node.IsSynthesizedAsyncCatchAll);
		}
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(_F.HiddenSequencePoint());
		if (exceptionFilterOpt == null)
		{
			BoundExpression exceptionSourceOpt2 = node.ExceptionSourceOpt;
			if (exceptionSourceOpt2 != null)
			{
				BoundExpression expr = AssignCatchSource((BoundExpression)Visit(exceptionSourceOpt2), awaitCatchFrame);
				instance.Add(_F.ExpressionStatement(expr));
			}
		}
		instance.Add((BoundStatement)Visit(node.Body));
		BoundBlock item = _F.Block(locals, instance.ToImmutableAndFree());
		awaitCatchFrame.handlers.Add(item);
		return result2;
	}

	private BoundExpression AssignCatchSource(BoundExpression rewrittenSource, AwaitCatchFrame currentAwaitCatchFrame)
	{
		BoundExpression result = null;
		if (rewrittenSource != null)
		{
			result = _F.AssignmentExpression(rewrittenSource, _F.Convert(rewrittenSource.Type, _F.Local(currentAwaitCatchFrame.pendingCaughtException)));
		}
		return result;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		AwaitCatchFrame currentAwaitCatchFrame = _currentAwaitCatchFrame;
		if (currentAwaitCatchFrame == null || !currentAwaitCatchFrame.TryGetHoistedLocal(node.LocalSymbol, out var hoistedLocal))
		{
			return base.VisitLocal(node);
		}
		return node.Update(hoistedLocal, node.ConstantValueOpt, hoistedLocal.Type);
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		if (node.ExpressionOpt != null || _currentAwaitCatchFrame == null)
		{
			return base.VisitThrowStatement(node);
		}
		return Rethrow(_currentAwaitCatchFrame.pendingCaughtException);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		_F.CurrentFunction = node.Symbol;
		_currentAwaitFinallyFrame = new AwaitFinallyFrame();
		BoundNode result = base.VisitLambda(node);
		_F.CurrentFunction = currentFunction;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame;
		return result;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		_F.CurrentFunction = node.Symbol;
		_currentAwaitFinallyFrame = new AwaitFinallyFrame();
		BoundNode result = base.VisitLocalFunctionStatement(node);
		_F.CurrentFunction = currentFunction;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame;
		return result;
	}

	private AwaitFinallyFrame PushFrame(BoundTryStatement statement)
	{
		return _currentAwaitFinallyFrame = new AwaitFinallyFrame(_currentAwaitFinallyFrame, _analysis.Labels(statement), (TryStatementSyntax)statement.Syntax);
	}

	private void PopFrame()
	{
		AwaitFinallyFrame currentAwaitFinallyFrame = _currentAwaitFinallyFrame;
		_currentAwaitFinallyFrame = currentAwaitFinallyFrame.ParentOpt;
	}
}
