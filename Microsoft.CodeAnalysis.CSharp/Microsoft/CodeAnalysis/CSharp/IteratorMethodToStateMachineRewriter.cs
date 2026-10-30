using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class IteratorMethodToStateMachineRewriter : MethodToStateMachineRewriter
{
	private sealed class IteratorFinallyFrame
	{
		public readonly int finalizeState;

		public readonly IteratorFinallyFrame parent;

		public readonly IteratorFinallyMethodSymbol handler;

		public Dictionary<int, IteratorFinallyFrame> knownStates;

		public readonly HashSet<LabelSymbol> labels;

		public Dictionary<LabelSymbol, LabelSymbol> proxyLabels;

		public IteratorFinallyFrame(IteratorFinallyFrame parent, int finalizeState, IteratorFinallyMethodSymbol handler, HashSet<LabelSymbol> labels)
		{
			this.parent = parent;
			this.finalizeState = finalizeState;
			this.handler = handler;
			this.labels = labels;
		}

		public IteratorFinallyFrame()
		{
			finalizeState = -1;
		}

		public bool IsRoot()
		{
			return parent == null;
		}

		public void AddState(int state)
		{
			if (parent != null)
			{
				parent.AddState(state, this);
			}
		}

		private void AddState(int state, IteratorFinallyFrame innerHandler)
		{
			Dictionary<int, IteratorFinallyFrame> dictionary = knownStates;
			if (dictionary == null)
			{
				dictionary = (knownStates = new Dictionary<int, IteratorFinallyFrame>());
			}
			dictionary.Add(state, innerHandler);
			if (parent != null)
			{
				parent.AddState(state, this);
			}
		}

		public LabelSymbol ProxyLabelIfNeeded(LabelSymbol label)
		{
			if (IsRoot() || (labels != null && labels.Contains(label)))
			{
				return label;
			}
			Dictionary<LabelSymbol, LabelSymbol> dictionary = proxyLabels;
			if (dictionary == null)
			{
				dictionary = (proxyLabels = new Dictionary<LabelSymbol, LabelSymbol>());
			}
			if (!dictionary.TryGetValue(label, out var value))
			{
				value = new GeneratedLabelSymbol("proxy" + label.Name);
				dictionary.Add(label, value);
			}
			return value;
		}
	}

	private sealed class YieldsInTryAnalysis : LabelCollector
	{
		private Dictionary<BoundTryStatement, HashSet<LabelSymbol>> _labelsInYieldingTrys;

		private bool _seenYield;

		public YieldsInTryAnalysis(BoundStatement body)
		{
			_seenYield = false;
			Visit(body);
		}

		public bool ContainsYields(BoundTryStatement statement)
		{
			if (_labelsInYieldingTrys != null)
			{
				return _labelsInYieldingTrys.ContainsKey(statement);
			}
			return false;
		}

		public bool ContainsYieldsInTrys()
		{
			return _labelsInYieldingTrys != null;
		}

		internal HashSet<LabelSymbol> Labels(BoundTryStatement statement)
		{
			return _labelsInYieldingTrys[statement];
		}

		public override BoundNode VisitTryStatement(BoundTryStatement node)
		{
			bool seenYield = _seenYield;
			HashSet<LabelSymbol> hashSet = currentLabels;
			_seenYield = false;
			currentLabels = null;
			base.VisitTryStatement(node);
			if (_seenYield)
			{
				Dictionary<BoundTryStatement, HashSet<LabelSymbol>> dictionary = _labelsInYieldingTrys;
				if (dictionary == null)
				{
					dictionary = (_labelsInYieldingTrys = new Dictionary<BoundTryStatement, HashSet<LabelSymbol>>());
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
			_seenYield |= seenYield;
			return null;
		}

		public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
		{
			_seenYield = true;
			return base.VisitYieldReturnStatement(node);
		}

		public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
		{
			return null;
		}
	}

	private readonly FieldSymbol _current;

	private YieldsInTryAnalysis _yieldsInTryAnalysis;

	private int _tryNestingLevel;

	private LabelSymbol _exitLabel;

	private LocalSymbol _methodValue;

	private IteratorFinallyFrame _currentFinallyFrame = new IteratorFinallyFrame();

	private int _nextFinalizeState = -3;

	internal IteratorMethodToStateMachineRewriter(SyntheticBoundNodeFactory F, MethodSymbol originalMethod, FieldSymbol state, FieldSymbol current, Roslyn.Utilities.IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, VariableSlotAllocator slotAllocatorOpt, int nextFreeHoistedLocalSlot, DiagnosticBag diagnostics)
		: base(F, originalMethod, state, hoistedVariables, nonReusableLocalProxies, synthesizedLocalOrdinals, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics, useFinalizerBookkeeping: false)
	{
		_current = current;
	}

	internal void GenerateMoveNextAndDispose(BoundStatement body, SynthesizedImplementationMethod moveNextMethod, SynthesizedImplementationMethod disposeMethod)
	{
		_yieldsInTryAnalysis = new YieldsInTryAnalysis(body);
		if (_yieldsInTryAnalysis.ContainsYieldsInTrys())
		{
			_tryNestingLevel++;
		}
		F.CurrentFunction = moveNextMethod;
		AddState(out var _, out var resumeLabel);
		BoundStatement boundStatement = (BoundStatement)Visit(body);
		boundStatement = F.Block(((object)cachedThis == null) ? ImmutableArray.Create(cachedState) : ImmutableArray.Create(cachedState, cachedThis), F.HiddenSequencePoint(), F.Assignment(F.Local(cachedState), F.Field(F.This(), stateField)), CacheThisIfNeeded(), Dispatch(), GenerateReturn(finished: true), F.Label(resumeLabel), F.Assignment(F.Field(F.This(), stateField), F.Literal(-1)), boundStatement);
		if (_yieldsInTryAnalysis.ContainsYieldsInTrys())
		{
			BoundBlock faultBlock = F.Block(F.ExpressionStatement(F.Call(F.This(), disposeMethod)));
			boundStatement = F.Fault((BoundBlock)boundStatement, faultBlock);
		}
		boundStatement = HandleReturn(boundStatement);
		F.CloseMethod(F.SequencePoint(body.Syntax, boundStatement));
		F.CurrentFunction = disposeMethod;
		IteratorFinallyFrame currentFinallyFrame = _currentFinallyFrame;
		if (currentFinallyFrame.knownStates == null)
		{
			F.CloseMethod(F.Return());
			return;
		}
		LocalSymbol localSymbol = F.SynthesizedLocal(stateField.Type);
		BoundLocal state = F.Local(localSymbol);
		BoundBlock body2 = F.Block(ImmutableArray.Create(localSymbol), F.Assignment(F.Local(localSymbol), F.Field(F.This(), stateField)), EmitFinallyFrame(currentFinallyFrame, state), F.Return());
		F.CloseMethod(body2);
	}

	private BoundStatement HandleReturn(BoundStatement newBody)
	{
		newBody = (((object)_exitLabel != null) ? F.Block(ImmutableArray.Create(_methodValue), newBody, F.Assignment(F.Local(_methodValue), F.Literal(value: true)), F.Label(_exitLabel), F.Return(F.Local(_methodValue))) : F.Block(newBody, F.Return(F.Literal(value: false))));
		return newBody;
	}

	private BoundStatement EmitFinallyFrame(IteratorFinallyFrame frame, BoundLocal state)
	{
		BoundStatement boundStatement = null;
		if (frame.knownStates != null)
		{
			GeneratedLabelSymbol breakLabel = F.GenerateLabel("break");
			IEnumerable<BoundSwitchSection> sections = from g in frame.knownStates.GroupBy((KeyValuePair<int, IteratorFinallyFrame> ft) =>
				{
					KeyValuePair<int, IteratorFinallyFrame> keyValuePair = ft;
					return keyValuePair.Value;
				}, (KeyValuePair<int, IteratorFinallyFrame> ft) =>
				{
					KeyValuePair<int, IteratorFinallyFrame> keyValuePair = ft;
					return keyValuePair.Key;
				})
				select F.SwitchSection(new List<int>(g), EmitFinallyFrame(g.Key, state), F.Goto(breakLabel));
			boundStatement = F.Block(F.Switch(state, sections), F.Label(breakLabel));
		}
		if (!frame.IsRoot())
		{
			BoundBlock tryBlock = ((boundStatement != null) ? F.Block(boundStatement) : F.Block());
			boundStatement = F.Try(tryBlock, ImmutableArray<BoundCatchBlock>.Empty, F.Block(F.ExpressionStatement(F.Call(F.This(), frame.handler))));
		}
		return boundStatement;
	}

	protected override BoundStatement GenerateReturn(bool finished)
	{
		BoundLiteral boundLiteral = F.Literal(!finished);
		if (_tryNestingLevel == 0)
		{
			return F.Return(boundLiteral);
		}
		if ((object)_exitLabel == null)
		{
			_exitLabel = F.GenerateLabel("exitLabel");
			_methodValue = F.SynthesizedLocal(boundLiteral.Type);
		}
		BoundGotoStatement boundGotoStatement = F.Goto(_exitLabel);
		if (finished)
		{
			boundGotoStatement = (BoundGotoStatement)VisitGotoStatement(boundGotoStatement);
		}
		return F.Block(F.Assignment(F.Local(_methodValue), boundLiteral), boundGotoStatement);
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return GenerateReturn(finished: true);
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		AddState(out var stateNumber, out var resumeLabel);
		_currentFinallyFrame.AddState(stateNumber);
		BoundExpression right = (BoundExpression)Visit(node.Expression);
		return F.Block(F.Assignment(F.Field(F.This(), _current), right), F.Assignment(F.Field(F.This(), stateField), F.Literal(stateNumber)), GenerateReturn(finished: false), F.Label(resumeLabel), F.HiddenSequencePoint(), F.Assignment(F.Field(F.This(), stateField), F.Literal(_currentFinallyFrame.finalizeState)));
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		LabelSymbol label = _currentFinallyFrame.ProxyLabelIfNeeded(node.Label);
		return node.Update(label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		return base.VisitConditionalGoto(node);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		if (!ContainsYields(node))
		{
			_tryNestingLevel++;
			BoundTryStatement result = node.Update((BoundBlock)Visit(node.TryBlock), VisitList(node.CatchBlocks), (BoundBlock)Visit(node.FinallyBlockOpt), node.PreferFaultHandler);
			_tryNestingLevel--;
			return result;
		}
		IteratorFinallyFrame iteratorFinallyFrame = PushFrame(node);
		_tryNestingLevel++;
		BoundStatement item = (BoundStatement)Visit(node.TryBlock);
		IteratorFinallyMethodSymbol handler = iteratorFinallyFrame.handler;
		MethodSymbol currentFunction = F.CurrentFunction;
		F.CurrentFunction = handler;
		BoundStatement boundStatement = (BoundStatement)Visit(node.FinallyBlockOpt);
		_tryNestingLevel--;
		PopFrame();
		boundStatement = F.Block(((object)cachedThis != null) ? ImmutableArray.Create(cachedThis) : ImmutableArray<LocalSymbol>.Empty, F.Assignment(F.Field(F.This(), stateField), F.Literal(iteratorFinallyFrame.parent.finalizeState)), CacheThisIfNeeded(), boundStatement, F.Return());
		F.CloseMethod(boundStatement);
		F.CurrentFunction = currentFunction;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.Literal(iteratorFinallyFrame.finalizeState)));
		instance.Add(item);
		instance.Add(F.ExpressionStatement(F.Call(F.This(), handler)));
		if (iteratorFinallyFrame.proxyLabels != null)
		{
			GeneratedLabelSymbol label = F.GenerateLabel("dropThrough");
			instance.Add(F.Goto(label));
			IteratorFinallyFrame parent = iteratorFinallyFrame.parent;
			foreach (KeyValuePair<LabelSymbol, LabelSymbol> proxyLabel in iteratorFinallyFrame.proxyLabels)
			{
				LabelSymbol value = proxyLabel.Value;
				LabelSymbol key = proxyLabel.Key;
				instance.Add(F.Label(value));
				instance.Add(F.ExpressionStatement(F.Call(F.This(), handler)));
				LabelSymbol label2 = parent.ProxyLabelIfNeeded(key);
				instance.Add(F.Goto(label2));
			}
			instance.Add(F.Label(label));
		}
		return F.Block(instance.ToImmutableAndFree());
	}

	private IteratorFinallyFrame PushFrame(BoundTryStatement statement)
	{
		int num = _nextFinalizeState--;
		IteratorFinallyMethodSymbol handler = MakeSynthesizedFinally(num);
		IteratorFinallyFrame iteratorFinallyFrame = new IteratorFinallyFrame(_currentFinallyFrame, num, handler, _yieldsInTryAnalysis.Labels(statement));
		iteratorFinallyFrame.AddState(num);
		_currentFinallyFrame = iteratorFinallyFrame;
		return iteratorFinallyFrame;
	}

	private void PopFrame()
	{
		IteratorFinallyFrame currentFinallyFrame = _currentFinallyFrame;
		_currentFinallyFrame = currentFinallyFrame.parent;
	}

	private bool ContainsYields(BoundTryStatement statement)
	{
		return _yieldsInTryAnalysis.ContainsYields(statement);
	}

	private IteratorFinallyMethodSymbol MakeSynthesizedFinally(int state)
	{
		IteratorStateMachine iteratorStateMachine = (IteratorStateMachine)F.CurrentType;
		IteratorFinallyMethodSymbol iteratorFinallyMethodSymbol = new IteratorFinallyMethodSymbol(iteratorStateMachine, GeneratedNames.MakeIteratorFinallyMethodName(state));
		F.ModuleBuilderOpt.AddSynthesizedDefinition(iteratorStateMachine, iteratorFinallyMethodSymbol);
		return iteratorFinallyMethodSymbol;
	}
}
