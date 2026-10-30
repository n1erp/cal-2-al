using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryColumnSyntax : QueryElementSyntax
{
	private IdentifierNameSyntax _name;

	private IdentifierNameSyntax _relatedField;

	private PropertyListSyntax _propertyList;

	public SyntaxToken ColumnKeyword => new SyntaxToken(this, ((InternalQueryColumnSyntax)Node)._columnKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalQueryColumnSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalQueryColumnSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public override IdentifierNameSyntax RelatedField => GetRed(ref _relatedField, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalQueryColumnSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalQueryColumnSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalQueryColumnSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal QueryColumnSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitQueryColumn(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryColumn(this);
	}

	public QueryColumnSyntax Update(SyntaxToken columnKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (columnKeyword != ColumnKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || relatedField != RelatedField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			QueryColumnSyntax queryColumnSyntax = SyntaxFactory.QueryColumn(columnKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryColumnSyntax.WithAnnotations(annotations);
			}
			return queryColumnSyntax;
		}
		return this;
	}

	public QueryColumnSyntax WithColumnKeyword(SyntaxToken columnKeyword)
	{
		return Update(columnKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ColumnKeyword, openParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, semicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithRelatedField(IdentifierNameSyntax relatedField)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, relatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public QueryColumnSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, RelatedField, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public QueryColumnSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
