using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
public struct OperationAnalysisContext
{
	private readonly Action<Diagnostic> reportDiagnostic;

	private readonly Func<Diagnostic, bool> isSupportedDiagnostic;

	public IOperation Operation { get; }

	public ISymbol ContainingSymbol { get; }

	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	public OperationAnalysisContext(IOperation operation, ISymbol containingSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		Operation = operation;
		ContainingSymbol = containingSymbol;
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
