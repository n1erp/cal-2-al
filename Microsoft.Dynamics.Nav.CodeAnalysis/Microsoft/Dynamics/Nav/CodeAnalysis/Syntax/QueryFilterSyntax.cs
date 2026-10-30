using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryFilterSyntax : QueryElementSyntax
{
	private IdentifierNameSyntax _name;

	private IdentifierNameSyntax _relatedField;

	private PropertyListSyntax _propertyList;

	public SyntaxToken FilterKeyword => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._filterKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public override IdentifierNameSyntax RelatedField => GetRed(ref _relatedField, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalQueryFilterSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal QueryFilterSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _relatedField, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _relatedField, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryFilter(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryFilter(this);
	}

	public QueryFilterSyntax Update(SyntaxToken filterKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (filterKeyword != FilterKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || relatedField != RelatedField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			QueryFilterSyntax queryFilterSyntax = SyntaxFactory.QueryFilter(filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryFilterSyntax.WithAnnotations(annotations);
			}
			return queryFilterSyntax;
		}
		return this;
	}

	public QueryFilterSyntax WithFilterKeyword(SyntaxToken filterKeyword)
	{
		return Update(filterKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(FilterKeyword, openParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(FilterKeyword, OpenParenthesisToken, name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, semicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithRelatedField(IdentifierNameSyntax relatedField)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, SemicolonToken, relatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public QueryFilterSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FilterKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public QueryFilterSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
