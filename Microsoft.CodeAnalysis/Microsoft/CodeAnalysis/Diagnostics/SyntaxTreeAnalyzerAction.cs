using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SyntaxTreeAnalyzerAction : AnalyzerAction
{
	private readonly Action<SyntaxTreeAnalysisContext> _action;

	public Action<SyntaxTreeAnalysisContext> Action => _action;

	public SyntaxTreeAnalyzerAction(Action<SyntaxTreeAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
