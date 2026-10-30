using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageActionAreaSyntax : ActionGroupBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _actions;

	public override SyntaxToken ActionKeyword => new SyntaxToken(this, ((InternalPageActionAreaSyntax)Node)._actionKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageActionAreaSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageActionAreaSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageActionAreaSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<ActionBaseSyntax> Actions => new SyntaxList<ActionBaseSyntax>(GetRed(ref _actions, 6));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageActionAreaSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal PageActionAreaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPageActionArea(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageActionArea(this);
	}

	public PageActionAreaSyntax Update(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		if (actionKeyword != ActionKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			PageActionAreaSyntax pageActionAreaSyntax = SyntaxFactory.PageActionArea(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageActionAreaSyntax.WithAnnotations(annotations);
			}
			return pageActionAreaSyntax;
		}
		return this;
	}

	public PageActionAreaSyntax WithActionKeyword(SyntaxToken actionKeyword)
	{
		return Update(actionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ActionKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ActionKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithActions(SyntaxList<ActionBaseSyntax> actions)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, actions, CloseBraceToken);
	}

	public PageActionAreaSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Actions, closeBraceToken);
	}

	public PageActionAreaSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageActionAreaSyntax AddActions(params ActionBaseSyntax[] items)
	{
		return WithActions(Actions.AddRange(items));
	}
}
