using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationStartAnalyzerAction : AnalyzerAction
{
	public Action<CompilationStartAnalysisContext> Action { get; }

	public CompilationStartAnalyzerAction(Action<CompilationStartAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
