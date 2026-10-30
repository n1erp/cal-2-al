using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageActionGroupSyntax : ActionGroupBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _actions;

	public override SyntaxToken ActionKeyword => new SyntaxToken(this, ((InternalPageActionGroupSyntax)Node)._actionKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageActionGroupSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageActionGroupSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageActionGroupSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<ActionBaseSyntax> Actions => new SyntaxList<ActionBaseSyntax>(GetRed(ref _actions, 6));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageActionGroupSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal PageActionGroupSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _actions, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _actions, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageActionGroup(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageActionGroup(this);
	}

	public PageActionGroupSyntax Update(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		if (actionKeyword != ActionKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			PageActionGroupSyntax pageActionGroupSyntax = SyntaxFactory.PageActionGroup(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageActionGroupSyntax.WithAnnotations(annotations);
			}
			return pageActionGroupSyntax;
		}
		return this;
	}

	public PageActionGroupSyntax WithActionKeyword(SyntaxToken actionKeyword)
	{
		return Update(actionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ActionKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ActionKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithActions(SyntaxList<ActionBaseSyntax> actions)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, actions, CloseBraceToken);
	}

	public PageActionGroupSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, closeBraceToken);
	}

	public PageActionGroupSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageActionGroupSyntax AddActions(params ActionBaseSyntax[] items)
	{
		return WithActions(Actions.AddRange(items));
	}
}
