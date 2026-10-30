using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerCodeBlockStartAnalysisContext : CodeBlockStartAnalysisContext
{
	private readonly DiagnosticAnalyzer analyzer;

	private readonly HostCodeBlockStartAnalysisScope scope;

	internal AnalyzerCodeBlockStartAnalysisContext(DiagnosticAnalyzer analyzer, HostCodeBlockStartAnalysisScope scope, SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, CancellationToken cancellationToken)
		: base(codeBlock, owningSymbol, semanticModel, options, cancellationToken)
	{
		this.analyzer = analyzer;
		this.scope = scope;
	}

	public override void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action)
	{
		scope.RegisterCodeBlockEndAction(analyzer, action);
	}

	public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
	{
		scope.RegisterSyntaxNodeAction(analyzer, action, syntaxKinds);
	}
}
