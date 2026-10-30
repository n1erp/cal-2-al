using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public abstract class StructuredTriviaSyntax : SyntaxNode, IStructuredTriviaSyntax
{
	private SyntaxTrivia parent;

	public override SyntaxTrivia ParentTrivia => parent;

	internal StructuredTriviaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, position, parent?.SyntaxTree)
	{
		DebugAssertHelper.Assert(parent == null || position >= 0);
	}

	internal static StructuredTriviaSyntax Create(SyntaxTrivia trivia)
	{
		InternalSyntaxNode underlyingNode = trivia.UnderlyingNode;
		SyntaxNode syntaxNode = trivia.Token.Parent;
		int position = trivia.Position;
		StructuredTriviaSyntax structuredTriviaSyntax = (StructuredTriviaSyntax)underlyingNode.CreateRed(syntaxNode, position);
		structuredTriviaSyntax.parent = trivia;
		return structuredTriviaSyntax;
	}
}
