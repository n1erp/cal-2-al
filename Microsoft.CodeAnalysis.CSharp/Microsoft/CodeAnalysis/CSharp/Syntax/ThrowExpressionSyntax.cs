using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ThrowExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken ThrowKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ThrowExpressionSyntax)Green).throwKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal ThrowExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitThrowExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowExpression(this);
	}

	public ThrowExpressionSyntax Update(SyntaxToken throwKeyword, ExpressionSyntax expression)
	{
		if (throwKeyword != ThrowKeyword || expression != Expression)
		{
			ThrowExpressionSyntax throwExpressionSyntax = SyntaxFactory.ThrowExpression(throwKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return throwExpressionSyntax.WithAnnotations(annotations);
			}
			return throwExpressionSyntax;
		}
		return this;
	}

	public ThrowExpressionSyntax WithThrowKeyword(SyntaxToken throwKeyword)
	{
		return Update(throwKeyword, Expression);
	}

	public ThrowExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ThrowKeyword, expression);
	}
}
