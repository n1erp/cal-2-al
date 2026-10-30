using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OrderByExpressionSyntax : ExpressionSyntax
{
	private SyntaxNode _sortingFields;

	public SyntaxToken OrderSpecifier => new SyntaxToken(this, ((InternalOrderByExpressionSyntax)Node)._orderSpecifier, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalOrderByExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public SeparatedSyntaxList<IdentifierNameSyntax> SortingFields
	{
		get
		{
			SyntaxNode red = GetRed(ref _sortingFields, 2);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalOrderByExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	internal OrderByExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _sortingFields, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _sortingFields;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOrderByExpression(this);
	}

	public OrderByExpressionSyntax Update(SyntaxToken orderSpecifier, SyntaxToken openParenthesisToken, SeparatedSyntaxList<IdentifierNameSyntax> sortingFields, SyntaxToken closeParenthesisToken)
	{
		if (orderSpecifier != OrderSpecifier || openParenthesisToken != OpenParenthesisToken || sortingFields != SortingFields || closeParenthesisToken != CloseParenthesisToken)
		{
			OrderByExpressionSyntax orderByExpressionSyntax = SyntaxFactory.OrderByExpression(orderSpecifier, openParenthesisToken, sortingFields, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return orderByExpressionSyntax.WithAnnotations(annotations);
			}
			return orderByExpressionSyntax;
		}
		return this;
	}

	public OrderByExpressionSyntax WithOrderSpecifier(SyntaxToken orderSpecifier)
	{
		return Update(orderSpecifier, OpenParenthesisToken, SortingFields, CloseParenthesisToken);
	}

	public OrderByExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(OrderSpecifier, openParenthesisToken, SortingFields, CloseParenthesisToken);
	}

	public OrderByExpressionSyntax WithSortingFields(SeparatedSyntaxList<IdentifierNameSyntax> sortingFields)
	{
		return Update(OrderSpecifier, OpenParenthesisToken, sortingFields, CloseParenthesisToken);
	}

	public OrderByExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OrderSpecifier, OpenParenthesisToken, SortingFields, closeParenthesisToken);
	}

	public OrderByExpressionSyntax AddSortingFields(params IdentifierNameSyntax[] items)
	{
		return WithSortingFields(SortingFields.AddRange(items));
	}
}
