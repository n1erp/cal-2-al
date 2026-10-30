using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class WhenClauseSyntax : CSharpSyntaxNode
{
	private ExpressionSyntax condition;

	public SyntaxToken WhenKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhenClauseSyntax)Green).whenKeyword, Position, 0);

	public ExpressionSyntax Condition => GetRed(ref condition, 1);

	internal WhenClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitWhenClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhenClause(this);
	}

	public WhenClauseSyntax Update(SyntaxToken whenKeyword, ExpressionSyntax condition)
	{
		if (whenKeyword != WhenKeyword || condition != Condition)
		{
			WhenClauseSyntax whenClauseSyntax = SyntaxFactory.WhenClause(whenKeyword, condition);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return whenClauseSyntax.WithAnnotations(annotations);
			}
			return whenClauseSyntax;
		}
		return this;
	}

	public WhenClauseSyntax WithWhenKeyword(SyntaxToken whenKeyword)
	{
		return Update(whenKeyword, Condition);
	}

	public WhenClauseSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(WhenKeyword, condition);
	}
}
