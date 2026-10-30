using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalyzerManager
{
	private sealed class AnalyzerAndOptions
	{
		public readonly DiagnosticAnalyzer Analyzer;

		private readonly AnalyzerOptions analyzerOptions;

		public AnalyzerAndOptions(DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions)
		{
			DebugAssertHelper.Assert(analyzer != null);
			DebugAssertHelper.Assert(analyzerOptions != null);
			Analyzer = analyzer;
			this.analyzerOptions = new AnalyzerOptions(analyzerOptions.AdditionalFiles);
		}

		public bool Equals(AnalyzerAndOptions other)
		{
			if (this == other)
			{
				return true;
			}
			if (other != null && Analyzer.Equals(other.Analyzer))
			{
				return analyzerOptions.Equals(other.analyzerOptions);
			}
			return false;
		}

		public override bool Equals(object other)
		{
			return Equals(other as AnalyzerAndOptions);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(Analyzer.GetHashCode(), analyzerOptions.GetHashCode());
		}
	}

	public static readonly AnalyzerManager Instance = new AnalyzerManager();

	private readonly ConcurrentDictionary<DiagnosticAnalyzer, Task<HostSessionStartAnalysisScope>> sessionScopeMap = new ConcurrentDictionary<DiagnosticAnalyzer, Task<HostSessionStartAnalysisScope>>(2, 5);

	private readonly ConcurrentDictionary<AnalyzerAndOptions, ConditionalWeakTable<Compilation, Task<HostCompilationStartAnalysisScope>>> compilationScopeMap = new ConcurrentDictionary<AnalyzerAndOptions, ConditionalWeakTable<Compilation, Task<HostCompilationStartAnalysisScope>>>(2, 5);

	private readonly ConcurrentDictionary<DiagnosticAnalyzer, Tuple<ImmutableArray<DiagnosticDescriptor>, EventHandler<Exception>>> descriptorCache = new ConcurrentDictionary<DiagnosticAnalyzer, Tuple<ImmutableArray<DiagnosticDescriptor>, EventHandler<Exception>>>(2, 5);

	private Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeCoreAsync(AnalyzerAndOptions analyzerAndOptions, HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor)
	{
		ConditionalWeakTable<Compilation, Task<HostCompilationStartAnalysisScope>>.CreateValueCallback createValueCallback = ((Func<Compilation, Task<HostCompilationStartAnalysisScope>>)((Compilation comp) => Task.Run(() =>
		{
			HostCompilationStartAnalysisScope hostCompilationStartAnalysisScope = new HostCompilationStartAnalysisScope(sessionScope);
			analyzerExecutor.ExecuteCompilationStartActions(sessionScope.CompilationStartActions, hostCompilationStartAnalysisScope);
			return hostCompilationStartAnalysisScope;
		}, analyzerExecutor.CancellationToken))).Invoke;
		return compilationScopeMap.GetOrAdd(analyzerAndOptions, new ConditionalWeakTable<Compilation, Task<HostCompilationStartAnalysisScope>>()).GetValue(analyzerExecutor.Compilation, createValueCallback);
	}

	private async Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeAsync(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor)
	{
		AnalyzerAndOptions analyzerAndOptions = new AnalyzerAndOptions(analyzer, analyzerExecutor.AnalyzerOptions);
		try
		{
			return await GetCompilationAnalysisScopeCoreAsync(analyzerAndOptions, sessionScope, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			if (compilationScopeMap.TryGetValue(analyzerAndOptions, out var value))
			{
				value.Remove(analyzerExecutor.Compilation);
			}
			analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
		}
		return await GetCompilationAnalysisScopeAsync(analyzer, sessionScope, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
	}

	private Task<HostSessionStartAnalysisScope> GetSessionAnalysisScopeCoreAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		Func<DiagnosticAnalyzer, Task<HostSessionStartAnalysisScope>> valueFactory = (DiagnosticAnalyzer a) => Task.Run(() =>
		{
			HostSessionStartAnalysisScope hostSessionStartAnalysisScope = new HostSessionStartAnalysisScope();
			analyzerExecutor.ExecuteInitializeMethod(a, hostSessionStartAnalysisScope);
			return hostSessionStartAnalysisScope;
		}, analyzerExecutor.CancellationToken);
		return sessionScopeMap.GetOrAdd(analyzer, valueFactory);
	}

	private async Task<HostSessionStartAnalysisScope> GetSessionAnalysisScopeAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		try
		{
			return await GetSessionAnalysisScopeCoreAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			sessionScopeMap.TryRemove(analyzer, out var _);
			analyzerExecutor.CancellationToken.ThrowIfCancellationRequested();
		}
		return await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalyzerActions> GetAnalyzerActionsAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		HostSessionStartAnalysisScope hostSessionStartAnalysisScope = await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false);
		if (hostSessionStartAnalysisScope.CompilationStartActions.Length > 0 && analyzerExecutor.Compilation != null)
		{
			return (await GetCompilationAnalysisScopeAsync(analyzer, hostSessionStartAnalysisScope, analyzerExecutor).ConfigureAwait(continueOnCapturedContext: false)).GetAnalyzerActions(analyzer);
		}
		return hostSessionStartAnalysisScope.GetAnalyzerActions(analyzer);
	}

	public ImmutableArray<DiagnosticDescriptor> GetSupportedDiagnosticDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor)
	{
		return descriptorCache.GetOrAdd(analyzer, (DiagnosticAnalyzer key) =>
		{
			ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray<DiagnosticDescriptor>.Empty;
			analyzerExecutor.ExecuteAndCatchIfThrows(analyzer, () =>
			{
				ImmutableArray<DiagnosticDescriptor> supportedDiagnostics2 = analyzer.SupportedDiagnostics;
				if (!supportedDiagnostics2.IsDefaultOrEmpty)
				{
					supportedDiagnostics = supportedDiagnostics2;
				}
			});
			EventHandler<Exception> eventHandler = null;
			Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = analyzerExecutor.OnAnalyzerException;
			if (onAnalyzerException != null)
			{
				eventHandler = (object sender, Exception ex) =>
				{
					Diagnostic arg = AnalyzerExecutor.CreateAnalyzerExceptionDiagnostic(analyzer, ex);
					onAnalyzerException(ex, analyzer, arg);
				};
				foreach (DiagnosticDescriptor item in supportedDiagnostics)
				{
					item.Title.OnException += eventHandler;
					item.MessageFormat.OnException += eventHandler;
					item.Description.OnException += eventHandler;
				}
			}
			return Tuple.Create(supportedDiagnostics, eventHandler);
		}).Item1;
	}

	internal void ClearAnalyzerState(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		if (!analyzers.IsDefaultOrEmpty)
		{
			foreach (DiagnosticAnalyzer item in analyzers)
			{
				ClearDescriptorState(item);
				ClearAnalysisScopeState(item);
			}
		}
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

	private void ClearDescriptorState(DiagnosticAnalyzer analyzer)
	{
		if (!descriptorCache.TryRemove(analyzer, out var value))
		{
			return;
		}
		ImmutableArray<DiagnosticDescriptor> item = value.Item1;
		EventHandler<Exception> item2 = value.Item2;
		if (item2 != null)
		{
			foreach (DiagnosticDescriptor item3 in item)
			{
				item3.Title.OnException -= item2;
				item3.MessageFormat.OnException -= item2;
				item3.Description.OnException -= item2;
			}
		}
	}

	private void ClearAnalysisScopeState(DiagnosticAnalyzer analyzer)
	{
		sessionScopeMap.TryRemove(analyzer, out var _);
		foreach (AnalyzerAndOptions item in compilationScopeMap.Keys.Where((AnalyzerAndOptions analyzerAndOptions) => analyzerAndOptions.Analyzer.Equals(analyzer)))
		{
			compilationScopeMap.TryRemove(item, out var _);
		}
	}
}
