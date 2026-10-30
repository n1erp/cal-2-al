using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ReturnStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken ReturnKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReturnStatementSyntax)Green).returnKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReturnStatementSyntax)Green).semicolonToken, GetChildPosition(2), GetChildIndex(2));

	internal ReturnStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitReturnStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitReturnStatement(this);
	}

	public ReturnStatementSyntax Update(SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (returnKeyword != ReturnKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ReturnStatementSyntax returnStatementSyntax = SyntaxFactory.ReturnStatement(returnKeyword, expression, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return returnStatementSyntax.WithAnnotations(annotations);
			}
			return returnStatementSyntax;
		}
		return this;
	}

	public ReturnStatementSyntax WithReturnKeyword(SyntaxToken returnKeyword)
	{
		return Update(returnKeyword, Expression, SemicolonToken);
	}

	public ReturnStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ReturnKeyword, expression, SemicolonToken);
	}

	public ReturnStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ReturnKeyword, Expression, semicolonToken);
	}
}
