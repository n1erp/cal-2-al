using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not used in hashing algorithms for now.")]
public struct SymbolAnalysisContext
{
	private readonly Action<Diagnostic> reportDiagnostic;

	private readonly Func<Diagnostic, bool> isSupportedDiagnostic;

	public ISymbol Symbol { get; }

	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	public SymbolAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		Symbol = symbol;
		Compilation = compilation;
		Options = options;
		this.reportDiagnostic = reportDiagnostic;
		this.isSupportedDiagnostic = isSupportedDiagnostic;
		CancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, isSupportedDiagnostic);
		lock (reportDiagnostic)
		{
			reportDiagnostic(diagnostic);
		}
	}
}
