using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalyzerExecutor
{
	private const string DiagnosticCategory = "Compiler";

	internal const string AnalyzerExceptionDiagnosticId = "AD0001";

	internal const string AnalyzerDriverExceptionDiagnosticId = "AD0002";

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _analyzerOptions;

	private readonly Action<Diagnostic> _addNonCategorizedDiagnosticOpt;

	private readonly Action<Diagnostic, DiagnosticAnalyzer, bool> _addCategorizedLocalDiagnosticOpt;

	private readonly Action<Diagnostic, DiagnosticAnalyzer> _addCategorizedNonLocalDiagnosticOpt;

	private readonly Action<Exception, DiagnosticAnalyzer, Diagnostic> _onAnalyzerException;

	private readonly Func<Exception, bool> _analyzerExceptionFilter;

	private readonly AnalyzerManager _analyzerManager;

	private readonly Func<DiagnosticAnalyzer, bool> _isCompilerAnalyzer;

	private readonly Func<DiagnosticAnalyzer, object> _getAnalyzerGateOpt;

	private readonly Func<DiagnosticAnalyzer, bool> _shouldSkipAnalysisOnGeneratedCode;

	private readonly Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> _shouldSuppressGeneratedCodeDiagnostic;

	private readonly Func<SyntaxTree, TextSpan, bool> _isGeneratedCodeLocation;

	private readonly ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>> _analyzerExecutionTimeMapOpt;

	private readonly CompilationAnalysisValueProviderFactory _compilationAnalysisValueProviderFactory;

	private readonly CancellationToken _cancellationToken;

	private ConcurrentDictionary<IOperation, ControlFlowGraph> _lazyControlFlowGraphMap;

	internal Compilation Compilation => _compilation;

	internal AnalyzerOptions AnalyzerOptions => _analyzerOptions;

	internal CancellationToken CancellationToken => _cancellationToken;

	internal Action<Exception, DiagnosticAnalyzer, Diagnostic> OnAnalyzerException => _onAnalyzerException;

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => _analyzerExecutionTimeMapOpt.ToImmutableDictionary((KeyValuePair<DiagnosticAnalyzer, StrongBox<long>> pair) => pair.Key, (KeyValuePair<DiagnosticAnalyzer, StrongBox<long>> pair) => TimeSpan.FromTicks(pair.Value.Value));

	public static AnalyzerExecutor Create(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool> analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<SyntaxTree, TextSpan, bool> isGeneratedCodeLocation, Func<DiagnosticAnalyzer, object> getAnalyzerGate, bool logExecutionTime = false, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt = null, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>> analyzerExecutionTimeMapOpt = (logExecutionTime ? new ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>>() : null);
		return new AnalyzerExecutor(compilation, analyzerOptions, addNonCategorizedDiagnosticOpt, onAnalyzerException, analyzerExceptionFilter, isCompilerAnalyzer, analyzerManager, shouldSkipAnalysisOnGeneratedCode, shouldSuppressGeneratedCodeDiagnostic, isGeneratedCodeLocation, getAnalyzerGate, analyzerExecutionTimeMapOpt, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, cancellationToken);
	}

	public static AnalyzerExecutor CreateForSupportedDiagnostics(Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, AnalyzerManager analyzerManager, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new AnalyzerExecutor(null, null, null, onAnalyzerException, null, null, analyzerManager, (DiagnosticAnalyzer _) => false, (Diagnostic diagnostic, DiagnosticAnalyzer analyzer, Compilation compilation, CancellationToken ct) => false, (SyntaxTree _1, TextSpan _2) => false, null, null, null, null, cancellationToken);
	}

	private AnalyzerExecutor(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool> analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<SyntaxTree, TextSpan, bool> isGeneratedCodeLocation, Func<DiagnosticAnalyzer, object> getAnalyzerGateOpt, ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>> analyzerExecutionTimeMapOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_analyzerOptions = analyzerOptions;
		_addNonCategorizedDiagnosticOpt = addNonCategorizedDiagnosticOpt;
		_onAnalyzerException = onAnalyzerException;
		_analyzerExceptionFilter = analyzerExceptionFilter;
		_isCompilerAnalyzer = isCompilerAnalyzer;
		_analyzerManager = analyzerManager;
		_shouldSkipAnalysisOnGeneratedCode = shouldSkipAnalysisOnGeneratedCode;
		_shouldSuppressGeneratedCodeDiagnostic = shouldSuppressGeneratedCodeDiagnostic;
		_isGeneratedCodeLocation = isGeneratedCodeLocation;
		_getAnalyzerGateOpt = getAnalyzerGateOpt;
		_analyzerExecutionTimeMapOpt = analyzerExecutionTimeMapOpt;
		_addCategorizedLocalDiagnosticOpt = addCategorizedLocalDiagnosticOpt;
		_addCategorizedNonLocalDiagnosticOpt = addCategorizedNonLocalDiagnosticOpt;
		_cancellationToken = cancellationToken;
		_compilationAnalysisValueProviderFactory = new CompilationAnalysisValueProviderFactory();
	}

	public AnalyzerExecutor WithCancellationToken(CancellationToken cancellationToken)
	{
		if (cancellationToken == _cancellationToken)
		{
			return this;
		}
		return new AnalyzerExecutor(_compilation, _analyzerOptions, _addNonCategorizedDiagnosticOpt, _onAnalyzerException, _analyzerExceptionFilter, _isCompilerAnalyzer, _analyzerManager, _shouldSkipAnalysisOnGeneratedCode, _shouldSuppressGeneratedCodeDiagnostic, _isGeneratedCodeLocation, _getAnalyzerGateOpt, _analyzerExecutionTimeMapOpt, _addCategorizedLocalDiagnosticOpt, _addCategorizedNonLocalDiagnosticOpt, cancellationToken);
	}

	public void ExecuteInitializeMethod(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope sessionScope)
	{
		AnalyzerAnalysisContext item = new AnalyzerAnalysisContext(analyzer, sessionScope);
		ExecuteAndCatchIfThrows(analyzer, ((DiagnosticAnalyzer analyzer, AnalyzerAnalysisContext context) data) =>
		{
			data.analyzer.Initialize(data.context);
		}, (analyzer, item));
	}

	public void ExecuteCompilationStartActions(ImmutableArray<CompilationStartAnalyzerAction> actions, HostCompilationStartAnalysisScope compilationScope)
	{
		foreach (CompilationStartAnalyzerAction item2 in actions)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			AnalyzerCompilationStartAnalysisContext item = new AnalyzerCompilationStartAnalysisContext(item2.Analyzer, compilationScope, _compilation, _analyzerOptions, _compilationAnalysisValueProviderFactory, _cancellationToken);
			ExecuteAndCatchIfThrows(item2.Analyzer, ((Action<CompilationStartAnalysisContext> action, AnalyzerCompilationStartAnalysisContext context) data) =>
			{
				data.action(data.context);
			}, (item2.Action, item), new AnalysisContextInfo(_compilation));
		}
	}

	public bool TryExecuteCompilationActions(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, CompilationEvent compilationEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartProcessingEvent(compilationEvent, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteCompilationActionsCore(compilationActions, analyzer, analyzerStateOpt);
				analysisStateOpt?.MarkEventComplete(compilationEvent, analyzer);
				return true;
			}
			return IsEventComplete(compilationEvent, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteCompilationActionsCore(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		Action<Diagnostic> addCompilationDiagnostic = GetAddCompilationDiagnostic(analyzer);
		Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
		foreach (CompilationAnalyzerAction item2 in compilationActions)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			if (ShouldExecuteAction(analyzerStateOpt, item2))
			{
				CompilationAnalysisContext item = new CompilationAnalysisContext(_compilation, _analyzerOptions, addCompilationDiagnostic, isSupportedDiagnostic, _compilationAnalysisValueProviderFactory, _cancellationToken);
				ExecuteAndCatchIfThrows(item2.Analyzer, ((Action<CompilationAnalysisContext> action, CompilationAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, (item2.Action, item), new AnalysisContextInfo(_compilation));
				analyzerStateOpt?.ProcessedActions.Add(item2);
			}
		}
	}

	public bool TryExecuteSymbolActions(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCodeSymbol)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			ISymbol symbol = symbolDeclaredEvent.Symbol;
			if (TryStartAnalyzingSymbol(symbol, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSymbolActionsCore(symbolActions, analyzer, symbolDeclaredEvent, getTopMostNodeForAnalysis, analyzerStateOpt, isGeneratedCodeSymbol);
				analysisStateOpt?.MarkSymbolComplete(symbol, analyzer);
				return true;
			}
			return IsEventComplete(symbolDeclaredEvent, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteSymbolActionsCore(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCodeSymbol)
	{
		if (isGeneratedCodeSymbol && _shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		ISymbol symbol = symbolDeclaredEvent.Symbol;
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(symbol, symbolDeclaredEvent.DeclaringSyntaxReferences, analyzer, getTopMostNodeForAnalysis);
		Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
		foreach (SymbolAnalyzerAction item in symbolActions)
		{
			Action<SymbolAnalysisContext> action = item.Action;
			if (item.Kinds.Contains(symbol.Kind) && ShouldExecuteAction(analyzerStateOpt, item))
			{
				CancellationToken cancellationToken = _cancellationToken;
				cancellationToken.ThrowIfCancellationRequested();
				ExecuteAndCatchIfThrows(argument: (action, new SymbolAnalysisContext(symbol, _compilation, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, _cancellationToken)), analyzer: item.Analyzer, analyze: ((Action<SymbolAnalysisContext> action, SymbolAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, info: new AnalysisContextInfo(_compilation, symbol));
				analyzerStateOpt?.ProcessedActions.Add(item);
			}
		}
	}

	public bool TryExecuteSemanticModelActions(ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions, DiagnosticAnalyzer analyzer, SemanticModel semanticModel, CompilationEvent compilationUnitCompletedEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartProcessingEvent(compilationUnitCompletedEvent, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSemanticModelActionsCore(semanticModelActions, analyzer, semanticModel, analyzerStateOpt, isGeneratedCode);
				analysisStateOpt?.MarkEventComplete(compilationUnitCompletedEvent, analyzer);
				return true;
			}
			return IsEventComplete(compilationUnitCompletedEvent, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteSemanticModelActionsCore(ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions, DiagnosticAnalyzer analyzer, SemanticModel semanticModel, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(semanticModel.SyntaxTree, analyzer, isSyntaxDiagnostic: false);
		Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
		foreach (SemanticModelAnalyzerAction item2 in semanticModelActions)
		{
			if (ShouldExecuteAction(analyzerStateOpt, item2))
			{
				CancellationToken cancellationToken = _cancellationToken;
				cancellationToken.ThrowIfCancellationRequested();
				SemanticModelAnalysisContext item = new SemanticModelAnalysisContext(semanticModel, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, _cancellationToken);
				ExecuteAndCatchIfThrows(item2.Analyzer, ((Action<SemanticModelAnalysisContext> action, SemanticModelAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, (item2.Action, item), new AnalysisContextInfo(semanticModel));
				analyzerStateOpt?.ProcessedActions.Add(item2);
			}
		}
	}

	public bool TryExecuteSyntaxTreeActions(ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions, DiagnosticAnalyzer analyzer, SyntaxTree tree, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartSyntaxAnalysis(tree, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSyntaxTreeActionsCore(syntaxTreeActions, analyzer, tree, analyzerStateOpt, isGeneratedCode);
				analysisStateOpt?.MarkSyntaxAnalysisComplete(tree, analyzer);
				return true;
			}
			return analysisStateOpt == null || !analysisStateOpt.HasPendingSyntaxAnalysis(analysisScope);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteSyntaxTreeActionsCore(ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions, DiagnosticAnalyzer analyzer, SyntaxTree tree, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(tree, analyzer, isSyntaxDiagnostic: true);
		Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
		foreach (SyntaxTreeAnalyzerAction item2 in syntaxTreeActions)
		{
			if (ShouldExecuteAction(analyzerStateOpt, item2))
			{
				CancellationToken cancellationToken = _cancellationToken;
				cancellationToken.ThrowIfCancellationRequested();
				SyntaxTreeAnalysisContext item = new SyntaxTreeAnalysisContext(tree, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, _compilation, _cancellationToken);
				ExecuteAndCatchIfThrows(item2.Analyzer, ((Action<SyntaxTreeAnalysisContext> action, SyntaxTreeAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, (item2.Action, item), new AnalysisContextInfo(_compilation, tree));
				analyzerStateOpt?.ProcessedActions.Add(item2);
			}
		}
	}

	private void ExecuteSyntaxNodeAction<TLanguageKindEnum>(SyntaxNodeAnalyzerAction<TLanguageKindEnum> syntaxNodeAction, SyntaxNode node, ISymbol containingSymbol, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt) where TLanguageKindEnum : struct
	{
		if (ShouldExecuteAction(analyzerStateOpt, syntaxNodeAction))
		{
			SyntaxNodeAnalysisContext item = new SyntaxNodeAnalysisContext(node, containingSymbol, semanticModel, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, _cancellationToken);
			ExecuteAndCatchIfThrows(syntaxNodeAction.Analyzer, ((Action<SyntaxNodeAnalysisContext> action, SyntaxNodeAnalysisContext context) data) =>
			{
				data.action(data.context);
			}, (syntaxNodeAction.Action, item), new AnalysisContextInfo(_compilation, node));
			analyzerStateOpt?.ProcessedActions.Add(syntaxNodeAction);
		}
	}

	private void ExecuteOperationAction(OperationAnalyzerAction operationAction, IOperation operation, ISymbol containingSymbol, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		if (ShouldExecuteAction(analyzerStateOpt, operationAction))
		{
			OperationAnalysisContext item = new OperationAnalysisContext(operation, containingSymbol, semanticModel.Compilation, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, GetControlFlowGraph, _cancellationToken);
			ExecuteAndCatchIfThrows(operationAction.Analyzer, ((Action<OperationAnalysisContext> action, OperationAnalysisContext context) data) =>
			{
				data.action(data.context);
			}, (operationAction.Action, item), new AnalysisContextInfo(_compilation, operation));
			analyzerStateOpt?.ProcessedActions.Add(operationAction);
		}
	}

	public bool TryExecuteCodeBlockActions<TLanguageKindEnum>(IEnumerable<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> codeBlockStartActions, IEnumerable<CodeBlockAnalyzerAction> codeBlockActions, IEnumerable<CodeBlockAnalyzerAction> codeBlockEndActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<SyntaxNode> executableCodeBlocks, SemanticModel semanticModel, Func<SyntaxNode, TLanguageKindEnum> getKind, SyntaxReference declaration, int declarationIndex, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode) where TLanguageKindEnum : struct
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteBlockActionsCore<CodeBlockStartAnalyzerAction<TLanguageKindEnum>, CodeBlockAnalyzerAction, SyntaxNodeAnalyzerAction<TLanguageKindEnum>, AnalysisState.SyntaxNodeAnalyzerStateData, SyntaxNode, TLanguageKindEnum>(codeBlockStartActions, codeBlockActions, codeBlockEndActions, analyzer, declaredNode, declaredSymbol, executableCodeBlocks, (ImmutableArray<SyntaxNode> codeBlocks) => codeBlocks.SelectMany((SyntaxNode cb) => cb.DescendantNodesAndSelf()), semanticModel, getKind, analyzerStateOpt?.CodeBlockAnalysisState, isGeneratedCode);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public bool TryExecuteOperationBlockActions(IEnumerable<OperationBlockStartAnalyzerAction> operationBlockStartActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockEndActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, SemanticModel semanticModel, SyntaxReference declaration, int declarationIndex, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteBlockActionsCore<OperationBlockStartAnalyzerAction, OperationBlockAnalyzerAction, OperationAnalyzerAction, AnalysisState.OperationAnalyzerStateData, IOperation, int>(operationBlockStartActions, operationBlockActions, operationBlockEndActions, analyzer, declaredNode, declaredSymbol, operationBlocks, (ImmutableArray<IOperation> blocks) => operations, semanticModel, null, analyzerStateOpt?.OperationBlockAnalysisState, isGeneratedCode);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteBlockActionsCore<TBlockStartAction, TBlockAction, TNodeAction, TNodeStateData, TNode, TLanguageKindEnum>(IEnumerable<TBlockStartAction> startActions, IEnumerable<TBlockAction> actions, IEnumerable<TBlockAction> endActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<TNode> executableBlocks, Func<ImmutableArray<TNode>, IEnumerable<TNode>> getNodesToAnalyze, SemanticModel semanticModel, Func<SyntaxNode, TLanguageKindEnum> getKind, AnalysisState.BlockAnalyzerStateData<TBlockAction, TNodeStateData> analyzerStateOpt, bool isGeneratedCode) where TBlockStartAction : AnalyzerAction where TBlockAction : AnalyzerAction where TNodeAction : AnalyzerAction where TNodeStateData : AnalysisState.AnalyzerStateData, new() where TLanguageKindEnum : struct
	{
		if (isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		PooledHashSet<TBlockAction> instance = PooledHashSet<TBlockAction>.GetInstance();
		PooledHashSet<TBlockAction> instance2 = PooledHashSet<TBlockAction>.GetInstance();
		ArrayBuilder<TNodeAction> instance3 = ArrayBuilder<TNodeAction>.GetInstance();
		ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> arrayBuilder = instance3 as ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>;
		ArrayBuilder<OperationAnalyzerAction> arrayBuilder2 = instance3 as ArrayBuilder<OperationAnalyzerAction>;
		ImmutableArray<IOperation> operationBlocks = ((executableBlocks[0] is IOperation) ? ((ImmutableArray<IOperation>)(object)executableBlocks) : ImmutableArray<IOperation>.Empty);
		instance2.AddAll(actions);
		if (analyzerStateOpt?.CurrentBlockEndActions != null)
		{
			instance.AddAll(analyzerStateOpt.CurrentBlockEndActions.Cast<TBlockAction>());
			instance3.AddRange(analyzerStateOpt.CurrentBlockNodeActions.Cast<TNodeAction>());
		}
		else
		{
			instance.AddAll(endActions);
		}
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(semanticModel.SyntaxTree, declaredNode.FullSpan, analyzer, isSyntaxDiagnostic: false);
		Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
		try
		{
			foreach (TBlockStartAction startAction in startActions)
			{
				if (!ShouldExecuteAction(analyzerStateOpt, startAction))
				{
					continue;
				}
				if (startAction is CodeBlockStartAnalyzerAction<TLanguageKindEnum> codeBlockStartAnalyzerAction)
				{
					PooledHashSet<CodeBlockAnalyzerAction> item = instance as PooledHashSet<CodeBlockAnalyzerAction>;
					HostCodeBlockStartAnalysisScope<TLanguageKindEnum> hostCodeBlockStartAnalysisScope = new HostCodeBlockStartAnalysisScope<TLanguageKindEnum>();
					AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum> item2 = new AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum>(startAction.Analyzer, hostCodeBlockStartAnalysisScope, declaredNode, declaredSymbol, semanticModel, _analyzerOptions, _cancellationToken);
					ExecuteAndCatchIfThrows(startAction.Analyzer, ((Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action, AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum> context, HostCodeBlockStartAnalysisScope<TLanguageKindEnum> scope, PooledHashSet<CodeBlockAnalyzerAction> blockEndActions, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> syntaxNodeActions) data) =>
					{
						data.action(data.context);
						data.blockEndActions.AddAll(data.scope.CodeBlockEndActions);
						data.syntaxNodeActions.AddRange(data.scope.SyntaxNodeActions);
					}, (codeBlockStartAnalyzerAction.Action, item2, hostCodeBlockStartAnalysisScope, item, arrayBuilder), new AnalysisContextInfo(_compilation, declaredSymbol, declaredNode));
				}
				else if (startAction is OperationBlockStartAnalyzerAction operationBlockStartAnalyzerAction)
				{
					PooledHashSet<OperationBlockAnalyzerAction> item3 = instance as PooledHashSet<OperationBlockAnalyzerAction>;
					HostOperationBlockStartAnalysisScope hostOperationBlockStartAnalysisScope = new HostOperationBlockStartAnalysisScope();
					AnalyzerOperationBlockStartAnalysisContext item4 = new AnalyzerOperationBlockStartAnalysisContext(startAction.Analyzer, hostOperationBlockStartAnalysisScope, operationBlocks, declaredSymbol, semanticModel.Compilation, _analyzerOptions, GetControlFlowGraph, _cancellationToken);
					ExecuteAndCatchIfThrows(startAction.Analyzer, ((Action<OperationBlockStartAnalysisContext> action, AnalyzerOperationBlockStartAnalysisContext context, HostOperationBlockStartAnalysisScope scope, PooledHashSet<OperationBlockAnalyzerAction> blockEndActions, ArrayBuilder<OperationAnalyzerAction> operationActions) data) =>
					{
						data.action(data.context);
						data.blockEndActions.AddAll(data.scope.OperationBlockEndActions);
						data.operationActions.AddRange(data.scope.OperationActions);
					}, (operationBlockStartAnalyzerAction.Action, item4, hostOperationBlockStartAnalysisScope, item3, arrayBuilder2), new AnalysisContextInfo(_compilation, declaredSymbol));
				}
				analyzerStateOpt?.ProcessedActions.Add(startAction);
			}
		}
		finally
		{
			if (analyzerStateOpt != null)
			{
				analyzerStateOpt.CurrentBlockEndActions = instance.ToImmutableHashSet();
				analyzerStateOpt.CurrentBlockNodeActions = ((IEnumerable<AnalyzerAction>)instance3).ToImmutableHashSet();
			}
		}
		if (instance3.Any())
		{
			if (arrayBuilder != null)
			{
				ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind = GetNodeActionsByKind(arrayBuilder);
				IEnumerable<SyntaxNode> nodesToAnalyze = (IEnumerable<SyntaxNode>)getNodesToAnalyze(executableBlocks);
				ExecuteSyntaxNodeActions(nodesToAnalyze, nodeActionsByKind, analyzer, declaredSymbol, semanticModel, getKind, addDiagnostic, isSupportedDiagnostic, ((analyzerStateOpt != null) ? analyzerStateOpt.ExecutableNodesAnalysisState : null) as AnalysisState.SyntaxNodeAnalyzerStateData);
			}
			else if (arrayBuilder2 != null)
			{
				ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind = GetOperationActionsByKind(arrayBuilder2);
				IEnumerable<IOperation> operationsToAnalyze = (IEnumerable<IOperation>)getNodesToAnalyze(executableBlocks);
				ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, analyzer, declaredSymbol, semanticModel, addDiagnostic, isSupportedDiagnostic, ((analyzerStateOpt != null) ? analyzerStateOpt.ExecutableNodesAnalysisState : null) as AnalysisState.OperationAnalyzerStateData);
			}
		}
		instance3.Free();
		ExecuteBlockActions(instance2, declaredNode, declaredSymbol, semanticModel, operationBlocks, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
		ExecuteBlockActions(instance, declaredNode, declaredSymbol, semanticModel, operationBlocks, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
	}

	private void ExecuteBlockActions<TBlockAction, TNodeStateData>(PooledHashSet<TBlockAction> blockActions, SyntaxNode declaredNode, ISymbol declaredSymbol, SemanticModel semanticModel, ImmutableArray<IOperation> operationBlocks, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.BlockAnalyzerStateData<TBlockAction, TNodeStateData> analyzerStateOpt) where TBlockAction : AnalyzerAction where TNodeStateData : AnalysisState.AnalyzerStateData, new()
	{
		foreach (TBlockAction blockAction in blockActions)
		{
			if (!ShouldExecuteAction(analyzerStateOpt, blockAction))
			{
				continue;
			}
			if (blockAction is CodeBlockAnalyzerAction codeBlockAnalyzerAction)
			{
				CodeBlockAnalysisContext item = new CodeBlockAnalysisContext(declaredNode, declaredSymbol, semanticModel, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, _cancellationToken);
				ExecuteAndCatchIfThrows(codeBlockAnalyzerAction.Analyzer, ((Action<CodeBlockAnalysisContext> action, CodeBlockAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, (codeBlockAnalyzerAction.Action, item), new AnalysisContextInfo(_compilation, declaredSymbol, declaredNode));
			}
			else if (blockAction is OperationBlockAnalyzerAction operationBlockAnalyzerAction)
			{
				OperationBlockAnalysisContext item2 = new OperationBlockAnalysisContext(operationBlocks, declaredSymbol, semanticModel.Compilation, _analyzerOptions, addDiagnostic, isSupportedDiagnostic, GetControlFlowGraph, _cancellationToken);
				ExecuteAndCatchIfThrows(operationBlockAnalyzerAction.Analyzer, ((Action<OperationBlockAnalysisContext> action, OperationBlockAnalysisContext context) data) =>
				{
					data.action(data.context);
				}, (operationBlockAnalyzerAction.Action, item2), new AnalysisContextInfo(_compilation, declaredSymbol));
			}
			analyzerStateOpt?.ProcessedActions.Add(blockAction);
		}
		blockActions.Free();
	}

	internal static ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> GetNodeActionsByKind<TLanguageKindEnum>(IEnumerable<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> nodeActions) where TLanguageKindEnum : struct
	{
		PooledDictionary<TLanguageKindEnum, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> instance = PooledDictionary<TLanguageKindEnum, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>.GetInstance();
		foreach (SyntaxNodeAnalyzerAction<TLanguageKindEnum> nodeAction in nodeActions)
		{
			foreach (TLanguageKindEnum kind in nodeAction.Kinds)
			{
				if (!instance.TryGetValue(kind, out var value))
				{
					instance.Add(kind, value = ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>.GetInstance());
				}
				value.Add(nodeAction);
			}
		}
		ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> result = ImmutableDictionary.CreateRange(instance.Select((KeyValuePair<TLanguageKindEnum, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> kvp) => KeyValuePairUtil.Create(kvp.Key, kvp.Value.ToImmutableAndFree())));
		instance.Free();
		return result;
	}

	public bool TryExecuteSyntaxNodeActions<TLanguageKindEnum>(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, Func<SyntaxNode, TLanguageKindEnum> getKind, TextSpan filterSpan, SyntaxReference declaration, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode) where TLanguageKindEnum : struct
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSyntaxNodeActionsCore(nodesToAnalyze, nodeActionsByKind, analyzer, declaredSymbol, model, getKind, filterSpan, analyzerStateOpt, isGeneratedCode);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteSyntaxNodeActionsCore<TLanguageKindEnum>(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, TLanguageKindEnum> getKind, TextSpan filterSpan, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt, bool isGeneratedCode) where TLanguageKindEnum : struct
	{
		if (!isGeneratedCode || !_shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			Action<Diagnostic> addDiagnostic = GetAddDiagnostic(model.SyntaxTree, filterSpan, analyzer, isSyntaxDiagnostic: false);
			Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
			ExecuteSyntaxNodeActions(nodesToAnalyze, nodeActionsByKind, analyzer, containingSymbol, model, getKind, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
		}
	}

	private void ExecuteSyntaxNodeActions<TLanguageKindEnum>(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, TLanguageKindEnum> getKind, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt) where TLanguageKindEnum : struct
	{
		SyntaxNode syntaxNode = analyzerStateOpt?.CurrentNode;
		if (syntaxNode != null)
		{
			ExecuteSyntaxNodeActions(syntaxNode, nodeActionsByKind, containingSymbol, model, getKind, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
		}
		foreach (SyntaxNode item in nodesToAnalyze)
		{
			if (ShouldExecuteNode(analyzerStateOpt, item, analyzer))
			{
				SetCurrentNode(analyzerStateOpt, item);
				ExecuteSyntaxNodeActions(item, nodeActionsByKind, containingSymbol, model, getKind, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
			}
		}
	}

	private void ExecuteSyntaxNodeActions<TLanguageKindEnum>(SyntaxNode node, IDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, TLanguageKindEnum> getKind, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt) where TLanguageKindEnum : struct
	{
		if (nodeActionsByKind.TryGetValue(getKind(node), out var value))
		{
			foreach (SyntaxNodeAnalyzerAction<TLanguageKindEnum> item in value)
			{
				ExecuteSyntaxNodeAction(item, node, containingSymbol, model, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
			}
		}
		analyzerStateOpt?.ClearNodeAnalysisState();
	}

	internal static ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> GetOperationActionsByKind(IEnumerable<OperationAnalyzerAction> operationActions)
	{
		PooledDictionary<OperationKind, ArrayBuilder<OperationAnalyzerAction>> instance = PooledDictionary<OperationKind, ArrayBuilder<OperationAnalyzerAction>>.GetInstance();
		foreach (OperationAnalyzerAction operationAction in operationActions)
		{
			foreach (OperationKind kind in operationAction.Kinds)
			{
				if (!instance.TryGetValue(kind, out var value))
				{
					instance.Add(kind, value = ArrayBuilder<OperationAnalyzerAction>.GetInstance());
				}
				value.Add(operationAction);
			}
		}
		ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> result = ImmutableDictionary.CreateRange(instance.Select((KeyValuePair<OperationKind, ArrayBuilder<OperationAnalyzerAction>> kvp) => KeyValuePairUtil.Create(kvp.Key, kvp.Value.ToImmutableAndFree())));
		instance.Free();
		return result;
	}

	public bool TryExecuteOperationActions(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, TextSpan filterSpan, SyntaxReference declaration, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.OperationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingOperationReference(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteOperationActionsCore(operationsToAnalyze, operationActionsByKind, analyzer, declaredSymbol, model, filterSpan, analyzerStateOpt, isGeneratedCode);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private void ExecuteOperationActionsCore(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, TextSpan filterSpan, AnalysisState.OperationAnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		if (!isGeneratedCode || !_shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			Action<Diagnostic> addDiagnostic = GetAddDiagnostic(model.SyntaxTree, filterSpan, analyzer, isSyntaxDiagnostic: false);
			Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(analyzer, d);
			ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, analyzer, containingSymbol, model, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
		}
	}

	private void ExecuteOperationActions(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		IOperation operation = analyzerStateOpt?.CurrentOperation;
		if (operation != null)
		{
			ExecuteOperationActions(operation, operationActionsByKind, containingSymbol, model, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
		}
		foreach (IOperation item in operationsToAnalyze)
		{
			if (ShouldExecuteOperation(analyzerStateOpt, item, analyzer))
			{
				SetCurrentOperation(analyzerStateOpt, item);
				ExecuteOperationActions(item, operationActionsByKind, containingSymbol, model, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
			}
		}
	}

	private void ExecuteOperationActions(IOperation operation, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, ISymbol containingSymbol, SemanticModel model, Action<Diagnostic> addDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		if (operationActionsByKind.TryGetValue(operation.Kind, out var value))
		{
			foreach (OperationAnalyzerAction item in value)
			{
				ExecuteOperationAction(item, operation, containingSymbol, model, addDiagnostic, isSupportedDiagnostic, analyzerStateOpt);
			}
		}
		analyzerStateOpt?.ClearNodeAnalysisState();
	}

	internal static bool CanHaveExecutableCodeBlock(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Namespace:
		case SymbolKind.Property:
			return true;
		case SymbolKind.Field:
			return true;
		default:
			return false;
		}
	}

	internal void ExecuteAndCatchIfThrows<TArg>(DiagnosticAnalyzer analyzer, Action<TArg> analyze, TArg argument, AnalysisContextInfo? info = null)
	{
		object obj = _getAnalyzerGateOpt?.Invoke(analyzer);
		if (obj != null)
		{
			lock (obj)
			{
				ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, argument, info);
				return;
			}
		}
		ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, argument, info);
	}

	private void ExecuteAndCatchIfThrows_NoLock<TArg>(DiagnosticAnalyzer analyzer, Action<TArg> analyze, TArg argument, AnalysisContextInfo? info)
	{
		try
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			PooledStopwatch pooledStopwatch = null;
			if (_analyzerExecutionTimeMapOpt != null)
			{
				pooledStopwatch = PooledStopwatch.StartInstance();
				pooledStopwatch.Restart();
			}
			analyze(argument);
			if (pooledStopwatch != null)
			{
				pooledStopwatch.Stop();
				Interlocked.Add(ref _analyzerExecutionTimeMapOpt.GetOrAdd(analyzer, (DiagnosticAnalyzer _) => new StrongBox<long>(0L)).Value, pooledStopwatch.Elapsed.Ticks);
				pooledStopwatch.Free();
			}
		}
		catch (Exception ex) when (ExceptionFilter(ex))
		{
			Diagnostic arg = CreateAnalyzerExceptionDiagnostic(analyzer, ex, info);
			try
			{
				_onAnalyzerException(ex, analyzer, arg);
			}
			catch (Exception)
			{
			}
		}
	}

	internal bool ExceptionFilter(Exception ex)
	{
		if ((ex as OperationCanceledException)?.CancellationToken == _cancellationToken)
		{
			return false;
		}
		if (_analyzerExceptionFilter != null)
		{
			return _analyzerExceptionFilter(ex);
		}
		return true;
	}

	internal static Diagnostic CreateAnalyzerExceptionDiagnostic(DiagnosticAnalyzer analyzer, Exception e, AnalysisContextInfo? info = null)
	{
		string text = analyzer.ToString();
		string compilerAnalyzerFailure = CodeAnalysisResources.CompilerAnalyzerFailure;
		string compilerAnalyzerThrows = CodeAnalysisResources.CompilerAnalyzerThrows;
		string[] array = new string[3]
		{
			text,
			((object)e).GetType().ToString(),
			e.Message
		};
		string description = string.Format(CodeAnalysisResources.CompilerAnalyzerThrowsDescription, text, CreateDiagnosticDescription(info, e));
		DiagnosticDescriptor analyzerExceptionDiagnosticDescriptor = GetAnalyzerExceptionDiagnosticDescriptor("AD0001", compilerAnalyzerFailure, description, compilerAnalyzerThrows);
		Location none = Location.None;
		object[] messageArgs = array;
		return Diagnostic.Create(analyzerExceptionDiagnosticDescriptor, none, messageArgs);
	}

	private static string CreateDiagnosticDescription(AnalysisContextInfo? info, Exception e)
	{
		if (!info.HasValue)
		{
			return e.CreateDiagnosticDescription();
		}
		return string.Join(Environment.NewLine, string.Format(CodeAnalysisResources.ExceptionContext, info?.GetContext()), e.CreateDiagnosticDescription());
	}

	internal static Diagnostic CreateDriverExceptionDiagnostic(Exception e)
	{
		string analyzerDriverFailure = CodeAnalysisResources.AnalyzerDriverFailure;
		string analyzerDriverThrows = CodeAnalysisResources.AnalyzerDriverThrows;
		string[] array = new string[2]
		{
			((object)e).GetType().ToString(),
			e.Message
		};
		string description = string.Format(CodeAnalysisResources.AnalyzerDriverThrowsDescription, e.CreateDiagnosticDescription());
		DiagnosticDescriptor analyzerExceptionDiagnosticDescriptor = GetAnalyzerExceptionDiagnosticDescriptor("AD0002", analyzerDriverFailure, description, analyzerDriverThrows);
		Location none = Location.None;
		object[] messageArgs = array;
		return Diagnostic.Create(analyzerExceptionDiagnosticDescriptor, none, messageArgs);
	}

	internal static DiagnosticDescriptor GetAnalyzerExceptionDiagnosticDescriptor(string id = null, string title = null, string description = null, string messageFormat = null)
	{
		id = id ?? "AD0001";
		title = title ?? CodeAnalysisResources.CompilerAnalyzerFailure;
		messageFormat = messageFormat ?? CodeAnalysisResources.CompilerAnalyzerThrows;
		description = description ?? CodeAnalysisResources.CompilerAnalyzerFailure;
		return new DiagnosticDescriptor(id, title, messageFormat, "Compiler", DiagnosticSeverity.Warning, true, description, null, "AnalyzerException");
	}

	internal static bool IsAnalyzerExceptionDiagnostic(Diagnostic diagnostic)
	{
		if (diagnostic.Id == "AD0001" || diagnostic.Id == "AD0002")
		{
			foreach (string customTag in diagnostic.Descriptor.CustomTags)
			{
				if (customTag == "AnalyzerException")
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool AreEquivalentAnalyzerExceptionDiagnostics(Diagnostic exceptionDiagnostic, Diagnostic other)
	{
		if (!IsAnalyzerExceptionDiagnostic(other))
		{
			return false;
		}
		if (exceptionDiagnostic.Id == other.Id && exceptionDiagnostic.Severity == other.Severity)
		{
			return exceptionDiagnostic.GetMessage() == other.GetMessage();
		}
		return false;
	}

	private bool IsSupportedDiagnostic(DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
	{
		if (diagnostic is DiagnosticWithInfo)
		{
			return true;
		}
		return _analyzerManager.IsSupportedDiagnostic(analyzer, diagnostic, _isCompilerAnalyzer, this);
	}

	private Action<Diagnostic> GetAddDiagnostic(ISymbol contextSymbol, ImmutableArray<SyntaxReference> cachedDeclaringReferences, DiagnosticAnalyzer analyzer, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis)
	{
		return GetAddDiagnostic(contextSymbol, cachedDeclaringReferences, _compilation, analyzer, _addNonCategorizedDiagnosticOpt, _addCategorizedLocalDiagnosticOpt, _addCategorizedNonLocalDiagnosticOpt, getTopMostNodeForAnalysis, _shouldSuppressGeneratedCodeDiagnostic, _cancellationToken);
	}

	private static Action<Diagnostic> GetAddDiagnostic(ISymbol contextSymbol, ImmutableArray<SyntaxReference> cachedDeclaringReferences, Compilation compilation, DiagnosticAnalyzer analyzer, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, CancellationToken cancellationToken)
	{
		return (Diagnostic diagnostic) =>
		{
			if (!shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, compilation, cancellationToken))
			{
				if (addCategorizedLocalDiagnosticOpt == null)
				{
					addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else
				{
					if (diagnostic.Location.IsInSource)
					{
						foreach (SyntaxReference item in cachedDeclaringReferences)
						{
							if (item.SyntaxTree == diagnostic.Location.SourceTree)
							{
								SyntaxNode syntaxNode = getTopMostNodeForAnalysis(contextSymbol, item, compilation);
								if (diagnostic.Location.SourceSpan.IntersectsWith(syntaxNode.FullSpan))
								{
									addCategorizedLocalDiagnosticOpt(diagnostic, analyzer, arg3: false);
									return;
								}
							}
						}
					}
					addCategorizedNonLocalDiagnosticOpt(diagnostic, analyzer);
				}
			}
		};
	}

	private Action<Diagnostic> GetAddCompilationDiagnostic(DiagnosticAnalyzer analyzer)
	{
		return (Diagnostic diagnostic) =>
		{
			if (!_shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, _compilation, _cancellationToken))
			{
				if (_addCategorizedNonLocalDiagnosticOpt == null)
				{
					_addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else
				{
					_addCategorizedNonLocalDiagnosticOpt(diagnostic, analyzer);
				}
			}
		};
	}

	private Action<Diagnostic> GetAddDiagnostic(SyntaxTree tree, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
	{
		return GetAddDiagnostic(tree, null, _compilation, analyzer, isSyntaxDiagnostic, _addNonCategorizedDiagnosticOpt, _addCategorizedLocalDiagnosticOpt, _addCategorizedNonLocalDiagnosticOpt, _shouldSuppressGeneratedCodeDiagnostic, _cancellationToken);
	}

	private Action<Diagnostic> GetAddDiagnostic(SyntaxTree tree, TextSpan? span, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
	{
		return GetAddDiagnostic(tree, span, _compilation, analyzer, isSyntaxDiagnostic: false, _addNonCategorizedDiagnosticOpt, _addCategorizedLocalDiagnosticOpt, _addCategorizedNonLocalDiagnosticOpt, _shouldSuppressGeneratedCodeDiagnostic, _cancellationToken);
	}

	private static Action<Diagnostic> GetAddDiagnostic(SyntaxTree contextTree, TextSpan? span, Compilation compilation, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, CancellationToken cancellationToken)
	{
		return (Diagnostic diagnostic) =>
		{
			if (!shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, compilation, cancellationToken))
			{
				if (addCategorizedLocalDiagnosticOpt == null)
				{
					addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else if (diagnostic.Location.IsInSource && contextTree == diagnostic.Location.SourceTree && (!span.HasValue || span.Value.IntersectsWith(diagnostic.Location.SourceSpan)))
				{
					addCategorizedLocalDiagnosticOpt(diagnostic, analyzer, isSyntaxDiagnostic);
				}
				else
				{
					addCategorizedNonLocalDiagnosticOpt(diagnostic, analyzer);
				}
			}
		};
	}

	private static bool ShouldExecuteAction(AnalysisState.AnalyzerStateData analyzerStateOpt, AnalyzerAction action)
	{
		if (analyzerStateOpt != null)
		{
			return !analyzerStateOpt.ProcessedActions.Contains(action);
		}
		return true;
	}

	private bool ShouldExecuteNode(AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt, SyntaxNode node, DiagnosticAnalyzer analyzer)
	{
		if (analyzerStateOpt != null && analyzerStateOpt.ProcessedNodes.Contains(node))
		{
			return false;
		}
		if (_shouldSkipAnalysisOnGeneratedCode(analyzer) && _isGeneratedCodeLocation(node.SyntaxTree, node.Span))
		{
			return false;
		}
		return true;
	}

	private bool ShouldExecuteOperation(AnalysisState.OperationAnalyzerStateData analyzerStateOpt, IOperation operation, DiagnosticAnalyzer analyzer)
	{
		if (analyzerStateOpt != null && analyzerStateOpt.ProcessedOperations.Contains(operation))
		{
			return false;
		}
		if (operation.Syntax != null && _shouldSkipAnalysisOnGeneratedCode(analyzer) && _isGeneratedCodeLocation(operation.Syntax.SyntaxTree, operation.Syntax.Span))
		{
			return false;
		}
		return true;
	}

	private static void SetCurrentNode(AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt, SyntaxNode node)
	{
		if (analyzerStateOpt != null)
		{
			analyzerStateOpt.CurrentNode = node;
		}
	}

	private static void SetCurrentOperation(AnalysisState.OperationAnalyzerStateData analyzerStateOpt, IOperation operation)
	{
		if (analyzerStateOpt != null)
		{
			analyzerStateOpt.CurrentOperation = operation;
		}
	}

	private static bool TryStartProcessingEvent(CompilationEvent nonSymbolCompilationEvent, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartProcessingEvent(nonSymbolCompilationEvent, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartSyntaxAnalysis(SyntaxTree tree, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartSyntaxAnalysis(tree, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingSymbol(ISymbol symbol, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartAnalyzingSymbol(symbol, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingDeclaration(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt)
	{
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartAnalyzingDeclaration(symbol, declarationIndex, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingOperationReference(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		analyzerStateOpt = null;
		if (analysisStateOpt == null)
		{
			return true;
		}
		if (analysisStateOpt.TryStartAnalyzingDeclaration(symbol, declarationIndex, analyzer, out var state))
		{
			analyzerStateOpt = state.OperationBlockAnalysisState.ExecutableNodesAnalysisState;
			return true;
		}
		analyzerStateOpt = null;
		return false;
	}

	private static bool IsEventComplete(CompilationEvent compilationEvent, DiagnosticAnalyzer analyzer, AnalysisState analysisStateOpt)
	{
		return analysisStateOpt?.IsEventComplete(compilationEvent, analyzer) ?? true;
	}

	private static bool IsDeclarationComplete(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisState analysisStateOpt)
	{
		return analysisStateOpt?.IsDeclarationComplete(symbol, declarationIndex, analyzer) ?? true;
	}

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		if (!_analyzerExecutionTimeMapOpt.TryRemove(analyzer, out var value))
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.FromTicks(value.Value);
	}

	private ControlFlowGraph GetControlFlowGraph(IOperation operation)
	{
		if (_lazyControlFlowGraphMap == null)
		{
			Interlocked.CompareExchange(ref _lazyControlFlowGraphMap, new ConcurrentDictionary<IOperation, ControlFlowGraph>(), null);
		}
		return _lazyControlFlowGraphMap.GetOrAdd(operation, (IOperation op) => ControlFlowGraphBuilder.Create(op, null, null, default(ControlFlowGraphBuilder.Context)));
	}
}
