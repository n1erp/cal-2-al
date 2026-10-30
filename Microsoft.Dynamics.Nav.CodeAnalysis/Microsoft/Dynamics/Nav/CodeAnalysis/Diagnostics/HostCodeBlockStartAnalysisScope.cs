using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class HostCodeBlockStartAnalysisScope
{
	private ImmutableArray<CodeBlockAnalyzerAction> codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxNodeAnalyzerAction> syntaxNodeActions = ImmutableArray<SyntaxNodeAnalyzerAction>.Empty;

	public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => codeBlockEndActions;

	public ImmutableArray<SyntaxNodeAnalyzerAction> SyntaxNodeActions => syntaxNodeActions;

	public void RegisterCodeBlockEndAction(DiagnosticAnalyzer analyzer, Action<CodeBlockAnalysisContext> action)
	{
		codeBlockEndActions = codeBlockEndActions.Add(new CodeBlockAnalyzerAction(action, analyzer));
	}

	public void RegisterSyntaxNodeAction(DiagnosticAnalyzer analyzer, Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
	{
		syntaxNodeActions = syntaxNodeActions.Add(new SyntaxNodeAnalyzerAction(action, syntaxKinds, analyzer));
	}
}
