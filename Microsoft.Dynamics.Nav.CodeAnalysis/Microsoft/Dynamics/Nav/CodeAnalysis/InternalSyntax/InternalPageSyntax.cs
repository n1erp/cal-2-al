using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageSyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _pageKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalPageLayoutSyntax _layout;

	internal readonly InternalPageActionListSyntax _actions;

	internal readonly InternalPageViewListSyntax _views;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken PageKeyword => _pageKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalPageLayoutSyntax Layout => _layout;

	public InternalPageActionListSyntax Actions => _actions;

	public InternalPageViewListSyntax Views => _views;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageSyntax(SyntaxKind kind, InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(pageKeyword);
		_pageKeyword = pageKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (layout != null)
		{
			AdjustFlagsAndWidth(layout);
			_layout = layout;
		}
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageSyntax(SyntaxKind kind, InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(pageKeyword);
		_pageKeyword = pageKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (layout != null)
		{
			AdjustFlagsAndWidth(layout);
			_layout = layout;
		}
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageSyntax(SyntaxKind kind, InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(pageKeyword);
		_pageKeyword = pageKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (layout != null)
		{
			AdjustFlagsAndWidth(layout);
			_layout = layout;
		}
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _pageKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _layout, 
			6 => _actions, 
			7 => _views, 
			8 => _members, 
			9 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPage(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPage(this);
	}

	public InternalPageSyntax Update(InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (pageKeyword != PageKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || actions != Actions || views != Views || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Page(pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageSyntax(Kind, _pageKeyword, _objectId, _name, _openBraceToken, _propertyList, _layout, _actions, _views, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageSyntax(Kind, _pageKeyword, _objectId, _name, _openBraceToken, _propertyList, _layout, _actions, _views, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
