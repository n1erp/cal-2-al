using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class OperationBlockStartAnalysisContext
{
	public ImmutableArray<IOperation> OperationBlocks { get; }

	public ISymbol OwningSymbol { get; }

	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	protected OperationBlockStartAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		OperationBlocks = operationBlocks;
		OwningSymbol = owningSymbol;
		Compilation = compilation;
		Options = options;
		CancellationToken = cancellationToken;
	}

	public abstract void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action);

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds);
}
