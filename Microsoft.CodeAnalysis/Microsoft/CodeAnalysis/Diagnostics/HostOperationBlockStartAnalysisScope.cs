using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostOperationBlockStartAnalysisScope
{
	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> _operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions => _operationBlockEndActions;

	public ImmutableArray<OperationAnalyzerAction> OperationActions => _operationActions;

	internal HostOperationBlockStartAnalysisScope()
	{
	}

	public void RegisterOperationBlockEndAction(DiagnosticAnalyzer analyzer, Action<OperationBlockAnalysisContext> action)
	{
		_operationBlockEndActions = _operationBlockEndActions.Add(new OperationBlockAnalyzerAction(action, analyzer));
	}

	public void RegisterOperationAction(DiagnosticAnalyzer analyzer, Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		_operationActions = _operationActions.Add(new OperationAnalyzerAction(action, operationKinds, analyzer));
	}
}
