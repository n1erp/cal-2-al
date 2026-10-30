using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerOperationBlockStartAnalysisContext : OperationBlockStartAnalysisContext
{
	private readonly DiagnosticAnalyzer analyzer;

	private readonly HostOperationBlockStartAnalysisScope scope;

	internal AnalyzerOperationBlockStartAnalysisContext(DiagnosticAnalyzer analyzer, HostOperationBlockStartAnalysisScope scope, ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
		: base(operationBlocks, owningSymbol, compilation, options, cancellationToken)
	{
		this.analyzer = analyzer;
		this.scope = scope;
	}

	public override void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action)
	{
		scope.RegisterOperationBlockEndAction(analyzer, action);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		scope.RegisterOperationAction(analyzer, action, operationKinds);
	}
}
