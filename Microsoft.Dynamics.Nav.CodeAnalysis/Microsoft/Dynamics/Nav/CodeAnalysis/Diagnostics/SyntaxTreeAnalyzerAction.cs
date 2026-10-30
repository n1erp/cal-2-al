using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class SyntaxTreeAnalyzerAction : AnalyzerAction
{
	public Action<SyntaxTreeAnalysisContext> Action { get; }

	public SyntaxTreeAnalyzerAction(Action<SyntaxTreeAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
