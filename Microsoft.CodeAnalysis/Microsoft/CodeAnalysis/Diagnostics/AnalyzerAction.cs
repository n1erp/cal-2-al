namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class AnalyzerAction
{
	private readonly DiagnosticAnalyzer _analyzer;

	internal DiagnosticAnalyzer Analyzer => _analyzer;

	internal AnalyzerAction(DiagnosticAnalyzer analyzer)
	{
		_analyzer = analyzer;
	}
}
