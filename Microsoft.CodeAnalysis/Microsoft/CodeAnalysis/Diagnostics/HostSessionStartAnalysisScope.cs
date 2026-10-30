using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostSessionStartAnalysisScope : HostAnalysisScope
{
	private ImmutableArray<CompilationStartAnalyzerAction> _compilationStartActions = ImmutableArray<CompilationStartAnalyzerAction>.Empty;

	private ImmutableHashSet<DiagnosticAnalyzer> _concurrentAnalyzers = ImmutableHashSet<DiagnosticAnalyzer>.Empty;

	private ConcurrentDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> _generatedCodeConfigurationMap = new ConcurrentDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>();

	public ImmutableArray<CompilationStartAnalyzerAction> CompilationStartActions => _compilationStartActions;

	public bool IsConcurrentAnalyzer(DiagnosticAnalyzer analyzer)
	{
		return _concurrentAnalyzers.Contains(analyzer);
	}

	public GeneratedCodeAnalysisFlags GetGeneratedCodeAnalysisFlags(DiagnosticAnalyzer analyzer)
	{
		if (!_generatedCodeConfigurationMap.TryGetValue(analyzer, out var value))
		{
			return GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics;
		}
		return value;
	}

	public void RegisterCompilationStartAction(DiagnosticAnalyzer analyzer, Action<CompilationStartAnalysisContext> action)
	{
		CompilationStartAnalyzerAction compilationStartAnalyzerAction = new CompilationStartAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationStartAction(compilationStartAnalyzerAction);
		_compilationStartActions = _compilationStartActions.Add(compilationStartAnalyzerAction);
	}

	public void EnableConcurrentExecution(DiagnosticAnalyzer analyzer)
	{
		_concurrentAnalyzers = _concurrentAnalyzers.Add(analyzer);
		GetOrCreateAnalyzerActions(analyzer).EnableConcurrentExecution();
	}

	public void ConfigureGeneratedCodeAnalysis(DiagnosticAnalyzer analyzer, GeneratedCodeAnalysisFlags mode)
	{
		_generatedCodeConfigurationMap.AddOrUpdate(analyzer, mode, (DiagnosticAnalyzer a, GeneratedCodeAnalysisFlags c) => mode);
	}
}
