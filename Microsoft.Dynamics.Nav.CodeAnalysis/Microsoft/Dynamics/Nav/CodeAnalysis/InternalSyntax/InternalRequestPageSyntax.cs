using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalRequestPageSyntax : InternalObjectSyntax
{
	internal readonly InternalSyntaxToken _requestPageKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalPageLayoutSyntax _layout;

	internal readonly InternalPageActionListSyntax _actions;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => RequestPageSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken RequestPageKeyword => _requestPageKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalPageLayoutSyntax Layout => _layout;

	public InternalPageActionListSyntax Actions => _actions;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalRequestPageSyntax(SyntaxKind kind, InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(requestPageKeyword);
		_requestPageKeyword = requestPageKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
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
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalRequestPageSyntax(SyntaxKind kind, InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(requestPageKeyword);
		_requestPageKeyword = requestPageKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
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
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalRequestPageSyntax(SyntaxKind kind, InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(requestPageKeyword);
		_requestPageKeyword = requestPageKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
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
			0 => _requestPageKeyword, 
			1 => _openBraceToken, 
			2 => _name, 
			3 => _propertyList, 
			4 => _layout, 
			5 => _actions, 
			6 => _members, 
			7 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new RequestPageSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRequestPage(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitRequestPage(this);
	}

	public InternalRequestPageSyntax Update(InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (requestPageKeyword != RequestPageKeyword || openBraceToken != OpenBraceToken || name != Name || propertyList != PropertyList || layout != Layout || actions != Actions || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.RequestPage(requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalRequestPageSyntax(Kind, _requestPageKeyword, _openBraceToken, _name, _propertyList, _layout, _actions, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalRequestPageSyntax(Kind, _requestPageKeyword, _openBraceToken, _name, _propertyList, _layout, _actions, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
