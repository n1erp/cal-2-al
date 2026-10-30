using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ArrowExpressionClauseSyntax : CSharpSyntaxNode
{
	private ExpressionSyntax expression;

	public SyntaxToken ArrowToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrowExpressionClauseSyntax)Green).arrowToken, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal ArrowExpressionClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitArrowExpressionClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrowExpressionClause(this);
	}

	public ArrowExpressionClauseSyntax Update(SyntaxToken arrowToken, ExpressionSyntax expression)
	{
		if (arrowToken != ArrowToken || expression != Expression)
		{
			ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = SyntaxFactory.ArrowExpressionClause(arrowToken, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return arrowExpressionClauseSyntax.WithAnnotations(annotations);
			}
			return arrowExpressionClauseSyntax;
		}
		return this;
	}

	public ArrowExpressionClauseSyntax WithArrowToken(SyntaxToken arrowToken)
	{
		return Update(arrowToken, Expression);
	}

	public ArrowExpressionClauseSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ArrowToken, expression);
	}
}
