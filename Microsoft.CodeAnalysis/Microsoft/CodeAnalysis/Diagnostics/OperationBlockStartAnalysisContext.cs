using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class OperationBlockStartAnalysisContext
{
	private readonly ImmutableArray<IOperation> _operationBlocks;

	private readonly ISymbol _owningSymbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Func<IOperation, ControlFlowGraph> _getControlFlowGraphOpt;

	private readonly CancellationToken _cancellationToken;

	public ImmutableArray<IOperation> OperationBlocks => _operationBlocks;

	public ISymbol OwningSymbol => _owningSymbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	protected OperationBlockStartAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		_operationBlocks = operationBlocks;
		_owningSymbol = owningSymbol;
		_compilation = compilation;
		_options = options;
		_cancellationToken = cancellationToken;
		_getControlFlowGraphOpt = null;
	}

	internal OperationBlockStartAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Func<IOperation, ControlFlowGraph> getControlFlowGraph, CancellationToken cancellationToken)
	{
		_operationBlocks = operationBlocks;
		_owningSymbol = owningSymbol;
		_compilation = compilation;
		_options = options;
		_getControlFlowGraphOpt = getControlFlowGraph;
		_cancellationToken = cancellationToken;
	}

	public abstract void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action);

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds);

	public ControlFlowGraph GetControlFlowGraph(IOperation operationBlock)
	{
		if (operationBlock == null)
		{
			throw new ArgumentNullException("operationBlock");
		}
		if (!OperationBlocks.Contains(operationBlock))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidOperationBlockForAnalysisContext, "operationBlock");
		}
		return DiagnosticAnalysisContextHelpers.GetControlFlowGraph(operationBlock, _getControlFlowGraphOpt, _cancellationToken);
	}
}
