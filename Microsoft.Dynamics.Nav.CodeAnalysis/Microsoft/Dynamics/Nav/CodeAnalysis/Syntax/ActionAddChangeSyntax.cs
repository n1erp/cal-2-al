using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ActionAddChangeSyntax : ActionChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _actions;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalActionAddChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalActionAddChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalActionAddChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalActionAddChangeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxList<ActionBaseSyntax> Actions => new SyntaxList<ActionBaseSyntax>(GetRed(ref _actions, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalActionAddChangeSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal ActionAddChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			5 => GetRed(ref _actions, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			5 => _actions, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitActionAddChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitActionAddChange(this);
	}

	public ActionAddChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			ActionAddChangeSyntax actionAddChangeSyntax = SyntaxFactory.ActionAddChange(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, actions, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return actionAddChangeSyntax.WithAnnotations(annotations);
			}
			return actionAddChangeSyntax;
		}
		return this;
	}

	public ActionAddChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, CloseParenthesisToken, OpenBraceToken, Actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, closeParenthesisToken, OpenBraceToken, Actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, openBraceToken, Actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithActions(SyntaxList<ActionBaseSyntax> actions)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, actions, CloseBraceToken);
	}

	public ActionAddChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Actions, closeBraceToken);
	}

	public ActionAddChangeSyntax AddActions(params ActionBaseSyntax[] items)
	{
		return WithActions(Actions.AddRange(items));
	}
}
