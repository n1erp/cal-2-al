using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class AnalyzerCompilationContext
{
	public AnalyzerDriver AnalyzerDriver { get; }

	public IEnumerable<Diagnostic> AnalyzerExceptionDiagnostics { get; }

	public bool ReportAnalyzerDiagnostics { get; }

	public Compilation Compilation { get; }

	public CancellationTokenSource AnalyzerTokenSource { get; }

	internal AnalyzerCompilationContext(AnalyzerDriver driver, IEnumerable<Diagnostic> analyzerExceptionDiagnostics, Compilation compilation, CancellationTokenSource analyzerTokenSource, bool reportDiagnostics)
	{
		AnalyzerDriver = driver;
		AnalyzerExceptionDiagnostics = analyzerExceptionDiagnostics;
		Compilation = compilation;
		ReportAnalyzerDiagnostics = reportDiagnostics;
		AnalyzerTokenSource = analyzerTokenSource;
	}

	internal static AnalyzerCompilationContext CreateAnalyzerContext(ImmutableArray<DiagnosticAnalyzer> analyzers, IList<DiagnosticInfo> diagnostics, ImmutableArray<AdditionalText> additionalTextFiles, Compilation compilation, bool reportAnalyzer, CancellationToken cancellationToken)
	{
		if (diagnostics.Any() || analyzers.IsDefaultOrEmpty)
		{
			return new AnalyzerCompilationContext(null, Enumerable.Empty<Diagnostic>(), compilation, null, reportDiagnostics: false);
		}
		cancellationToken.ThrowIfCancellationRequested();
		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		AnalyzerManager analyzerManager = new AnalyzerManager();
		ConcurrentSet<Diagnostic> analyzerExceptionDiagnostics = new ConcurrentSet<Diagnostic>();
		Action<Diagnostic> addExceptionDiagnostic = (Diagnostic diagnostic) =>
		{
			analyzerExceptionDiagnostics.Add(diagnostic);
		};
		AnalyzerOptions options = new AnalyzerOptions(additionalTextFiles);
		Compilation newCompilation;
		return new AnalyzerCompilationContext(AnalyzerDriverBase.CreateAndAttachToCompilation(compilation, analyzers, options, analyzerManager, addExceptionDiagnostic, reportAnalyzer, out newCompilation, cancellationTokenSource.Token), analyzerExceptionDiagnostics, newCompilation, cancellationTokenSource, reportAnalyzer && !analyzers.IsEmpty);
	}
}
