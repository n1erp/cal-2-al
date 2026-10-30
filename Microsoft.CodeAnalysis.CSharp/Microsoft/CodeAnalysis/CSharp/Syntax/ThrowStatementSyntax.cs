using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ThrowStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken ThrowKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ThrowStatementSyntax)Green).throwKeyword, Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ThrowStatementSyntax)Green).semicolonToken, GetChildPosition(2), GetChildIndex(2));

	internal ThrowStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitThrowStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowStatement(this);
	}

	public ThrowStatementSyntax Update(SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (throwKeyword != ThrowKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ThrowStatementSyntax throwStatementSyntax = SyntaxFactory.ThrowStatement(throwKeyword, expression, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return throwStatementSyntax.WithAnnotations(annotations);
			}
			return throwStatementSyntax;
		}
		return this;
	}

	public ThrowStatementSyntax WithThrowKeyword(SyntaxToken throwKeyword)
	{
		return Update(throwKeyword, Expression, SemicolonToken);
	}

	public ThrowStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ThrowKeyword, expression, SemicolonToken);
	}

	public ThrowStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ThrowKeyword, Expression, semicolonToken);
	}
}
