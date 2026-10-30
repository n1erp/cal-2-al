using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public class CompilationWithAnalyzers
{
	private readonly Compilation _compilation;

	private readonly AnalyzerDriver.CompilationData _compilationData;

	private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

	private readonly CompilationWithAnalyzersOptions _analysisOptions;

	private readonly CancellationToken _cancellationToken;

	private readonly AnalyzerManager _analyzerManager;

	private readonly ObjectPool<AnalyzerDriver> _driverPool;

	private readonly AnalysisState _analysisState;

	private readonly AnalysisResultBuilder _analysisResultBuilder;

	private readonly ConcurrentSet<Diagnostic> _exceptionDiagnostics = new ConcurrentSet<Diagnostic>();

	private readonly object _executingTasksLock = new object();

	private readonly Dictionary<SyntaxTree, Tuple<Task, CancellationTokenSource>> _executingConcurrentTreeTasksOpt;

	private Tuple<Task, CancellationTokenSource> _executingCompilationOrNonConcurrentTreeTask;

	private int _currentToken;

	private readonly Dictionary<Task, int> _concurrentTreeTaskTokensOpt;

	private readonly ObjectPool<AsyncQueue<CompilationEvent>> _eventQueuePool = new ObjectPool<AsyncQueue<CompilationEvent>>(() => new AsyncQueue<CompilationEvent>());

	private static readonly AsyncQueue<CompilationEvent> s_EmptyEventQueue = new AsyncQueue<CompilationEvent>();

	public Compilation Compilation => _compilation;

	public ImmutableArray<DiagnosticAnalyzer> Analyzers => _analyzers;

	public CompilationWithAnalyzersOptions AnalysisOptions => _analysisOptions;

	public CancellationToken CancellationToken => _cancellationToken;

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
		: this(compilation, analyzers, new CompilationWithAnalyzersOptions(options, null, concurrentAnalysis: true, logAnalyzerExecutionTime: true, reportSuppressedDiagnostics: false, null), cancellationToken)
	{
	}

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
		: this(compilation, analyzers, analysisOptions, CancellationToken.None)
	{
	}

	private CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions, CancellationToken cancellationToken)
	{
		CompilationWithAnalyzers compilationWithAnalyzers = this;
		VerifyArguments(compilation, analyzers, analysisOptions);
		compilation = compilation.WithOptions(compilation.Options.WithReportSuppressedDiagnostics(analysisOptions.ReportSuppressedDiagnostics)).WithEventQueue(new AsyncQueue<CompilationEvent>());
		_compilation = compilation;
		_analyzers = analyzers;
		_analysisOptions = analysisOptions;
		_cancellationToken = cancellationToken;
		_compilationData = new AnalyzerDriver.CompilationData(_compilation);
		_analysisState = new AnalysisState(analyzers, _compilationData, _compilation.Options);
		_analysisResultBuilder = new AnalysisResultBuilder(analysisOptions.LogAnalyzerExecutionTime, analyzers);
		_analyzerManager = new AnalyzerManager(analyzers);
		_driverPool = new ObjectPool<AnalyzerDriver>(() => compilationWithAnalyzers._compilation.AnalyzerForLanguage(analyzers, compilationWithAnalyzers._analyzerManager));
		_executingConcurrentTreeTasksOpt = (analysisOptions.ConcurrentAnalysis ? new Dictionary<SyntaxTree, Tuple<Task, CancellationTokenSource>>() : null);
		_concurrentTreeTaskTokensOpt = (analysisOptions.ConcurrentAnalysis ? new Dictionary<Task, int>() : null);
		_executingCompilationOrNonConcurrentTreeTask = null;
	}

	private void AddExceptionDiagnostic(Exception exception, DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
	{
		_analysisOptions.OnAnalyzerException?.Invoke(exception, analyzer, diagnostic);
		_exceptionDiagnostics.Add(diagnostic);
	}

	private static void VerifyArguments(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
	{
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (analysisOptions == null)
		{
			throw new ArgumentNullException("analysisOptions");
		}
		VerifyAnalyzersArgumentForStaticApis(analyzers);
	}

	private static void VerifyAnalyzersArgumentForStaticApis(ImmutableArray<DiagnosticAnalyzer> analyzers, bool allowDefaultOrEmpty = false)
	{
		if (analyzers.IsDefaultOrEmpty)
		{
			if (!allowDefaultOrEmpty)
			{
				throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "analyzers");
			}
			return;
		}
		if (analyzers.Any((DiagnosticAnalyzer a) => a == null))
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentElementCannotBeNull, "analyzers");
		}
		if (analyzers.Distinct().Length != analyzers.Length)
		{
			throw new ArgumentException(CodeAnalysisResources.DuplicateAnalyzerInstances, "analyzers");
		}
	}

	private void VerifyAnalyzersArgument(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		VerifyAnalyzersArgumentForStaticApis(analyzers);
		if (analyzers.Any((DiagnosticAnalyzer a) => !_analyzers.Contains(a)))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "analyzers");
		}
	}

	private void VerifyAnalyzerArgument(DiagnosticAnalyzer analyzer)
	{
		VerifyAnalyzerArgumentForStaticApis(analyzer);
		if (!_analyzers.Contains(analyzer))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "analyzer");
		}
	}

	private static void VerifyAnalyzerArgumentForStaticApis(DiagnosticAnalyzer analyzer)
	{
		if (analyzer == null)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "analyzer");
		}
	}

	private void VerifyExistingAnalyzersArgument(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		VerifyAnalyzersArgumentForStaticApis(analyzers);
		if (analyzers.Any((DiagnosticAnalyzer a) => !_analyzers.Contains(a)))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "_analyzers");
		}
		if (analyzers.Distinct().Length != analyzers.Length)
		{
			throw new ArgumentException(CodeAnalysisResources.DuplicateAnalyzerInstances, "analyzers");
		}
	}

	private void VerifyModel(SemanticModel model)
	{
		if (model == null)
		{
			throw new ArgumentNullException("model");
		}
		if (!_compilation.ContainsSyntaxTree(model.SyntaxTree))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidTree, "model");
		}
	}

	private void VerifyTree(SyntaxTree tree)
	{
		if (tree == null)
		{
			throw new ArgumentNullException("tree");
		}
		if (!_compilation.ContainsSyntaxTree(tree))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidTree, "tree");
		}
	}

	public Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync()
	{
		return GetAnalyzerDiagnosticsAsync(_cancellationToken);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return await GetAnalyzerDiagnosticsWithoutStateTrackingAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerDiagnosticsWithoutStateTrackingAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(CancellationToken cancellationToken)
	{
		return await GetAnalysisResultWithoutStateTrackingAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalysisResultWithoutStateTrackingAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync()
	{
		return GetAllDiagnosticsAsync(_cancellationToken);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return (await GetAllDiagnosticsWithoutStateTrackingAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).AddRange(_exceptionDiagnostics);
	}

	[Obsolete("This API was found to have performance issues and hence has been deprecated. Instead, invoke the API 'GetAnalysisResultAsync' and access the property 'CompilationDiagnostics' on the returned 'AnalysisResult' to fetch the compilation diagnostics.")]
	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[Obsolete("This API was found to have performance issues and hence has been deprecated. Instead, invoke the API 'GetAnalysisResultAsync' and access the property 'CompilationDiagnostics' on the returned 'AnalysisResult' to fetch the compilation diagnostics.")]
	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		await WaitForActiveAnalysisTasksAsync(waitForTreeTasks: true, waitForCompilationOrNonConcurrentTask: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		_ = ImmutableArray<Diagnostic>.Empty;
		AnalysisScope analysisScope = new AnalysisScope(_compilation, analyzers, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
		Func<AsyncQueue<CompilationEvent>> getEventQueue = () => GetPendingEvents(analyzers, includeSourceEvents: true, includeNonSourceEvents: true);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, getEventQueue, 0, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: false, getNonLocalDiagnostics: true);
	}

	private async Task<AnalysisResult> GetAnalysisResultWithoutStateTrackingAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		await ComputeAnalyzerDiagnosticsWithoutStateTrackingAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.ToAnalysisResult(analyzers, cancellationToken);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsWithoutStateTrackingAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		await ComputeAnalyzerDiagnosticsWithoutStateTrackingAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		AnalysisScope analysisScope = new AnalysisScope(_compilation, analyzers, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: true);
	}

	private async Task ComputeAnalyzerDiagnosticsWithoutStateTrackingAsync(CancellationToken cancellationToken)
	{
		ImmutableArray<DiagnosticAnalyzer> analyzers = _analysisResultBuilder.GetPendingAnalyzers(Analyzers);
		if (analyzers.IsEmpty)
		{
			return;
		}
		AsyncQueue<CompilationEvent> eventQueue = _eventQueuePool.Allocate();
		AnalyzerDriver driver = null;
		try
		{
			Compilation compilation = _compilation.WithEventQueue(eventQueue);
			AnalyzerDriver.CompilationData compilationData = new AnalyzerDriver.CompilationData(compilation);
			bool categorizeDiagnostics = true;
			driver = compilation.AnalyzerForLanguage(analyzers, _analyzerManager);
			driver.Initialize(compilation, _analysisOptions, compilationData, categorizeDiagnostics, cancellationToken);
			AnalysisScope analysisScope = new AnalysisScope(compilation, analyzers, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics);
			driver.AttachQueueAndStartProcessingEvents(compilation.EventQueue, analysisScope, cancellationToken);
			compilation.GetDiagnostics(cancellationToken);
			await driver.WhenCompletedTask.ConfigureAwait(continueOnCapturedContext: false);
			Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts> analyzerActionCounts = new Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts>(analyzers.Length);
			foreach (DiagnosticAnalyzer analyzer in analyzers)
			{
				AnalyzerActionCounts value = await driver.GetAnalyzerActionCountsAsync(analyzer, compilation.Options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				analyzerActionCounts.Add(analyzer, value);
			}
			Func<DiagnosticAnalyzer, AnalyzerActionCounts> getAnalyzerActionCounts = (DiagnosticAnalyzer key) => analyzerActionCounts[key];
			_analysisResultBuilder.StoreAnalysisResult(analysisScope, driver, compilation, getAnalyzerActionCounts, fullAnalysisResultForAnalyzersInScope: true);
		}
		finally
		{
			driver?.Dispose();
			FreeEventQueue(eventQueue, _eventQueuePool);
		}
	}

	private async Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsWithoutStateTrackingAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AsyncQueue<CompilationEvent> eventQueue = _eventQueuePool.Allocate();
		AnalyzerDriver driver = null;
		try
		{
			Compilation compilation = _compilation.WithEventQueue(eventQueue);
			AnalyzerDriver.CompilationData compilationData = new AnalyzerDriver.CompilationData(compilation);
			bool categorizeDiagnostics = false;
			driver = compilation.AnalyzerForLanguage(analyzers, _analyzerManager);
			driver.Initialize(compilation, _analysisOptions, compilationData, categorizeDiagnostics, cancellationToken);
			AnalysisScope analysisScope = new AnalysisScope(compilation, analyzers, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics);
			driver.AttachQueueAndStartProcessingEvents(compilation.EventQueue, analysisScope, cancellationToken);
			return compilation.GetDiagnostics(cancellationToken).AddRange(await driver.GetDiagnosticsAsync(compilation).ConfigureAwait(continueOnCapturedContext: false));
		}
		finally
		{
			driver?.Dispose();
			FreeEventQueue(eventQueue, _eventQueuePool);
		}
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		try
		{
			VerifyTree(tree);
			VerifyExistingAnalyzersArgument(analyzers);
			return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsCoreAsync(SyntaxTree tree, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		try
		{
			int newTaskToken = Interlocked.Increment(ref _currentToken);
			AnalysisScope analysisScope = new AnalysisScope(analyzers, tree, null, syntaxAnalysis: true, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
			ImmutableArray<DiagnosticAnalyzer> pendingAnalyzers = _analysisResultBuilder.GetPendingAnalyzers(analyzers);
			if (pendingAnalyzers.Length > 0)
			{
				AnalysisScope analysisScope2 = ((pendingAnalyzers.Length < analyzers.Length) ? analysisScope.WithAnalyzers(pendingAnalyzers) : analysisScope);
				Func<AsyncQueue<CompilationEvent>> getEventQueue = () => s_EmptyEventQueue;
				await ComputeAnalyzerDiagnosticsAsync(analysisScope2, getEventQueue, newTaskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsAsync(SemanticModel model, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		try
		{
			VerifyModel(model);
			return await GetAnalyzerSemanticDiagnosticsCoreAsync(model, filterSpan, Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyModel(model);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerSemanticDiagnosticsCoreAsync(model, filterSpan, analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsCoreAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		try
		{
			int taskToken = Interlocked.Increment(ref _currentToken);
			AnalysisScope analysisScope = new AnalysisScope(analyzers, model.SyntaxTree, filterSpan, syntaxAnalysis: false, _analysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
			ImmutableArray<DiagnosticAnalyzer> pendingAnalyzers = _analysisResultBuilder.GetPendingAnalyzers(analyzers);
			if (pendingAnalyzers.Length > 0)
			{
				AnalysisScope pendingAnalysisScope = ((pendingAnalyzers.Length < analyzers.Length) ? analysisScope.WithAnalyzers(pendingAnalyzers) : analysisScope);
				Func<AsyncQueue<CompilationEvent>> getEventQueue = () => GetPendingEvents(analyzers, model.SyntaxTree);
				do
				{
					await ComputeAnalyzerDiagnosticsAsync(pendingAnalysisScope, getEventQueue, taskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (_analysisOptions.ConcurrentAnalysis && _analysisState.HasPendingSymbolAnalysis(pendingAnalysisScope));
				if (_analysisOptions.ConcurrentAnalysis)
				{
					await WaitForActiveAnalysisTasksAsync(waitForTreeTasks: true, waitForCompilationOrNonConcurrentTask: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task ComputeAnalyzerDiagnosticsAsync(AnalysisScope analysisScope, Func<AsyncQueue<CompilationEvent>> getEventQueue, int newTaskToken, CancellationToken cancellationToken)
	{
		try
		{
			AnalyzerDriver driver = null;
			Task computeTask = null;
			try
			{
				driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				cancellationToken.ThrowIfCancellationRequested();
				await GenerateCompilationEventsAndPopulateEventsCacheAsync(analysisScope, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				bool suspendend;
				do
				{
					suspendend = false;
					CancellationTokenSource cts;
					using (cts = new CancellationTokenSource())
					{
						using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
						try
						{
							CancellationToken linkedCancellationToken = linkedCts.Token;
							Func<Tuple<Task, CancellationTokenSource>> getNewAnalysisTask = () => Tuple.Create(Task.Run(async () =>
							{
								try
								{
									AsyncQueue<CompilationEvent> eventQueue = null;
									try
									{
										eventQueue = getEventQueue();
										linkedCancellationToken.ThrowIfCancellationRequested();
										await ComputeAnalyzerDiagnosticsCoreAsync(driver, eventQueue, analysisScope, linkedCancellationToken).ConfigureAwait(continueOnCapturedContext: false);
									}
									finally
									{
										FreeEventQueue(eventQueue, _eventQueuePool);
									}
								}
								catch (Exception exception2) when (FatalError.ReportUnlessCanceled(exception2))
								{
									throw ExceptionUtilities.Unreachable;
								}
							}, linkedCancellationToken), cts);
							computeTask = await SetActiveAnalysisTaskAsync(getNewAnalysisTask, analysisScope.FilterTreeOpt, newTaskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
							cancellationToken.ThrowIfCancellationRequested();
							await computeTask.ConfigureAwait(continueOnCapturedContext: false);
						}
						catch (OperationCanceledException)
						{
							cancellationToken.ThrowIfCancellationRequested();
							if (!cts.IsCancellationRequested)
							{
								throw;
							}
							suspendend = true;
						}
						finally
						{
							ClearExecutingTask(computeTask, analysisScope.FilterTreeOpt);
							computeTask = null;
						}
					}
				}
				while (suspendend);
			}
			finally
			{
				FreeDriver(driver);
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task GenerateCompilationEventsAndPopulateEventsCacheAsync(AnalysisScope analysisScope, AnalyzerDriver driver, CancellationToken cancellationToken)
	{
		GenerateCompilationEvents(analysisScope, cancellationToken);
		await PopulateEventsCacheAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void GenerateCompilationEvents(AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (analysisScope.FilterTreeOpt == null)
		{
			_compilation.GetDiagnostics(cancellationToken);
		}
		else if (!analysisScope.IsSyntaxOnlyTreeAnalysis)
		{
			_compilationData.GetOrCreateCachedSemanticModel(analysisScope.FilterTreeOpt, _compilation, cancellationToken).GetDiagnostics(null, cancellationToken);
		}
	}

	private async Task PopulateEventsCacheAsync(CancellationToken cancellationToken)
	{
		if (_compilation.EventQueue.Count > 0)
		{
			AnalyzerDriver driver = null;
			try
			{
				driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				cancellationToken.ThrowIfCancellationRequested();
				ImmutableArray<CompilationEvent> compilationEvents = DequeueGeneratedCompilationEvents();
				await _analysisState.OnCompilationEventsGeneratedAsync(compilationEvents, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				FreeDriver(driver);
			}
		}
	}

	private ImmutableArray<CompilationEvent> DequeueGeneratedCompilationEvents()
	{
		ImmutableArray<CompilationEvent>.Builder builder = ImmutableArray.CreateBuilder<CompilationEvent>();
		CompilationEvent d;
		while (_compilation.EventQueue.TryDequeue(out d))
		{
			builder.Add(d);
		}
		return builder.ToImmutable();
	}

	private async Task<AnalyzerDriver> GetAnalyzerDriverAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AnalyzerDriver driver = _driverPool.Allocate();
		try
		{
			if (driver.WhenInitializedTask == null)
			{
				driver.Initialize(_compilation, _analysisOptions, _compilationData, categorizeDiagnostics: true, cancellationToken);
			}
			await driver.WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
			return driver;
		}
		catch (OperationCanceledException)
		{
			FreeDriver(driver);
			throw;
		}
	}

	private void FreeDriver(AnalyzerDriver driver)
	{
		if (driver != null && driver.WhenInitializedTask != null && !driver.WhenInitializedTask.IsCanceled)
		{
			_driverPool.Free(driver);
		}
	}

	private async Task ComputeAnalyzerDiagnosticsCoreAsync(AnalyzerDriver driver, AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		try
		{
			if (eventQueue.Count > 0 || _analysisState.HasPendingSyntaxAnalysis(analysisScope))
			{
				try
				{
					await driver.AttachQueueAndProcessAllEventsAsync(eventQueue, analysisScope, _analysisState, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				finally
				{
					_analysisResultBuilder.StoreAnalysisResult(analysisScope, driver, _compilation, _analysisState.GetAnalyzerActionCounts, fullAnalysisResultForAnalyzersInScope: false);
				}
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private Task<Task> SetActiveAnalysisTaskAsync(Func<Tuple<Task, CancellationTokenSource>> getNewAnalysisTask, SyntaxTree treeOpt, int newTaskToken, CancellationToken cancellationToken)
	{
		if (treeOpt != null)
		{
			return SetActiveTreeAnalysisTaskAsync(getNewAnalysisTask, treeOpt, newTaskToken, cancellationToken);
		}
		return SetActiveCompilationAnalysisTaskAsync(getNewAnalysisTask, cancellationToken);
	}

	private async Task<Task> SetActiveCompilationAnalysisTaskAsync(Func<Tuple<Task, CancellationTokenSource>> getNewCompilationTask, CancellationToken cancellationToken)
	{
		while (true)
		{
			await WaitForActiveAnalysisTasksAsync(waitForTreeTasks: true, waitForCompilationOrNonConcurrentTask: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			lock (_executingTasksLock)
			{
				if ((_executingConcurrentTreeTasksOpt == null || _executingConcurrentTreeTasksOpt.Count == 0) && _executingCompilationOrNonConcurrentTreeTask == null)
				{
					_executingCompilationOrNonConcurrentTreeTask = getNewCompilationTask();
					return _executingCompilationOrNonConcurrentTreeTask.Item1;
				}
			}
		}
	}

	private async Task WaitForActiveAnalysisTasksAsync(bool waitForTreeTasks, bool waitForCompilationOrNonConcurrentTask, CancellationToken cancellationToken)
	{
		ArrayBuilder<Tuple<Task, CancellationTokenSource>> executingTasks = ArrayBuilder<Tuple<Task, CancellationTokenSource>>.GetInstance();
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lock (_executingTasksLock)
			{
				if (waitForTreeTasks)
				{
					Dictionary<SyntaxTree, Tuple<Task, CancellationTokenSource>> executingConcurrentTreeTasksOpt = _executingConcurrentTreeTasksOpt;
					if (executingConcurrentTreeTasksOpt != null && executingConcurrentTreeTasksOpt.Count > 0)
					{
						executingTasks.AddRange(_executingConcurrentTreeTasksOpt.Values);
					}
				}
				if (waitForCompilationOrNonConcurrentTask && _executingCompilationOrNonConcurrentTreeTask != null)
				{
					executingTasks.Add(_executingCompilationOrNonConcurrentTreeTask);
				}
			}
			if (executingTasks.Count == 0)
			{
				break;
			}
			foreach (Tuple<Task, CancellationTokenSource> item in executingTasks)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await item.Item1.ConfigureAwait(continueOnCapturedContext: false);
			}
			executingTasks.Clear();
		}
		executingTasks.Free();
	}

	private async Task<Task> SetActiveTreeAnalysisTaskAsync(Func<Tuple<Task, CancellationTokenSource>> getNewTreeAnalysisTask, SyntaxTree tree, int newTaskToken, CancellationToken cancellationToken)
	{
		try
		{
			while (true)
			{
				Tuple<Task, CancellationTokenSource> value = null;
				lock (_executingTasksLock)
				{
					if (!_analysisOptions.ConcurrentAnalysis)
					{
						if (_executingCompilationOrNonConcurrentTreeTask != null)
						{
							SuspendAnalysis_NoLock(_executingCompilationOrNonConcurrentTreeTask.Item1, _executingCompilationOrNonConcurrentTreeTask.Item2);
							_executingCompilationOrNonConcurrentTreeTask = null;
						}
						return (_executingCompilationOrNonConcurrentTreeTask = getNewTreeAnalysisTask()).Item1;
					}
					if (!_executingConcurrentTreeTasksOpt.TryGetValue(tree, out value) || _concurrentTreeTaskTokensOpt[value.Item1] < newTaskToken)
					{
						if (value != null)
						{
							SuspendAnalysis_NoLock(value.Item1, value.Item2);
						}
						if (_executingCompilationOrNonConcurrentTreeTask != null)
						{
							SuspendAnalysis_NoLock(_executingCompilationOrNonConcurrentTreeTask.Item1, _executingCompilationOrNonConcurrentTreeTask.Item2);
							_executingCompilationOrNonConcurrentTreeTask = null;
						}
						Tuple<Task, CancellationTokenSource> tuple = getNewTreeAnalysisTask();
						_concurrentTreeTaskTokensOpt[tuple.Item1] = newTaskToken;
						_executingConcurrentTreeTasksOpt[tree] = tuple;
						return tuple.Item1;
					}
				}
				await value.Item1.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private void SuspendAnalysis_NoLock(Task computeTask, CancellationTokenSource cts)
	{
		if (!computeTask.IsCompleted)
		{
			cts.Cancel();
		}
	}

	private void ClearExecutingTask(Task computeTask, SyntaxTree treeOpt)
	{
		if (computeTask == null)
		{
			return;
		}
		lock (_executingTasksLock)
		{
			if (treeOpt != null && _analysisOptions.ConcurrentAnalysis)
			{
				if (_executingConcurrentTreeTasksOpt.TryGetValue(treeOpt, out var value) && value.Item1 == computeTask)
				{
					_executingConcurrentTreeTasksOpt.Remove(treeOpt);
				}
				if (_concurrentTreeTaskTokensOpt.ContainsKey(computeTask))
				{
					_concurrentTreeTaskTokensOpt.Remove(computeTask);
				}
			}
			else if (_executingCompilationOrNonConcurrentTreeTask?.Item1 == computeTask)
			{
				_executingCompilationOrNonConcurrentTreeTask = null;
			}
		}
	}

	private AsyncQueue<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree tree)
	{
		AsyncQueue<CompilationEvent> asyncQueue = _eventQueuePool.Allocate();
		foreach (CompilationEvent pendingEvent in _analysisState.GetPendingEvents(analyzers, tree))
		{
			asyncQueue.TryEnqueue(pendingEvent);
		}
		return asyncQueue;
	}

	private AsyncQueue<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeSourceEvents, bool includeNonSourceEvents)
	{
		AsyncQueue<CompilationEvent> asyncQueue = _eventQueuePool.Allocate();
		foreach (CompilationEvent pendingEvent in _analysisState.GetPendingEvents(analyzers, includeSourceEvents, includeNonSourceEvents))
		{
			asyncQueue.TryEnqueue(pendingEvent);
		}
		return asyncQueue;
	}

	private static void FreeEventQueue(AsyncQueue<CompilationEvent> eventQueue, ObjectPool<AsyncQueue<CompilationEvent>> eventQueuePool)
	{
		if (eventQueue == null || eventQueue == s_EmptyEventQueue)
		{
			return;
		}
		if (eventQueue.Count > 0)
		{
			CompilationEvent d;
			while (eventQueue.TryDequeue(out d))
			{
			}
		}
		if (!eventQueue.IsCompleted)
		{
			eventQueuePool.Free(eventQueue);
		}
	}

	public static IEnumerable<Diagnostic> GetEffectiveDiagnostics(IEnumerable<Diagnostic> diagnostics, Compilation compilation)
	{
		return GetEffectiveDiagnostics(diagnostics.AsImmutableOrNull(), compilation);
	}

	public static IEnumerable<Diagnostic> GetEffectiveDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		if (diagnostics.IsDefault)
		{
			throw new ArgumentNullException("diagnostics");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		return GetEffectiveDiagnosticsImpl(diagnostics, compilation);
	}

	private static IEnumerable<Diagnostic> GetEffectiveDiagnosticsImpl(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		SuppressMessageAttributeState suppressMessageState = new SuppressMessageAttributeState(compilation);
		foreach (Diagnostic item in diagnostics)
		{
			if (item != null)
			{
				Diagnostic diagnostic = compilation.Options.FilterDiagnostic(item);
				if (diagnostic != null)
				{
					yield return suppressMessageState.ApplySourceSuppressions(diagnostic);
				}
			}
		}
	}

	public static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = null)
	{
		VerifyAnalyzerArgumentForStaticApis(analyzer);
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		AnalyzerManager analyzerManager = new AnalyzerManager(analyzer);
		AnalyzerExecutor analyzerExecutor = AnalyzerExecutor.CreateForSupportedDiagnostics(onAnalyzerException, analyzerManager);
		return AnalyzerDriver.IsDiagnosticAnalyzerSuppressed(analyzer, options, analyzerManager, analyzerExecutor);
	}

	[Obsolete("This API is no longer required to be invoked. Analyzer state is automatically cleaned up when CompilationWithAnalyzers instance is released.")]
	public static void ClearAnalyzerState(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
	}

	public async Task<AnalyzerTelemetryInfo> GetAnalyzerTelemetryInfoAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		VerifyAnalyzerArgument(analyzer);
		try
		{
			AnalyzerActionCounts actionCounts = await GetAnalyzerActionCountsAsync(analyzer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			TimeSpan analyzerExecutionTime = GetAnalyzerExecutionTime(analyzer);
			return new AnalyzerTelemetryInfo(actionCounts, analyzerExecutionTime);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		AnalyzerDriver driver = null;
		try
		{
			driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
			return await _analysisState.GetOrComputeAnalyzerActionCountsAsync(analyzer, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			FreeDriver(driver);
		}
	}

	private TimeSpan GetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		if (!_analysisOptions.LogAnalyzerExecutionTime)
		{
			return default;
		}
		return _analysisResultBuilder.GetAnalyzerExecutionTime(analyzer);
	}
}
