using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedExpressionSyntax)Green).openParenToken, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedExpressionSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal ParenthesizedExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref expression, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public ParenthesizedExpressionSyntax Update(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken)
		{
			ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = SyntaxFactory.ParenthesizedExpression(openParenToken, expression, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parenthesizedExpressionSyntax.WithAnnotations(annotations);
			}
			return parenthesizedExpressionSyntax;
		}
		return this;
	}

	public ParenthesizedExpressionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Expression, CloseParenToken);
	}

	public ParenthesizedExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(OpenParenToken, expression, CloseParenToken);
	}

	public ParenthesizedExpressionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Expression, closeParenToken);
	}
}
