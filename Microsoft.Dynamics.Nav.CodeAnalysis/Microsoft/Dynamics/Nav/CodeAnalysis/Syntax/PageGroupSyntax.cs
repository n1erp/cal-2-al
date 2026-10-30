using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageGroupSyntax : ControlGroupBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _controls;

	private GroupActionListSyntax _actionList;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageGroupSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageGroupSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageGroupSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageGroupSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<ControlBaseSyntax> Controls => new SyntaxList<ControlBaseSyntax>(GetRed(ref _controls, 6));

	public GroupActionListSyntax ActionList => GetRed(ref _actionList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageGroupSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal PageGroupSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _controls, 6), 
			7 => GetRed(ref _actionList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _controls, 
			7 => _actionList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageGroup(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageGroup(this);
	}

	public PageGroupSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls, GroupActionListSyntax actionList, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || controls != Controls || actionList != ActionList || closeBraceToken != CloseBraceToken)
		{
			PageGroupSyntax pageGroupSyntax = SyntaxFactory.PageGroup(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, actionList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageGroupSyntax.WithAnnotations(annotations);
			}
			return pageGroupSyntax;
		}
		return this;
	}

	public PageGroupSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithControls(SyntaxList<ControlBaseSyntax> controls)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, controls, ActionList, CloseBraceToken);
	}

	public PageGroupSyntax WithActionList(GroupActionListSyntax actionList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, actionList, CloseBraceToken);
	}

	public PageGroupSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, ActionList, closeBraceToken);
	}

	public PageGroupSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageGroupSyntax AddControls(params ControlBaseSyntax[] items)
	{
		return WithControls(Controls.AddRange(items));
	}

	public PageGroupSyntax AddActionListActions(params ActionBaseSyntax[] items)
	{
		GroupActionListSyntax groupActionListSyntax = ActionList ?? SyntaxFactory.GroupActionList();
		return WithActionList(groupActionListSyntax.WithActions(groupActionListSyntax.Actions.AddRange(items)));
	}
}
