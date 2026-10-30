using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class OperationBlockStartAnalyzerAction : AnalyzerAction
{
	public Action<OperationBlockStartAnalysisContext> Action { get; }

	public OperationBlockStartAnalyzerAction(Action<OperationBlockStartAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
