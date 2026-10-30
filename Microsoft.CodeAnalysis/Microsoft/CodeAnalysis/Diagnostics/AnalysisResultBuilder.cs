using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalysisResultBuilder
{
	private readonly object _gate = new object();

	private readonly Dictionary<DiagnosticAnalyzer, TimeSpan> _analyzerExecutionTimeOpt;

	private readonly HashSet<DiagnosticAnalyzer> _completedAnalyzers;

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts> _analyzerActionCounts;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> _localSemanticDiagnosticsOpt;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> _localSyntaxDiagnosticsOpt;

	private Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> _nonLocalDiagnosticsOpt;

	internal AnalysisResultBuilder(bool logAnalyzerExecutionTime, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		_analyzerExecutionTimeOpt = (logAnalyzerExecutionTime ? CreateAnalyzerExecutionTimeMap(analyzers) : null);
		_completedAnalyzers = new HashSet<DiagnosticAnalyzer>();
		_analyzerActionCounts = new Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts>(analyzers.Length);
	}

	private static Dictionary<DiagnosticAnalyzer, TimeSpan> CreateAnalyzerExecutionTimeMap(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		Dictionary<DiagnosticAnalyzer, TimeSpan> dictionary = new Dictionary<DiagnosticAnalyzer, TimeSpan>(analyzers.Length);
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			dictionary[item] = default;
		}
		return dictionary;
	}

	public TimeSpan GetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		lock (_gate)
		{
			return _analyzerExecutionTimeOpt[analyzer];
		}
	}

	internal ImmutableArray<DiagnosticAnalyzer> GetPendingAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		lock (_gate)
		{
			ArrayBuilder<DiagnosticAnalyzer> arrayBuilder = null;
			foreach (DiagnosticAnalyzer item in analyzers)
			{
				if (!_completedAnalyzers.Contains(item))
				{
					arrayBuilder = arrayBuilder ?? ArrayBuilder<DiagnosticAnalyzer>.GetInstance();
					arrayBuilder.Add(item);
				}
			}
			return arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<DiagnosticAnalyzer>.Empty;
		}
	}

	internal void StoreAnalysisResult(AnalysisScope analysisScope, AnalyzerDriver driver, Compilation compilation, Func<DiagnosticAnalyzer, AnalyzerActionCounts> getAnalyzerActionCounts, bool fullAnalysisResultForAnalyzersInScope)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			ImmutableArray<Diagnostic> diagnostics = driver.DequeueLocalDiagnostics(analyzer, syntax: true, compilation);
			ImmutableArray<Diagnostic> diagnostics2 = driver.DequeueLocalDiagnostics(analyzer, syntax: false, compilation);
			ImmutableArray<Diagnostic> diagnostics3 = driver.DequeueNonLocalDiagnostics(analyzer, compilation);
			lock (_gate)
			{
				if (!_completedAnalyzers.Contains(analyzer))
				{
					if ((diagnostics.Length > 0 || diagnostics2.Length > 0 || diagnostics3.Length > 0) | fullAnalysisResultForAnalyzersInScope)
					{
						UpdateLocalDiagnostics_NoLock(analyzer, diagnostics, fullAnalysisResultForAnalyzersInScope, ref _localSyntaxDiagnosticsOpt);
						UpdateLocalDiagnostics_NoLock(analyzer, diagnostics2, fullAnalysisResultForAnalyzersInScope, ref _localSemanticDiagnosticsOpt);
						UpdateNonLocalDiagnostics_NoLock(analyzer, diagnostics3, fullAnalysisResultForAnalyzersInScope);
					}
					if (_analyzerExecutionTimeOpt != null)
					{
						TimeSpan timeSpan = driver.ResetAnalyzerExecutionTime(analyzer);
						_analyzerExecutionTimeOpt[analyzer] = (fullAnalysisResultForAnalyzersInScope ? timeSpan : (_analyzerExecutionTimeOpt[analyzer] + timeSpan));
					}
					if (!_analyzerActionCounts.ContainsKey(analyzer))
					{
						_analyzerActionCounts.Add(analyzer, getAnalyzerActionCounts(analyzer));
					}
					if (fullAnalysisResultForAnalyzersInScope)
					{
						_completedAnalyzers.Add(analyzer);
					}
				}
			}
		}
	}

	private void UpdateLocalDiagnostics_NoLock(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics, bool overwrite, ref Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> lazyLocalDiagnostics)
	{
		if (diagnostics.IsEmpty)
		{
			return;
		}
		lazyLocalDiagnostics = lazyLocalDiagnostics ?? new Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>();
		foreach (IGrouping<SyntaxTree, Diagnostic> item in from d in diagnostics
			group d by d.Location.SourceTree)
		{
			SyntaxTree key = item.Key;
			if (!lazyLocalDiagnostics.TryGetValue(key, out var value))
			{
				value = new Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>();
				lazyLocalDiagnostics[key] = value;
			}
			if (!value.TryGetValue(analyzer, out var value2))
			{
				value2 = (value[analyzer] = ImmutableArray.CreateBuilder<Diagnostic>());
			}
			if (overwrite)
			{
				value2.Clear();
			}
			value2.AddRange(item);
		}
	}

	private void UpdateNonLocalDiagnostics_NoLock(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics, bool overwrite)
	{
		if (!diagnostics.IsEmpty)
		{
			_nonLocalDiagnosticsOpt = _nonLocalDiagnosticsOpt ?? new Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>();
			if (!_nonLocalDiagnosticsOpt.TryGetValue(analyzer, out var value))
			{
				value = ImmutableArray.CreateBuilder<Diagnostic>();
				_nonLocalDiagnosticsOpt[analyzer] = value;
			}
			if (overwrite)
			{
				value.Clear();
			}
			value.AddRange(diagnostics);
		}
	}

	internal ImmutableArray<Diagnostic> GetDiagnostics(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		lock (_gate)
		{
			return GetDiagnostics_NoLock(analysisScope, getLocalDiagnostics, getNonLocalDiagnostics);
		}
	}

	private ImmutableArray<Diagnostic> GetDiagnostics_NoLock(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		if (getLocalDiagnostics)
		{
			if (!analysisScope.IsTreeAnalysis)
			{
				AddAllLocalDiagnostics_NoLock(_localSyntaxDiagnosticsOpt, analysisScope, builder);
				AddAllLocalDiagnostics_NoLock(_localSemanticDiagnosticsOpt, analysisScope, builder);
			}
			else if (analysisScope.IsSyntaxOnlyTreeAnalysis)
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(_localSyntaxDiagnosticsOpt, analysisScope, builder);
			}
			else
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(_localSemanticDiagnosticsOpt, analysisScope, builder);
			}
		}
		if (getNonLocalDiagnostics && _nonLocalDiagnosticsOpt != null)
		{
			AddDiagnostics_NoLock(_nonLocalDiagnosticsOpt, analysisScope, builder);
		}
		return builder.ToImmutableArray();
	}

	private static void AddAllLocalDiagnostics_NoLock(Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		if (localDiagnostics == null)
		{
			return;
		}
		foreach (Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> value in localDiagnostics.Values)
		{
			AddDiagnostics_NoLock(value, analysisScope, builder);
		}
	}

	private static void AddLocalDiagnosticsForPartialAnalysis_NoLock(Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		if (localDiagnostics != null && localDiagnostics.TryGetValue(analysisScope.FilterTreeOpt, out var value))
		{
			AddDiagnostics_NoLock(value, analysisScope, builder);
		}
	}

	private static void AddDiagnostics_NoLock(Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> diagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (diagnostics.TryGetValue(analyzer, out var value))
			{
				builder.AddRange(value);
			}
		}
	}

	internal AnalysisResult ToAnalysisResult(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableHashSet<DiagnosticAnalyzer> analyzers2 = analyzers.ToImmutableHashSet();
		ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> immutable;
		ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> immutable2;
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> immutable3;
		lock (_gate)
		{
			immutable = GetImmutable(analyzers2, _localSyntaxDiagnosticsOpt);
			immutable2 = GetImmutable(analyzers2, _localSemanticDiagnosticsOpt);
			immutable3 = GetImmutable(analyzers2, _nonLocalDiagnosticsOpt);
		}
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetryInfo = GetTelemetryInfo(analyzers, cancellationToken);
		return new AnalysisResult(analyzers, immutable, immutable2, immutable3, telemetryInfo);
	}

	private static ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> GetImmutable(ImmutableHashSet<DiagnosticAnalyzer> analyzers, Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> localDiagnosticsOpt)
	{
		if (localDiagnosticsOpt == null)
		{
			return ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>.Empty;
		}
		ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>.Builder builder = ImmutableDictionary.CreateBuilder<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>();
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Builder builder2 = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>();
		foreach (KeyValuePair<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> item in localDiagnosticsOpt)
		{
			SyntaxTree key = item.Key;
			foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> item2 in item.Value)
			{
				if (analyzers.Contains(item2.Key))
				{
					builder2.Add(item2.Key, item2.Value.ToImmutable());
				}
			}
			builder.Add(key, builder2.ToImmutable());
			builder2.Clear();
		}
		return builder.ToImmutable();
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> GetImmutable(ImmutableHashSet<DiagnosticAnalyzer> analyzers, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> nonLocalDiagnosticsOpt)
	{
		if (nonLocalDiagnosticsOpt == null)
		{
			return ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Empty;
		}
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>();
		foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> item in nonLocalDiagnosticsOpt)
		{
			if (analyzers.Contains(item.Key))
			{
				builder.Add(item.Key, item.Value.ToImmutable());
			}
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> GetTelemetryInfo(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, AnalyzerTelemetryInfo>();
		lock (_gate)
		{
			foreach (DiagnosticAnalyzer item in analyzers)
			{
				AnalyzerActionCounts actionCounts = _analyzerActionCounts[item];
				TimeSpan executionTime = ((_analyzerExecutionTimeOpt != null) ? _analyzerExecutionTimeOpt[item] : default(TimeSpan));
				AnalyzerTelemetryInfo value = new AnalyzerTelemetryInfo(actionCounts, executionTime);
				builder.Add(item, value);
			}
		}
		return builder.ToImmutable();
	}
}
