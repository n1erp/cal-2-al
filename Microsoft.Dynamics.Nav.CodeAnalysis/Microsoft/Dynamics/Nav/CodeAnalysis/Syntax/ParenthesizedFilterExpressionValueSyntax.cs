using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ParenthesizedFilterExpressionValueSyntax : FilterExpressionValueSyntax
{
	private FilterExpressionValueSyntax _expression;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalParenthesizedFilterExpressionValueSyntax)Node)._openParenthesisToken, Position, 0);

	public FilterExpressionValueSyntax Expression => GetRed(ref _expression, 1);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalParenthesizedFilterExpressionValueSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal ParenthesizedFilterExpressionValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitParenthesizedFilterExpressionValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedFilterExpressionValue(this);
	}

	public ParenthesizedFilterExpressionValueSyntax Update(SyntaxToken openParenthesisToken, FilterExpressionValueSyntax expression, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || expression != Expression || closeParenthesisToken != CloseParenthesisToken)
		{
			ParenthesizedFilterExpressionValueSyntax parenthesizedFilterExpressionValueSyntax = SyntaxFactory.ParenthesizedFilterExpressionValue(openParenthesisToken, expression, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parenthesizedFilterExpressionValueSyntax.WithAnnotations(annotations);
			}
			return parenthesizedFilterExpressionValueSyntax;
		}
		return this;
	}

	public ParenthesizedFilterExpressionValueSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Expression, CloseParenthesisToken);
	}

	public ParenthesizedFilterExpressionValueSyntax WithExpression(FilterExpressionValueSyntax expression)
	{
		return Update(OpenParenthesisToken, expression, CloseParenthesisToken);
	}

	public ParenthesizedFilterExpressionValueSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Expression, closeParenthesisToken);
	}
}
