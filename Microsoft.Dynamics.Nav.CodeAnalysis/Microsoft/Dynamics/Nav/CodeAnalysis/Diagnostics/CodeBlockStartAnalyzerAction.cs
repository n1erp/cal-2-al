using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CodeBlockStartAnalyzerAction : AnalyzerAction
{
	public Action<CodeBlockStartAnalysisContext> Action { get; }

	public CodeBlockStartAnalyzerAction(Action<CodeBlockStartAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
