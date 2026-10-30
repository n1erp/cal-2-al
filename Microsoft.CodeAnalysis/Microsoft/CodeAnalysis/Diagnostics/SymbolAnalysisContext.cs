using System;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

public struct SymbolAnalysisContext
{
	private readonly ISymbol _symbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public ISymbol Symbol => _symbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	internal Func<Diagnostic, bool> IsSupportedDiagnostic => _isSupportedDiagnostic;

	public SymbolAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		_symbol = symbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
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
}
