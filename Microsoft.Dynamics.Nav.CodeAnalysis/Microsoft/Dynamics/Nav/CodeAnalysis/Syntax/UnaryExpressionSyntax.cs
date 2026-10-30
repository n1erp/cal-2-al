using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class UnaryExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((InternalUnaryExpressionSyntax)Node)._operatorToken, Position, 0);

	public CodeExpressionSyntax Expression => GetRed(ref _expression, 1);

	internal UnaryExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _expression, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnaryExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitUnaryExpression(this);
	}

	public UnaryExpressionSyntax Update(SyntaxToken operatorToken, CodeExpressionSyntax expression)
	{
		if (operatorToken != OperatorToken || expression != Expression)
		{
			UnaryExpressionSyntax unaryExpressionSyntax = SyntaxFactory.UnaryExpression(Kind, operatorToken, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return unaryExpressionSyntax.WithAnnotations(annotations);
			}
			return unaryExpressionSyntax;
		}
		return this;
	}

	public UnaryExpressionSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(operatorToken, Expression);
	}

	public UnaryExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(OperatorToken, expression);
	}
}
