using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class RequestPageSyntax : ObjectSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private PageLayoutSyntax _layout;

	private PageActionListSyntax _actions;

	private SyntaxNode _members;

	public SyntaxToken RequestPageKeyword => new SyntaxToken(this, ((InternalRequestPageSyntax)Node)._requestPageKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalRequestPageSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 3);

	public PageLayoutSyntax Layout => GetRed(ref _layout, 4);

	public PageActionListSyntax Actions => GetRed(ref _actions, 5);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 6));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalRequestPageSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal RequestPageSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			3 => GetRed(ref _propertyList, 3), 
			4 => GetRed(ref _layout, 4), 
			5 => GetRed(ref _actions, 5), 
			6 => GetRed(ref _members, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			3 => _propertyList, 
			4 => _layout, 
			5 => _actions, 
			6 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRequestPage(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitRequestPage(this);
	}

	public RequestPageSyntax Update(SyntaxToken requestPageKeyword, SyntaxToken openBraceToken, IdentifierNameSyntax name, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (requestPageKeyword != RequestPageKeyword || openBraceToken != OpenBraceToken || name != Name || propertyList != PropertyList || layout != Layout || actions != Actions || members != Members || closeBraceToken != CloseBraceToken)
		{
			RequestPageSyntax requestPageSyntax = SyntaxFactory.RequestPage(requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return requestPageSyntax.WithAnnotations(annotations);
			}
			return requestPageSyntax;
		}
		return this;
	}

	public RequestPageSyntax WithRequestPageKeyword(SyntaxToken requestPageKeyword)
	{
		return Update(requestPageKeyword, OpenBraceToken, Name, PropertyList, Layout, Actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(RequestPageKeyword, openBraceToken, Name, PropertyList, Layout, Actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(RequestPageKeyword, OpenBraceToken, name, PropertyList, Layout, Actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(RequestPageKeyword, OpenBraceToken, Name, propertyList, Layout, Actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithLayout(PageLayoutSyntax layout)
	{
		return Update(RequestPageKeyword, OpenBraceToken, Name, PropertyList, layout, Actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithActions(PageActionListSyntax actions)
	{
		return Update(RequestPageKeyword, OpenBraceToken, Name, PropertyList, Layout, actions, Members, CloseBraceToken);
	}

	public RequestPageSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(RequestPageKeyword, OpenBraceToken, Name, PropertyList, Layout, Actions, members, CloseBraceToken);
	}

	public RequestPageSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(RequestPageKeyword, OpenBraceToken, Name, PropertyList, Layout, Actions, Members, closeBraceToken);
	}

	public RequestPageSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public RequestPageSyntax AddLayoutAreas(params PageAreaSyntax[] items)
	{
		PageLayoutSyntax pageLayoutSyntax = Layout ?? SyntaxFactory.PageLayout();
		return WithLayout(pageLayoutSyntax.WithAreas(pageLayoutSyntax.Areas.AddRange(items)));
	}

	public RequestPageSyntax AddActionsAreas(params PageActionAreaSyntax[] items)
	{
		PageActionListSyntax pageActionListSyntax = Actions ?? SyntaxFactory.PageActionList();
		return WithActions(pageActionListSyntax.WithAreas(pageActionListSyntax.Areas.AddRange(items)));
	}

	public RequestPageSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
