using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SimpleSyntaxReference : SyntaxReference
{
	private readonly SyntaxNode _node;

	public override SyntaxTree SyntaxTree => _node.SyntaxTree;

	public override TextSpan Span => _node.Span;

	internal SimpleSyntaxReference(SyntaxNode node)
	{
		_node = node;
	}

	public override SyntaxNode GetSyntax(CancellationToken cancellationToken)
	{
		return _node;
	}
}
