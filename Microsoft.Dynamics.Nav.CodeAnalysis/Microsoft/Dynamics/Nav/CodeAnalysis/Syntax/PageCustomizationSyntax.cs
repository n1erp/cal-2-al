using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageCustomizationSyntax : AbstractPageExtensionSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _baseObject;

	private PropertyListSyntax _propertyList;

	private PageExtensionLayoutSyntax _layout;

	private PageExtensionActionListSyntax _actions;

	private PageExtensionViewListSyntax _views;

	private SyntaxNode _members;

	public SyntaxToken PageCustomizationKeyword => new SyntaxToken(this, ((InternalPageCustomizationSyntax)Node)._pageCustomizationKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken CustomizesKeyword => new SyntaxToken(this, ((InternalPageCustomizationSyntax)Node)._customizesKeyword, GetChildPosition(3), GetChildIndex(3));

	public override ObjectNameOrIdSyntax BaseObject => GetRed(ref _baseObject, 4);

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageCustomizationSyntax)Node)._openBraceToken, GetChildPosition(5), GetChildIndex(5));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 6);

	public override PageExtensionLayoutSyntax Layout => GetRed(ref _layout, 7);

	public override PageExtensionActionListSyntax Actions => GetRed(ref _actions, 8);

	public override PageExtensionViewListSyntax Views => GetRed(ref _views, 9);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 10));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageCustomizationSyntax)Node)._closeBraceToken, GetChildPosition(11), GetChildIndex(11));

	internal PageCustomizationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPageCustomization(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageCustomization(this);
	}

	public PageCustomizationSyntax Update(SyntaxToken pageCustomizationKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken customizesKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (pageCustomizationKeyword != PageCustomizationKeyword || objectId != ObjectId || name != Name || customizesKeyword != CustomizesKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || actions != Actions || views != Views || members != Members || closeBraceToken != CloseBraceToken)
		{
			PageCustomizationSyntax pageCustomizationSyntax = SyntaxFactory.PageCustomization(pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageCustomizationSyntax.WithAnnotations(annotations);
			}
			return pageCustomizationSyntax;
		}
		return this;
	}

	public PageCustomizationSyntax WithPageCustomizationKeyword(SyntaxToken pageCustomizationKeyword)
	{
		return Update(pageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(PageCustomizationKeyword, objectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(PageCustomizationKeyword, ObjectId, name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithCustomizesKeyword(SyntaxToken customizesKeyword)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, customizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithBaseObject(ObjectNameOrIdSyntax baseObject)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, baseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, openBraceToken, PropertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, propertyList, Layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithLayout(PageExtensionLayoutSyntax layout)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, layout, Actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithActions(PageExtensionActionListSyntax actions)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, actions, Views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithViews(PageExtensionViewListSyntax views)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, views, Members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, members, CloseBraceToken);
	}

	public PageCustomizationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(PageCustomizationKeyword, ObjectId, Name, CustomizesKeyword, BaseObject, OpenBraceToken, PropertyList, Layout, Actions, Views, Members, closeBraceToken);
	}

	public PageCustomizationSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		PropertyListSyntax propertyListSyntax = PropertyList ?? SyntaxFactory.PropertyList();
		return WithPropertyList(propertyListSyntax.WithProperties(propertyListSyntax.Properties.AddRange(items)));
	}

	public PageCustomizationSyntax AddLayoutChanges(params ControlChangeBaseSyntax[] items)
	{
		PageExtensionLayoutSyntax pageExtensionLayoutSyntax = Layout ?? SyntaxFactory.PageExtensionLayout();
		return WithLayout(pageExtensionLayoutSyntax.WithChanges(pageExtensionLayoutSyntax.Changes.AddRange(items)));
	}

	public PageCustomizationSyntax AddActionsChanges(params ActionChangeBaseSyntax[] items)
	{
		PageExtensionActionListSyntax pageExtensionActionListSyntax = Actions ?? SyntaxFactory.PageExtensionActionList();
		return WithActions(pageExtensionActionListSyntax.WithChanges(pageExtensionActionListSyntax.Changes.AddRange(items)));
	}

	public PageCustomizationSyntax AddViewsChanges(params ViewChangeBaseSyntax[] items)
	{
		PageExtensionViewListSyntax pageExtensionViewListSyntax = Views ?? SyntaxFactory.PageExtensionViewList();
		return WithViews(pageExtensionViewListSyntax.WithChanges(pageExtensionViewListSyntax.Changes.AddRange(items)));
	}

	public PageCustomizationSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
