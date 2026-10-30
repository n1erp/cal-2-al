using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AwaitExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken AwaitKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AwaitExpressionSyntax)Green).awaitKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal AwaitExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitAwaitExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAwaitExpression(this);
	}

	public AwaitExpressionSyntax Update(SyntaxToken awaitKeyword, ExpressionSyntax expression)
	{
		if (awaitKeyword != AwaitKeyword || expression != Expression)
		{
			AwaitExpressionSyntax awaitExpressionSyntax = SyntaxFactory.AwaitExpression(awaitKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return awaitExpressionSyntax.WithAnnotations(annotations);
			}
			return awaitExpressionSyntax;
		}
		return this;
	}

	public AwaitExpressionSyntax WithAwaitKeyword(SyntaxToken awaitKeyword)
	{
		return Update(awaitKeyword, Expression);
	}

	public AwaitExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(AwaitKeyword, expression);
	}
}
