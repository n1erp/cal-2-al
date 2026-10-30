using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryDataItemSyntax : QueryDataItemElementSyntax
{
	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _dataItemTable;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _elements;

	public SyntaxToken DataItemKeyword => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._dataItemKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public ObjectNameOrIdSyntax DataItemTable => GetRed(ref _dataItemTable, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxList<QueryDataItemElementSyntax> Elements => new SyntaxList<QueryDataItemElementSyntax>(GetRed(ref _elements, 8));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalQueryDataItemSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal QueryDataItemSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _dataItemTable, 4), 
			7 => GetRed(ref _propertyList, 7), 
			8 => GetRed(ref _elements, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			4 => _dataItemTable, 
			7 => _propertyList, 
			8 => _elements, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItem(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItem(this);
	}

	public QueryDataItemSyntax Update(SyntaxToken dataItemKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax dataItemTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<QueryDataItemElementSyntax> elements, SyntaxToken closeBraceToken)
	{
		if (dataItemKeyword != DataItemKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || dataItemTable != DataItemTable || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || elements != Elements || closeBraceToken != CloseBraceToken)
		{
			QueryDataItemSyntax queryDataItemSyntax = SyntaxFactory.QueryDataItem(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryDataItemSyntax.WithAnnotations(annotations);
			}
			return queryDataItemSyntax;
		}
		return this;
	}

	public QueryDataItemSyntax WithDataItemKeyword(SyntaxToken dataItemKeyword)
	{
		return Update(dataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(DataItemKeyword, openParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, semicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithDataItemTable(ObjectNameOrIdSyntax dataItemTable)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, dataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, closeParenthesisToken, OpenBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, openBraceToken, PropertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, propertyList, Elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithElements(SyntaxList<QueryDataItemElementSyntax> elements)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, elements, CloseBraceToken);
	}

	public QueryDataItemSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, closeBraceToken);
	}

	public QueryDataItemSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public QueryDataItemSyntax AddElements(params QueryDataItemElementSyntax[] items)
	{
		return WithElements(Elements.AddRange(items));
	}
}
