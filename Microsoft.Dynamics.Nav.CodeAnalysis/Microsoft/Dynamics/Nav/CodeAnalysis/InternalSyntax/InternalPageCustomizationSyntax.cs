using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageCustomizationSyntax : InternalAbstractPageExtensionSyntax
{
	internal readonly InternalSyntaxToken _pageCustomizationKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _customizesKeyword;

	internal readonly InternalObjectNameOrIdSyntax _baseObject;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalPageExtensionLayoutSyntax _layout;

	internal readonly InternalPageExtensionActionListSyntax _actions;

	internal readonly InternalPageExtensionViewListSyntax _views;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageCustomizationSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken PageCustomizationKeyword => _pageCustomizationKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken CustomizesKeyword => _customizesKeyword;

	public override InternalObjectNameOrIdSyntax BaseObject => _baseObject;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalPageExtensionLayoutSyntax Layout => _layout;

	public override InternalPageExtensionActionListSyntax Actions => _actions;

	public override InternalPageExtensionViewListSyntax Views => _views;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageCustomizationSyntax(SyntaxKind kind, InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(pageCustomizationKeyword);
		_pageCustomizationKeyword = pageCustomizationKeyword;
		if (objectId != null)
		{
			AdjustFlagsAndWidth(objectId);
			_objectId = objectId;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(customizesKeyword);
		_customizesKeyword = customizesKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
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

	internal InternalPageCustomizationSyntax(SyntaxKind kind, InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(pageCustomizationKeyword);
		_pageCustomizationKeyword = pageCustomizationKeyword;
		if (objectId != null)
		{
			AdjustFlagsAndWidth(objectId);
			_objectId = objectId;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(customizesKeyword);
		_customizesKeyword = customizesKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
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

	internal InternalPageCustomizationSyntax(SyntaxKind kind, InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(pageCustomizationKeyword);
		_pageCustomizationKeyword = pageCustomizationKeyword;
		if (objectId != null)
		{
			AdjustFlagsAndWidth(objectId);
			_objectId = objectId;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(customizesKeyword);
		_customizesKeyword = customizesKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
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
			0 => _pageCustomizationKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _customizesKeyword, 
			4 => _baseObject, 
			5 => _openBraceToken, 
			6 => _propertyList, 
			7 => _layout, 
			8 => _actions, 
			9 => _views, 
			10 => _members, 
			11 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageCustomizationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageCustomization(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageCustomization(this);
	}

	public InternalPageCustomizationSyntax Update(InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (pageCustomizationKeyword != PageCustomizationKeyword || objectId != ObjectId || name != Name || customizesKeyword != CustomizesKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || layout != Layout || actions != Actions || views != Views || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageCustomization(pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageCustomizationSyntax(Kind, _pageCustomizationKeyword, _objectId, _name, _customizesKeyword, _baseObject, _openBraceToken, _propertyList, _layout, _actions, _views, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageCustomizationSyntax(Kind, _pageCustomizationKeyword, _objectId, _name, _customizesKeyword, _baseObject, _openBraceToken, _propertyList, _layout, _actions, _views, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
