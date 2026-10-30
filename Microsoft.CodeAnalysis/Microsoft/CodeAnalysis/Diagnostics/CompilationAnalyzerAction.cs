using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CompilationAnalyzerAction : AnalyzerAction
{
	private readonly Action<CompilationAnalysisContext> _action;

	public Action<CompilationAnalysisContext> Action => _action;

	public CompilationAnalyzerAction(Action<CompilationAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
