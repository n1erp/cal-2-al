using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal class SimpleSyntaxReference : SyntaxReference
{
	private readonly SyntaxNode node;

	public override SyntaxTree SyntaxTree => node.SyntaxTree;

	public override TextSpan Span => node.Span;

	internal SimpleSyntaxReference(SyntaxNode node)
	{
		this.node = node;
	}

	public override SyntaxNode GetSyntax(CancellationToken cancellationToken)
	{
		return node;
	}
}
