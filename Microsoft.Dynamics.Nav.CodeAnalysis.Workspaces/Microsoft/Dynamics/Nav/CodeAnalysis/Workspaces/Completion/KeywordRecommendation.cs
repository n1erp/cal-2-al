using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class KeywordRecommendation
{
	internal SyntaxKind SyntaxKind { get; }

	internal SyntaxNodeDefinition SyntaxNodeDefinition { get; }

	public KeywordRecommendation(SyntaxKind kind, SyntaxNodeDefinition node)
	{
		SyntaxKind = kind;
		SyntaxNodeDefinition = node;
	}
}
