using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public class CompilationWithAnalyzersOptions
{
	public AnalyzerOptions AnalyzerOptions { get; }

	public Action<Exception, DiagnosticAnalyzer, Diagnostic> OnAnalyzerException { get; }

	public bool ConcurrentAnalysis { get; }

	public bool LogAnalyzerExecutionTime { get; }

	public bool ReportSuppressedDiagnostics { get; }

	public CompilationWithAnalyzersOptions(AnalyzerOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool concurrentAnalysis, bool logAnalyzerExecutionTime)
		: this(options, onAnalyzerException, concurrentAnalysis, logAnalyzerExecutionTime, reportSuppressedDiagnostics: false)
	{
	}

	public CompilationWithAnalyzersOptions(AnalyzerOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool concurrentAnalysis, bool logAnalyzerExecutionTime, bool reportSuppressedDiagnostics)
	{
		AnalyzerOptions = options;
		OnAnalyzerException = onAnalyzerException;
		ConcurrentAnalysis = concurrentAnalysis;
		LogAnalyzerExecutionTime = logAnalyzerExecutionTime;
		ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
	}
}
