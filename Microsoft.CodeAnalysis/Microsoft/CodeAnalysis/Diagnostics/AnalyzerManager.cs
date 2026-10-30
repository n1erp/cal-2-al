using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalyzerManager
{
	private sealed class AnalyzerExecutionContext
	{
		private readonly object _gate = new object();

		private Task<HostSessionStartAnalysisScope> _lazySessionScopeTask;

		private Task<HostCompilationStartAnalysisScope> _lazyCompilationScopeTask;

		private ImmutableArray<DiagnosticDescriptor> _lazyDescriptors;

		public Task<HostSessionStartAnalysisScope> GetSessionAnalysisScopeTask(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
		{
			lock (_gate)
			{
				if (_lazySessionScopeTask != null)
				{
					return _lazySessionScopeTask;
				}
				return _lazySessionScopeTask = Task.Run(() =>
				{
					HostSessionStartAnalysisScope hostSessionStartAnalysisScope = new HostSessionStartAnalysisScope();
					analyzerExecutor.ExecuteInitializeMethod(analyzer, hostSessionStartAnalysisScope);
					return hostSessionStartAnalysisScope;
				}, analyzerExecutor.CancellationToken);
			}
		}

		public void ClearSessionScopeTask()
		{
			lock (_gate)
			{
				_lazySessionScopeTask = null;
			}
		}

		public Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeAsync(HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor)
		{
			lock (_gate)
			{
				if (_lazyCompilationScopeTask == null)
				{
					_lazyCompilationScopeTask = Task.Run(() =>
					{
						HostCompilationStartAnalysisScope hostCompilationStartAnalysisScope = new HostCompilationStartAnalysisScope(sessionScope);
						analyzerExecutor.ExecuteCompilationStartActions(sessionScope.CompilationStartActions, hostCompilationStartAnalysisScope);
						return hostCompilationStartAnalysisScope;
					}, analyzerExecutor.CancellationToken);
				}
				return _lazyCompilationScopeTask;
			}
		}

		public void ClearCompilationScopeTask()
		{
			lock (_gate)
			{
				_lazyCompilationScopeTask = null;
			}
		}

		public ImmutableArray<DiagnosticDescriptor> GetOrComputeDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
		{
			lock (_gate)
			{
				if (!_lazyDescriptors.IsDefault)
				{
					return _lazyDescriptors;
				}
			}
			ImmutableArray<DiagnosticDescriptor> immutableArray = ComputeDescriptors(analyzer, analyzerExecutor);
			lock (_gate)
			{
				if (!_lazyDescriptors.IsDefault)
				{
					immutableArray = _lazyDescriptors;
				}
				else
				{
					_lazyDescriptors = immutableArray;
				}
			}
			return immutableArray;
		}

		private static ImmutableArray<DiagnosticDescriptor> ComputeDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
		{
			ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray<DiagnosticDescriptor>.Empty;
			analyzerExecutor.ExecuteAndCatchIfThrows(analyzer, (object _) =>
			{
				ImmutableArray<DiagnosticDescriptor> supportedDiagnostics2 = analyzer.SupportedDiagnostics;
				if (!supportedDiagnostics2.IsDefaultOrEmpty)
				{
					supportedDiagnostics = supportedDiagnostics2;
				}
			}, null);
			Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = analyzerExecutor.OnAnalyzerException;
			if (onAnalyzerException != null)
			{
				EventHandler<Exception> handler = (object sender, Exception ex) =>
				{
					Diagnostic arg = AnalyzerExecutor.CreateAnalyzerExceptionDiagnostic(analyzer, ex);
					onAnalyzerException(ex, analyzer, arg);
				};
				foreach (DiagnosticDescriptor item in supportedDiagnostics)
				{
					ForceLocalizableStringExceptions(item.Title, handler);
					ForceLocalizableStringExceptions(item.MessageFormat, handler);
					ForceLocalizableStringExceptions(item.Description, handler);
				}
			}
			return supportedDiagnostics;
		}
	}

	private readonly ImmutableDictionary<DiagnosticAnalyzer, AnalyzerExecutionContext> _analyzerExecutionContextMap;

	public AnalyzerManager(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		_analyzerExecutionContextMap = CreateAnalyzerExecutionContextMap(analyzers);
	}

	public AnalyzerManager(DiagnosticAnalyzer analyzer)
	{
		_analyzerExecutionContextMap = CreateAnalyzerExecutionContextMap(SpecializedCollections.SingletonEnumerable(analyzer));
	}

	private ImmutableDictionary<DiagnosticAnalyzer, AnalyzerExecutionContext> CreateAnalyzerExecutionContextMap(IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerExecutionContext>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, AnalyzerExecutionContext>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			builder.Add(analyzer, new AnalyzerExecutionContext());
		}
		return builder.ToImmutable();
	}

	private AnalyzerExecutionContext GetAnalyzerExecutionContext(DiagnosticAnalyzer analyzer)
	{
		return _analyzerExecutionContextMap[analyzer];
	}

	private async Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeAsync(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor)
	{
		AnalyzerExecutionContext analyzerExecutionContext = GetAnalyzerExecutionContext(analyzer);
		return await GetCompilationAnalysisScopeCoreAsync(sessionScope, analyzerExecutor, analyzerExecutionContext).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeCoreAsync(HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor, AnalyzerExecutionContext analyzerExecutionContext)
	{
		HostCompilationStartAnalysisScope result = default;
		int num;
		try
		{
			result = await analyzerExecutionContext.GetCompilationAnalysisScopeAsync(sessionScope, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		catch (OperationCanceledException)
		{
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		analyzerExecutionContext.ClearCompilationScopeTask();
		analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
		return await GetCompilationAnalysisScopeCoreAsync(sessionScope, analyzerExecutor, analyzerExecutionContext).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async ValueTask<HostSessionStartAnalysisScope> GetSessionAnalysisScopeAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		AnalyzerExecutionContext analyzerExecutionContext = GetAnalyzerExecutionContext(analyzer);
		return await GetSessionAnalysisScopeCoreAsync(analyzer, analyzerExecutor, analyzerExecutionContext).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async ValueTask<HostSessionStartAnalysisScope> GetSessionAnalysisScopeCoreAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, AnalyzerExecutionContext analyzerExecutionContext)
	{
		HostSessionStartAnalysisScope result = default;
		int num;
		try
		{
			result = await analyzerExecutionContext.GetSessionAnalysisScopeTask(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		catch (OperationCanceledException)
		{
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		analyzerExecutionContext.ClearSessionScopeTask();
		analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
		return await GetSessionAnalysisScopeCoreAsync(analyzer, analyzerExecutor, analyzerExecutionContext).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async ValueTask<AnalyzerActions> GetAnalyzerActionsAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		HostSessionStartAnalysisScope hostSessionStartAnalysisScope = await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
		if (hostSessionStartAnalysisScope.CompilationStartActions.Length > 0 && analyzerExecutor.Compilation != null)
		{
			return (await GetCompilationAnalysisScopeAsync(analyzer, hostSessionStartAnalysisScope, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false)).GetAnalyzerActions(analyzer);
		}
		return hostSessionStartAnalysisScope.GetAnalyzerActions(analyzer);
	}

	public async Task<bool> IsConcurrentAnalyzerAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		return (await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false)).IsConcurrentAnalyzer(analyzer);
	}

	public async Task<GeneratedCodeAnalysisFlags> GetGeneratedCodeAnalysisFlagsAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		return (await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false)).GetGeneratedCodeAnalysisFlags(analyzer);
	}

	private static void ForceLocalizableStringExceptions(LocalizableString localizableString, EventHandler<Exception> handler)
	{
		if (localizableString.CanThrowExceptions)
		{
			localizableString.OnException += handler;
			localizableString.ToString();
			localizableString.OnException -= handler;
		}
	}

	public ImmutableArray<DiagnosticDescriptor> GetSupportedDiagnosticDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		return GetAnalyzerExecutionContext(analyzer).GetOrComputeDescriptors(analyzer, analyzerExecutor);
	}

	internal bool IsSupportedDiagnostic(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerExecutor analyzerExecutor)
	{
		if (isCompilerAnalyzer(analyzer))
		{
			return true;
		}
		foreach (DiagnosticDescriptor supportedDiagnosticDescriptor in GetSupportedDiagnosticDescriptors(analyzer, analyzerExecutor))
		{
			if (supportedDiagnosticDescriptor.Id.Equals(diagnostic.Id, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerExecutor analyzerExecutor)
	{
		if (isCompilerAnalyzer(analyzer))
		{
			return false;
		}
		ImmutableArray<DiagnosticDescriptor> supportedDiagnosticDescriptors = GetSupportedDiagnosticDescriptors(analyzer, analyzerExecutor);
		ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = options.SpecificDiagnosticOptions;
		foreach (DiagnosticDescriptor item in supportedDiagnosticDescriptors)
		{
			if (HasNotConfigurableTag(item.CustomTags))
			{
				if (item.IsEnabledByDefault)
				{
					return false;
				}
				continue;
			}
			bool flag = !item.IsEnabledByDefault;
			if (specificDiagnosticOptions.TryGetValue(item.Id, out var value))
			{
				flag = value == ReportDiagnostic.Suppress;
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool HasNotConfigurableTag(IEnumerable<string> customTags)
	{
		foreach (string customTag in customTags)
		{
			if (customTag == "NotConfigurable")
			{
				return true;
			}
		}
		return false;
	}
}
