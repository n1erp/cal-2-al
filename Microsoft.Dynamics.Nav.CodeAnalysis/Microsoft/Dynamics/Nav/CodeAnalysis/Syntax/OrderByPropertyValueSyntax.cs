using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OrderByPropertyValueSyntax : PropertyValueSyntax
{
	private SyntaxNode _order;

	public SeparatedSyntaxList<OrderByExpressionSyntax> Order
	{
		get
		{
			SyntaxNode red = GetRed(ref _order, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<OrderByExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal OrderByPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _order);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _order;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOrderByPropertyValue(this);
	}

	public OrderByPropertyValueSyntax Update(SeparatedSyntaxList<OrderByExpressionSyntax> order)
	{
		if (order != Order)
		{
			OrderByPropertyValueSyntax orderByPropertyValueSyntax = SyntaxFactory.OrderByPropertyValue(order);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return orderByPropertyValueSyntax.WithAnnotations(annotations);
			}
			return orderByPropertyValueSyntax;
		}
		return this;
	}

	public OrderByPropertyValueSyntax WithOrder(SeparatedSyntaxList<OrderByExpressionSyntax> order)
	{
		return Update(order);
	}

	public OrderByPropertyValueSyntax AddOrder(params OrderByExpressionSyntax[] items)
	{
		return Update(Order.AddRange(items));
	}
}
