using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public class CompilationWithAnalyzers
{
	private readonly ObjectPool<AnalyzerDriver> driverPool;

	private readonly AnalysisState analysisState;

	private readonly AnalysisResult analysisResult;

	private readonly ConcurrentSet<Diagnostic> exceptionDiagnostics = new ConcurrentSet<Diagnostic>();

	private readonly object executingTasksLock = new object();

	private readonly Dictionary<SyntaxTree, Tuple<Task, CancellationTokenSource>> executingConcurrentTreeTasksOpt;

	private Tuple<Task, CancellationTokenSource> executingCompilationOrNonConcurrentTreeTask;

	private readonly AnalyzerDriverBase.CompilationData compilationData;

	private int currentToken;

	private readonly Dictionary<Task, int> concurrentTreeTaskTokensOpt;

	private static readonly ObjectPool<AsyncQueue<CompilationEvent>> EventQueuePool = new ObjectPool<AsyncQueue<CompilationEvent>>(() => new AsyncQueue<CompilationEvent>());

	private static readonly AsyncQueue<CompilationEvent> EmptyEventQueue = new AsyncQueue<CompilationEvent>();

	public Compilation Compilation { get; }

	public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

	public CompilationWithAnalyzersOptions AnalysisOptions { get; }

	public CancellationToken CancellationToken { get; }

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
		: this(compilation, analyzers, new CompilationWithAnalyzersOptions(options, null, concurrentAnalysis: true, logAnalyzerExecutionTime: true), cancellationToken)
	{
	}

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
		: this(compilation, analyzers, analysisOptions, CancellationToken.None)
	{
	}

	private CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions, CancellationToken cancellationToken)
	{
		VerifyArguments(compilation, analyzers, analysisOptions);
		Compilation = compilation.WithEventQueue(new AsyncQueue<CompilationEvent>());
		Analyzers = analyzers;
		AnalysisOptions = analysisOptions;
		CancellationToken = cancellationToken;
		compilationData = new AnalyzerDriverBase.CompilationData();
		analysisState = new AnalysisState(analyzers, compilationData);
		analysisResult = new AnalysisResult(analysisOptions.LogAnalyzerExecutionTime, analyzers);
		driverPool = new ObjectPool<AnalyzerDriver>(() => Compilation.AnalyzerForLanguage(analyzers, AnalyzerManager.Instance));
		executingConcurrentTreeTasksOpt = (analysisOptions.ConcurrentAnalysis ? new Dictionary<SyntaxTree, Tuple<Task, CancellationTokenSource>>() : null);
		concurrentTreeTaskTokensOpt = (analysisOptions.ConcurrentAnalysis ? new Dictionary<Task, int>() : null);
		executingCompilationOrNonConcurrentTreeTask = null;
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
		if (Enumerable.Distinct(analyzers).Count() != analyzers.Length)
		{
			throw new ArgumentException(CodeAnalysisResources.DuplicateAnalyzerInstances, "analyzers");
		}
	}

	private void VerifyAnalyzerArgument(DiagnosticAnalyzer analyzer)
	{
		VerifyAnalyzerArgumentForStaticApis(analyzer);
		if (!Analyzers.Contains(analyzer))
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
		if (analyzers.Any((DiagnosticAnalyzer a) => !Analyzers.Contains(a)))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "analyzers");
		}
		if (Enumerable.Distinct(analyzers).Count() != analyzers.Length)
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
		if (!Compilation.ContainsSyntaxTree(model.SyntaxTree))
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
		if (!Compilation.ContainsSyntaxTree(tree))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidTree, "tree");
		}
	}

	public Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync()
	{
		return GetAnalyzerDiagnosticsAsync(CancellationToken);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return await GetAnalyzerDiagnosticsAsync(Analyzers, includeCompilerDiagnostics: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerDiagnosticsAsync(analyzers, includeCompilerDiagnostics: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeCompilerDiagnostics, CancellationToken cancellationToken)
	{
		return await GetAnalyzerDiagnosticsCoreAsync(analyzers, includeCompilerDiagnostics, includeSourceEvents: true, includeNonSourceEvents: true, forceCompleteCompilation: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync()
	{
		return GetAllDiagnosticsAsync(CancellationToken);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return (await GetAnalyzerDiagnosticsAsync(Analyzers, includeCompilerDiagnostics: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).AddRange(exceptionDiagnostics);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		bool forceCompleteCompilation = !(analyzers.SingleOrDefault() is CompilerDiagnosticAnalyzer);
		if (forceCompleteCompilation)
		{
			await GetAnalyzerDiagnosticsCoreAsync(analyzers, includeCompilerDiagnostics: false, includeSourceEvents: true, includeNonSourceEvents: false, forceCompleteCompilation: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return await GetAnalyzerDiagnosticsCoreAsync(analyzers, includeCompilerDiagnostics: false, includeSourceEvents: false, includeNonSourceEvents: true, forceCompleteCompilation, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeCompilerDiagnostics, bool includeSourceEvents, bool includeNonSourceEvents, bool forceCompleteCompilation, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(!includeCompilerDiagnostics | forceCompleteCompilation);
		await WaitForActiveAnalysisTasksAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;
		AnalysisScope analysisScope = new AnalysisScope(Compilation, analyzers, AnalysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
		Action generateCompilationEventsOpt = () =>
		{
			if (forceCompleteCompilation)
			{
				ImmutableArray<Diagnostic> diagnostics3 = Compilation.GetDiagnostics(cancellationToken);
				if (includeCompilerDiagnostics)
				{
					diagnostics = diagnostics3;
				}
			}
		};
		Func<AsyncQueue<CompilationEvent>> getEventQueue = () => GetPendingEvents(analyzers, includeSourceEvents, includeNonSourceEvents);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, generateCompilationEventsOpt, getEventQueue, 0, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<Diagnostic> diagnostics2 = analysisResult.GetDiagnostics(analysisScope, includeSourceEvents, includeNonSourceEvents);
		return diagnostics.AddRange(diagnostics2);
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
			int newTaskToken = Interlocked.Increment(ref currentToken);
			AnalysisScope analysisScope = new AnalysisScope(analyzers, tree, null, syntaxAnalysis: true, AnalysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
			Action generateCompilationEventsOpt = null;
			Func<AsyncQueue<CompilationEvent>> getEventQueue = () => EmptyEventQueue;
			await ComputeAnalyzerDiagnosticsAsync(analysisScope, generateCompilationEventsOpt, getEventQueue, newTaskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return analysisResult.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
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
			int taskToken = Interlocked.Increment(ref currentToken);
			AnalysisScope analysisScope = new AnalysisScope(analyzers, model.SyntaxTree, filterSpan, syntaxAnalysis: false, AnalysisOptions.ConcurrentAnalysis, categorizeDiagnostics: true);
			Action generateCompilationEvents = () =>
			{
				AnalyzerDriverBase.GetOrCreateCachedSemanticModel(model.SyntaxTree, Compilation, cancellationToken);
			};
			Func<AsyncQueue<CompilationEvent>> getEventQueue = () => GetPendingEvents(analyzers, model.SyntaxTree);
			do
			{
				await ComputeAnalyzerDiagnosticsAsync(analysisScope, generateCompilationEvents, getEventQueue, taskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			while (AnalysisOptions.ConcurrentAnalysis && analysisState.HasPendingSymbolAnalysis(analysisScope));
			return analysisResult.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private async Task ComputeAnalyzerDiagnosticsAsync(AnalysisScope analysisScope, Action generateCompilationEventsOpt, Func<AsyncQueue<CompilationEvent>> getEventQueue, int newTaskToken, CancellationToken cancellationToken)
	{
		try
		{
			AnalyzerDriver driver = null;
			Task computeTask = null;
			generateCompilationEventsOpt?.Invoke();
			await PopulateEventsCacheAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				DebugAssertHelper.Assert(driver.WhenInitializedTask != null);
				DebugAssertHelper.Assert(!driver.WhenInitializedTask.IsCanceled);
				cancellationToken.ThrowIfCancellationRequested();
				bool suspendend;
				do
				{
					suspendend = false;
					CancellationTokenSource cts = new CancellationTokenSource();
					CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
					try
					{
						try
						{
							Func<Tuple<Task, CancellationTokenSource>> getNewAnalysisTask = () => Tuple.Create(Task.Run(async () =>
							{
								try
								{
									AsyncQueue<CompilationEvent> eventQueue = null;
									try
									{
										eventQueue = getEventQueue();
										await ComputeAnalyzerDiagnosticsCoreAsync(driver, eventQueue, analysisScope, linkedCts.Token).ConfigureAwait(continueOnCapturedContext: false);
									}
									finally
									{
										FreeEventQueue(eventQueue);
									}
								}
								catch (Exception exception2) when (FatalError.ReportUnlessCanceled(exception2))
								{
									throw ExceptionUtilities.Unreachable;
								}
							}, linkedCts.Token), cts);
							computeTask = await SetActiveAnalysisTaskAsync(getNewAnalysisTask, analysisScope.FilterTreeOpt, newTaskToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
							cancellationToken.ThrowIfCancellationRequested();
							await computeTask.ConfigureAwait(continueOnCapturedContext: false);
						}
						catch (OperationCanceledException ex)
						{
							cancellationToken.ThrowIfCancellationRequested();
							if (!cts.IsCancellationRequested)
							{
								throw ex;
							}
							suspendend = true;
						}
						finally
						{
							ClearExecutingTask(computeTask, analysisScope.FilterTreeOpt);
							cts.Dispose();
							computeTask = null;
						}
					}
					finally
					{
						if (linkedCts != null)
						{
							((IDisposable)linkedCts).Dispose();
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

	private async Task<AnalyzerDriver> GetAnalyzerDriverAsync(CancellationToken cancellationToken)
	{
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			AnalyzerDriver driver = driverPool.Allocate();
			try
			{
				if (driver.WhenInitializedTask == null)
				{
					driver.Initialize(Compilation, AnalysisOptions, compilationData, categorizeDiagnostics: true, cancellationToken);
				}
				await driver.WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				if (driver.WhenInitializedTask.IsCanceled)
				{
					driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			return driver;
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private void FreeDriver(AnalyzerDriver driver)
	{
		if (driver != null && !driver.WhenInitializedTask.IsCanceled)
		{
			driverPool.Free(driver);
		}
	}

	private async Task ComputeAnalyzerDiagnosticsCoreAsync(AnalyzerDriver driver, AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		try
		{
			DebugAssertHelper.Assert(!driver.WhenInitializedTask.IsCanceled);
			if (eventQueue.Count > 0 || analysisState.HasPendingSyntaxAnalysis(analysisScope))
			{
				try
				{
					DebugAssertHelper.Assert(!eventQueue.IsCompleted);
					await driver.AttachQueueAndProcessAllEventsAsync(eventQueue, analysisScope, analysisState, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				finally
				{
					analysisResult.StoreAnalysisResult(analysisScope, driver, Compilation);
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
			await WaitForActiveAnalysisTasksAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			lock (executingTasksLock)
			{
				if ((executingConcurrentTreeTasksOpt == null || executingConcurrentTreeTasksOpt.Count == 0) && executingCompilationOrNonConcurrentTreeTask == null)
				{
					executingCompilationOrNonConcurrentTreeTask = getNewCompilationTask();
					return executingCompilationOrNonConcurrentTreeTask.Item1;
				}
			}
		}
	}

	private async Task WaitForActiveAnalysisTasksAsync(CancellationToken cancellationToken)
	{
		ArrayBuilder<Tuple<Task, CancellationTokenSource>> executingTasks = ArrayBuilder<Tuple<Task, CancellationTokenSource>>.GetInstance();
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lock (executingTasksLock)
			{
				if (executingConcurrentTreeTasksOpt != null && executingConcurrentTreeTasksOpt.Count > 0)
				{
					executingTasks.AddRange(executingConcurrentTreeTasksOpt.Values);
				}
				if (executingCompilationOrNonConcurrentTreeTask != null)
				{
					executingTasks.Add(executingCompilationOrNonConcurrentTreeTask);
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
				lock (executingTasksLock)
				{
					if (!AnalysisOptions.ConcurrentAnalysis)
					{
						if (executingCompilationOrNonConcurrentTreeTask != null)
						{
							SuspendAnalysisNoLock(executingCompilationOrNonConcurrentTreeTask.Item1, executingCompilationOrNonConcurrentTreeTask.Item2);
							executingCompilationOrNonConcurrentTreeTask = null;
						}
						return (executingCompilationOrNonConcurrentTreeTask = getNewTreeAnalysisTask()).Item1;
					}
					DebugAssertHelper.Assert(executingConcurrentTreeTasksOpt != null);
					DebugAssertHelper.Assert(concurrentTreeTaskTokensOpt != null);
					if (!executingConcurrentTreeTasksOpt.TryGetValue(tree, out value) || concurrentTreeTaskTokensOpt[value.Item1] < newTaskToken)
					{
						if (value != null)
						{
							SuspendAnalysisNoLock(value.Item1, value.Item2);
						}
						if (executingCompilationOrNonConcurrentTreeTask != null)
						{
							SuspendAnalysisNoLock(executingCompilationOrNonConcurrentTreeTask.Item1, executingCompilationOrNonConcurrentTreeTask.Item2);
							executingCompilationOrNonConcurrentTreeTask = null;
						}
						Tuple<Task, CancellationTokenSource> tuple = getNewTreeAnalysisTask();
						concurrentTreeTaskTokensOpt[tuple.Item1] = newTaskToken;
						executingConcurrentTreeTasksOpt[tree] = tuple;
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

	private static void SuspendAnalysisNoLock(Task computeTask, CancellationTokenSource cts)
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
		lock (executingTasksLock)
		{
			if (treeOpt != null && AnalysisOptions.ConcurrentAnalysis)
			{
				DebugAssertHelper.Assert(executingConcurrentTreeTasksOpt != null);
				DebugAssertHelper.Assert(concurrentTreeTaskTokensOpt != null);
				if (executingConcurrentTreeTasksOpt.TryGetValue(treeOpt, out var value) && value.Item1 == computeTask)
				{
					executingConcurrentTreeTasksOpt.Remove(treeOpt);
				}
				if (concurrentTreeTaskTokensOpt.ContainsKey(computeTask))
				{
					concurrentTreeTaskTokensOpt.Remove(computeTask);
				}
			}
			else if (executingCompilationOrNonConcurrentTreeTask != null && executingCompilationOrNonConcurrentTreeTask.Item1 == computeTask)
			{
				executingCompilationOrNonConcurrentTreeTask = null;
			}
		}
	}

	private async Task PopulateEventsCacheAsync(CancellationToken cancellationToken)
	{
		if (Compilation.EventQueue.Count > 0)
		{
			AnalyzerDriver driver = null;
			try
			{
				driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				cancellationToken.ThrowIfCancellationRequested();
				ImmutableArray<CompilationEvent> compilationEvents = DequeueGeneratedCompilationEvents();
				await analysisState.OnCompilationEventsGeneratedAsync(compilationEvents, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
		while (Compilation.EventQueue.TryDequeue(out d))
		{
			builder.Add(d);
		}
		return builder.ToImmutable();
	}

	private AsyncQueue<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree tree)
	{
		AsyncQueue<CompilationEvent> asyncQueue = EventQueuePool.Allocate();
		DebugAssertHelper.Assert(!asyncQueue.IsCompleted);
		DebugAssertHelper.Assert(asyncQueue.Count == 0);
		foreach (CompilationEvent pendingEvent in analysisState.GetPendingEvents(analyzers, tree))
		{
			asyncQueue.Enqueue(pendingEvent);
		}
		return asyncQueue;
	}

	private AsyncQueue<CompilationEvent> GetPendingEvents(ImmutableArray<DiagnosticAnalyzer> analyzers, bool includeSourceEvents, bool includeNonSourceEvents)
	{
		DebugAssertHelper.Assert(includeSourceEvents | includeNonSourceEvents);
		AsyncQueue<CompilationEvent> asyncQueue = EventQueuePool.Allocate();
		DebugAssertHelper.Assert(!asyncQueue.IsCompleted);
		DebugAssertHelper.Assert(asyncQueue.Count == 0);
		foreach (CompilationEvent pendingEvent in analysisState.GetPendingEvents(analyzers, includeSourceEvents, includeNonSourceEvents))
		{
			asyncQueue.Enqueue(pendingEvent);
		}
		return asyncQueue;
	}

	private static void FreeEventQueue(AsyncQueue<CompilationEvent> eventQueue)
	{
		if (eventQueue == null || eventQueue == EmptyEventQueue)
		{
			return;
		}
		DebugAssertHelper.Assert(!eventQueue.IsCompleted);
		if (eventQueue.Count > 0)
		{
			CompilationEvent d;
			while (eventQueue.TryDequeue(out d))
			{
			}
		}
		EventQueuePool.Free(eventQueue);
	}

	public static IEnumerable<Diagnostic> GetEffectiveDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		if (diagnostics == null)
		{
			throw new ArgumentNullException("diagnostics");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		foreach (Diagnostic item in diagnostics)
		{
			if (item != null)
			{
				Diagnostic diagnostic = compilation.Options.FilterDiagnostic(item);
				if (diagnostic != null)
				{
					yield return diagnostic;
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
		AnalyzerExecutor analyzerExecutor = AnalyzerExecutor.CreateForSupportedDiagnostics(onAnalyzerException, AnalyzerManager.Instance);
		return AnalyzerDriverBase.IsDiagnosticAnalyzerSuppressed(analyzer, options, AnalyzerManager.Instance, analyzerExecutor);
	}

	public static void ClearAnalyzerState(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		VerifyAnalyzersArgumentForStaticApis(analyzers, allowDefaultOrEmpty: true);
		AnalyzerManager.Instance.ClearAnalyzerState(analyzers);
	}

	internal async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		VerifyAnalyzerArgument(analyzer);
		AnalyzerDriver driver = null;
		try
		{
			driver = await GetAnalyzerDriverAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
			return await analysisState.GetOrComputeAnalyzerActionCountsAsync(analyzer, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			FreeDriver(driver);
		}
	}

	internal TimeSpan GetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		VerifyAnalyzerArgument(analyzer);
		if (!AnalysisOptions.LogAnalyzerExecutionTime)
		{
			throw new InvalidOperationException();
		}
		return analysisResult.GetAnalyzerExecutionTime(analyzer);
	}
}
