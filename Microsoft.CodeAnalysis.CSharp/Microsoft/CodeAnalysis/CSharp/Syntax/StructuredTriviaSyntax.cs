using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class StructuredTriviaSyntax : CSharpSyntaxNode, IStructuredTriviaSyntax
{
	private SyntaxTrivia _parent;

	public override SyntaxTrivia ParentTrivia => _parent;

	internal StructuredTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, position, parent?.SyntaxTree)
	{
	}

	internal static StructuredTriviaSyntax Create(SyntaxTrivia trivia)
	{
		GreenNode underlyingNode = trivia.UnderlyingNode;
		SyntaxNode parent = trivia.Token.Parent;
		int position = trivia.Position;
		StructuredTriviaSyntax structuredTriviaSyntax = (StructuredTriviaSyntax)underlyingNode.CreateRed(parent, position);
		structuredTriviaSyntax._parent = trivia;
		return structuredTriviaSyntax;
	}
}
