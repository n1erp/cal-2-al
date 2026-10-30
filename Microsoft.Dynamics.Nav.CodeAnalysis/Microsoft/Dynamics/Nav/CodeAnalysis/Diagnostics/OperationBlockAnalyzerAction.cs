using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class OperationBlockAnalyzerAction : AnalyzerAction
{
	public Action<OperationBlockAnalysisContext> Action { get; }

	public OperationBlockAnalyzerAction(Action<OperationBlockAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
