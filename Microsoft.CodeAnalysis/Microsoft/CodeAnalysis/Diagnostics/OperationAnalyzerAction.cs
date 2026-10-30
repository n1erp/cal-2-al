using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class OperationAnalyzerAction : AnalyzerAction
{
	private readonly Action<OperationAnalysisContext> _action;

	private readonly ImmutableArray<OperationKind> _kinds;

	public Action<OperationAnalysisContext> Action => _action;

	public ImmutableArray<OperationKind> Kinds => _kinds;

	public OperationAnalyzerAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> kinds, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
		_kinds = kinds;
	}
}
