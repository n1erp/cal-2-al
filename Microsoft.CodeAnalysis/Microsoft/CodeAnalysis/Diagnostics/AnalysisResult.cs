using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public class AnalysisResult
{
	public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; private set; }

	public ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> SyntaxDiagnostics { get; private set; }

	public ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> SemanticDiagnostics { get; private set; }

	public ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> CompilationDiagnostics { get; private set; }

	public ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> AnalyzerTelemetryInfo { get; private set; }

	internal AnalysisResult(ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> localSyntaxDiagnostics, ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> localSemanticDiagnostics, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> nonLocalDiagnostics, ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> analyzerTelemetryInfo)
	{
		Analyzers = analyzers;
		SyntaxDiagnostics = localSyntaxDiagnostics;
		SemanticDiagnostics = localSemanticDiagnostics;
		CompilationDiagnostics = nonLocalDiagnostics;
		AnalyzerTelemetryInfo = analyzerTelemetryInfo;
	}

	public ImmutableArray<Diagnostic> GetAllDiagnostics(DiagnosticAnalyzer analyzer)
	{
		if (!Analyzers.Contains(analyzer))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "analyzer");
		}
		return GetDiagnostics(SpecializedCollections.SingletonEnumerable(analyzer), getLocalSyntaxDiagnostics: true, getLocalSemanticDiagnostics: true, getNonLocalDiagnostics: true);
	}

	public ImmutableArray<Diagnostic> GetAllDiagnostics()
	{
		return GetDiagnostics(Analyzers, getLocalSyntaxDiagnostics: true, getLocalSemanticDiagnostics: true, getNonLocalDiagnostics: true);
	}

	internal ImmutableArray<Diagnostic> GetSyntaxDiagnostics(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		return GetDiagnostics(analyzers, getLocalSyntaxDiagnostics: true, getLocalSemanticDiagnostics: false, getNonLocalDiagnostics: false);
	}

	internal ImmutableArray<Diagnostic> GetSemanticDiagnostics(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		return GetDiagnostics(analyzers, getLocalSyntaxDiagnostics: false, getLocalSemanticDiagnostics: true, getNonLocalDiagnostics: false);
	}

	internal ImmutableArray<Diagnostic> GetDiagnostics(IEnumerable<DiagnosticAnalyzer> analyzers, bool getLocalSyntaxDiagnostics, bool getLocalSemanticDiagnostics, bool getNonLocalDiagnostics)
	{
		IEnumerable<DiagnosticAnalyzer> source = Analyzers.Except(analyzers);
		ImmutableHashSet<DiagnosticAnalyzer> excludedAnalyzers = (source.Any() ? source.ToImmutableHashSet() : ImmutableHashSet<DiagnosticAnalyzer>.Empty);
		return GetDiagnostics(excludedAnalyzers, getLocalSyntaxDiagnostics, getLocalSemanticDiagnostics, getNonLocalDiagnostics);
	}

	private ImmutableArray<Diagnostic> GetDiagnostics(ImmutableHashSet<DiagnosticAnalyzer> excludedAnalyzers, bool getLocalSyntaxDiagnostics, bool getLocalSemanticDiagnostics, bool getNonLocalDiagnostics)
	{
		if (SyntaxDiagnostics.Count > 0 || SemanticDiagnostics.Count > 0 || CompilationDiagnostics.Count > 0)
		{
			ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
			if (getLocalSyntaxDiagnostics)
			{
				AddLocalDiagnostics(SyntaxDiagnostics, excludedAnalyzers, builder);
			}
			if (getLocalSemanticDiagnostics)
			{
				AddLocalDiagnostics(SemanticDiagnostics, excludedAnalyzers, builder);
			}
			if (getNonLocalDiagnostics)
			{
				AddNonLocalDiagnostics(CompilationDiagnostics, excludedAnalyzers, builder);
			}
			return builder.ToImmutable();
		}
		return ImmutableArray<Diagnostic>.Empty;
	}

	private static void AddLocalDiagnostics(ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> localDiagnostics, ImmutableHashSet<DiagnosticAnalyzer> excludedAnalyzers, ImmutableArray<Diagnostic>.Builder builder)
	{
		foreach (KeyValuePair<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> localDiagnostic in localDiagnostics)
		{
			foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> item in localDiagnostic.Value)
			{
				if (!excludedAnalyzers.Contains(item.Key))
				{
					builder.AddRange(item.Value);
				}
			}
		}
	}

	private static void AddNonLocalDiagnostics(ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> nonLocalDiagnostics, ImmutableHashSet<DiagnosticAnalyzer> excludedAnalyzers, ImmutableArray<Diagnostic>.Builder builder)
	{
		foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> nonLocalDiagnostic in nonLocalDiagnostics)
		{
			if (!excludedAnalyzers.Contains(nonLocalDiagnostic.Key))
			{
				builder.AddRange(nonLocalDiagnostic.Value);
			}
		}
	}
}
