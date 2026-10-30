using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private PageLayoutSyntax _layout;

	private PageActionListSyntax _actions;

	private PageViewListSyntax _views;

	private SyntaxNode _members;

	public SyntaxToken PageKeyword => new SyntaxToken(this, ((InternalPageSyntax)Node)._pageKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public PageLayoutSyntax Layout => GetRed(ref _layout, 5);

	public PageActionListSyntax Actions => GetRed(ref _actions, 6);

	public PageViewListSyntax Views => GetRed(ref _views, 7);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 8));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal PageSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
			5 => GetRed(ref _layout, 5), 
			6 => GetRed(ref _actions, 6), 
			7 => GetRed(ref _views, 7), 
			8 => GetRed(ref _members, 8), 
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
			5 => _layout, 
			6 => _actions, 
			7 => _views, 
			8 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPage(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPage(this);
	}

	public PageSyntax Update(SyntaxToken pageKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, PageViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (pageKeyword != PageKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || actions != Actions || views != Views || members != Members || closeBraceToken != CloseBraceToken)
		{
			PageSyntax pageSyntax = SyntaxFactory.Page(pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageSyntax.WithAnnotations(annotations);
			}
			return pageSyntax;
		}
		return this;
	}

	public PageSyntax WithPageKeyword(SyntaxToken pageKeyword)
	{
		return Update(pageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(PageKeyword, objectId, Name, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(PageKeyword, ObjectId, name, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(PageKeyword, ObjectId, Name, openBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, propertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithLayout(PageLayoutSyntax layout)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithActions(PageActionListSyntax actions)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Layout, actions, Views, Members, CloseBraceToken);
	}

	public PageSyntax WithViews(PageViewListSyntax views)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Layout, Actions, views, Members, CloseBraceToken);
	}

	public PageSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Layout, Actions, Views, members, CloseBraceToken);
	}

	public PageSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(PageKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, closeBraceToken);
	}

	public PageSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageSyntax AddLayoutAreas(params PageAreaSyntax[] items)
	{
		PageLayoutSyntax pageLayoutSyntax = Layout ?? SyntaxFactory.PageLayout();
		return WithLayout(pageLayoutSyntax.WithAreas(pageLayoutSyntax.Areas.AddRange(items)));
	}

	public PageSyntax AddActionsAreas(params PageActionAreaSyntax[] items)
	{
		PageActionListSyntax pageActionListSyntax = Actions ?? SyntaxFactory.PageActionList();
		return WithActions(pageActionListSyntax.WithAreas(pageActionListSyntax.Areas.AddRange(items)));
	}

	public PageSyntax AddViewsViews(params PageViewSyntax[] items)
	{
		PageViewListSyntax pageViewListSyntax = Views ?? SyntaxFactory.PageViewList();
		return WithViews(pageViewListSyntax.WithViews(pageViewListSyntax.Views.AddRange(items)));
	}

	public PageSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
