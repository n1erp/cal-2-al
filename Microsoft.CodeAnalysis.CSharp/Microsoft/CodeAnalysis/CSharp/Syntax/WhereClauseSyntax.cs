using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class WhereClauseSyntax : QueryClauseSyntax
{
	private ExpressionSyntax condition;

	public SyntaxToken WhereKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhereClauseSyntax)Green).whereKeyword, Position, 0);

	public ExpressionSyntax Condition => GetRed(ref condition, 1);

	internal WhereClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref condition, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return condition;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhereClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhereClause(this);
	}

	public WhereClauseSyntax Update(SyntaxToken whereKeyword, ExpressionSyntax condition)
	{
		if (whereKeyword != WhereKeyword || condition != Condition)
		{
			WhereClauseSyntax whereClauseSyntax = SyntaxFactory.WhereClause(whereKeyword, condition);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return whereClauseSyntax.WithAnnotations(annotations);
			}
			return whereClauseSyntax;
		}
		return this;
	}

	public WhereClauseSyntax WithWhereKeyword(SyntaxToken whereKeyword)
	{
		return Update(whereKeyword, Condition);
	}

	public WhereClauseSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(WhereKeyword, condition);
	}
}
