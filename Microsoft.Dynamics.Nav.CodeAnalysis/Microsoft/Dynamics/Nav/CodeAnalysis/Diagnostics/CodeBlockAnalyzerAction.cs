using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CodeBlockAnalyzerAction : AnalyzerAction
{
	public Action<CodeBlockAnalysisContext> Action { get; }

	public CodeBlockAnalyzerAction(Action<CodeBlockAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
