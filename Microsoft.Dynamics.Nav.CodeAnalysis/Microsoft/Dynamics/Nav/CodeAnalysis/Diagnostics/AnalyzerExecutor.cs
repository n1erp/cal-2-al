using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalyzerExecutor
{
	private const string DiagnosticCategory = "Compiler";

	internal const string AnalyzerExceptionDiagnosticId = "AD0001";

	internal const string AnalyzerDriverExceptionDiagnosticId = "AD0002";

	private readonly Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt;

	private readonly Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt;

	private readonly Action<Diagnostic> addNonCategorizedDiagnosticOpt;

	private readonly Func<Exception, bool> analyzerExceptionFilter;

	private readonly ConcurrentDictionary<DiagnosticAnalyzer, TimeSpan> analyzerExecutionTimeMapOpt;

	private readonly AnalyzerManager analyzerManager;

	private readonly Func<DiagnosticAnalyzer, object> getAnalyzerGateOpt;

	private readonly Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer;

	private readonly Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode;

	private readonly Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic;

	internal Compilation Compilation { get; }

	internal AnalyzerOptions AnalyzerOptions { get; }

	internal CancellationToken CancellationToken { get; }

	internal Action<Exception, DiagnosticAnalyzer, Diagnostic> OnAnalyzerException { get; }

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => analyzerExecutionTimeMapOpt.ToImmutableDictionary();

	private AnalyzerExecutor(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool> analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<DiagnosticAnalyzer, object> getAnalyzerGateOpt, ConcurrentDictionary<DiagnosticAnalyzer, TimeSpan> analyzerExecutionTimeMapOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, CancellationToken cancellationToken)
	{
		Compilation = compilation;
		AnalyzerOptions = analyzerOptions;
		this.addNonCategorizedDiagnosticOpt = addNonCategorizedDiagnosticOpt;
		OnAnalyzerException = onAnalyzerException;
		this.analyzerExceptionFilter = analyzerExceptionFilter;
		this.isCompilerAnalyzer = isCompilerAnalyzer;
		this.analyzerManager = analyzerManager;
		this.shouldSkipAnalysisOnGeneratedCode = shouldSkipAnalysisOnGeneratedCode;
		this.shouldSuppressGeneratedCodeDiagnostic = shouldSuppressGeneratedCodeDiagnostic;
		this.getAnalyzerGateOpt = getAnalyzerGateOpt;
		this.analyzerExecutionTimeMapOpt = analyzerExecutionTimeMapOpt;
		this.addCategorizedLocalDiagnosticOpt = addCategorizedLocalDiagnosticOpt;
		this.addCategorizedNonLocalDiagnosticOpt = addCategorizedNonLocalDiagnosticOpt;
		CancellationToken = cancellationToken;
	}

	public static AnalyzerExecutor Create(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool> analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<DiagnosticAnalyzer, object> getAnalyzerGate, bool logExecutionTime = false, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt = null, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		DebugAssertHelper.Assert((addNonCategorizedDiagnosticOpt != null) ^ (addCategorizedLocalDiagnosticOpt != null));
		DebugAssertHelper.Assert(addCategorizedLocalDiagnosticOpt != null == (addCategorizedNonLocalDiagnosticOpt != null));
		ConcurrentDictionary<DiagnosticAnalyzer, TimeSpan> concurrentDictionary = (logExecutionTime ? new ConcurrentDictionary<DiagnosticAnalyzer, TimeSpan>() : null);
		return new AnalyzerExecutor(compilation, analyzerOptions, addNonCategorizedDiagnosticOpt, onAnalyzerException, analyzerExceptionFilter, isCompilerAnalyzer, analyzerManager, shouldSkipAnalysisOnGeneratedCode, shouldSuppressGeneratedCodeDiagnostic, getAnalyzerGate, concurrentDictionary, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, cancellationToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "logExecutionTime")]
	public static AnalyzerExecutor CreateForSupportedDiagnostics(Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, AnalyzerManager analyzerManager, bool logExecutionTime = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new AnalyzerExecutor(null, null, null, onAnalyzerException, null, null, analyzerManager, (DiagnosticAnalyzer _) => false, (Diagnostic diagnostic, DiagnosticAnalyzer analyzer, Compilation compilation, CancellationToken ct) => false, null, null, null, null, cancellationToken);
	}

	public AnalyzerExecutor WithCancellationToken(CancellationToken cancellationToken)
	{
		if (cancellationToken == CancellationToken)
		{
			return this;
		}
		return new AnalyzerExecutor(Compilation, AnalyzerOptions, addNonCategorizedDiagnosticOpt, OnAnalyzerException, analyzerExceptionFilter, isCompilerAnalyzer, analyzerManager, shouldSkipAnalysisOnGeneratedCode, shouldSuppressGeneratedCodeDiagnostic, getAnalyzerGateOpt, analyzerExecutionTimeMapOpt, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, cancellationToken);
	}

	public void ExecuteInitializeMethod(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope sessionScope)
	{
		ExecuteAndCatchIfThrows(analyzer, () =>
		{
			analyzer.Initialize(new AnalyzerAnalysisContext(analyzer, sessionScope));
		});
	}

	public void ExecuteCompilationStartActions(ImmutableArray<CompilationStartAnalyzerAction> actions, HostCompilationStartAnalysisScope compilationScope)
	{
		foreach (CompilationStartAnalyzerAction startAction in actions)
		{
			CancellationToken.ThrowIfCancellationRequested();
			ExecuteAndCatchIfThrows(startAction.Analyzer, () =>
			{
				startAction.Action(new AnalyzerCompilationStartAnalysisContext(startAction.Analyzer, compilationScope, Compilation, AnalyzerOptions, CancellationToken));
			}, new AnalysisContextInfo(Compilation));
		}
	}

	public void ExecuteCompilationActions(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, CompilationEvent compilationEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		DebugAssertHelper.Assert(compilationEvent is CompilationStartedEvent || compilationEvent is CompilationCompletedEvent);
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartProcessingEvent(compilationEvent, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteCompilationActionsCore(compilationActions, analyzer, analyzerStateOpt);
				analysisStateOpt?.MarkEventComplete(compilationEvent, analyzer);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteSymbolActions(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCodeSymbol)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			ISymbol symbol = symbolDeclaredEvent.Symbol;
			if (TryStartAnalyzingSymbol(symbol, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSymbolActionsCore(symbolActions, analyzer, symbolDeclaredEvent, getTopMostNodeForAnalysis, analyzerStateOpt, isGeneratedCodeSymbol);
				analysisStateOpt?.MarkSymbolComplete(symbol, analyzer);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteSemanticModelActions(ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions, DiagnosticAnalyzer analyzer, SemanticModel semanticModel, CompilationEvent compilationUnitCompletedEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartProcessingEvent(compilationUnitCompletedEvent, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSemanticModelActionsCore(semanticModelActions, analyzer, semanticModel, analyzerStateOpt, isGeneratedCode);
				analysisStateOpt?.MarkEventComplete(compilationUnitCompletedEvent, analyzer);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteSyntaxTreeActions(ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions, DiagnosticAnalyzer analyzer, SyntaxTree tree, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartSyntaxAnalysis(tree, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSyntaxTreeActionsCore(syntaxTreeActions, analyzer, tree, analyzerStateOpt, isGeneratedCode);
				analysisStateOpt?.MarkSyntaxAnalysisComplete(tree, analyzer);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteOperationBlockActions(IEnumerable<OperationBlockStartAnalyzerAction> operationBlockStartActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockEndActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, SemanticModel semanticModel, int declarationIndex, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingSyntaxRefence(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteBlockActionsCore<OperationBlockStartAnalyzerAction, OperationBlockAnalyzerAction, OperationAnalyzerAction, AnalysisState.OperationAnalyzerStateData, IOperation>(operationBlockStartActions, operationBlockActions, operationBlockEndActions, analyzer, declaredNode, declaredSymbol, operationBlocks, (ImmutableArray<IOperation> blocks) => operations, semanticModel, null, analyzerStateOpt?.OperationBlockAnalysisState);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteSyntaxNodeActions(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, Func<SyntaxNode, SyntaxKind> getKind, TextSpan filterSpan, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingSyntaxRefence(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSyntaxNodeActionsCore(nodesToAnalyze, nodeActionsByKind, analyzer, declaredSymbol, model, getKind, filterSpan, analyzerStateOpt);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public void ExecuteOperationActions(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, TextSpan filterSpan, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		AnalysisState.OperationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingOperationReference(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteOperationActionsCore(operationsToAnalyze, operationActionsByKind, analyzer, declaredSymbol, model, filterSpan, analyzerStateOpt);
			}
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	internal static ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> GetNodeActionsByKind(IEnumerable<SyntaxNodeAnalyzerAction> nodeActions)
	{
		DebugAssertHelper.Assert(nodeActions?.Any() ?? false);
		PooledDictionary<SyntaxKind, ArrayBuilder<SyntaxNodeAnalyzerAction>> instance = PooledDictionary<SyntaxKind, ArrayBuilder<SyntaxNodeAnalyzerAction>>.GetInstance();
		foreach (SyntaxNodeAnalyzerAction nodeAction in nodeActions)
		{
			foreach (SyntaxKind kind in nodeAction.Kinds)
			{
				if (!instance.TryGetValue(kind, out var value))
				{
					instance.Add(kind, value = ArrayBuilder<SyntaxNodeAnalyzerAction>.GetInstance());
				}
				value.Add(nodeAction);
			}
		}
		ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> result = ImmutableDictionary.CreateRange(instance.Select((KeyValuePair<SyntaxKind, ArrayBuilder<SyntaxNodeAnalyzerAction>> kvp) => KeyValuePair.Create(kvp.Key, kvp.Value.ToImmutableAndFree())));
		instance.Free();
		return result;
	}

	internal static ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> GetOperationActionsByKind(IEnumerable<OperationAnalyzerAction> operationActions)
	{
		DebugAssertHelper.Assert(operationActions?.Any() ?? false);
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
		ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> result = ImmutableDictionary.CreateRange(instance.Select((KeyValuePair<OperationKind, ArrayBuilder<OperationAnalyzerAction>> kvp) => KeyValuePair.Create(kvp.Key, kvp.Value.ToImmutableAndFree())));
		instance.Free();
		return result;
	}

	internal static bool CanHaveExecutableCodeBlock(ISymbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.Module || (uint)(kind - 10) <= 2u || kind == SymbolKind.Method)
		{
			return true;
		}
		return false;
	}

	internal void ExecuteAndCatchIfThrows(DiagnosticAnalyzer analyzer, Action analyze, AnalysisContextInfo? info = null)
	{
		object obj = getAnalyzerGateOpt?.Invoke(analyzer);
		if (obj != null)
		{
			lock (obj)
			{
				ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, info);
				return;
			}
		}
		ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, info);
	}

	internal bool ExceptionFilter(Exception ex)
	{
		if ((ex as OperationCanceledException)?.CancellationToken == CancellationToken)
		{
			return false;
		}
		if (analyzerExceptionFilter != null)
		{
			return analyzerExceptionFilter(ex);
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
			e.GetType().ToString(),
			e.ToString()
		};
		string description = string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.CompilerAnalyzerThrowsDescription, text, CreateDiagnosticDescription(info, e));
		DiagnosticDescriptor analyzerExceptionDiagnosticDescriptor = GetAnalyzerExceptionDiagnosticDescriptor("AD0001", compilerAnalyzerFailure, description, compilerAnalyzerThrows);
		Location none = Location.None;
		object[] messageArgs = array;
		return Diagnostic.Create(analyzerExceptionDiagnosticDescriptor, none, messageArgs);
	}

	internal static Diagnostic CreateDriverExceptionDiagnostic(Exception e)
	{
		string analyzerDriverFailure = CodeAnalysisResources.AnalyzerDriverFailure;
		string analyzerDriverThrows = CodeAnalysisResources.AnalyzerDriverThrows;
		string[] array = new string[2]
		{
			e.GetType().ToString(),
			e.ToString()
		};
		string description = string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.AnalyzerDriverThrowsDescription, e.CreateDiagnosticDescription());
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

	private void ExecuteCompilationActionsCore(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		Action<Diagnostic> addDiagnostic = GetAddCompilationDiagnostic(analyzer);
		foreach (CompilationAnalyzerAction endAction in compilationActions)
		{
			CancellationToken.ThrowIfCancellationRequested();
			if (!ShouldExecuteAction(analyzerStateOpt, endAction))
			{
				continue;
			}
			ExecuteAndCatchIfThrows(endAction.Analyzer, () =>
			{
				endAction.Action(new CompilationAnalysisContext(Compilation, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(endAction.Analyzer, d), CancellationToken));
			}, new AnalysisContextInfo(Compilation));
			analyzerStateOpt?.ProcessedActions.Add(endAction);
		}
	}

	private void ExecuteSymbolActionsCore(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCodeSymbol)
	{
		DebugAssertHelper.Assert(getTopMostNodeForAnalysis != null);
		if (isGeneratedCodeSymbol && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		ISymbol symbol = symbolDeclaredEvent.Symbol;
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(symbol, symbolDeclaredEvent.DeclaringSyntaxReferences, analyzer, getTopMostNodeForAnalysis);
		foreach (SymbolAnalyzerAction symbolAction in symbolActions)
		{
			Action<SymbolAnalysisContext> action = symbolAction.Action;
			if (!symbolAction.Kinds.Contains(symbol.Kind) || !ShouldExecuteAction(analyzerStateOpt, symbolAction))
			{
				continue;
			}
			CancellationToken.ThrowIfCancellationRequested();
			ExecuteAndCatchIfThrows(symbolAction.Analyzer, () =>
			{
				action(new SymbolAnalysisContext(symbol, Compilation, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(symbolAction.Analyzer, d), CancellationToken));
			}, new AnalysisContextInfo(Compilation, symbol));
			analyzerStateOpt?.ProcessedActions.Add(symbolAction);
		}
	}

	private void ExecuteSemanticModelActionsCore(ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions, DiagnosticAnalyzer analyzer, SemanticModel semanticModel, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(semanticModel.SyntaxTree, analyzer, isSyntaxDiagnostic: false);
		foreach (SemanticModelAnalyzerAction semanticModelAction in semanticModelActions)
		{
			if (!ShouldExecuteAction(analyzerStateOpt, semanticModelAction))
			{
				continue;
			}
			CancellationToken.ThrowIfCancellationRequested();
			ExecuteAndCatchIfThrows(semanticModelAction.Analyzer, () =>
			{
				semanticModelAction.Action(new SemanticModelAnalysisContext(semanticModel, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(semanticModelAction.Analyzer, d), CancellationToken));
			}, new AnalysisContextInfo(semanticModel));
			analyzerStateOpt?.ProcessedActions.Add(semanticModelAction);
		}
	}

	private void ExecuteSyntaxTreeActionsCore(ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions, DiagnosticAnalyzer analyzer, SyntaxTree tree, AnalysisState.AnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		if (isGeneratedCode && shouldSkipAnalysisOnGeneratedCode(analyzer))
		{
			return;
		}
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(tree, analyzer, isSyntaxDiagnostic: true);
		foreach (SyntaxTreeAnalyzerAction syntaxTreeAction in syntaxTreeActions)
		{
			if (!ShouldExecuteAction(analyzerStateOpt, syntaxTreeAction))
			{
				continue;
			}
			CancellationToken.ThrowIfCancellationRequested();
			ExecuteAndCatchIfThrows(syntaxTreeAction.Analyzer, () =>
			{
				syntaxTreeAction.Action(new SyntaxTreeAnalysisContext(tree, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(syntaxTreeAction.Analyzer, d), Compilation, CancellationToken));
			}, new AnalysisContextInfo(Compilation, tree));
			analyzerStateOpt?.ProcessedActions.Add(syntaxTreeAction);
		}
	}

	private void ExecuteSyntaxNodeAction(SyntaxNodeAnalyzerAction syntaxNodeAction, SyntaxNode node, ISymbol containingSymbol, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analyzerStateOpt == null || analyzerStateOpt.CurrentNode == node);
		if (ShouldExecuteAction(analyzerStateOpt, syntaxNodeAction))
		{
			SyntaxNodeAnalysisContext syntaxNodeContext = new SyntaxNodeAnalysisContext(node, containingSymbol, semanticModel, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(syntaxNodeAction.Analyzer, d), CancellationToken);
			ExecuteAndCatchIfThrows(syntaxNodeAction.Analyzer, () =>
			{
				syntaxNodeAction.Action(syntaxNodeContext);
			}, new AnalysisContextInfo(Compilation, node));
			analyzerStateOpt?.ProcessedActions.Add(syntaxNodeAction);
		}
	}

	private void ExecuteOperationAction(OperationAnalyzerAction operationAction, IOperation operation, ISymbol containingSymbol, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analyzerStateOpt == null || analyzerStateOpt.CurrentOperation == operation);
		if (ShouldExecuteAction(analyzerStateOpt, operationAction))
		{
			OperationAnalysisContext operationContext = new OperationAnalysisContext(operation, containingSymbol, semanticModel.Compilation, AnalyzerOptions, addDiagnostic, (Diagnostic d) => IsSupportedDiagnostic(operationAction.Analyzer, d), CancellationToken);
			ExecuteAndCatchIfThrows(operationAction.Analyzer, () =>
			{
				operationAction.Action(operationContext);
			}, new AnalysisContextInfo(Compilation, operation));
			analyzerStateOpt?.ProcessedActions.Add(operationAction);
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private void ExecuteBlockActionsCore<TBlockStartAction, TBlockAction, TNodeAction, TNodeStateData, TNode>(IEnumerable<TBlockStartAction> startActions, IEnumerable<TBlockAction> actions, IEnumerable<TBlockAction> endActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<TNode> executableBlocks, Func<ImmutableArray<TNode>, IEnumerable<TNode>> getNodesToAnalyze, SemanticModel semanticModel, Func<SyntaxNode, SyntaxKind> getKind, AnalysisState.BlockAnalyzerStateData<TBlockAction, TNodeStateData> analyzerStateOpt) where TBlockStartAction : AnalyzerAction where TBlockAction : AnalyzerAction where TNodeAction : AnalyzerAction where TNodeStateData : AnalysisState.AnalyzerStateData, new()
	{
		DebugAssertHelper.Assert(declaredNode != null);
		DebugAssertHelper.Assert(declaredSymbol != null);
		DebugAssertHelper.Assert(CanHaveExecutableCodeBlock(declaredSymbol));
		DebugAssertHelper.Assert(startActions.Any() || endActions.Any() || actions.Any());
		DebugAssertHelper.Assert(!executableBlocks.IsEmpty);
		PooledHashSet<TBlockAction> instance = PooledHashSet<TBlockAction>.GetInstance();
		PooledHashSet<TBlockAction> instance2 = PooledHashSet<TBlockAction>.GetInstance();
		ArrayBuilder<TNodeAction> instance3 = ArrayBuilder<TNodeAction>.GetInstance();
		ArrayBuilder<SyntaxNodeAnalyzerAction> syntaxNodeActions = instance3 as ArrayBuilder<SyntaxNodeAnalyzerAction>;
		ArrayBuilder<OperationAnalyzerAction> operationActions = instance3 as ArrayBuilder<OperationAnalyzerAction>;
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
		try
		{
			foreach (TBlockStartAction startAction in startActions)
			{
				if (!ShouldExecuteAction(analyzerStateOpt, startAction))
				{
					continue;
				}
				CodeBlockStartAnalyzerAction codeBlockStartAction = startAction as CodeBlockStartAnalyzerAction;
				if (codeBlockStartAction != null)
				{
					PooledHashSet<CodeBlockAnalyzerAction> codeBlockEndActions = instance as PooledHashSet<CodeBlockAnalyzerAction>;
					ExecuteAndCatchIfThrows(startAction.Analyzer, () =>
					{
						HostCodeBlockStartAnalysisScope hostCodeBlockStartAnalysisScope = new HostCodeBlockStartAnalysisScope();
						AnalyzerCodeBlockStartAnalysisContext obj = new AnalyzerCodeBlockStartAnalysisContext(startAction.Analyzer, hostCodeBlockStartAnalysisScope, declaredNode, declaredSymbol, semanticModel, AnalyzerOptions, CancellationToken);
						codeBlockStartAction.Action(obj);
						codeBlockEndActions.AddAll(hostCodeBlockStartAnalysisScope.CodeBlockEndActions);
						syntaxNodeActions.AddRange(hostCodeBlockStartAnalysisScope.SyntaxNodeActions);
					}, new AnalysisContextInfo(Compilation, declaredSymbol, declaredNode));
				}
				else
				{
					OperationBlockStartAnalyzerAction operationBlockStartAction = startAction as OperationBlockStartAnalyzerAction;
					if (operationBlockStartAction != null)
					{
						PooledHashSet<OperationBlockAnalyzerAction> operationBlockEndActions = instance as PooledHashSet<OperationBlockAnalyzerAction>;
						ExecuteAndCatchIfThrows(startAction.Analyzer, () =>
						{
							HostOperationBlockStartAnalysisScope hostOperationBlockStartAnalysisScope = new HostOperationBlockStartAnalysisScope();
							AnalyzerOperationBlockStartAnalysisContext obj = new AnalyzerOperationBlockStartAnalysisContext(startAction.Analyzer, hostOperationBlockStartAnalysisScope, operationBlocks, declaredSymbol, semanticModel.Compilation, AnalyzerOptions, CancellationToken);
							operationBlockStartAction.Action(obj);
							operationBlockEndActions.AddAll(hostOperationBlockStartAnalysisScope.OperationBlockEndActions);
							operationActions.AddRange(hostOperationBlockStartAnalysisScope.OperationActions);
						}, new AnalysisContextInfo(Compilation, declaredSymbol));
					}
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
			if (syntaxNodeActions != null)
			{
				ImmutableDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind = GetNodeActionsByKind(syntaxNodeActions);
				IEnumerable<SyntaxNode> nodesToAnalyze = (IEnumerable<SyntaxNode>)getNodesToAnalyze(executableBlocks);
				ExecuteSyntaxNodeActions(nodesToAnalyze, nodeActionsByKind, declaredSymbol, semanticModel, getKind, addDiagnostic, ((analyzerStateOpt != null) ? analyzerStateOpt.ExecutableNodesAnalysisState : null) as AnalysisState.SyntaxNodeAnalyzerStateData);
			}
			else if (operationActions != null)
			{
				ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind = GetOperationActionsByKind(operationActions);
				IEnumerable<IOperation> operationsToAnalyze = (IEnumerable<IOperation>)getNodesToAnalyze(executableBlocks);
				ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, declaredSymbol, semanticModel, addDiagnostic, ((analyzerStateOpt != null) ? analyzerStateOpt.ExecutableNodesAnalysisState : null) as AnalysisState.OperationAnalyzerStateData);
			}
		}
		instance3.Free();
		ExecuteBlockActions(instance2, declaredNode, declaredSymbol, semanticModel, operationBlocks, addDiagnostic, analyzerStateOpt);
		ExecuteBlockActions(instance, declaredNode, declaredSymbol, semanticModel, operationBlocks, addDiagnostic, analyzerStateOpt);
	}

	private void ExecuteBlockActions<TBlockAction, TNodeStateData>(PooledHashSet<TBlockAction> blockActions, SyntaxNode declaredNode, ISymbol declaredSymbol, SemanticModel semanticModel, ImmutableArray<IOperation> operationBlocks, Action<Diagnostic> addDiagnostic, AnalysisState.BlockAnalyzerStateData<TBlockAction, TNodeStateData> analyzerStateOpt) where TBlockAction : AnalyzerAction where TNodeStateData : AnalysisState.AnalyzerStateData, new()
	{
		foreach (TBlockAction blockAction in blockActions)
		{
			if (!ShouldExecuteAction(analyzerStateOpt, blockAction))
			{
				continue;
			}
			CodeBlockAnalyzerAction codeBlockAction = blockAction as CodeBlockAnalyzerAction;
			Func<Diagnostic, bool> isSupportedDiagnostic = (Diagnostic d) => IsSupportedDiagnostic(blockAction.Analyzer, d);
			if (codeBlockAction != null)
			{
				ExecuteAndCatchIfThrows(codeBlockAction.Analyzer, () =>
				{
					codeBlockAction.Action(new CodeBlockAnalysisContext(declaredNode, declaredSymbol, semanticModel, AnalyzerOptions, addDiagnostic, isSupportedDiagnostic, CancellationToken));
				}, new AnalysisContextInfo(Compilation, declaredSymbol, declaredNode));
			}
			else
			{
				OperationBlockAnalyzerAction operationBlockAction = blockAction as OperationBlockAnalyzerAction;
				if (operationBlockAction != null)
				{
					ExecuteAndCatchIfThrows(operationBlockAction.Analyzer, () =>
					{
						operationBlockAction.Action(new OperationBlockAnalysisContext(operationBlocks, declaredSymbol, semanticModel.Compilation, AnalyzerOptions, addDiagnostic, isSupportedDiagnostic, CancellationToken));
					}, new AnalysisContextInfo(Compilation, declaredSymbol));
				}
			}
			analyzerStateOpt?.ProcessedActions.Add(blockAction);
		}
		blockActions.Free();
	}

	private void ExecuteSyntaxNodeActionsCore(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, SyntaxKind> getKind, TextSpan filterSpan, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt)
	{
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(model.SyntaxTree, filterSpan, analyzer, isSyntaxDiagnostic: false);
		ExecuteSyntaxNodeActions(nodesToAnalyze, nodeActionsByKind, containingSymbol, model, getKind, addDiagnostic, analyzerStateOpt);
	}

	private void ExecuteSyntaxNodeActions(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, SyntaxKind> getKind, Action<Diagnostic> addDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(nodeActionsByKind != null);
		DebugAssertHelper.Assert(nodeActionsByKind.Any());
		SyntaxNode syntaxNode = analyzerStateOpt?.CurrentNode;
		if (syntaxNode != null)
		{
			ExecuteSyntaxNodeActions(syntaxNode, nodeActionsByKind, containingSymbol, model, getKind, addDiagnostic, analyzerStateOpt);
		}
		foreach (SyntaxNode item in nodesToAnalyze)
		{
			if (ShouldExecuteNode(analyzerStateOpt, item))
			{
				SetCurrentNode(analyzerStateOpt, item);
				ExecuteSyntaxNodeActions(item, nodeActionsByKind, containingSymbol, model, getKind, addDiagnostic, analyzerStateOpt);
			}
		}
	}

	private void ExecuteSyntaxNodeActions(SyntaxNode node, IDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind, ISymbol containingSymbol, SemanticModel model, Func<SyntaxNode, SyntaxKind> getKind, Action<Diagnostic> addDiagnostic, AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt)
	{
		if (nodeActionsByKind.TryGetValue(getKind(node), out var value))
		{
			foreach (SyntaxNodeAnalyzerAction item in value)
			{
				ExecuteSyntaxNodeAction(item, node, containingSymbol, model, addDiagnostic, analyzerStateOpt);
			}
		}
		analyzerStateOpt?.ClearNodeAnalysisState();
	}

	private void ExecuteOperationActionsCore(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, TextSpan filterSpan, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(model.SyntaxTree, filterSpan, analyzer, isSyntaxDiagnostic: false);
		ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, containingSymbol, model, addDiagnostic, analyzerStateOpt);
	}

	private void ExecuteOperationActions(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, ISymbol containingSymbol, SemanticModel model, Action<Diagnostic> addDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(operationActionsByKind != null);
		DebugAssertHelper.Assert(operationActionsByKind.Any());
		IOperation operation = analyzerStateOpt?.CurrentOperation;
		if (operation != null)
		{
			ExecuteOperationActions(operation, operationActionsByKind, containingSymbol, model, addDiagnostic, analyzerStateOpt);
		}
		foreach (IOperation item in operationsToAnalyze)
		{
			if (ShouldExecuteOperation(analyzerStateOpt, item))
			{
				SetCurrentOperation(analyzerStateOpt, item);
				ExecuteOperationActions(item, operationActionsByKind, containingSymbol, model, addDiagnostic, analyzerStateOpt);
			}
		}
	}

	private void ExecuteOperationActions(IOperation operation, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, ISymbol containingSymbol, SemanticModel model, Action<Diagnostic> addDiagnostic, AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		if (operationActionsByKind.TryGetValue(operation.Kind, out var value))
		{
			foreach (OperationAnalyzerAction item in value)
			{
				ExecuteOperationAction(item, operation, containingSymbol, model, addDiagnostic, analyzerStateOpt);
			}
		}
		analyzerStateOpt?.ClearNodeAnalysisState();
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private void ExecuteAndCatchIfThrows_NoLock(DiagnosticAnalyzer analyzer, Action analyze, AnalysisContextInfo? info)
	{
		try
		{
			Stopwatch timer = null;
			if (analyzerExecutionTimeMapOpt != null)
			{
				timer = Stopwatch.StartNew();
			}
			analyze();
			if (timer != null)
			{
				timer.Stop();
				analyzerExecutionTimeMapOpt.AddOrUpdate(analyzer, timer.Elapsed, (DiagnosticAnalyzer a, TimeSpan accumulatedTime) => accumulatedTime + timer.Elapsed);
			}
		}
		catch (Exception ex) when (ExceptionFilter(ex))
		{
			Diagnostic arg = CreateAnalyzerExceptionDiagnostic(analyzer, ex, info);
			try
			{
				OnAnalyzerException(ex, analyzer, arg);
			}
			catch (Exception)
			{
			}
		}
	}

	private static string CreateDiagnosticDescription(AnalysisContextInfo? info, Exception e)
	{
		if (!info.HasValue)
		{
			return e.CreateDiagnosticDescription();
		}
		return string.Join(Environment.NewLine, string.Format(CultureInfo.CurrentUICulture, CodeAnalysisResources.ExceptionContext, info?.GetContext()), e.CreateDiagnosticDescription());
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
		DebugAssertHelper.Assert(IsAnalyzerExceptionDiagnostic(exceptionDiagnostic));
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
		DebugAssertHelper.Assert(isCompilerAnalyzer != null);
		if (diagnostic is DiagnosticWithInfo)
		{
			return true;
		}
		return analyzerManager.IsSupportedDiagnostic(analyzer, diagnostic, isCompilerAnalyzer, this);
	}

	private Action<Diagnostic> GetAddDiagnostic(ISymbol contextSymbol, ImmutableArray<SyntaxReference> cachedDeclaringReferences, DiagnosticAnalyzer analyzer, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis)
	{
		return GetAddDiagnostic(contextSymbol, cachedDeclaringReferences, Compilation, analyzer, addNonCategorizedDiagnosticOpt, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, getTopMostNodeForAnalysis, shouldSuppressGeneratedCodeDiagnostic, CancellationToken);
	}

	private static Action<Diagnostic> GetAddDiagnostic(ISymbol contextSymbol, ImmutableArray<SyntaxReference> cachedDeclaringReferences, Compilation compilation, DiagnosticAnalyzer analyzer, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, CancellationToken cancellationToken)
	{
		return (Diagnostic diagnostic) =>
		{
			if (!shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, compilation, cancellationToken))
			{
				if (addCategorizedLocalDiagnosticOpt == null)
				{
					DebugAssertHelper.Assert(addNonCategorizedDiagnosticOpt != null);
					addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else
				{
					DebugAssertHelper.Assert(addNonCategorizedDiagnosticOpt == null);
					DebugAssertHelper.Assert(addCategorizedNonLocalDiagnosticOpt != null);
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
			if (!shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, Compilation, CancellationToken))
			{
				if (addCategorizedNonLocalDiagnosticOpt == null)
				{
					DebugAssertHelper.Assert(addNonCategorizedDiagnosticOpt != null);
					addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else
				{
					addCategorizedNonLocalDiagnosticOpt(diagnostic, analyzer);
				}
			}
		};
	}

	private Action<Diagnostic> GetAddDiagnostic(SyntaxTree tree, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
	{
		return GetAddDiagnostic(tree, null, Compilation, analyzer, isSyntaxDiagnostic, addNonCategorizedDiagnosticOpt, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, shouldSuppressGeneratedCodeDiagnostic, CancellationToken);
	}

	private Action<Diagnostic> GetAddDiagnostic(SyntaxTree tree, TextSpan? span, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
	{
		return GetAddDiagnostic(tree, span, Compilation, analyzer, isSyntaxDiagnostic: false, addNonCategorizedDiagnosticOpt, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, shouldSuppressGeneratedCodeDiagnostic, CancellationToken);
	}

	private static Action<Diagnostic> GetAddDiagnostic(SyntaxTree contextTree, TextSpan? span, Compilation compilation, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic, Action<Diagnostic> addNonCategorizedDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt, Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, CancellationToken cancellationToken)
	{
		return (Diagnostic diagnostic) =>
		{
			if (!shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, compilation, cancellationToken))
			{
				if (addCategorizedLocalDiagnosticOpt == null)
				{
					DebugAssertHelper.Assert(addNonCategorizedDiagnosticOpt != null);
					addNonCategorizedDiagnosticOpt(diagnostic);
				}
				else
				{
					DebugAssertHelper.Assert(addNonCategorizedDiagnosticOpt == null);
					DebugAssertHelper.Assert(addCategorizedNonLocalDiagnosticOpt != null);
					if (diagnostic.Location.IsInSource && contextTree == diagnostic.Location.SourceTree && (!span.HasValue || span.Value.IntersectsWith(diagnostic.Location.SourceSpan)))
					{
						addCategorizedLocalDiagnosticOpt(diagnostic, analyzer, isSyntaxDiagnostic);
					}
					else
					{
						addCategorizedNonLocalDiagnosticOpt(diagnostic, analyzer);
					}
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

	private static bool ShouldExecuteNode(AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt, SyntaxNode node)
	{
		if (analyzerStateOpt != null)
		{
			return !analyzerStateOpt.ProcessedNodes.Contains(node);
		}
		return true;
	}

	private static bool ShouldExecuteOperation(AnalysisState.OperationAnalyzerStateData analyzerStateOpt, IOperation operation)
	{
		if (analyzerStateOpt != null)
		{
			return !analyzerStateOpt.ProcessedOperations.Contains(operation);
		}
		return true;
	}

	private static void SetCurrentNode(AnalysisState.SyntaxNodeAnalyzerStateData analyzerStateOpt, SyntaxNode node)
	{
		if (analyzerStateOpt != null)
		{
			DebugAssertHelper.Assert(node != null);
			analyzerStateOpt.CurrentNode = node;
		}
	}

	private static void SetCurrentOperation(AnalysisState.OperationAnalyzerStateData analyzerStateOpt, IOperation operation)
	{
		if (analyzerStateOpt != null)
		{
			DebugAssertHelper.Assert(operation != null);
			analyzerStateOpt.CurrentOperation = operation;
		}
	}

	private static bool TryStartProcessingEvent(CompilationEvent nonSymbolCompilationEvent, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(!(nonSymbolCompilationEvent is SymbolDeclaredCompilationEvent));
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartProcessingEvent(nonSymbolCompilationEvent, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartSyntaxAnalysis(SyntaxTree tree, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartSyntaxAnalysis(tree, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingSymbol(ISymbol symbol, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.AnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartAnalyzingSymbol(symbol, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingSyntaxRefence(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartAnalyzingDeclaration(symbol, declarationIndex, analyzer, out analyzerStateOpt) ?? true;
	}

	private static bool TryStartAnalyzingOperationReference(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.OperationAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
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

	public bool TryExecuteSymbolActions(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, SyntaxNode> getTopMostNodeForAnalysis, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.AnalyzerStateData analyzerStateOpt = null;
		try
		{
			ISymbol symbol = symbolDeclaredEvent.Symbol;
			if (TryStartAnalyzingSymbol(symbol, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSymbolActionsCore(symbolActions, analyzer, symbolDeclaredEvent, getTopMostNodeForAnalysis, analyzerStateOpt, isGeneratedCodeSymbol: false);
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

	public bool TryExecuteSyntaxNodeActions(IEnumerable<SyntaxNode> nodesToAnalyze, IDictionary<SyntaxKind, ImmutableArray<SyntaxNodeAnalyzerAction>> nodeActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, Func<SyntaxNode, SyntaxKind> getKind, TextSpan filterSpan, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteSyntaxNodeActionsCore(nodesToAnalyze, nodeActionsByKind, analyzer, declaredSymbol, model, getKind, filterSpan, analyzerStateOpt);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	private static bool TryStartAnalyzingDeclaration(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisScope analysisScope, AnalysisState analysisStateOpt, out AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt)
	{
		DebugAssertHelper.Assert(analysisScope.Analyzers.Contains(analyzer));
		analyzerStateOpt = null;
		return analysisStateOpt?.TryStartAnalyzingDeclaration(symbol, declarationIndex, analyzer, out analyzerStateOpt) ?? true;
	}

	public bool TryExecuteCompilationActions(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, CompilationEvent compilationEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		DebugAssertHelper.Assert(compilationEvent is CompilationStartedEvent || compilationEvent is CompilationCompletedEvent);
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

	private static bool IsEventComplete(CompilationEvent compilationEvent, DiagnosticAnalyzer analyzer, AnalysisState analysisStateOpt)
	{
		return analysisStateOpt?.IsEventComplete(compilationEvent, analyzer) ?? true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "analyzer")]
	private static bool IsDeclarationComplete(ISymbol symbol, int declarationIndex, DiagnosticAnalyzer analyzer, AnalysisState analysisStateOpt)
	{
		return analysisStateOpt?.IsDeclarationComplete(symbol, declarationIndex) ?? true;
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

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		DebugAssertHelper.Assert(analyzerExecutionTimeMapOpt != null);
		if (!analyzerExecutionTimeMapOpt.TryRemove(analyzer, out var value))
		{
			value = default;
		}
		return value;
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

	public bool TryExecuteOperationActions(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, TextSpan filterSpan, int declarationIndex, ISymbol declaredSymbol, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.OperationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingOperationReference(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteOperationActionsCore(operationsToAnalyze, operationActionsByKind, analyzer, declaredSymbol, model, filterSpan, analyzerStateOpt, isGeneratedCode: false);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isGeneratedCode")]
	private void ExecuteOperationActionsCore(IEnumerable<IOperation> operationsToAnalyze, IDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, ISymbol containingSymbol, SemanticModel model, TextSpan filterSpan, AnalysisState.OperationAnalyzerStateData analyzerStateOpt, bool isGeneratedCode)
	{
		Action<Diagnostic> addDiagnostic = GetAddDiagnostic(model.SyntaxTree, filterSpan, analyzer, isSyntaxDiagnostic: false);
		ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, containingSymbol, model, addDiagnostic, analyzerStateOpt);
	}

	public bool TryExecuteOperationBlockActions(IEnumerable<OperationBlockStartAnalyzerAction> operationBlockStartActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockActions, IEnumerable<OperationBlockAnalyzerAction> operationBlockEndActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, SemanticModel semanticModel, int declarationIndex, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteBlockActionsCore<OperationBlockStartAnalyzerAction, OperationBlockAnalyzerAction, OperationAnalyzerAction, AnalysisState.OperationAnalyzerStateData, IOperation>(operationBlockStartActions, operationBlockActions, operationBlockEndActions, analyzer, declaredNode, declaredSymbol, operationBlocks, (ImmutableArray<IOperation> blocks) => operations, semanticModel, null, analyzerStateOpt?.OperationBlockAnalysisState);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}

	public bool TryExecuteCodeBlockActions(IEnumerable<CodeBlockStartAnalyzerAction> codeBlockStartActions, IEnumerable<CodeBlockAnalyzerAction> codeBlockActions, IEnumerable<CodeBlockAnalyzerAction> codeBlockEndActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<SyntaxNode> executableCodeBlocks, SemanticModel semanticModel, Func<SyntaxNode, SyntaxKind> getKind, int declarationIndex, AnalysisScope analysisScope, AnalysisState analysisStateOpt)
	{
		AnalysisState.DeclarationAnalyzerStateData analyzerStateOpt = null;
		try
		{
			if (TryStartAnalyzingDeclaration(declaredSymbol, declarationIndex, analyzer, analysisScope, analysisStateOpt, out analyzerStateOpt))
			{
				ExecuteBlockActionsCore<CodeBlockStartAnalyzerAction, CodeBlockAnalyzerAction, SyntaxNodeAnalyzerAction, AnalysisState.SyntaxNodeAnalyzerStateData, SyntaxNode>(codeBlockStartActions, codeBlockActions, codeBlockEndActions, analyzer, declaredNode, declaredSymbol, executableCodeBlocks, (ImmutableArray<SyntaxNode> codeBlocks) => codeBlocks.SelectMany((SyntaxNode cb) => cb.DescendantNodesAndSelf()), semanticModel, getKind, analyzerStateOpt?.CodeBlockAnalysisState);
				return true;
			}
			return IsDeclarationComplete(declaredSymbol, declarationIndex, analyzer, analysisStateOpt);
		}
		finally
		{
			analyzerStateOpt?.ResetToReadyState();
		}
	}
}
