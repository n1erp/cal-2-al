using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class OperationBlockStartAnalyzerAction : AnalyzerAction
{
	private readonly Action<OperationBlockStartAnalysisContext> _action;

	public Action<OperationBlockStartAnalysisContext> Action => _action;

	public OperationBlockStartAnalyzerAction(Action<OperationBlockStartAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
