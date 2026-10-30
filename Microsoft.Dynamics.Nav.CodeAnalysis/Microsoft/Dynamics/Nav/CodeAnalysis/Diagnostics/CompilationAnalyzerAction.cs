using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationAnalyzerAction : AnalyzerAction
{
	public Action<CompilationAnalysisContext> Action { get; }

	public CompilationAnalyzerAction(Action<CompilationAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
