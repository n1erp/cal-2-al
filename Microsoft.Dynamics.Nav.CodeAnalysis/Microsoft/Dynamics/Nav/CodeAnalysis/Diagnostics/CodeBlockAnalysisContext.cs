using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not used in hashing algorithms for now.")]
public struct CodeBlockAnalysisContext
{
	private readonly Action<Diagnostic> reportDiagnostic;

	private readonly Func<Diagnostic, bool> isSupportedDiagnostic;

	public SyntaxNode CodeBlock { get; }

	public ISymbol OwningSymbol { get; }

	public SemanticModel SemanticModel { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	public CodeBlockAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		CodeBlock = codeBlock;
		OwningSymbol = owningSymbol;
		SemanticModel = semanticModel;
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
