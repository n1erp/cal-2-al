using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public struct CompilationAnalysisContext
{
	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, bool> _isSupportedDiagnostic;

	private readonly CompilationAnalysisValueProviderFactory _compilationAnalysisValueProviderFactoryOpt;

	private readonly CancellationToken _cancellationToken;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	public CompilationAnalysisContext(Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(compilation, options, reportDiagnostic, isSupportedDiagnostic, null, cancellationToken)
	{
	}

	internal CompilationAnalysisContext(Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CompilationAnalysisValueProviderFactory compilationAnalysisValueProviderFactoryOpt, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_compilationAnalysisValueProviderFactoryOpt = compilationAnalysisValueProviderFactoryOpt;
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

	public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value)
	{
		return TryGetValue(text, valueProvider.CoreValueProvider, out value);
	}

	public bool TryGetValue<TValue>(SyntaxTree tree, SyntaxTreeValueProvider<TValue> valueProvider, out TValue value)
	{
		return TryGetValue(tree, valueProvider.CoreValueProvider, out value);
	}

	private bool TryGetValue<TKey, TValue>(TKey key, AnalysisValueProvider<TKey, TValue> valueProvider, out TValue value) where TKey : class
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(key, valueProvider);
		if (_compilationAnalysisValueProviderFactoryOpt != null)
		{
			return _compilationAnalysisValueProviderFactoryOpt.GetValueProvider(valueProvider).TryGetValue(key, out value);
		}
		return valueProvider.TryGetValue(key, out value);
	}
}
