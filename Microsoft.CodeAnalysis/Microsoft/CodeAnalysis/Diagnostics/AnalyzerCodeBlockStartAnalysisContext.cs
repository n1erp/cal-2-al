using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum> : CodeBlockStartAnalysisContext<TLanguageKindEnum> where TLanguageKindEnum : struct
{
	private readonly DiagnosticAnalyzer _analyzer;

	private readonly HostCodeBlockStartAnalysisScope<TLanguageKindEnum> _scope;

	internal AnalyzerCodeBlockStartAnalysisContext(DiagnosticAnalyzer analyzer, HostCodeBlockStartAnalysisScope<TLanguageKindEnum> scope, SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, CancellationToken cancellationToken)
		: base(codeBlock, owningSymbol, semanticModel, options, cancellationToken)
	{
		_analyzer = analyzer;
		_scope = scope;
	}

	public override void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action)
	{
		_scope.RegisterCodeBlockEndAction(_analyzer, action);
	}

	public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
	{
		_scope.RegisterSyntaxNodeAction(_analyzer, action, syntaxKinds);
	}
}
