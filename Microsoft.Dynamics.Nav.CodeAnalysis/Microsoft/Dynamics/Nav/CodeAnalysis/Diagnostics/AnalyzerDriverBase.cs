using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class AnalyzerDriverBase : IDisposable
{
	internal class CompilationData
	{
		private readonly Dictionary<SyntaxReference, DeclarationAnalysisData> declarationAnalysisDataMap;

		private readonly ObjectPool<DeclarationAnalysisData> declarationAnalysisDataPool;

		private readonly Dictionary<SyntaxTree, SemanticModel> semanticModelsMap;

		public CompilationData()
		{
			semanticModelsMap = new Dictionary<SyntaxTree, SemanticModel>();
			declarationAnalysisDataMap = new Dictionary<SyntaxReference, DeclarationAnalysisData>();
			declarationAnalysisDataPool = new ObjectPool<DeclarationAnalysisData>(() => new DeclarationAnalysisData());
		}

		public SemanticModel GetOrCreateCachedSemanticModel(SyntaxTree tree, Compilation compilation)
		{
			SemanticModel value;
			lock (semanticModelsMap)
			{
				if (semanticModelsMap.TryGetValue(tree, out value) && value.Compilation == compilation)
				{
					return value;
				}
			}
			value = compilation.GetSemanticModel(tree);
			lock (semanticModelsMap)
			{
				semanticModelsMap[tree] = value;
				return value;
			}
		}

		public bool RemoveCachedSemanticModel(SyntaxTree tree)
		{
			lock (semanticModelsMap)
			{
				return semanticModelsMap.Remove(tree);
			}
		}

		internal DeclarationAnalysisData GetOrComputeDeclarationAnalysisData(SyntaxReference declaration, Func<Func<DeclarationAnalysisData>, DeclarationAnalysisData> computeDeclarationAnalysisData, bool cacheAnalysisData)
		{
			if (!cacheAnalysisData)
			{
				return computeDeclarationAnalysisData(declarationAnalysisDataPool.Allocate);
			}
			DeclarationAnalysisData value;
			lock (declarationAnalysisDataMap)
			{
				if (declarationAnalysisDataMap.TryGetValue(declaration, out value))
				{
					return value;
				}
			}
			value = computeDeclarationAnalysisData(declarationAnalysisDataPool.Allocate);
			lock (declarationAnalysisDataMap)
			{
				declarationAnalysisDataMap[declaration] = value;
				return value;
			}
		}

		internal void ClearDeclarationAnalysisData(SyntaxReference declaration)
		{
			lock (declarationAnalysisDataMap)
			{
				if (declarationAnalysisDataMap.TryGetValue(declaration, out var value))
				{
					declarationAnalysisDataMap.Remove(declaration);
					value.Free();
					declarationAnalysisDataPool.Free(value);
				}
			}
		}
	}

	internal class DeclarationAnalysisData
	{
		public SyntaxNode DeclaringReferenceSyntax { get; set; }

		public SyntaxNode TopmostNodeForAnalysis { get; set; }

		public List<DeclarationInfo> DeclarationsInNode { get; }

		public List<SyntaxNode> DescendantNodesToAnalyze { get; }

		public bool IsPartialAnalysis { get; set; }

		public DeclarationAnalysisData()
		{
			DeclarationsInNode = new List<DeclarationInfo>();
			DescendantNodesToAnalyze = new List<SyntaxNode>();
		}

		public void Free()
		{
			DeclaringReferenceSyntax = null;
			TopmostNodeForAnalysis = null;
			DeclarationsInNode.Clear();
			DescendantNodesToAnalyze.Clear();
			IsPartialAnalysis = false;
		}
	}

	private const int MaxSymbolKind = 100;

	protected static readonly ConditionalWeakTable<Compilation, CompilationData> CompilationDataCache = new ConditionalWeakTable<Compilation, CompilationData>();

	private readonly int workerCount = Environment.ProcessorCount;

	private ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim> analyzerGateMap = ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Empty;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> compilationActionsMap;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> compilationEndActionsMap;

	private bool initializeSucceeded;

	private Task initializeTask;

	private Task primaryTask;

	private CancellationTokenRegistration queueRegistration;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>> semanticModelActionsMap;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>> symbolActionsByKind;

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>> syntaxTreeActionsMap;

	internal AnalyzerActions AnalyzerActions;

	protected AnalyzerExecutor AnalyzerExecutor;

	protected CompilationData compilationData;

	protected internal AnalyzerManager AnalyzerManager { get; }

	protected internal ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

	public AsyncQueue<CompilationEvent> CompilationEventQueue { get; private set; }

	public DiagnosticQueue DiagnosticQueue { get; private set; }

	public Task WhenInitializedTask => initializeTask;

	public Task WhenCompletedTask => primaryTask;

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => AnalyzerExecutor.AnalyzerExecutionTimes;

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isComment")]
	protected AnalyzerDriverBase(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, Func<SyntaxTrivia, bool> isComment)
	{
		Analyzers = analyzers;
		AnalyzerManager = analyzerManager;
	}

	public void Dispose()
	{
		CompilationEventQueue?.TryComplete();
		DiagnosticQueue?.TryComplete();
		queueRegistration.Dispose();
	}

	[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "nullFilter")]
	public static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Diagnostic> addExceptionDiagnostic, bool reportAnalyzer, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = (Exception ex, DiagnosticAnalyzer analyzer, Diagnostic diagnostic) =>
		{
			addExceptionDiagnostic?.Invoke(diagnostic);
		};
		return CreateAndAttachToCompilation(compilation, analyzers, options, analyzerManager, onAnalyzerException, reportAnalyzer, out newCompilation, cancellationToken);
	}

	public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Compilation compilation)
	{
		DiagnosticBag allDiagnostics = DiagnosticBag.GetInstance();
		if (CompilationEventQueue.IsCompleted)
		{
			await WhenCompletedTask.ConfigureAwait(continueOnCapturedContext: false);
			if (WhenCompletedTask.IsFaulted)
			{
				OnDriverException(WhenCompletedTask, AnalyzerExecutor, Analyzers);
			}
		}
		bool reportSuppressedDiagnostics = compilation.Options.ReportSuppressedDiagnostics;
		Diagnostic d;
		while (DiagnosticQueue.TryDequeue(out d))
		{
			if (reportSuppressedDiagnostics || !d.IsSuppressed)
			{
				allDiagnostics.Add(d);
			}
		}
		return allDiagnostics.ToReadOnlyAndFree();
	}

	public ImmutableArray<Diagnostic> DequeueLocalDiagnostics(DiagnosticAnalyzer analyzer, bool syntax, Compilation compilation)
	{
		return FilterDiagnosticsSuppressedInSource(syntax ? DiagnosticQueue.DequeueLocalSyntaxDiagnostics(analyzer) : DiagnosticQueue.DequeueLocalSemanticDiagnostics(analyzer), compilation);
	}

	public ImmutableArray<Diagnostic> DequeueNonLocalDiagnostics(DiagnosticAnalyzer analyzer, Compilation compilation)
	{
		return FilterDiagnosticsSuppressedInSource(DiagnosticQueue.DequeueNonLocalDiagnostics(analyzer), compilation);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	public static SemanticModel GetOrCreateCachedSemanticModel(SyntaxTree tree, Compilation compilation, CancellationToken cancellationToken)
	{
		return GetOrCreateCachedCompilationData(compilation).GetOrCreateCachedSemanticModel(tree, compilation);
	}

	public static bool RemoveCachedSemanticModel(SyntaxTree tree, Compilation compilation)
	{
		if (CompilationDataCache.TryGetValue(compilation, out var value))
		{
			return value.RemoveCachedSemanticModel(tree);
		}
		return false;
	}

	protected abstract bool TryExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken);

	internal void Initialize(Compilation compilation, CompilationWithAnalyzersOptions analysisOptions, CompilationData initialCompilationData, bool categorizeDiagnostics, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(initializeTask == null);
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
		AnalyzerExecutor analyzerExecutor = AnalyzerExecutor.Create(compilation, AnalyzerOptions.Empty, addNotCategorizedDiagnosticOpt, onAnalyzerException, null, IsCompilerAnalyzer, AnalyzerManager, (DiagnosticAnalyzer d) => false, (Diagnostic d, DiagnosticAnalyzer dd, Compilation c, CancellationToken cc) => false, GetAnalyzerGate, analysisOptions.LogAnalyzerExecutionTime, addCategorizedLocalDiagnosticOpt, addCategorizedNonLocalDiagnosticOpt, cancellationToken);
		Initialize(analyzerExecutor, diagnosticQueue, initialCompilationData, cancellationToken);
	}

	internal async Task AttachQueueAndProcessAllEventsAsync(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		try
		{
			if (initializeSucceeded)
			{
				CompilationEventQueue = eventQueue;
				queueRegistration = default;
				await ExecutePrimaryAnalysisTaskAsync(analysisScope, analysisStateOpt, usingPrePopulatedEventQueue: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				primaryTask = Task.FromResult(result: true);
			}
		}
		finally
		{
			if (primaryTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				primaryTask = taskCompletionSource.Task;
			}
		}
	}

	internal void AttachQueueAndStartProcessingEvents(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		try
		{
			if (initializeSucceeded)
			{
				CompilationEventQueue = eventQueue;
				queueRegistration = cancellationToken.Register(() =>
				{
					CompilationEventQueue.TryComplete();
					DiagnosticQueue.TryComplete();
				});
				primaryTask = ExecutePrimaryAnalysisTaskAsync(analysisScope, null, usingPrePopulatedEventQueue: false, cancellationToken).ContinueWith((Task c) => DiagnosticQueue.TryComplete(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}
		}
		finally
		{
			if (primaryTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				primaryTask = taskCompletionSource.Task;
				DiagnosticQueue.TryComplete();
			}
		}
	}

	internal static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool reportAnalyzer, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		AnalyzerDriver analyzerDriver = Compilation.AnalyzerForLanguage(analyzers, analyzerManager);
		newCompilation = compilation.WithEventQueue(new AsyncQueue<CompilationEvent>());
		bool categorizeDiagnostics = false;
		analyzerDriver.Initialize(analysisOptions: new CompilationWithAnalyzersOptions(options, onAnalyzerException, concurrentAnalysis: true, reportAnalyzer, reportSuppressedDiagnostics: false), compilation: newCompilation, initialCompilationData: new CompilationData(), categorizeDiagnostics: categorizeDiagnostics, cancellationToken: cancellationToken);
		analyzerDriver.AttachQueueAndStartProcessingEvents(analysisScope: new AnalysisScope(newCompilation, analyzers, newCompilation.Options.ConcurrentBuild, categorizeDiagnostics), eventQueue: newCompilation.EventQueue, cancellationToken: cancellationToken);
		return analyzerDriver;
	}

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		return AnalyzerExecutor.ResetAnalyzerExecutionTime(analyzer);
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

	internal async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		AnalyzerExecutor analyzerExecutor = AnalyzerExecutor.WithCancellationToken(cancellationToken);
		return AnalyzerActionCounts.Create(await AnalyzerManager.GetAnalyzerActionsAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		return analyzerManager.IsDiagnosticAnalyzerSuppressed(analyzer, options, IsCompilerAnalyzer, analyzerExecutor);
	}

	internal static CompilationData GetOrCreateCachedCompilationData(Compilation compilation)
	{
		return CompilationDataCache.GetValue(compilation, (Compilation c) => new CompilationData());
	}

	internal static bool RemoveCachedCompilationData(Compilation compilation)
	{
		return CompilationDataCache.Remove(compilation);
	}

	internal void ClearAnalyzerState()
	{
		if (AnalyzerManager != null)
		{
			AnalyzerManager.ClearAnalyzerState(Analyzers);
		}
	}

	private void Initialize(AnalyzerExecutor analyzerExecutor, DiagnosticQueue diagnosticQueue, CompilationData initialCompilationData, CancellationToken cancellationToken)
	{
		try
		{
			DebugAssertHelper.Assert(initializeTask == null);
			AnalyzerExecutor = analyzerExecutor;
			compilationData = initialCompilationData;
			DiagnosticQueue = diagnosticQueue;
			initializeTask = Task.Run(async () =>
			{
				ImmutableArray<DiagnosticAnalyzer> unsuppressedAnalyzers = GetUnsuppressedAnalyzers(Analyzers, AnalyzerManager, analyzerExecutor);
				AnalyzerActions analyzerActions = await GetAnalyzerActionsAsync(unsuppressedAnalyzers, AnalyzerManager, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
				AnalyzerActions = analyzerActions;
				ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim> immutableDictionary = await GetAnalyzerGateMapAsync(unsuppressedAnalyzers, AnalyzerManager, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
				analyzerGateMap = immutableDictionary;
				symbolActionsByKind = MakeSymbolActionsByKind();
				semanticModelActionsMap = MakeSemanticModelActionsByAnalyzer();
				syntaxTreeActionsMap = MakeSyntaxTreeActionsByAnalyzer();
				compilationActionsMap = MakeCompilationActionsByAnalyzer(AnalyzerActions.CompilationActions);
				compilationEndActionsMap = MakeCompilationActionsByAnalyzer(AnalyzerActions.CompilationEndActions);
			}, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			initializeSucceeded = true;
		}
		finally
		{
			if (initializeTask == null)
			{
				TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				initializeTask = taskCompletionSource.Task;
				taskCompletionSource = new TaskCompletionSource<int>();
				taskCompletionSource.SetCanceled();
				primaryTask = taskCompletionSource.Task;
				DiagnosticQueue.TryComplete();
			}
		}
	}

	private SemaphoreSlim GetAnalyzerGate(DiagnosticAnalyzer analyzer)
	{
		if (analyzerGateMap.TryGetValue(analyzer, out var value))
		{
			DebugAssertHelper.Assert(value != null);
			return value;
		}
		return null;
	}

	private async Task ExecutePrimaryAnalysisTaskAsync(AnalysisScope analysisScope, AnalysisState analysisStateOpt, bool usingPrePopulatedEventQueue, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(analysisScope != null);
		DebugAssertHelper.Assert(WhenInitializedTask != null);
		await WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
		if (WhenInitializedTask.IsFaulted)
		{
			OnDriverException(WhenInitializedTask, AnalyzerExecutor, analysisScope.Analyzers);
		}
		else if (!WhenInitializedTask.IsCanceled)
		{
			AnalyzerExecutor = AnalyzerExecutor.WithCancellationToken(cancellationToken);
			await ProcessCompilationEventsAsync(analysisScope, analysisStateOpt, usingPrePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private static void OnDriverException(Task faultedTask, AnalyzerExecutor analyzerExecutor, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		DebugAssertHelper.Assert(faultedTask.IsFaulted);
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
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (syntaxTreeActionsMap.TryGetValue(analyzer, out var value))
				{
					AnalyzerExecutor.TryExecuteSyntaxTreeActions(value, analyzer, syntaxTree, analysisScope, analysisStateOpt, isGeneratedCode: false);
				}
				else
				{
					analysisStateOpt?.MarkSyntaxAnalysisComplete(syntaxTree, analyzer);
				}
			}
		}
	}

	private static ImmutableArray<Diagnostic> FilterDiagnosticsSuppressedInSource(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		if (diagnostics.IsEmpty)
		{
			return diagnostics;
		}
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		for (int i = 0; i < diagnostics.Length; i++)
		{
			builder.Add(diagnostics[i]);
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>> MakeSymbolActionsByKind()
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>>();
		foreach (IGrouping<DiagnosticAnalyzer, SymbolAnalyzerAction> item in from action in AnalyzerActions.SymbolActions
			group action by action.Analyzer)
		{
			List<ArrayBuilder<SymbolAnalyzerAction>> list = new List<ArrayBuilder<SymbolAnalyzerAction>>();
			foreach (SymbolAnalyzerAction item2 in item)
			{
				foreach (SymbolKind item3 in Enumerable.Distinct(item2.Kinds))
				{
					if (item3 <= SymbolKind.QueryDataItem)
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
		foreach (IGrouping<DiagnosticAnalyzer, SyntaxTreeAnalyzerAction> item in from action in AnalyzerActions.SyntaxTreeActions
			group action by action.Analyzer)
		{
			builder.Add(item.Key, item.ToImmutableArray());
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>> MakeSemanticModelActionsByAnalyzer()
	{
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>>();
		foreach (IGrouping<DiagnosticAnalyzer, SemanticModelAnalyzerAction> item in from action in AnalyzerActions.SemanticModelActions
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
				int workerCount = (prePopulatedEventQueue ? Math.Min(CompilationEventQueue.Count, this.workerCount) : this.workerCount);
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
					if (!prePopulatedEventQueue)
					{
						d = await CompilationEventQueue.DequeueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (!CompilationEventQueue.TryDequeue(out d))
					{
						return completedEvent;
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
			if (!AnalysisScope.ShouldSkipSymbolAnalysis(symbolEvent) && !TryExecuteSymbolActions(symbolEvent, analysisScope, analysisStateOpt, cancellationToken))
			{
				result = false;
			}
			if (!AnalysisScope.ShouldSkipDeclarationAnalysis(symbol) && !TryExecuteDeclaringReferenceActions(symbolEvent, analysisScope, analysisStateOpt, cancellationToken))
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

	private bool TryExecuteSymbolActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		if (!analysisScope.ShouldAnalyze(symbol))
		{
			return true;
		}
		bool result = true;
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (symbolActionsByKind.TryGetValue(analyzer, out var value) && (int)symbol.Kind < value.Length)
			{
				if (!AnalyzerExecutor.TryExecuteSymbolActions(value[(int)symbol.Kind], analyzer, symbolEvent, GetTopmostNodeForAnalysis, analysisScope, analysisStateOpt))
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

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	private bool TryProcessCompilationUnitCompleted(CompilationUnitCompletedEvent completedEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = ((analysisStateOpt != null) ? compilationData.GetOrCreateCachedSemanticModel(completedEvent.CompilationUnit, completedEvent.Compilation) : completedEvent.SemanticModel);
		if (!analysisScope.ShouldAnalyze(semanticModel.SyntaxTree))
		{
			return true;
		}
		try
		{
			bool result = true;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (semanticModelActionsMap.TryGetValue(analyzer, out var value))
				{
					if (!AnalyzerExecutor.TryExecuteSemanticModelActions(value, analyzer, semanticModel, completedEvent, analysisScope, analysisStateOpt, isGeneratedCode: false))
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
		return TryExecuteCompilationActions(compilationActionsMap, startedEvent, analysisScope, analysisStateOpt, cancellationToken);
	}

	private bool TryProcessCompilationCompleted(CompilationCompletedEvent endEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		return TryExecuteCompilationActions(compilationEndActionsMap, endEvent, analysisScope, analysisStateOpt, cancellationToken);
	}

	private bool TryExecuteCompilationActions(ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>> actionsMap, CompilationEvent compilationEvent, AnalysisScope analysisScope, AnalysisState analysisStateOpt, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(compilationEvent is CompilationStartedEvent || compilationEvent is CompilationCompletedEvent);
		try
		{
			bool result = true;
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (actionsMap.TryGetValue(analyzer, out var value))
				{
					if (!AnalyzerExecutor.TryExecuteCompilationActions(value, analyzer, compilationEvent, analysisScope, analysisStateOpt))
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
			DebugAssertHelper.Assert(!IsDiagnosticAnalyzerSuppressed(item, analyzerExecutor.Compilation.Options, analyzerManager, analyzerExecutor));
			AnalyzerActions analyzerActions = await analyzerManager.GetAnalyzerActionsAsync(item, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
			if (analyzerActions != null)
			{
				allAnalyzerActions = allAnalyzerActions.Append(analyzerActions);
			}
		}
		return allAnalyzerActions;
	}

	private static Task<ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>> GetAnalyzerGateMapAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor)
	{
		ImmutableDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, SemaphoreSlim>();
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			DebugAssertHelper.Assert(!IsDiagnosticAnalyzerSuppressed(item, analyzerExecutor.Compilation.Options, analyzerManager, analyzerExecutor));
		}
		return Task.FromResult(builder.ToImmutable());
	}

	private static bool IsCompilerAnalyzer(DiagnosticAnalyzer analyzer)
	{
		return analyzer is CompilerDiagnosticAnalyzer;
	}
}
