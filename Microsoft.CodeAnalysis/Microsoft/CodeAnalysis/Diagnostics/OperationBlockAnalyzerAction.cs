using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class OperationBlockAnalyzerAction : AnalyzerAction
{
	private readonly Action<OperationBlockAnalysisContext> _action;

	public Action<OperationBlockAnalysisContext> Action => _action;

	public OperationBlockAnalyzerAction(Action<OperationBlockAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
