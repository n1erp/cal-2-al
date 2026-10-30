using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class AnalyzerDriver : IDisposable
{
	internal class CompilationData
	{
		private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModelsMap;

		private readonly Dictionary<SyntaxReference, DeclarationAnalysisData> _declarationAnalysisDataMap;

		public SuppressMessageAttributeState SuppressMessageAttributeState { get; }

		public CompilationData(Compilation comp)
		{
			_semanticModelsMap = new Dictionary<SyntaxTree, SemanticModel>();
			SuppressMessageAttributeState = new SuppressMessageAttributeState(comp);
			_declarationAnalysisDataMap = new Dictionary<SyntaxReference, DeclarationAnalysisData>();
		}

		public SemanticModel GetOrCreateCachedSemanticModel(SyntaxTree tree, Compilation compilation, CancellationToken cancellationToken)
		{
			SemanticModel value;
			lock (_semanticModelsMap)
			{
				if (_semanticModelsMap.TryGetValue(tree, out value) && value.Compilation == compilation)
				{
					return value;
				}
			}
			value = compilation.GetSemanticModel(tree);
			lock (_semanticModelsMap)
			{
				_semanticModelsMap[tree] = value;
				return value;
			}
		}

		public bool RemoveCachedSemanticModel(SyntaxTree tree)
		{
			lock (_semanticModelsMap)
			{
				return _semanticModelsMap.Remove(tree);
			}
		}

		internal DeclarationAnalysisData GetOrComputeDeclarationAnalysisData(SyntaxReference declaration, Func<DeclarationAnalysisData> computeDeclarationAnalysisData, bool cacheAnalysisData)
		{
			if (!cacheAnalysisData)
			{
				return computeDeclarationAnalysisData();
			}
			lock (_declarationAnalysisDataMap)
			{
				if (_declarationAnalysisDataMap.TryGetValue(declaration, out var value))
				{
					return value;
				}
			}
			DeclarationAnalysisData declarationAnalysisData = computeDeclarationAnalysisData();
			lock (_declarationAnalysisDataMap)
			{
				if (!_declarationAnalysisDataMap.TryGetValue(declaration, out var value2))
				{
					_declarationAnalysisDataMap.Add(declaration, declarationAnalysisData);
				}
				else
				{
					declarationAnalysisData = value2;
				}
			}
			return declarationAnalysisData;
		}

		internal void ClearDeclarationAnalysisData(SyntaxReference declaration)
		{
			lock (_declarationAnalysisDataMap)
			{
				_declarationAnalysisDataMap.Remove(declaration);
			}
		}
	}

	internal sealed class DeclarationAnalysisData
	{
		public SyntaxNode DeclaringReferenceSyntax { get; }

		public SyntaxNode TopmostNodeForAnalysis { get; }

		public ImmutableArray<DeclarationInfo> DeclarationsInNode { get; }

		public ImmutableArray<SyntaxNode> DescendantNodesToAnalyze { get; }

		public bool IsPartialAnalysis { get; }

		public DeclarationAnalysisData(SyntaxNode declaringReferenceSyntax, SyntaxNode topmostNodeForAnalysis, ImmutableArray<DeclarationInfo> declarationsInNodeBuilder, ImmutableArray<SyntaxNode> descendantNodesToAnalyze, bool isPartialAnalysis)
		{
			DeclaringReferenceSyntax = declaringReferenceSyntax;
			TopmostNodeForAnalysis = topmostNodeForAnalysis;
			DeclarationsInNode = declarationsInNodeBuilder;
			DescendantNodesToAnalyze = descendantNodesToAnalyze;
			IsPartialAnalysis = isPartialAnalysis;
		}
	}

	private const int MaxSymbolKind = 100;

	private static readonly Func<DiagnosticAnalyzer, bool> s_IsCompilerAnalyzerFunc = IsCompilerAnalyzer;

	protected readonly ImmutableArray<DiagnosticAnalyzer> analyzers;

	protected readonly AnalyzerManager analyzerManager;

	private readonly Func<SyntaxTree, CancellationToken, bool> _isGeneratedCode;

	private CancellationTokenRegistration _queueRegistration;

	protected AnalyzerExecutor analyzerExecutor;

	protected CompilationData compilationData;

	internal AnalyzerActions analyzerActions;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>> _symbolActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>> _semanticModelActionsMap;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>> _syntaxTreeActionsMap;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> _compilationActionsMap;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> _compilationEndActionsMap;

	internal const GeneratedCodeAnalysisFlags DefaultGeneratedCodeAnalysisFlags = GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics;

	private ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim> _analyzerGateMap = ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Empty;

	private ImmutableDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> _generatedCodeAnalysisFlagsMap;

	private bool _treatAllCodeAsNonGeneratedCode;

	private bool _doNotAnalyzeGeneratedCode;

	private ConcurrentDictionary<SyntaxTree, bool> _lazyGeneratedCodeFilesMap;

	private Dictionary<SyntaxTree, ImmutableHashSet<ISymbol>> _lazyGeneratedCodeSymbolsMap;

	private ConcurrentDictionary<SyntaxTree, bool> _lazyTreesWithHiddenRegionsMap;

	private INamedTypeSymbol _generatedCodeAttribute;

	private Task _initializeTask;

	private bool _initializeSucceeded;

	private Task _primaryTask;

	private readonly int _workerCount = Environment.ProcessorCount;

	public AsyncQueue<CompilationEvent> CompilationEventQueue { get; private set; }

	public DiagnosticQueue DiagnosticQueue { get; private set; }

	public Task WhenInitializedTask => _initializeTask;

	public Task WhenCompletedTask => _primaryTask;

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => analyzerExecutor.AnalyzerExecutionTimes;

	protected bool DoNotAnalyzeGeneratedCode => _doNotAnalyzeGeneratedCode;

	protected AnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, Func<SyntaxTrivia, bool> isComment)
	{
		this.analyzers = analyzers;
		this.analyzerManager = analyzerManager;
		_isGeneratedCode = (SyntaxTree tree, CancellationToken ct) => GeneratedCodeUtilities.IsGeneratedCode(tree, isComment, ct);
	}

	private void Initialize(AnalyzerExecutor analyzerExecutor, DiagnosticQueue diagnosticQueue, CompilationData compilationData, CancellationToken cancellationToken)
	{
		try
		{
			this.analyzerExecutor = analyzerExecutor;
			this.compilationData = compilationData;
			DiagnosticQueue = diagnosticQueue;
			_initializeTask = Task.Run(async () =>
			{
				ImmutableArray<DiagnosticAnalyzer> unsuppressedAnalyzers = GetUnsuppressedAnalyzers(analyzers, analyzerManager, analyzerExecutor);
				AnalyzerActions analyzerActions = await GetAnalyzerActionsAsync(unsuppressedAnalyzers, analyzerManager, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
				this.analyzerActions = analyzerActions;
				ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim> analyzerGateMap = await GetAnalyzerGateMapAsync(unsuppressedAnalyzers, analyzerManager, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
				_analyzerGateMap = analyzerGateMap;
				ImmutableDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> generatedCodeAnalysisFlagsMap = await GetGeneratedCodeAnalysisFlagsAsync(unsuppressedAnalyzers, analyzerManager, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
				_generatedCodeAnalysisFlagsMap = generatedCodeAnalysisFlagsMap;
				_doNotAnalyzeGeneratedCode = ShouldSkipAnalysisOnGeneratedCode(unsuppressedAnalyzers);
				_treatAllCodeAsNonGeneratedCode = ShouldTreatAllCodeAsNonGeneratedCode(unsuppressedAnalyzers, _generatedCodeAnalysisFlagsMap);
				_lazyGeneratedCodeFilesMap = (_treatAllCodeAsNonGeneratedCode ? null : new ConcurrentDictionary<SyntaxTree, bool>());
				_lazyGeneratedCodeSymbolsMap = (_treatAllCodeAsNonGeneratedCode ? null : new Dictionary<SyntaxTree, ImmutableHashSet<ISymbol>>());
				_lazyTreesWithHiddenRegionsMap = (_treatAllCodeAsNonGeneratedCode ? null : new ConcurrentDictionary<SyntaxTree, bool>());
				_generatedCodeAttribute = analyzerExecutor.Compilation?.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");
				_symbolActionsByKind = MakeSymbolActionsByKind();
				_semanticModelActionsMap = MakeSemanticModelActionsByAnalyzer();
				_syntaxTreeActionsMap = MakeSyntaxTreeActionsByAnalyzer();
				_compilationActionsMap = MakeCompilationActionsByAnalyzer(this.analyzerActions.CompilationActions);
				_compilationEndActionsMap = MakeCompilationActionsByAnalyzer(this.analyzerActions.CompilationEndActions);
			}, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			_initializeSucceeded = true;
		}
		finally
		{
			if (_initializeTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				_initializeTask = taskCompletionSource.Task;
				taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				_primaryTask = taskCompletionSource.Task;
				DiagnosticQueue.TryComplete();
			}
		}
	}

	internal void Initialize(Compilation compilation, CompilationWithAnalyzersOptions analysisOptions, CompilationData compilationData, bool categorizeDiagnostics, CancellationToken cancellationToken)
	{
		DiagnosticQueue diagnosticQueue = DiagnosticQueue.Create(categorizeDiagnostics);
		Action<Diagnostic> addNotCategorizedDiagnosticOpt = null;
		Action<Diagnostic, DiagnosticAnalyzer, bool> addCategorizedLocalDiagnosticOpt = null;
		Action<Diagnostic, DiagnosticAnalyzer> addCategorizedNonLocalDiagnosticOpt = null;
		if (categorizeDiagnostics)
		{
			addCategorizedLocalDiagnosticOpt = GetDiagnosticSink(diagnosticQueue.EnqueueLocal, compilation);
			addCategorizedNonLocalDiagnosticOpt = GetDiagnosticSink(diagnosticQueue.EnqueueNonLocal, compilation);
		}
		else
		{
			addNotCategorizedDiagnosticOpt = GetDiagnosticSink(diagnosticQueue.Enqueue, compilation);
		}
		Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = (Exception ex, DiagnosticAnalyzer analyzer, Diagnostic diagnostic) =>
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation);
			if (filteredDiagnostic != null)
			{
				if (analysisOptions.OnAnalyzerException != null)
				{
					analysisOptions.OnAnalyzerException(ex, analyzer, filteredDiagnostic);
				}
				else if (categorizeDiagnostics)
				{
					addCategorizedNonLocalDiagnosticOpt(filteredDiagnostic, analyzer);
				}
				else
				{
					addNotCategorizedDiagnosticOpt(filteredDiagnostic);
				}
			}
		};
		AnalyzerExecutor analyzerExecutor = AnalyzerExecutor.Create(compilation, analysisOptions.Options ?? AnalyzerOptions.Empty, addNotCategorizedDiagnosticOpt, onAnalyzerException, analysisOptions.AnalyzerExceptionFilter, IsCompilerAnalyzer, analyzerManager, ShouldSkipAnalysisOnGeneratedCode, ShouldSuppressGeneratedCodeDiagnostic, IsGeneratedOrHiddenCodeLocation, GetAnalyzerGate, analysisOptions.LogAnalyzerExecutionTime, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, cancellationToken);
		Initialize(analyzerExecutor, diagnosticQueue, compilationData, cancellationToken);
	}

	private SemaphoreSlim GetAnalyzerGate(DiagnosticAnalyzer analyzer)
	{
		if (_analyzerGateMap.TryGetValue(analyzer, out var value))
		{
			return value;
		}
		return null;
	}

	private bool ShouldSkipAnalysisOnGeneratedCode(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			if (!ShouldSkipAnalysisOnGeneratedCode(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool ShouldTreatAllCodeAsNonGeneratedCode(ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> generatedCodeAnalysisFlagsMap)
	{
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			GeneratedCodeAnalysisFlags generatedCodeAnalysisFlags = generatedCodeAnalysisFlagsMap[item];
			bool flag = (generatedCodeAnalysisFlags & GeneratedCodeAnalysisFlags.Analyze) != 0;
			bool flag2 = (generatedCodeAnalysisFlags & GeneratedCodeAnalysisFlags.ReportDiagnostics) != 0;
			if (!flag || !flag2)
			{
				return false;
			}
		}
		return true;
	}

	private bool ShouldSkipAnalysisOnGeneratedCode(DiagnosticAnalyzer analyzer)
	{
		if (_treatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		return (_generatedCodeAnalysisFlagsMap[analyzer] & GeneratedCodeAnalysisFlags.Analyze) == 0;
	}

	private bool ShouldSuppressGeneratedCodeDiagnostic(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, Compilation compilation, CancellationToken cancellationToken)
	{
		if (_treatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		if ((_generatedCodeAnalysisFlagsMap[analyzer] & GeneratedCodeAnalysisFlags.ReportDiagnostics) == 0)
		{
			return IsInGeneratedCode(diagnostic.Location, compilation, cancellationToken);
		}
		return false;
	}

	internal async Task AttachQueueAndProcessAllEventsAsync(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		try
		{
			if (_initializeSucceeded)
			{
				CompilationEventQueue = eventQueue;
				_queueRegistration = default;
				await ExecutePrimaryAnalysisTaskAsync(analysisScope, analysisStateOpt, usingPrePopulatedEventQueue: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_primaryTask = Task.FromResult(result: true);
			}
		}
		finally
		{
			if (_primaryTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				_primaryTask = taskCompletionSource.Task;
			}
		}
	}

	internal void AttachQueueAndStartProcessingEvents(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		try
		{
			if (_initializeSucceeded)
			{
				CompilationEventQueue = eventQueue;
				_queueRegistration = cancellationToken.Register(() =>
				{
					CompilationEventQueue.TryComplete();
					DiagnosticQueue.TryComplete();
				});
				_primaryTask = ExecutePrimaryAnalysisTaskAsync(analysisScope, null, usingPrePopulatedEventQueue: false, cancellationToken).ContinueWith((Task c) => DiagnosticQueue.TryComplete(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}
		}
		finally
		{
			if (_primaryTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				_primaryTask = taskCompletionSource.Task;
				DiagnosticQueue.TryComplete();
			}
		}
	}

	private async Task ExecutePrimaryAnalysisTaskAsync(AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool usingPrePopulatedEventQueue, CancellationToken cancellationToken)
	{
		await WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
		if (WhenInitializedTask.IsFaulted)
		{
			OnDriverException(WhenInitializedTask, analyzerExecutor, analysisScope.Analyzers);
		}
		else if (!WhenInitializedTask.IsCanceled)
		{
			analyzerExecutor = analyzerExecutor.WithCancellationToken(cancellationToken);
			await ProcessCompilationEventsAsync(analysisScope, analysisStateOpt, usingPrePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private static void OnDriverException(Task faultedTask, AnalyzerExecutor analyzerExecutor, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		Exception ex = faultedTask.Exception?.InnerException;
		if (ex != null && !(ex is OperationCanceledException))
		{
			Diagnostic arg = AnalyzerExecutor.CreateDriverExceptionDiagnostic(ex);
			DiagnosticAnalyzer arg2 = analyzers[0];
			analyzerExecutor.OnAnalyzerException(ex, arg2, arg);
		}
	}

	private void ExecuteSyntaxTreeActions(AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		if (analysisScope.IsTreeAnalysis && !analysisScope.IsSyntaxOnlyTreeAnalysis)
		{
			return;
		}
		foreach (SyntaxTree syntaxTree in analysisScope.SyntaxTrees)
		{
			bool flag = IsGeneratedCode(syntaxTree);
			if (flag && DoNotAnalyzeGeneratedCode)
			{
				analysisStateOpt?.MarkSyntaxAnalysisComplete(syntaxTree, analysisScope.Analyzers);
				continue;
			}
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (_syntaxTreeActionsMap.TryGetValue(analyzer, out var value))
				{
					analyzerExecutor.TryExecuteSyntaxTreeActions(value, analyzer, syntaxTree, analysisScope, analysisStateOpt, flag);
				}
				else
				{
					analysisStateOpt?.MarkSyntaxAnalysisComplete(syntaxTree, analyzer);
				}
			}
		}
	}

	public static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Diagnostic> addExceptionDiagnostic, bool reportAnalyzer, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = (Exception ex, DiagnosticAnalyzer analyzer, Diagnostic diagnostic) =>
		{
			addExceptionDiagnostic?.Invoke(diagnostic);
		};
		Func<Exception, bool> analyzerExceptionFilter = null;
		return CreateAndAttachToCompilation(compilation, analyzers, options, analyzerManager, onAnalyzerException, analyzerExceptionFilter, reportAnalyzer, out newCompilation, cancellationToken);
	}

	internal static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool> analyzerExceptionFilter, bool reportAnalyzer, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		AnalyzerDriver analyzerDriver = compilation.AnalyzerForLanguage(analyzers, analyzerManager);
		newCompilation = compilation.WithEventQueue(new AsyncQueue<CompilationEvent>());
		bool categorizeDiagnostics = false;
		analyzerDriver.Initialize(analysisOptions: new CompilationWithAnalyzersOptions(options, onAnalyzerException, concurrentAnalysis: true, reportAnalyzer, reportSuppressedDiagnostics: false, analyzerExceptionFilter), compilation: newCompilation, compilationData: new CompilationData(newCompilation), categorizeDiagnostics: categorizeDiagnostics, cancellationToken: cancellationToken);
		analyzerDriver.AttachQueueAndStartProcessingEvents(analysisScope: new AnalysisScope(newCompilation, analyzers, newCompilation.Options.ConcurrentBuild, categorizeDiagnostics), eventQueue: newCompilation.EventQueue, cancellationToken: cancellationToken);
		return analyzerDriver;
	}

	public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Compilation compilation)
	{
		DiagnosticBag allDiagnostics = DiagnosticBag.GetInstance();
		if (CompilationEventQueue.IsCompleted)
		{
			await WhenCompletedTask.ConfigureAwait(continueOnCapturedContext: false);
			if (WhenCompletedTask.IsFaulted)
			{
				OnDriverException(WhenCompletedTask, analyzerExecutor, analyzers);
			}
		}
		SuppressMessageAttributeState suppressMessageAttributeState = compilationData.SuppressMessageAttributeState;
		bool reportSuppressedDiagnostics = compilation.Options.ReportSuppressedDiagnostics;
		Diagnostic d;
		while (DiagnosticQueue.TryDequeue(out d))
		{
			d = suppressMessageAttributeState.ApplySourceSuppressions(d);
			if (reportSuppressedDiagnostics || !d.IsSuppressed)
			{
				allDiagnostics.Add(d);
			}
		}
		return allDiagnostics.ToReadOnlyAndFree();
	}

	public ImmutableArray<Diagnostic> DequeueLocalDiagnostics(DiagnosticAnalyzer analyzer, bool syntax, Compilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics = (syntax ? DiagnosticQueue.DequeueLocalSyntaxDiagnostics(analyzer) : DiagnosticQueue.DequeueLocalSemanticDiagnostics(analyzer));
		return FilterDiagnosticsSuppressedInSource(diagnostics, compilation, compilationData.SuppressMessageAttributeState);
	}

	public ImmutableArray<Diagnostic> DequeueNonLocalDiagnostics(DiagnosticAnalyzer analyzer, Compilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics = DiagnosticQueue.DequeueNonLocalDiagnostics(analyzer);
		return FilterDiagnosticsSuppressedInSource(diagnostics, compilation, compilationData.SuppressMessageAttributeState);
	}

	private ImmutableArray<Diagnostic> FilterDiagnosticsSuppressedInSource(ImmutableArray<Diagnostic> diagnostics, Compilation compilation, SuppressMessageAttributeState suppressMessageState)
	{
		if (diagnostics.IsEmpty)
		{
			return diagnostics;
		}
		bool reportSuppressedDiagnostics = compilation.Options.ReportSuppressedDiagnostics;
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		for (int i = 0; i < diagnostics.Length; i++)
		{
			Diagnostic diagnostic = suppressMessageState.ApplySourceSuppressions(diagnostics[i]);
			if (reportSuppressedDiagnostics || !diagnostic.IsSuppressed)
			{
				builder.Add(diagnostic);
			}
		}
		return builder.ToImmutable();
	}

	private bool IsInGeneratedCode(Location location, Compilation compilation, CancellationToken cancellationToken)
	{
		if (_treatAllCodeAsNonGeneratedCode || !location.IsInSource)
		{
			return false;
		}
		if (IsGeneratedOrHiddenCodeLocation(location.SourceTree, location.SourceSpan))
		{
			return true;
		}
		if (_generatedCodeAttribute != null && _lazyGeneratedCodeSymbolsMap != null)
		{
			ImmutableHashSet<ISymbol> orComputeGeneratedCodeSymbolsInTree = GetOrComputeGeneratedCodeSymbolsInTree(location.SourceTree, compilation, cancellationToken);
			if (orComputeGeneratedCodeSymbolsInTree.Count > 0)
			{
				SemanticModel semanticModel = compilation.GetSemanticModel(location.SourceTree);
				for (SyntaxNode syntaxNode = location.SourceTree.GetRoot(cancellationToken).FindNode(location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true); syntaxNode != null; syntaxNode = syntaxNode.Parent)
				{
					foreach (ISymbol item in semanticModel.GetDeclaredSymbolsForNode(syntaxNode, cancellationToken))
					{
						if (orComputeGeneratedCodeSymbolsInTree.Contains(item))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private ImmutableHashSet<ISymbol> GetOrComputeGeneratedCodeSymbolsInTree(SyntaxTree tree, Compilation compilation, CancellationToken cancellationToken)
	{
		ImmutableHashSet<ISymbol> value;
		lock (_lazyGeneratedCodeSymbolsMap)
		{
			if (_lazyGeneratedCodeSymbolsMap.TryGetValue(tree, out value))
			{
				return value;
			}
		}
		value = ComputeGeneratedCodeSymbolsInTree(tree, compilation, cancellationToken);
		lock (_lazyGeneratedCodeSymbolsMap)
		{
			if (!_lazyGeneratedCodeSymbolsMap.TryGetValue(tree, out var _))
			{
				_lazyGeneratedCodeSymbolsMap.Add(tree, value);
			}
		}
		return value;
	}

	private ImmutableHashSet<ISymbol> ComputeGeneratedCodeSymbolsInTree(SyntaxTree tree, Compilation compilation, CancellationToken cancellationToken)
	{
		if (!tree.GetText(cancellationToken).ToString().Contains("GeneratedCode"))
		{
			return ImmutableHashSet<ISymbol>.Empty;
		}
		SemanticModel semanticModel = compilation.GetSemanticModel(tree);
		TextSpan fullSpan = tree.GetRoot(cancellationToken).FullSpan;
		ArrayBuilder<DeclarationInfo> instance = ArrayBuilder<DeclarationInfo>.GetInstance();
		semanticModel.ComputeDeclarationsInSpan(fullSpan, getSymbol: true, instance, cancellationToken);
		ImmutableHashSet<ISymbol>.Builder builder = null;
		foreach (DeclarationInfo item in instance)
		{
			ISymbol declaredSymbol = item.DeclaredSymbol;
			if (declaredSymbol != null && GeneratedCodeUtilities.IsGeneratedSymbolWithGeneratedCodeAttribute(declaredSymbol, _generatedCodeAttribute))
			{
				builder = builder ?? ImmutableHashSet.CreateBuilder<ISymbol>();
				builder.Add(declaredSymbol);
			}
		}
		instance.Free();
		if (builder == null)
		{
			return ImmutableHashSet<ISymbol>.Empty;
		}
		return builder.ToImmutable();
	}

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		return analyzerExecutor.ResetAnalyzerExecutionTime(analyzer);
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>> MakeSymbolActionsByKind()
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>>();
		foreach (IGrouping<DiagnosticAnalyzer, SymbolAnalyzerAction> item in from action in analyzerActions.SymbolActions
			group action by action.Analyzer)
		{
			List<ArrayBuilder<SymbolAnalyzerAction>> list = new List<ArrayBuilder<SymbolAnalyzerAction>>();
			foreach (SymbolAnalyzerAction item2 in item)
			{
				foreach (SymbolKind item3 in item2.Kinds.Distinct())
				{
					if (item3 <= (SymbolKind)100)
					{
						while ((int)item3 >= list.Count)
						{
							list.Add(ArrayBuilder<SymbolAnalyzerAction>.GetInstance());
						}
						list[(int)item3].Add(item2);
					}
				}
			}
			ImmutableArray<ImmutableArray<SymbolAnalyzerAction>> value = list.Select((ArrayBuilder<SymbolAnalyzerAction> a) => a.ToImmutableAndFree()).ToImmutableArray();
			builder.Add(item.Key, value);
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>> MakeSyntaxTreeActionsByAnalyzer()
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>>();
		foreach (IGrouping<DiagnosticAnalyzer, SyntaxTreeAnalyzerAction> item in from action in analyzerActions.SyntaxTreeActions
			group action by action.Analyzer)
		{
			builder.Add(item.Key, item.ToImmutableArray());
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>> MakeSemanticModelActionsByAnalyzer()
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>>();
		foreach (IGrouping<DiagnosticAnalyzer, SemanticModelAnalyzerAction> item in from action in analyzerActions.SemanticModelActions
			group action by action.Analyzer)
		{
			builder.Add(item.Key, item.ToImmutableArray());
		}
		return builder.ToImmutable();
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> MakeCompilationActionsByAnalyzer(ImmutableArray<CompilationAnalyzerAction> compilationActions)
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>>();
		foreach (IGrouping<DiagnosticAnalyzer, CompilationAnalyzerAction> item in from action in compilationActions
			group action by action.Analyzer)
		{
			builder.Add(item.Key, item.ToImmutableArray());
		}
		return builder.ToImmutable();
	}

	private async Task ProcessCompilationEventsAsync(AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool prePopulatedEventQueue, CancellationToken cancellationToken)
	{
		try
		{
			CompilationCompletedEvent completedEvent = null;
			if (analysisScope.ConcurrentAnalysis)
			{
				int workerCount = (prePopulatedEventQueue ? Math.Min(CompilationEventQueue.Count, _workerCount) : _workerCount);
				Task<CompilationCompletedEvent>[] workerTasks = new Task<CompilationCompletedEvent>[workerCount];
				for (int i = 0; i < workerCount; i++)
				{
					workerTasks[i] = Task.Run(async () => await ProcessCompilationEventsCoreAsync(analysisScope, analysisStateOpt, prePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				}
				cancellationToken.ThrowIfCancellationRequested();
				Task value = Task.Run(() =>
				{
					ExecuteSyntaxTreeActions(analysisScope, analysisStateOpt, cancellationToken);
				});
				await Task.WhenAll(workerTasks.Concat(value)).ConfigureAwait(continueOnCapturedContext: false);
				for (int num = 0; num < workerCount; num++)
				{
					if (workerTasks[num].Status == TaskStatus.RanToCompletion && workerTasks[num].Result != null)
					{
						completedEvent = workerTasks[num].Result;
						break;
					}
				}
			}
			else
			{
				completedEvent = await ProcessCompilationEventsCoreAsync(analysisScope, analysisStateOpt, prePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				ExecuteSyntaxTreeActions(analysisScope, analysisStateOpt, cancellationToken);
			}
			if (completedEvent != null)
			{
				ProcessEvent(completedEvent, analysisScope, analysisStateOpt, cancellationToken);
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task<CompilationCompletedEvent> ProcessCompilationEventsCoreAsync(AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool prePopulatedEventQueue, CancellationToken cancellationToken)
	{
		try
		{
			CompilationCompletedEvent completedEvent = null;
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if ((prePopulatedEventQueue || CompilationEventQueue.IsCompleted) && CompilationEventQueue.Count == 0)
				{
					break;
				}
				CompilationEvent d;
				try
				{
					if (!CompilationEventQueue.TryDequeue(out d))
					{
						if (prePopulatedEventQueue)
						{
							return completedEvent;
						}
						d = await CompilationEventQueue.DequeueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				catch (TaskCanceledException) when (!prePopulatedEventQueue)
				{
					break;
				}
				if (d is CompilationCompletedEvent compilationCompletedEvent)
				{
					completedEvent = compilationCompletedEvent;
				}
				else
				{
					ProcessEvent(d, analysisScope, analysisStateOpt, cancellationToken);
				}
			}
			return completedEvent;
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private void ProcessEvent(CompilationEvent e, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		if (TryProcessEventCore(e, analysisScope, analysisStateOpt, cancellationToken))
		{
			analysisStateOpt?.OnCompilationEventProcessed(e, analysisScope);
		}
	}

	private bool TryProcessEventCore(CompilationEvent e, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (e is SymbolDeclaredCompilationEvent symbolEvent)
		{
			return TryProcessSymbolDeclared(symbolEvent, analysisScope, analysisStateOpt, cancellationToken);
		}
		if (e is CompilationUnitCompletedEvent completedEvent)
		{
			return TryProcessCompilationUnitCompleted(completedEvent, analysisScope, analysisStateOpt, cancellationToken);
		}
		if (e is CompilationCompletedEvent endEvent)
		{
			return TryProcessCompilationCompleted(endEvent, analysisScope, analysisStateOpt, cancellationToken);
		}
		if (e is CompilationStartedEvent startedEvent)
		{
			return TryProcessCompilationStarted(startedEvent, analysisScope, analysisStateOpt, cancellationToken);
		}
		throw new InvalidOperationException("Unexpected compilation event of type " + e.GetType().Name);
	}

	private bool TryProcessSymbolDeclared(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		try
		{
			bool result = true;
			ISymbol symbol = symbolEvent.Symbol;
			bool isGeneratedCodeSymbol = IsGeneratedCodeSymbol(symbol);
			if (!AnalysisScope.ShouldSkipSymbolAnalysis(symbolEvent) && !TryExecuteSymbolActions(symbolEvent, analysisScope, analysisStateOpt, isGeneratedCodeSymbol, cancellationToken))
			{
				result = false;
			}
			if (!AnalysisScope.ShouldSkipDeclarationAnalysis(symbol) && !TryExecuteDeclaringReferenceActions(symbolEvent, analysisScope, analysisStateOpt, isGeneratedCodeSymbol, cancellationToken))
			{
				result = false;
			}
			return result;
		}
		finally
		{
			symbolEvent.FlushCache();
		}
	}

	private bool TryExecuteSymbolActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCodeSymbol, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		if (!analysisScope.ShouldAnalyze(symbol))
		{
			return true;
		}
		bool result = true;
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (_symbolActionsByKind.TryGetValue(analyzer, out var value) && (int)symbol.Kind < value.Length)
			{
				if (!analyzerExecutor.TryExecuteSymbolActions(value[(int)symbol.Kind], analyzer, symbolEvent, GetTopmostNodeForAnalysis, analysisScope, analysisStateOpt, isGeneratedCodeSymbol))
				{
					result = false;
				}
			}
			else
			{
				analysisStateOpt?.MarkSymbolComplete(symbol, analyzer);
			}
		}
		return result;
	}

	private static SyntaxNode GetTopmostNodeForAnalysis(ISymbol symbol, SyntaxReference syntaxReference, Compilation compilation)
	{
		return compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetTopmostNodeForDiagnosticAnalysis(symbol, syntaxReference.GetSyntax());
	}

	protected abstract bool TryExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCodeSymbol, CancellationToken cancellationToken);

	private bool TryProcessCompilationUnitCompleted(CompilationUnitCompletedEvent completedEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = ((analysisStateOpt != null) ? compilationData.GetOrCreateCachedSemanticModel(completedEvent.CompilationUnit, completedEvent.Compilation, cancellationToken) : completedEvent.SemanticModel);
		if (!analysisScope.ShouldAnalyze(semanticModel.SyntaxTree))
		{
			return true;
		}
		bool flag = IsGeneratedCode(semanticModel.SyntaxTree);
		if (flag && DoNotAnalyzeGeneratedCode)
		{
			analysisStateOpt?.MarkEventComplete(completedEvent, analysisScope.Analyzers);
			return true;
		}
		try
		{
			bool result = true;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (_semanticModelActionsMap.TryGetValue(analyzer, out var value))
				{
					if (!analyzerExecutor.TryExecuteSemanticModelActions(value, analyzer, semanticModel, completedEvent, analysisScope, analysisStateOpt, flag))
					{
						result = false;
					}
				}
				else
				{
					analysisStateOpt?.MarkEventComplete(completedEvent, analyzer);
				}
			}
			return result;
		}
		finally
		{
			completedEvent.FlushCache();
		}
	}

	private bool TryProcessCompilationStarted(CompilationStartedEvent startedEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		return TryExecuteCompilationActions(_compilationActionsMap, startedEvent, analysisScope, analysisStateOpt, cancellationToken);
	}

	private bool TryProcessCompilationCompleted(CompilationCompletedEvent endEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		return TryExecuteCompilationActions(_compilationEndActionsMap, endEvent, analysisScope, analysisStateOpt, cancellationToken);
	}

	private bool TryExecuteCompilationActions(ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> compilationActionsMap, CompilationEvent compilationEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		try
		{
			bool result = true;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (compilationActionsMap.TryGetValue(analyzer, out var value))
				{
					if (!analyzerExecutor.TryExecuteCompilationActions(value, analyzer, compilationEvent, analysisScope, analysisStateOpt))
					{
						result = false;
					}
				}
				else
				{
					analysisStateOpt?.MarkEventComplete(compilationEvent, analyzer);
				}
			}
			return result;
		}
		finally
		{
			compilationEvent.FlushCache();
		}
	}

	internal static Action<Diagnostic> GetDiagnosticSink(Action<Diagnostic> addDiagnosticCore, Compilation compilation)
	{
		return (Diagnostic diagnostic) =>
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation);
			if (filteredDiagnostic != null)
			{
				addDiagnosticCore(filteredDiagnostic);
			}
		};
	}

	internal static Action<Diagnostic, DiagnosticAnalyzer, bool> GetDiagnosticSink(Action<Diagnostic, DiagnosticAnalyzer, bool> addLocalDiagnosticCore, Compilation compilation)
	{
		return (Diagnostic diagnostic, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic) =>
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation);
			if (filteredDiagnostic != null)
			{
				addLocalDiagnosticCore(filteredDiagnostic, analyzer, isSyntaxDiagnostic);
			}
		};
	}

	internal static Action<Diagnostic, DiagnosticAnalyzer> GetDiagnosticSink(Action<Diagnostic, DiagnosticAnalyzer> addDiagnosticCore, Compilation compilation)
	{
		return (Diagnostic diagnostic, DiagnosticAnalyzer analyzer) =>
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation);
			if (filteredDiagnostic != null)
			{
				addDiagnosticCore(filteredDiagnostic, analyzer);
			}
		};
	}

	private static Diagnostic GetFilteredDiagnostic(Diagnostic diagnostic, Compilation compilation)
	{
		return compilation.Options.FilterDiagnostic(diagnostic);
	}

	private static ImmutableArray<DiagnosticAnalyzer> GetUnsuppressedAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			if (!IsDiagnosticAnalyzerSuppressed(item, analyzerExecutor.Compilation.Options, analyzerManager, analyzerExecutor))
			{
				builder.Add(item);
			}
		}
		return builder.ToImmutable();
	}

	private static async Task<AnalyzerActions> GetAnalyzerActionsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		AnalyzerActions allAnalyzerActions = new AnalyzerActions();
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			AnalyzerActions analyzerActions = await analyzerManager.GetAnalyzerActionsAsync(item, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
			if (analyzerActions != null)
			{
				allAnalyzerActions = allAnalyzerActions.Append(analyzerActions);
			}
		}
		return allAnalyzerActions;
	}

	private static async Task<ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>> GetAnalyzerGateMapAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, SemaphoreSlim>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			if (!(await analyzerManager.IsConcurrentAnalyzerAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false)))
			{
				SemaphoreSlim value = new SemaphoreSlim(1);
				builder.Add(analyzer, value);
			}
		}
		return builder.ToImmutable();
	}

	private static async Task<ImmutableDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>> GetGeneratedCodeAnalysisFlagsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		ImmutableDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			builder.Add(analyzer, await analyzerManager.GetGeneratedCodeAnalysisFlagsAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutable();
	}

	private bool IsGeneratedCodeSymbol(ISymbol symbol)
	{
		if (_treatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		if (_generatedCodeAttribute != null && GeneratedCodeUtilities.IsGeneratedSymbolWithGeneratedCodeAttribute(symbol, _generatedCodeAttribute))
		{
			return true;
		}
		foreach (SyntaxReference declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
		{
			if (!IsGeneratedOrHiddenCodeLocation(declaringSyntaxReference.SyntaxTree, declaringSyntaxReference.Span))
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsGeneratedCode(SyntaxTree tree)
	{
		if (_treatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		if (!_lazyGeneratedCodeFilesMap.TryGetValue(tree, out var value))
		{
			value = _isGeneratedCode(tree, analyzerExecutor.CancellationToken);
			_lazyGeneratedCodeFilesMap.TryAdd(tree, value);
		}
		return value;
	}

	protected bool IsGeneratedOrHiddenCodeLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		if (!IsGeneratedCode(syntaxTree))
		{
			return IsHiddenSourceLocation(syntaxTree, span);
		}
		return true;
	}

	protected bool IsHiddenSourceLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		if (HasHiddenRegions(syntaxTree))
		{
			return syntaxTree.IsHiddenPosition(span.Start);
		}
		return false;
	}

	private bool HasHiddenRegions(SyntaxTree tree)
	{
		if (_lazyTreesWithHiddenRegionsMap == null)
		{
			return false;
		}
		if (!_lazyTreesWithHiddenRegionsMap.TryGetValue(tree, out var value))
		{
			value = tree.HasHiddenRegions();
			_lazyTreesWithHiddenRegionsMap.TryAdd(tree, value);
		}
		return value;
	}

	internal async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CompilationOptions compilationOptions, CancellationToken cancellationToken)
	{
		AnalyzerExecutor analyzerExecutor = this.analyzerExecutor.WithCancellationToken(cancellationToken);
		if (IsDiagnosticAnalyzerSuppressed(analyzer, compilationOptions, analyzerManager, analyzerExecutor))
		{
			return AnalyzerActionCounts.Empty;
		}
		return new AnalyzerActionCounts(await analyzerManager.GetAnalyzerActionsAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		return analyzerManager.IsDiagnosticAnalyzerSuppressed(analyzer, options, s_IsCompilerAnalyzerFunc, analyzerExecutor);
	}

	private static bool IsCompilerAnalyzer(DiagnosticAnalyzer analyzer)
	{
		return analyzer is CompilerDiagnosticAnalyzer;
	}

	public void Dispose()
	{
		CompilationEventQueue?.TryComplete();
		DiagnosticQueue?.TryComplete();
		_queueRegistration.Dispose();
	}
}
internal class AnalyzerDriver<TLanguageKindEnum> : AnalyzerDriver where TLanguageKindEnum : struct
{
	[StructLayout(LayoutKind.Auto)]
	private struct CodeBlockAnalyzerActions
	{
		public DiagnosticAnalyzer Analyzer;

		public ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> CodeBlockStartActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions;

		public ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions;
	}

	private readonly Func<SyntaxNode, TLanguageKindEnum> _getKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>> _lazyNodeActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> _lazyOperationActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>> _lazyCodeBlockStartActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> _lazyCodeBlockEndActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> _lazyCodeBlockActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockStartAnalyzerAction>> _lazyOperationBlockStartActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> _lazyOperationBlockActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> _lazyOperationBlockEndActionsByAnalyzer;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>> NodeActionsByAnalyzerAndKind
	{
		get
		{
			if (_lazyNodeActionsByKind == null)
			{
				ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> syntaxNodeActions = analyzerActions.GetSyntaxNodeActions<TLanguageKindEnum>();
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>> value;
				if (syntaxNodeActions.Any())
				{
					IEnumerable<IGrouping<DiagnosticAnalyzer, SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> enumerable = from a in syntaxNodeActions
						group a by a.Analyzer;
					ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>>();
					foreach (IGrouping<DiagnosticAnalyzer, SyntaxNodeAnalyzerAction<TLanguageKindEnum>> item in enumerable)
					{
						builder.Add(value: (!item.Any()) ? ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>.Empty : AnalyzerExecutor.GetNodeActionsByKind(item), key: item.Key);
					}
					value = builder.ToImmutable();
				}
				else
				{
					value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>>.Empty;
				}
				Interlocked.CompareExchange(ref _lazyNodeActionsByKind, value, null);
			}
			return _lazyNodeActionsByKind;
		}
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> OperationActionsByAnalyzerAndKind
	{
		get
		{
			if (_lazyOperationActionsByKind == null)
			{
				ImmutableArray<OperationAnalyzerAction> operationActions = analyzerActions.OperationActions;
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>> value;
				if (operationActions.Any())
				{
					IEnumerable<IGrouping<DiagnosticAnalyzer, OperationAnalyzerAction>> enumerable = from a in operationActions
						group a by a.Analyzer;
					ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>();
					foreach (IGrouping<DiagnosticAnalyzer, OperationAnalyzerAction> item in enumerable)
					{
						builder.Add(value: (!item.Any()) ? ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>.Empty : AnalyzerExecutor.GetOperationActionsByKind(item), key: item.Key);
					}
					value = builder.ToImmutable();
				}
				else
				{
					value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>>.Empty;
				}
				Interlocked.CompareExchange(ref _lazyOperationActionsByKind, value, null);
			}
			return _lazyOperationActionsByKind;
		}
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>> CodeBlockStartActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyCodeBlockStartActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.GetCodeBlockStartActions<TLanguageKindEnum>(), analyzerActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> CodeBlockEndActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyCodeBlockEndActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.CodeBlockEndActions, analyzerActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CodeBlockAnalyzerAction>> CodeBlockActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyCodeBlockActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.CodeBlockActions, analyzerActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockStartAnalyzerAction>> OperationBlockStartActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockStartActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.OperationBlockStartActions, analyzerActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> OperationBlockEndActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockEndActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.OperationBlockEndActions, analyzerActions);

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<OperationBlockAnalyzerAction>> OperationBlockActionsByAnalyzer => GetBlockActionsByAnalyzer(ref _lazyOperationBlockActionsByAnalyzer, (AnalyzerActions analyzerActions) => analyzerActions.OperationBlockActions, analyzerActions);

	internal AnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, Func<SyntaxNode, TLanguageKindEnum> getKind, AnalyzerManager analyzerManager, Func<SyntaxTrivia, bool> isComment)
		: base(analyzers, analyzerManager, isComment)
	{
		_getKind = getKind;
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> GetBlockActionsByAnalyzer<ActionType>(ref ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> lazyCodeBlockActionsByAnalyzer, Func<AnalyzerActions, ImmutableArray<ActionType>> codeBlockActionsFactory, AnalyzerActions analyzerActions) where ActionType : AnalyzerAction
	{
		if (lazyCodeBlockActionsByAnalyzer == null)
		{
			ImmutableArray<ActionType> immutableArray = codeBlockActionsFactory(analyzerActions);
			ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>> value;
			if (!immutableArray.IsEmpty)
			{
				ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<ActionType>>();
				foreach (IGrouping<DiagnosticAnalyzer, ActionType> item in from action in immutableArray
					group action by action.Analyzer)
				{
					builder.Add(item.Key, item.ToImmutableArrayOrEmpty());
				}
				value = builder.ToImmutable();
			}
			else
			{
				value = ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ActionType>>.Empty;
			}
			Interlocked.CompareExchange(ref lazyCodeBlockActionsByAnalyzer, value, null);
		}
		return lazyCodeBlockActionsByAnalyzer;
	}

	private bool ShouldExecuteSyntaxNodeActions(AnalysisScope analysisScope)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (NodeActionsByAnalyzerAndKind.ContainsKey(analyzer))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldExecuteOperationActions(AnalysisScope analysisScope)
	{
		return analysisScope.Analyzers.Any((DiagnosticAnalyzer analyzer) => OperationActionsByAnalyzerAndKind.ContainsKey(analyzer));
	}

	private bool ShouldExecuteBlockActions<T0, T1>(ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<T0>> blockStartActions, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<T1>> blockActions, AnalysisScope analysisScope, ISymbol symbol)
	{
		if (AnalyzerExecutor.CanHaveExecutableCodeBlock(symbol))
		{
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (blockStartActions.ContainsKey(analyzer) || blockActions.ContainsKey(analyzer))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ShouldExecuteCodeBlockActions(AnalysisScope analysisScope, ISymbol symbol)
	{
		return ShouldExecuteBlockActions(CodeBlockStartActionsByAnalyzer, CodeBlockActionsByAnalyzer, analysisScope, symbol);
	}

	private bool ShouldExecuteOperationBlockActions(AnalysisScope analysisScope, ISymbol symbol)
	{
		return ShouldExecuteBlockActions(OperationBlockStartActionsByAnalyzer, OperationBlockActionsByAnalyzer, analysisScope, symbol);
	}

	protected override bool TryExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool isGeneratedCodeSymbol, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		bool flag = ShouldExecuteSyntaxNodeActions(analysisScope);
		bool flag2 = ShouldExecuteCodeBlockActions(analysisScope, symbol);
		bool flag3 = ShouldExecuteOperationActions(analysisScope);
		bool flag4 = ShouldExecuteOperationBlockActions(analysisScope, symbol);
		bool result = true;
		if (flag | flag3 | flag2 | flag4)
		{
			ImmutableArray<SyntaxReference> declaringSyntaxReferences = symbolEvent.DeclaringSyntaxReferences;
			for (int i = 0; i < declaringSyntaxReferences.Length; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				SyntaxReference syntaxReference = declaringSyntaxReferences[i];
				if (analysisScope.FilterTreeOpt == null || analysisScope.FilterTreeOpt == syntaxReference.SyntaxTree)
				{
					bool flag5 = isGeneratedCodeSymbol || IsGeneratedOrHiddenCodeLocation(syntaxReference.SyntaxTree, syntaxReference.Span);
					if (flag5 && DoNotAnalyzeGeneratedCode)
					{
						analysisStateOpt?.MarkDeclarationComplete(symbol, i, analysisScope.Analyzers);
					}
					else if (!TryExecuteDeclaringReferenceActions(syntaxReference, i, symbolEvent, analysisScope, analysisStateOpt, flag, flag3, flag2, flag4, flag5, cancellationToken))
					{
						result = false;
					}
				}
			}
		}
		else if (analysisStateOpt != null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			analysisStateOpt.MarkDeclarationsComplete(symbol, analysisScope.Analyzers);
			ImmutableArray<SyntaxReference> declaringSyntaxReferences2 = symbolEvent.DeclaringSyntaxReferences;
			for (int j = 0; j < declaringSyntaxReferences2.Length; j++)
			{
				SyntaxReference declaration = declaringSyntaxReferences2[j];
				ClearCachedAnalysisDataIfAnalyzed(declaration, symbol, j, analysisStateOpt);
			}
		}
		return result;
	}

	private void ClearCachedAnalysisDataIfAnalyzed(SyntaxReference declaration, ISymbol symbol, int declarationIndex, AnalysisState analysisState)
	{
		if (analysisState.IsDeclarationComplete(symbol, declarationIndex))
		{
			compilationData.ClearDeclarationAnalysisData(declaration);
		}
	}

	private DeclarationAnalysisData ComputeDeclarationAnalysisData(ISymbol symbol, SyntaxReference declaration, SemanticModel semanticModel, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayBuilder<DeclarationInfo> instance = ArrayBuilder<DeclarationInfo>.GetInstance();
		SyntaxNode syntax = declaration.GetSyntax(cancellationToken);
		SyntaxNode topmostNodeForDiagnosticAnalysis = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, syntax);
		ComputeDeclarationsInNode(semanticModel, symbol, syntax, topmostNodeForDiagnosticAnalysis, instance, cancellationToken);
		ImmutableArray<DeclarationInfo> immutableArray = instance.ToImmutableAndFree();
		bool flag = analysisScope.FilterSpanOpt.HasValue && !analysisScope.ContainsSpan(topmostNodeForDiagnosticAnalysis.FullSpan);
		ImmutableArray<SyntaxNode> syntaxNodesToAnalyze = GetSyntaxNodesToAnalyze(topmostNodeForDiagnosticAnalysis, symbol, immutableArray, analysisScope, flag, semanticModel, analyzerExecutor);
		return new DeclarationAnalysisData(syntax, topmostNodeForDiagnosticAnalysis, immutableArray, syntaxNodesToAnalyze, flag);
	}

	private static void ComputeDeclarationsInNode(SemanticModel semanticModel, ISymbol declaredSymbol, SyntaxNode declaringReferenceSyntax, SyntaxNode topmostNodeForAnalysis, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		int? levelsToCompute = 2;
		bool getSymbol = topmostNodeForAnalysis != declaringReferenceSyntax || declaredSymbol.Kind == SymbolKind.Namespace;
		semanticModel.ComputeDeclarationsInNode(topmostNodeForAnalysis, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	private bool TryExecuteDeclaringReferenceActions(SyntaxReference decl, int declarationIndex, SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool shouldExecuteSyntaxNodeActions, bool shouldExecuteOperationActions, bool shouldExecuteCodeBlockActions, bool shouldExecuteOperationBlockActions, bool isInGeneratedCode, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		SemanticModel semanticModel = ((analysisStateOpt != null) ? compilationData.GetOrCreateCachedSemanticModel(decl.SyntaxTree, symbolEvent.Compilation, cancellationToken) : symbolEvent.SemanticModel(decl));
		bool flag = analysisScope.Analyzers.Length < analyzers.Length && (!analysisScope.FilterSpanOpt.HasValue || analysisScope.FilterSpanOpt.Value.Length >= decl.SyntaxTree.GetRoot(cancellationToken).Span.Length);
		DeclarationAnalysisData orComputeDeclarationAnalysisData = compilationData.GetOrComputeDeclarationAnalysisData(decl, () => ComputeDeclarationAnalysisData(symbol, decl, semanticModel, analysisScope, cancellationToken), flag);
		if (!analysisScope.ShouldAnalyze(orComputeDeclarationAnalysisData.TopmostNodeForAnalysis))
		{
			return true;
		}
		bool flag2 = true;
		if (shouldExecuteSyntaxNodeActions)
		{
			ImmutableArray<SyntaxNode> descendantNodesToAnalyze = orComputeDeclarationAnalysisData.DescendantNodesToAnalyze;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (NodeActionsByAnalyzerAndKind.TryGetValue(analyzer, out var value) && !value.IsEmpty && !analyzerExecutor.TryExecuteSyntaxNodeActions(descendantNodesToAnalyze, value, analyzer, semanticModel, _getKind, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis.FullSpan, decl, declarationIndex, symbol, analysisScope, analysisStateOpt, isInGeneratedCode))
				{
					flag2 = false;
				}
			}
		}
		if (shouldExecuteCodeBlockActions | shouldExecuteOperationActions | shouldExecuteOperationBlockActions)
		{
			ImmutableArray<SyntaxNode> immutableArray = ImmutableArray<SyntaxNode>.Empty;
			IEnumerable<CodeBlockAnalyzerActions> enumerable = null;
			foreach (DeclarationInfo item in orComputeDeclarationAnalysisData.DeclarationsInNode)
			{
				if (item.DeclaredNode != orComputeDeclarationAnalysisData.TopmostNodeForAnalysis && item.DeclaredNode != orComputeDeclarationAnalysisData.DeclaringReferenceSyntax)
				{
					continue;
				}
				immutableArray = item.ExecutableCodeBlocks;
				if (!immutableArray.Any())
				{
					continue;
				}
				if (shouldExecuteCodeBlockActions | shouldExecuteOperationBlockActions)
				{
					enumerable = GetCodeBlockActions(analysisScope);
				}
				if (!(shouldExecuteOperationActions | shouldExecuteOperationBlockActions))
				{
					break;
				}
				ImmutableArray<IOperation> operationBlocksToAnalyze = GetOperationBlocksToAnalyze(immutableArray, semanticModel, cancellationToken);
				ImmutableArray<IOperation> immutableArray2 = ImmutableArray<IOperation>.Empty;
				try
				{
					immutableArray2 = GetOperationsToAnalyze(operationBlocksToAnalyze);
				}
				catch (Exception ex) when (ex is InsufficientExecutionStackException || FatalError.ReportWithoutCrashUnlessCanceled(ex))
				{
					Diagnostic arg = AnalyzerExecutor.CreateDriverExceptionDiagnostic(ex);
					DiagnosticAnalyzer arg2 = analyzers[0];
					analyzerExecutor.OnAnalyzerException(ex, arg2, arg);
				}
				if (immutableArray2.IsEmpty)
				{
					break;
				}
				if (shouldExecuteOperationActions)
				{
					foreach (DiagnosticAnalyzer analyzer2 in analysisScope.Analyzers)
					{
						if (OperationActionsByAnalyzerAndKind.TryGetValue(analyzer2, out var value2) && !value2.IsEmpty && !analyzerExecutor.TryExecuteOperationActions(immutableArray2, value2, analyzer2, semanticModel, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis.FullSpan, decl, declarationIndex, symbol, analysisScope, analysisStateOpt, isInGeneratedCode))
						{
							flag2 = false;
						}
					}
				}
				if (!shouldExecuteOperationBlockActions)
				{
					break;
				}
				foreach (CodeBlockAnalyzerActions item2 in enumerable)
				{
					ImmutableArray<OperationBlockStartAnalyzerAction> operationBlockStartActions = item2.OperationBlockStartActions;
					if (operationBlockStartActions.IsEmpty)
					{
						ImmutableArray<OperationBlockAnalyzerAction> operationBlockActions = item2.OperationBlockActions;
						if (operationBlockActions.IsEmpty)
						{
							operationBlockActions = item2.OperationBlockEndActions;
							if (operationBlockActions.IsEmpty)
							{
								continue;
							}
						}
					}
					if (!analyzerExecutor.TryExecuteOperationBlockActions(item2.OperationBlockStartActions, item2.OperationBlockActions, item2.OperationBlockEndActions, item2.Analyzer, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis, symbol, operationBlocksToAnalyze, immutableArray2, semanticModel, decl, declarationIndex, analysisScope, analysisStateOpt, isInGeneratedCode))
					{
						flag2 = false;
					}
				}
				break;
			}
			if (immutableArray.Any() & shouldExecuteCodeBlockActions)
			{
				foreach (CodeBlockAnalyzerActions item3 in enumerable)
				{
					ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> codeBlockStartActions = item3.CodeBlockStartActions;
					if (codeBlockStartActions.IsEmpty)
					{
						ImmutableArray<CodeBlockAnalyzerAction> codeBlockActions = item3.CodeBlockActions;
						if (codeBlockActions.IsEmpty)
						{
							codeBlockActions = item3.CodeBlockEndActions;
							if (codeBlockActions.IsEmpty)
							{
								continue;
							}
						}
					}
					if (!analyzerExecutor.TryExecuteCodeBlockActions(item3.CodeBlockStartActions, item3.CodeBlockActions, item3.CodeBlockEndActions, item3.Analyzer, orComputeDeclarationAnalysisData.TopmostNodeForAnalysis, symbol, immutableArray, semanticModel, _getKind, decl, declarationIndex, analysisScope, analysisStateOpt, isInGeneratedCode))
					{
						flag2 = false;
					}
				}
			}
		}
		if (flag2 && analysisStateOpt != null && !orComputeDeclarationAnalysisData.IsPartialAnalysis)
		{
			foreach (DiagnosticAnalyzer analyzer3 in analysisScope.Analyzers)
			{
				analysisStateOpt.MarkDeclarationComplete(symbol, declarationIndex, analyzer3);
			}
			if (flag)
			{
				ClearCachedAnalysisDataIfAnalyzed(decl, symbol, declarationIndex, analysisStateOpt);
			}
		}
		return flag2;
	}

	private IEnumerable<CodeBlockAnalyzerActions> GetCodeBlockActions(AnalysisScope analysisScope)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (!CodeBlockStartActionsByAnalyzer.TryGetValue(analyzer, out var value))
			{
				value = ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>.Empty;
			}
			if (!CodeBlockActionsByAnalyzer.TryGetValue(analyzer, out var value2))
			{
				value2 = ImmutableArray<CodeBlockAnalyzerAction>.Empty;
			}
			if (!CodeBlockEndActionsByAnalyzer.TryGetValue(analyzer, out var value3))
			{
				value3 = ImmutableArray<CodeBlockAnalyzerAction>.Empty;
			}
			if (!OperationBlockStartActionsByAnalyzer.TryGetValue(analyzer, out var value4))
			{
				value4 = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;
			}
			if (!OperationBlockActionsByAnalyzer.TryGetValue(analyzer, out var value5))
			{
				value5 = ImmutableArray<OperationBlockAnalyzerAction>.Empty;
			}
			if (!OperationBlockEndActionsByAnalyzer.TryGetValue(analyzer, out var value6))
			{
				value6 = ImmutableArray<OperationBlockAnalyzerAction>.Empty;
			}
			if (!value.IsEmpty || !value2.IsEmpty || !value3.IsEmpty || !value4.IsEmpty || !value5.IsEmpty || !value6.IsEmpty)
			{
				yield return new CodeBlockAnalyzerActions
				{
					Analyzer = analyzer,
					CodeBlockStartActions = value,
					CodeBlockActions = value2,
					CodeBlockEndActions = value3,
					OperationBlockStartActions = value4,
					OperationBlockActions = value5,
					OperationBlockEndActions = value6
				};
			}
		}
	}

	private static ImmutableArray<SyntaxNode> GetSyntaxNodesToAnalyze(SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<DeclarationInfo> declarationsInNode, AnalysisScope analysisScope, bool isPartialDeclAnalysis, SemanticModel semanticModel, AnalyzerExecutor analyzerExecutor)
	{
		HashSet<SyntaxNode> descendantDeclsToSkipOpt = null;
		bool flag = true;
		foreach (DeclarationInfo item2 in declarationsInNode)
		{
			analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
			if (item2.DeclaredNode != declaredNode)
			{
				if (IsEquivalentSymbol(declaredSymbol, item2.DeclaredSymbol))
				{
					if (flag)
					{
						break;
					}
					return ImmutableArray<SyntaxNode>.Empty;
				}
				SyntaxNode item = item2.DeclaredNode;
				ISymbol symbol = item2.DeclaredSymbol ?? semanticModel.GetDeclaredSymbol(item2.DeclaredNode, analyzerExecutor.CancellationToken);
				if (symbol != null)
				{
					item = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, item2.DeclaredNode);
				}
				descendantDeclsToSkipOpt = descendantDeclsToSkipOpt ?? new HashSet<SyntaxNode>();
				descendantDeclsToSkipOpt.Add(item);
			}
			flag = false;
		}
		ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
		foreach (SyntaxNode item3 in declaredNode.DescendantNodesAndSelf(shouldAddNode, descendIntoTrivia: true))
		{
			if (shouldAddNode(item3) && (!isPartialDeclAnalysis || analysisScope.ShouldAnalyze(item3)))
			{
				instance.Add(item3);
			}
		}
		return instance.ToImmutableAndFree();
		bool shouldAddNode(SyntaxNode node)
		{
			if (descendantDeclsToSkipOpt != null)
			{
				return !descendantDeclsToSkipOpt.Contains(node);
			}
			return true;
		}
	}

	private static bool IsEquivalentSymbol(ISymbol declaredSymbol, ISymbol otherSymbol)
	{
		if (declaredSymbol.Equals(otherSymbol))
		{
			return true;
		}
		if (otherSymbol != null && declaredSymbol.Kind == SymbolKind.Namespace && otherSymbol.Kind == SymbolKind.Namespace && declaredSymbol.Name == otherSymbol.Name)
		{
			return declaredSymbol.ToDisplayString() == otherSymbol.ToDisplayString();
		}
		return false;
	}

	private static ImmutableArray<IOperation> GetOperationBlocksToAnalyze(ImmutableArray<SyntaxNode> executableBlocks, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		foreach (SyntaxNode item in executableBlocks)
		{
			IOperation operation = semanticModel.GetOperation(item, cancellationToken);
			if (operation != null)
			{
				instance.AddRange(operation);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<IOperation> GetOperationsToAnalyze(ImmutableArray<IOperation> operationBlocks)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		bool flag = true;
		foreach (IOperation item in operationBlocks)
		{
			if (flag && item.Parent != null)
			{
				OperationKind kind = item.Parent.Kind;
				if ((uint)(kind - 88) <= 1u)
				{
					instance.Add(item.Parent);
				}
				flag = false;
			}
			instance.AddRange(item.DescendantsAndSelf());
		}
		return instance.ToImmutableAndFree();
	}
}
