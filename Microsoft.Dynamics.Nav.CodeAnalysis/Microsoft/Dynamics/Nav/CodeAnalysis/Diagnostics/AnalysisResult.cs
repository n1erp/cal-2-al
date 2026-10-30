using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalysisResult
{
	private readonly object gate = new object();

	private readonly Dictionary<DiagnosticAnalyzer, TimeSpan> analyzerExecutionTimeOpt;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>> localSemanticDiagnosticsOpt;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>> localSyntaxDiagnosticsOpt;

	private Dictionary<DiagnosticAnalyzer, List<Diagnostic>> nonLocalDiagnosticsOpt;

	public AnalysisResult(bool logAnalyzerExecutionTime, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		analyzerExecutionTimeOpt = (logAnalyzerExecutionTime ? CreateAnalyzerExecutionTimeMap(analyzers) : null);
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
		DebugAssertHelper.Assert(analyzerExecutionTimeOpt != null);
		lock (gate)
		{
			return analyzerExecutionTimeOpt[analyzer];
		}
	}

	public void StoreAnalysisResult(AnalysisScope analysisScope, AnalyzerDriver driver, Compilation compilation)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			ImmutableArray<Diagnostic> diagnostics = driver.DequeueLocalDiagnostics(analyzer, syntax: true, compilation);
			ImmutableArray<Diagnostic> diagnostics2 = driver.DequeueLocalDiagnostics(analyzer, syntax: false, compilation);
			ImmutableArray<Diagnostic> diagnostics3 = driver.DequeueNonLocalDiagnostics(analyzer, compilation);
			lock (gate)
			{
				if (diagnostics.Length > 0 || diagnostics2.Length > 0 || diagnostics3.Length > 0)
				{
					UpdateLocalDiagnosticsNoLock(analyzer, diagnostics, ref localSyntaxDiagnosticsOpt);
					UpdateLocalDiagnosticsNoLock(analyzer, diagnostics2, ref localSemanticDiagnosticsOpt);
					UpdateNonLocalDiagnosticsNoLock(analyzer, diagnostics3);
				}
				if (analyzerExecutionTimeOpt != null)
				{
					analyzerExecutionTimeOpt[analyzer] += driver.ResetAnalyzerExecutionTime(analyzer);
				}
			}
		}
	}

	private static void UpdateLocalDiagnosticsNoLock(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics, ref Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>> lazyLocalDiagnostics)
	{
		if (diagnostics.IsEmpty)
		{
			return;
		}
		lazyLocalDiagnostics = lazyLocalDiagnostics ?? new Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>>();
		foreach (IGrouping<SyntaxTree, Diagnostic> item in from d in diagnostics
			group d by d.Location.SourceTree)
		{
			SyntaxTree key = item.Key;
			if (!lazyLocalDiagnostics.TryGetValue(key, out var value))
			{
				value = new Dictionary<DiagnosticAnalyzer, List<Diagnostic>>();
				lazyLocalDiagnostics[key] = value;
			}
			if (!value.TryGetValue(analyzer, out var value2))
			{
				value2 = (value[analyzer] = new List<Diagnostic>());
			}
			value2.AddRange(item);
		}
	}

	private void UpdateNonLocalDiagnosticsNoLock(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics)
	{
		if (!diagnostics.IsEmpty)
		{
			nonLocalDiagnosticsOpt = nonLocalDiagnosticsOpt ?? new Dictionary<DiagnosticAnalyzer, List<Diagnostic>>();
			if (!nonLocalDiagnosticsOpt.TryGetValue(analyzer, out var value))
			{
				value = new List<Diagnostic>();
				nonLocalDiagnosticsOpt[analyzer] = value;
			}
			value.AddRange(diagnostics);
		}
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		lock (gate)
		{
			return GetDiagnostics_NoLock(analysisScope, getLocalDiagnostics, getNonLocalDiagnostics);
		}
	}

	private ImmutableArray<Diagnostic> GetDiagnostics_NoLock(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		DebugAssertHelper.Assert(getLocalDiagnostics | getNonLocalDiagnostics);
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		if (getLocalDiagnostics)
		{
			if (!analysisScope.IsTreeAnalysis)
			{
				AddAllLocalDiagnostics_NoLock(localSyntaxDiagnosticsOpt, analysisScope, builder);
				AddAllLocalDiagnostics_NoLock(localSemanticDiagnosticsOpt, analysisScope, builder);
			}
			else if (analysisScope.IsSyntaxOnlyTreeAnalysis)
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(localSyntaxDiagnosticsOpt, analysisScope, builder);
			}
			else
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(localSemanticDiagnosticsOpt, analysisScope, builder);
			}
		}
		if (getNonLocalDiagnostics && nonLocalDiagnosticsOpt != null)
		{
			AddDiagnostics_NoLock(nonLocalDiagnosticsOpt, analysisScope, builder);
		}
		return builder.ToImmutable();
	}

	private static void AddAllLocalDiagnostics_NoLock(Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>> localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		if (localDiagnostics == null)
		{
			return;
		}
		foreach (Dictionary<DiagnosticAnalyzer, List<Diagnostic>> value in localDiagnostics.Values)
		{
			AddDiagnostics_NoLock(value, analysisScope, builder);
		}
	}

	private static void AddLocalDiagnosticsForPartialAnalysis_NoLock(Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, List<Diagnostic>>> localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		if (localDiagnostics != null && localDiagnostics.TryGetValue(analysisScope.FilterTreeOpt, out var value))
		{
			AddDiagnostics_NoLock(value, analysisScope, builder);
		}
	}

	private static void AddDiagnostics_NoLock(Dictionary<DiagnosticAnalyzer, List<Diagnostic>> diagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		DebugAssertHelper.Assert(diagnostics != null);
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (diagnostics.TryGetValue(analyzer, out var value))
			{
				builder.AddRange(value);
			}
		}
	}
}
