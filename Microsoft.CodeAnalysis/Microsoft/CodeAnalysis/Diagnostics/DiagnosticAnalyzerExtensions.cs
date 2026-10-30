using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

public static class DiagnosticAnalyzerExtensions
{
	public static CompilationWithAnalyzers WithAnalyzers(this Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new CompilationWithAnalyzers(compilation, analyzers, options, cancellationToken);
	}

	public static CompilationWithAnalyzers WithAnalyzers(this Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
	{
		return new CompilationWithAnalyzers(compilation, analyzers, analysisOptions);
	}
}
