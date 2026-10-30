using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class GroupActionListSyntax : SyntaxNode
{
	private SyntaxNode _actions;

	public SyntaxToken ActionsKeyword => new SyntaxToken(this, ((InternalGroupActionListSyntax)Node)._actionsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalGroupActionListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ActionBaseSyntax> Actions => new SyntaxList<ActionBaseSyntax>(GetRed(ref _actions, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalGroupActionListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal GroupActionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _actions, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _actions;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGroupActionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitGroupActionList(this);
	}

	public GroupActionListSyntax Update(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || actions != Actions || closeBraceToken != CloseBraceToken)
		{
			GroupActionListSyntax groupActionListSyntax = SyntaxFactory.GroupActionList(actionsKeyword, openBraceToken, actions, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return groupActionListSyntax.WithAnnotations(annotations);
			}
			return groupActionListSyntax;
		}
		return this;
	}

	public GroupActionListSyntax WithActionsKeyword(SyntaxToken actionsKeyword)
	{
		return Update(actionsKeyword, OpenBraceToken, Actions, CloseBraceToken);
	}

	public GroupActionListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionsKeyword, openBraceToken, Actions, CloseBraceToken);
	}

	public GroupActionListSyntax WithActions(SyntaxList<ActionBaseSyntax> actions)
	{
		return Update(ActionsKeyword, OpenBraceToken, actions, CloseBraceToken);
	}

	public GroupActionListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionsKeyword, OpenBraceToken, Actions, closeBraceToken);
	}

	public GroupActionListSyntax AddActions(params ActionBaseSyntax[] items)
	{
		return WithActions(Actions.AddRange(items));
	}
}
