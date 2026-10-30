using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QuerySyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private QueryElementsSyntax _elements;

	private SyntaxNode _members;

	public SyntaxToken QueryKeyword => new SyntaxToken(this, ((InternalQuerySyntax)Node)._queryKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalQuerySyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public QueryElementsSyntax Elements => GetRed(ref _elements, 5);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 6));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalQuerySyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal QuerySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _propertyList, 4), 
			5 => GetRed(ref _elements, 5), 
			6 => GetRed(ref _members, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _propertyList, 
			5 => _elements, 
			6 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQuery(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQuery(this);
	}

	public QuerySyntax Update(SyntaxToken queryKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, QueryElementsSyntax elements, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (queryKeyword != QueryKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || elements != Elements || members != Members || closeBraceToken != CloseBraceToken)
		{
			QuerySyntax querySyntax = SyntaxFactory.Query(queryKeyword, objectId, name, openBraceToken, propertyList, elements, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return querySyntax.WithAnnotations(annotations);
			}
			return querySyntax;
		}
		return this;
	}

	public QuerySyntax WithQueryKeyword(SyntaxToken queryKeyword)
	{
		return Update(queryKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(QueryKeyword, objectId, Name, OpenBraceToken, PropertyList, Elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithName(IdentifierNameSyntax name)
	{
		return Update(QueryKeyword, ObjectId, name, OpenBraceToken, PropertyList, Elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(QueryKeyword, ObjectId, Name, openBraceToken, PropertyList, Elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(QueryKeyword, ObjectId, Name, OpenBraceToken, propertyList, Elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithElements(QueryElementsSyntax elements)
	{
		return Update(QueryKeyword, ObjectId, Name, OpenBraceToken, PropertyList, elements, Members, CloseBraceToken);
	}

	public QuerySyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(QueryKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Elements, members, CloseBraceToken);
	}

	public QuerySyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(QueryKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Elements, Members, closeBraceToken);
	}

	public QuerySyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public QuerySyntax AddElementsDataItems(params QueryDataItemSyntax[] items)
	{
		QueryElementsSyntax queryElementsSyntax = Elements ?? SyntaxFactory.QueryElements();
		return WithElements(queryElementsSyntax.WithDataItems(queryElementsSyntax.DataItems.AddRange(items)));
	}

	public QuerySyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
