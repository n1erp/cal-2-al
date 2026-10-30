using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class YieldStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken YieldKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.YieldStatementSyntax)Green).yieldKeyword, Position, 0);

	public SyntaxToken ReturnOrBreakKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.YieldStatementSyntax)Green).returnOrBreakKeyword, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.YieldStatementSyntax)Green).semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal YieldStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitYieldStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitYieldStatement(this);
	}

	public YieldStatementSyntax Update(SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (yieldKeyword != YieldKeyword || returnOrBreakKeyword != ReturnOrBreakKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			YieldStatementSyntax yieldStatementSyntax = SyntaxFactory.YieldStatement(Kind(), yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return yieldStatementSyntax.WithAnnotations(annotations);
			}
			return yieldStatementSyntax;
		}
		return this;
	}

	public YieldStatementSyntax WithYieldKeyword(SyntaxToken yieldKeyword)
	{
		return Update(yieldKeyword, ReturnOrBreakKeyword, Expression, SemicolonToken);
	}

	public YieldStatementSyntax WithReturnOrBreakKeyword(SyntaxToken returnOrBreakKeyword)
	{
		return Update(YieldKeyword, returnOrBreakKeyword, Expression, SemicolonToken);
	}

	public YieldStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(YieldKeyword, ReturnOrBreakKeyword, expression, SemicolonToken);
	}

	public YieldStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(YieldKeyword, ReturnOrBreakKeyword, Expression, semicolonToken);
	}
}
