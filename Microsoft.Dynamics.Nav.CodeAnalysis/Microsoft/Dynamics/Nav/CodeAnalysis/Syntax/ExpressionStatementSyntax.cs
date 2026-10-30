using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ExpressionStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _expression;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalExpressionStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	internal ExpressionStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _expression);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}

	public ExpressionStatementSyntax Update(CodeExpressionSyntax expression, SyntaxToken semicolonToken)
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

	public ExpressionStatementSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, SemicolonToken);
	}

	public ExpressionStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Expression, semicolonToken);
	}
}
