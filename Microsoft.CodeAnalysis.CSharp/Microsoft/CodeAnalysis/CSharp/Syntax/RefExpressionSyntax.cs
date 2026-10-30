using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class RefExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken RefKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefExpressionSyntax)Green).refKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal RefExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitRefExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefExpression(this);
	}

	public RefExpressionSyntax Update(SyntaxToken refKeyword, ExpressionSyntax expression)
	{
		if (refKeyword != RefKeyword || expression != Expression)
		{
			RefExpressionSyntax refExpressionSyntax = SyntaxFactory.RefExpression(refKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return refExpressionSyntax.WithAnnotations(annotations);
			}
			return refExpressionSyntax;
		}
		return this;
	}

	public RefExpressionSyntax WithRefKeyword(SyntaxToken refKeyword)
	{
		return Update(refKeyword, Expression);
	}

	public RefExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(RefKeyword, expression);
	}
}
