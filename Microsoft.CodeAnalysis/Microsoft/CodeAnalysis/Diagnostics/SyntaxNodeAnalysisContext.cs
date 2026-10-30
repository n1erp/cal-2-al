using System;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

public struct SyntaxNodeAnalysisContext
{
	private readonly SyntaxNode _node;

	private readonly ISymbol _containingSymbol;

	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public SyntaxNode Node => _node;

	public ISymbol ContainingSymbol => _containingSymbol;

	public SemanticModel SemanticModel => _semanticModel;

	public Compilation Compilation => _semanticModel?.Compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	public SyntaxNodeAnalysisContext(SyntaxNode node, ISymbol containingSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		_node = node;
		_containingSymbol = containingSymbol;
		_semanticModel = semanticModel;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_cancellationToken = cancellationToken;
	}

	public SyntaxNodeAnalysisContext(SyntaxNode node, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(node, null, semanticModel, options, reportDiagnostic, isSupportedDiagnostic, cancellationToken)
	{
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, _semanticModel.Compilation, _isSupportedDiagnostic);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}
}
