using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class HostSessionStartAnalysisScope : HostAnalysisScope
{
	private ImmutableArray<CompilationStartAnalyzerAction> compilationStartActions = ImmutableArray<CompilationStartAnalyzerAction>.Empty;

	public ImmutableArray<CompilationStartAnalyzerAction> CompilationStartActions => compilationStartActions;

	public void RegisterCompilationStartAction(DiagnosticAnalyzer analyzer, Action<CompilationStartAnalysisContext> action)
	{
		CompilationStartAnalyzerAction compilationStartAnalyzerAction = new CompilationStartAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationStartAction(compilationStartAnalyzerAction);
		compilationStartActions = compilationStartActions.Add(compilationStartAnalyzerAction);
	}
}
