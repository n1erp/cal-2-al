namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal abstract class AnalyzerAction
{
	internal DiagnosticAnalyzer Analyzer { get; }

	internal AnalyzerAction(DiagnosticAnalyzer analyzer)
	{
		Analyzer = analyzer;
	}
}
