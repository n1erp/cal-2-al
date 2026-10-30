using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CodeBlockAnalyzerAction : AnalyzerAction
{
	private readonly Action<CodeBlockAnalysisContext> _action;

	public Action<CodeBlockAnalysisContext> Action => _action;

	public CodeBlockAnalyzerAction(Action<CodeBlockAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
