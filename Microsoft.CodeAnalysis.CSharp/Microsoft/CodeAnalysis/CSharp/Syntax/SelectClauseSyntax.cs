using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SelectClauseSyntax : SelectOrGroupClauseSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken SelectKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SelectClauseSyntax)Green).selectKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal SelectClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitSelectClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSelectClause(this);
	}

	public SelectClauseSyntax Update(SyntaxToken selectKeyword, ExpressionSyntax expression)
	{
		if (selectKeyword != SelectKeyword || expression != Expression)
		{
			SelectClauseSyntax selectClauseSyntax = SyntaxFactory.SelectClause(selectKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return selectClauseSyntax.WithAnnotations(annotations);
			}
			return selectClauseSyntax;
		}
		return this;
	}

	public SelectClauseSyntax WithSelectKeyword(SyntaxToken selectKeyword)
	{
		return Update(selectKeyword, Expression);
	}

	public SelectClauseSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(SelectKeyword, expression);
	}
}
