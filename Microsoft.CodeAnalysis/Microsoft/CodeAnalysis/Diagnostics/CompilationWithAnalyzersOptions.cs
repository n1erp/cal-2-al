using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class CompilationWithAnalyzersOptions
{
	private readonly AnalyzerOptions _options;

	private readonly Action<Exception, DiagnosticAnalyzer, Diagnostic> _onAnalyzerException;

	private readonly Func<Exception, bool> _analyzerExceptionFilter;

	private readonly bool _concurrentAnalysis;

	private readonly bool _logAnalyzerExecutionTime;

	private readonly bool _reportSuppressedDiagnostics;

	public AnalyzerOptions Options => _options;

	public Action<Exception, DiagnosticAnalyzer, Diagnostic> OnAnalyzerException => _onAnalyzerException;

	public Func<Exception, bool> AnalyzerExceptionFilter => _analyzerExceptionFilter;

	public bool ConcurrentAnalysis => _concurrentAnalysis;

	public bool LogAnalyzerExecutionTime => _logAnalyzerExecutionTime;

	public bool ReportSuppressedDiagnostics => _reportSuppressedDiagnostics;

	public CompilationWithAnalyzersOptions(AnalyzerOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool concurrentAnalysis, bool logAnalyzerExecutionTime)
		: this(options, onAnalyzerException, concurrentAnalysis, logAnalyzerExecutionTime, reportSuppressedDiagnostics: false)
	{
	}

	public CompilationWithAnalyzersOptions(AnalyzerOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool concurrentAnalysis, bool logAnalyzerExecutionTime, bool reportSuppressedDiagnostics)
		: this(options, onAnalyzerException, concurrentAnalysis, logAnalyzerExecutionTime, reportSuppressedDiagnostics, null)
	{
	}

	public CompilationWithAnalyzersOptions(AnalyzerOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, bool concurrentAnalysis, bool logAnalyzerExecutionTime, bool reportSuppressedDiagnostics, Func<Exception, bool> analyzerExceptionFilter)
	{
		_options = options;
		_onAnalyzerException = onAnalyzerException;
		_analyzerExceptionFilter = analyzerExceptionFilter;
		_concurrentAnalysis = concurrentAnalysis;
		_logAnalyzerExecutionTime = logAnalyzerExecutionTime;
		_reportSuppressedDiagnostics = reportSuppressedDiagnostics;
	}
}
