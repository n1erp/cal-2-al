using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class CodeBlockStartAnalysisContext<TLanguageKindEnum> where TLanguageKindEnum : struct
{
	private readonly SyntaxNode _codeBlock;

	private readonly ISymbol _owningSymbol;

	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly CancellationToken _cancellationToken;

	public SyntaxNode CodeBlock => _codeBlock;

	public ISymbol OwningSymbol => _owningSymbol;

	public SemanticModel SemanticModel => _semanticModel;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	protected CodeBlockStartAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		_codeBlock = codeBlock;
		_owningSymbol = owningSymbol;
		_semanticModel = semanticModel;
		_options = options;
		_cancellationToken = cancellationToken;
	}

	public abstract void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action);

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds);
}
