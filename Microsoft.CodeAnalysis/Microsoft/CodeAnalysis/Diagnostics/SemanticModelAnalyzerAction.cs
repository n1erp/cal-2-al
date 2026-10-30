using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SemanticModelAnalyzerAction : AnalyzerAction
{
	private readonly Action<SemanticModelAnalysisContext> _action;

	public Action<SemanticModelAnalysisContext> Action => _action;

	public SemanticModelAnalyzerAction(Action<SemanticModelAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
