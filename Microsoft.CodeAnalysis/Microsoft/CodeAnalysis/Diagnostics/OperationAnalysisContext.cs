using System;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

public struct OperationAnalysisContext
{
	private readonly IOperation _operation;

	private readonly ISymbol _containingSymbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, bool> _isSupportedDiagnostic;

	private readonly Func<IOperation, ControlFlowGraph> _getControlFlowGraphOpt;

	private readonly CancellationToken _cancellationToken;

	public IOperation Operation => _operation;

	public ISymbol ContainingSymbol => _containingSymbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	public OperationAnalysisContext(IOperation operation, ISymbol containingSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		_operation = operation;
		_containingSymbol = containingSymbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_cancellationToken = cancellationToken;
		_getControlFlowGraphOpt = null;
	}

	internal OperationAnalysisContext(IOperation operation, ISymbol containingSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, Func<IOperation, ControlFlowGraph> getControlFlowGraph, CancellationToken cancellationToken)
	{
		_operation = operation;
		_containingSymbol = containingSymbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_getControlFlowGraphOpt = getControlFlowGraph;
		_cancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, _compilation, _isSupportedDiagnostic);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}

	public ControlFlowGraph GetControlFlowGraph()
	{
		return DiagnosticAnalysisContextHelpers.GetControlFlowGraph(Operation, _getControlFlowGraphOpt, _cancellationToken);
	}
}
