using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LetClauseSyntax : QueryClauseSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken LetKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LetClauseSyntax)Green).letKeyword, Position, 0);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LetClauseSyntax)Green).identifier, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LetClauseSyntax)Green).equalsToken, GetChildPosition(2), GetChildIndex(2));

	public ExpressionSyntax Expression => GetRed(ref expression, 3);

	internal LetClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 3)
		{
			return GetRed(ref expression, 3);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 3)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLetClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLetClause(this);
	}

	public LetClauseSyntax Update(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
	{
		if (letKeyword != LetKeyword || identifier != Identifier || equalsToken != EqualsToken || expression != Expression)
		{
			LetClauseSyntax letClauseSyntax = SyntaxFactory.LetClause(letKeyword, identifier, equalsToken, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return letClauseSyntax.WithAnnotations(annotations);
			}
			return letClauseSyntax;
		}
		return this;
	}

	public LetClauseSyntax WithLetKeyword(SyntaxToken letKeyword)
	{
		return Update(letKeyword, Identifier, EqualsToken, Expression);
	}

	public LetClauseSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(LetKeyword, identifier, EqualsToken, Expression);
	}

	public LetClauseSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LetKeyword, Identifier, equalsToken, Expression);
	}

	public LetClauseSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(LetKeyword, Identifier, EqualsToken, expression);
	}
}
