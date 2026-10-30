using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
public struct OperationBlockAnalysisContext
{
	private readonly Action<Diagnostic> reportDiagnostic;

	private readonly Func<Diagnostic, bool> isSupportedDiagnostic;

	public ImmutableArray<IOperation> OperationBlocks { get; }

	public ISymbol OwningSymbol { get; }

	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	public OperationBlockAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		OperationBlocks = operationBlocks;
		OwningSymbol = owningSymbol;
		Compilation = compilation;
		Options = options;
		this.reportDiagnostic = reportDiagnostic;
		this.isSupportedDiagnostic = isSupportedDiagnostic;
		CancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, Compilation, isSupportedDiagnostic);
		lock (reportDiagnostic)
		{
			reportDiagnostic(diagnostic);
		}
	}
}
