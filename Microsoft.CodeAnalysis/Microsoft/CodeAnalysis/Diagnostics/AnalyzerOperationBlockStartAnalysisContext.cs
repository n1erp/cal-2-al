using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerOperationBlockStartAnalysisContext : OperationBlockStartAnalysisContext
{
	private readonly DiagnosticAnalyzer _analyzer;

	private readonly HostOperationBlockStartAnalysisScope _scope;

	internal AnalyzerOperationBlockStartAnalysisContext(DiagnosticAnalyzer analyzer, HostOperationBlockStartAnalysisScope scope, ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Func<IOperation, ControlFlowGraph> getControlFlowGraph, CancellationToken cancellationToken)
		: base(operationBlocks, owningSymbol, compilation, options, getControlFlowGraph, cancellationToken)
	{
		_analyzer = analyzer;
		_scope = scope;
	}

	public override void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action)
	{
		_scope.RegisterOperationBlockEndAction(_analyzer, action);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		_scope.RegisterOperationAction(_analyzer, action, operationKinds);
	}
}
