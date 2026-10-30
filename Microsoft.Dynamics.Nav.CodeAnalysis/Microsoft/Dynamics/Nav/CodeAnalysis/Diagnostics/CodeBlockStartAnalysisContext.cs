using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class CodeBlockStartAnalysisContext
{
	public SyntaxNode CodeBlock { get; }

	public ISymbol OwningSymbol { get; }

	public SemanticModel SemanticModel { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	protected CodeBlockStartAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		CodeBlock = codeBlock;
		OwningSymbol = owningSymbol;
		SemanticModel = semanticModel;
		Options = options;
		CancellationToken = cancellationToken;
	}

	public abstract void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action);

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrNull());
	}

	public abstract void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds);
}
