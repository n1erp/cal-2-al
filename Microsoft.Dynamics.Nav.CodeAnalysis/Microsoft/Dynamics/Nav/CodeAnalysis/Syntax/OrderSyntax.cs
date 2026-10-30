using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OrderSyntax : PropertyExpressionSyntax
{
	public SyntaxToken OrderToken => new SyntaxToken(this, ((InternalOrderSyntax)Node)._orderToken, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalOrderSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken OrderBy => new SyntaxToken(this, ((InternalOrderSyntax)Node)._orderBy, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalOrderSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	internal OrderSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrder(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOrder(this);
	}

	public OrderSyntax Update(SyntaxToken orderToken, SyntaxToken openParenthesisToken, SyntaxToken orderBy, SyntaxToken closeParenthesisToken)
	{
		if (orderToken != OrderToken || openParenthesisToken != OpenParenthesisToken || orderBy != OrderBy || closeParenthesisToken != CloseParenthesisToken)
		{
			OrderSyntax orderSyntax = SyntaxFactory.Order(orderToken, openParenthesisToken, orderBy, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return orderSyntax.WithAnnotations(annotations);
			}
			return orderSyntax;
		}
		return this;
	}

	public OrderSyntax WithOrderToken(SyntaxToken orderToken)
	{
		return Update(orderToken, OpenParenthesisToken, OrderBy, CloseParenthesisToken);
	}

	public OrderSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(OrderToken, openParenthesisToken, OrderBy, CloseParenthesisToken);
	}

	public OrderSyntax WithOrderBy(SyntaxToken orderBy)
	{
		return Update(OrderToken, OpenParenthesisToken, orderBy, CloseParenthesisToken);
	}

	public OrderSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OrderToken, OpenParenthesisToken, OrderBy, closeParenthesisToken);
	}
}
