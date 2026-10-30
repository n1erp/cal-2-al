using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ActionMoveChangeSyntax : ActionChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _actions;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalActionMoveChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalActionMoveChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalActionMoveChangeSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public SeparatedSyntaxList<IdentifierNameSyntax> Actions
	{
		get
		{
			SyntaxNode red = GetRed(ref _actions, 4);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(4));
			}
			return default;
		}
	}

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalActionMoveChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ActionMoveChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			4 => GetRed(ref _actions, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			4 => _actions, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitActionMoveChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitActionMoveChange(this);
	}

	public ActionMoveChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> actions, SyntaxToken closeParenthesisToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken != SemicolonToken || actions != Actions || closeParenthesisToken != CloseParenthesisToken)
		{
			ActionMoveChangeSyntax actionMoveChangeSyntax = SyntaxFactory.ActionMoveChange(changeKeyword, openParenthesisToken, anchor, semicolonToken, actions, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return actionMoveChangeSyntax.WithAnnotations(annotations);
			}
			return actionMoveChangeSyntax;
		}
		return this;
	}

	public ActionMoveChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Actions, CloseParenthesisToken);
	}

	public ActionMoveChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, SemicolonToken, Actions, CloseParenthesisToken);
	}

	public ActionMoveChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, SemicolonToken, Actions, CloseParenthesisToken);
	}

	public ActionMoveChangeSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, semicolonToken, Actions, CloseParenthesisToken);
	}

	public ActionMoveChangeSyntax WithActions(SeparatedSyntaxList<IdentifierNameSyntax> actions)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, actions, CloseParenthesisToken);
	}

	public ActionMoveChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Actions, closeParenthesisToken);
	}

	public ActionMoveChangeSyntax AddActions(params IdentifierNameSyntax[] items)
	{
		return WithActions(Actions.AddRange(items));
	}
}
