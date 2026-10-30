using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ParenthesizedExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalParenthesizedExpressionSyntax)Node)._openParenthesisToken, Position, 0);

	public CodeExpressionSyntax Expression => GetRed(ref _expression, 1);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalParenthesizedExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal ParenthesizedExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitParenthesizedExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public ParenthesizedExpressionSyntax Update(SyntaxToken openParenthesisToken, CodeExpressionSyntax expression, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || expression != Expression || closeParenthesisToken != CloseParenthesisToken)
		{
			ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = SyntaxFactory.ParenthesizedExpression(openParenthesisToken, expression, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parenthesizedExpressionSyntax.WithAnnotations(annotations);
			}
			return parenthesizedExpressionSyntax;
		}
		return this;
	}

	public ParenthesizedExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Expression, CloseParenthesisToken);
	}

	public ParenthesizedExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(OpenParenthesisToken, expression, CloseParenthesisToken);
	}

	public ParenthesizedExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Expression, closeParenthesisToken);
	}
}
