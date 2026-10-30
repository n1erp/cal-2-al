using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SortingSyntax : PropertyExpressionSyntax
{
	private SyntaxNode _sortingFields;

	public SyntaxToken SortingToken => new SyntaxToken(this, ((InternalSortingSyntax)Node)._sortingToken, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalSortingSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

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

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalSortingSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	internal SortingSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitSorting(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSorting(this);
	}

	public SortingSyntax Update(SyntaxToken sortingToken, SyntaxToken openParenthesisToken, SeparatedSyntaxList<IdentifierNameSyntax> sortingFields, SyntaxToken closeParenthesisToken)
	{
		if (sortingToken != SortingToken || openParenthesisToken != OpenParenthesisToken || sortingFields != SortingFields || closeParenthesisToken != CloseParenthesisToken)
		{
			SortingSyntax sortingSyntax = SyntaxFactory.Sorting(sortingToken, openParenthesisToken, sortingFields, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return sortingSyntax.WithAnnotations(annotations);
			}
			return sortingSyntax;
		}
		return this;
	}

	public SortingSyntax WithSortingToken(SyntaxToken sortingToken)
	{
		return Update(sortingToken, OpenParenthesisToken, SortingFields, CloseParenthesisToken);
	}

	public SortingSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(SortingToken, openParenthesisToken, SortingFields, CloseParenthesisToken);
	}

	public SortingSyntax WithSortingFields(SeparatedSyntaxList<IdentifierNameSyntax> sortingFields)
	{
		return Update(SortingToken, OpenParenthesisToken, sortingFields, CloseParenthesisToken);
	}

	public SortingSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(SortingToken, OpenParenthesisToken, SortingFields, closeParenthesisToken);
	}

	public SortingSyntax AddSortingFields(params IdentifierNameSyntax[] items)
	{
		return WithSortingFields(SortingFields.AddRange(items));
	}
}
