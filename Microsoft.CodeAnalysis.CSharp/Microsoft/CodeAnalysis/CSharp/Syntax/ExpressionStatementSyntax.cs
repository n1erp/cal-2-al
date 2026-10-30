using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExpressionStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionStatementSyntax)Green).semicolonToken, GetChildPosition(1), GetChildIndex(1));

	public bool AllowsAnyExpression
	{
		get
		{
			SyntaxToken semicolonToken = SemicolonToken;
			if (semicolonToken.IsMissing)
			{
				return !semicolonToken.ContainsDiagnostics;
			}
			return false;
		}
	}

	internal ExpressionStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref expression);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}

	public ExpressionStatementSyntax Update(ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (expression != Expression || semicolonToken != SemicolonToken)
		{
			ExpressionStatementSyntax expressionStatementSyntax = SyntaxFactory.ExpressionStatement(expression, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return expressionStatementSyntax.WithAnnotations(annotations);
			}
			return expressionStatementSyntax;
		}
		return this;
	}

	public ExpressionStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression, SemicolonToken);
	}

	public ExpressionStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Expression, semicolonToken);
	}
}
