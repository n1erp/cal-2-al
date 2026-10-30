using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageExtensionSyntax : AbstractPageExtensionSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _baseObject;

	private PropertyListSyntax _propertyList;

	private PageExtensionLayoutSyntax _layout;

	private PageExtensionActionListSyntax _actions;

	private PageExtensionViewListSyntax _views;

	private SyntaxNode _members;

	public SyntaxToken PageExtensionKeyword => new SyntaxToken(this, ((InternalPageExtensionSyntax)Node)._pageExtensionKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken ExtendsKeyword => new SyntaxToken(this, ((InternalPageExtensionSyntax)Node)._extendsKeyword, GetChildPosition(3), GetChildIndex(3));

	public override ObjectNameOrIdSyntax BaseObject => GetRed(ref _baseObject, 4);

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageExtensionSyntax)Node)._openBraceToken, GetChildPosition(5), GetChildIndex(5));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 6);

	public override PageExtensionLayoutSyntax Layout => GetRed(ref _layout, 7);

	public override PageExtensionActionListSyntax Actions => GetRed(ref _actions, 8);

	public override PageExtensionViewListSyntax Views => GetRed(ref _views, 9);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 10));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageExtensionSyntax)Node)._closeBraceToken, GetChildPosition(11), GetChildIndex(11));

	internal PageExtensionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _baseObject, 4), 
			6 => GetRed(ref _propertyList, 6), 
			7 => GetRed(ref _layout, 7), 
			8 => GetRed(ref _actions, 8), 
			9 => GetRed(ref _views, 9), 
			10 => GetRed(ref _members, 10), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _baseObject, 
			6 => _propertyList, 
			7 => _layout, 
			8 => _actions, 
			9 => _views, 
			10 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtension(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageExtension(this);
	}

	public PageExtensionSyntax Update(SyntaxToken pageExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (pageExtensionKeyword != PageExtensionKeyword || objectId != ObjectId || name != Name || extendsKeyword != ExtendsKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || actions != Actions || views != Views || members != Members || closeBraceToken != CloseBraceToken)
		{
			PageExtensionSyntax pageExtensionSyntax = SyntaxFactory.PageExtension(pageExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageExtensionSyntax.WithAnnotations(annotations);
			}
			return pageExtensionSyntax;
		}
		return this;
	}

	public PageExtensionSyntax WithPageExtensionKeyword(SyntaxToken pageExtensionKeyword)
	{
		return Update(pageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(PageExtensionKeyword, objectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(PageExtensionKeyword, ObjectId, name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithExtendsKeyword(SyntaxToken extendsKeyword)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, extendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithBaseObject(ObjectNameOrIdSyntax baseObject)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, baseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, openBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, propertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithLayout(PageExtensionLayoutSyntax layout)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithActions(PageExtensionActionListSyntax actions)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, actions, Views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithViews(PageExtensionViewListSyntax views)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, views, Members, CloseBraceToken);
	}

	public PageExtensionSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, members, CloseBraceToken);
	}

	public PageExtensionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(PageExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, closeBraceToken);
	}

	public PageExtensionSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageExtensionSyntax AddLayoutChanges(params ControlChangeBaseSyntax[] items)
	{
		PageExtensionLayoutSyntax pageExtensionLayoutSyntax = Layout ?? SyntaxFactory.PageExtensionLayout();
		return WithLayout(pageExtensionLayoutSyntax.WithChanges(pageExtensionLayoutSyntax.Changes.AddRange(items)));
	}

	public PageExtensionSyntax AddActionsChanges(params ActionChangeBaseSyntax[] items)
	{
		PageExtensionActionListSyntax pageExtensionActionListSyntax = Actions ?? SyntaxFactory.PageExtensionActionList();
		return WithActions(pageExtensionActionListSyntax.WithChanges(pageExtensionActionListSyntax.Changes.AddRange(items)));
	}

	public PageExtensionSyntax AddViewsChanges(params ViewChangeBaseSyntax[] items)
	{
		PageExtensionViewListSyntax pageExtensionViewListSyntax = Views ?? SyntaxFactory.PageExtensionViewList();
		return WithViews(pageExtensionViewListSyntax.WithChanges(pageExtensionViewListSyntax.Changes.AddRange(items)));
	}

	public PageExtensionSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
