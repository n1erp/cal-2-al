using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

public struct OperationBlockAnalysisContext
{
	private readonly ImmutableArray<IOperation> _operationBlocks;

	private readonly ISymbol _owningSymbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, bool> _isSupportedDiagnostic;

	private readonly Func<IOperation, ControlFlowGraph> _getControlFlowGraphOpt;

	private readonly CancellationToken _cancellationToken;

	public ImmutableArray<IOperation> OperationBlocks => _operationBlocks;

	public ISymbol OwningSymbol => _owningSymbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	public OperationBlockAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		_operationBlocks = operationBlocks;
		_owningSymbol = owningSymbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_cancellationToken = cancellationToken;
		_getControlFlowGraphOpt = null;
	}

	internal OperationBlockAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, Func<IOperation, ControlFlowGraph> getControlFlowGraph, CancellationToken cancellationToken)
	{
		_operationBlocks = operationBlocks;
		_owningSymbol = owningSymbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_getControlFlowGraphOpt = getControlFlowGraph;
		_cancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, Compilation, _isSupportedDiagnostic);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}

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
