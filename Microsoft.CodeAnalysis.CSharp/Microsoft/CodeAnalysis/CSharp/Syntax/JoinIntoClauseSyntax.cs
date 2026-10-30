using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class JoinIntoClauseSyntax : CSharpSyntaxNode
{
	public SyntaxToken IntoKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinIntoClauseSyntax)Green).intoKeyword, Position, 0);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinIntoClauseSyntax)Green).identifier, GetChildPosition(1), GetChildIndex(1));

	internal JoinIntoClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitJoinIntoClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitJoinIntoClause(this);
	}

	public JoinIntoClauseSyntax Update(SyntaxToken intoKeyword, SyntaxToken identifier)
	{
		if (intoKeyword != IntoKeyword || identifier != Identifier)
		{
			JoinIntoClauseSyntax joinIntoClauseSyntax = SyntaxFactory.JoinIntoClause(intoKeyword, identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return joinIntoClauseSyntax.WithAnnotations(annotations);
			}
			return joinIntoClauseSyntax;
		}
		return this;
	}

	public JoinIntoClauseSyntax WithIntoKeyword(SyntaxToken intoKeyword)
	{
		return Update(intoKeyword, Identifier);
	}

	public JoinIntoClauseSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(IntoKeyword, identifier);
	}
}
