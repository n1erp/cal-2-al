using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CompilationStartAnalyzerAction : AnalyzerAction
{
	private readonly Action<CompilationStartAnalysisContext> _action;

	public Action<CompilationStartAnalysisContext> Action => _action;

	public CompilationStartAnalyzerAction(Action<CompilationStartAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
