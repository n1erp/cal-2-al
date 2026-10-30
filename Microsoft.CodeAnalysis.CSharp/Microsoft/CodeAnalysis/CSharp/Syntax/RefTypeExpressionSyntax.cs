using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class RefTypeExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefTypeExpressionSyntax)Green).keyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefTypeExpressionSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefTypeExpressionSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	internal RefTypeExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref expression, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefTypeExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefTypeExpression(this);
	}

	public RefTypeExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken)
		{
			RefTypeExpressionSyntax refTypeExpressionSyntax = SyntaxFactory.RefTypeExpression(keyword, openParenToken, expression, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return refTypeExpressionSyntax.WithAnnotations(annotations);
			}
			return refTypeExpressionSyntax;
		}
		return this;
	}

	public RefTypeExpressionSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(keyword, OpenParenToken, Expression, CloseParenToken);
	}

	public RefTypeExpressionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(Keyword, openParenToken, Expression, CloseParenToken);
	}

	public RefTypeExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(Keyword, OpenParenToken, expression, CloseParenToken);
	}

	public RefTypeExpressionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(Keyword, OpenParenToken, Expression, closeParenToken);
	}
}
