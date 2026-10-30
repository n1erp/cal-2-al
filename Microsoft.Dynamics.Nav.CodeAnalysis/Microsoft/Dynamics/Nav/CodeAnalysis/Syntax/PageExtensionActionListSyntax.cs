using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageExtensionActionListSyntax : SyntaxNode
{
	private SyntaxNode _changes;

	public SyntaxToken ActionsKeyword => new SyntaxToken(this, ((InternalPageExtensionActionListSyntax)Node)._actionsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageExtensionActionListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ActionChangeBaseSyntax> Changes => new SyntaxList<ActionChangeBaseSyntax>(GetRed(ref _changes, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageExtensionActionListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageExtensionActionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _changes, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _changes;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionActionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionActionList(this);
	}

	public PageExtensionActionListSyntax Update(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<ActionChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			PageExtensionActionListSyntax pageExtensionActionListSyntax = SyntaxFactory.PageExtensionActionList(actionsKeyword, openBraceToken, changes, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageExtensionActionListSyntax.WithAnnotations(annotations);
			}
			return pageExtensionActionListSyntax;
		}
		return this;
	}

	public PageExtensionActionListSyntax WithActionsKeyword(SyntaxToken actionsKeyword)
	{
		return Update(actionsKeyword, OpenBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionActionListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionsKeyword, openBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionActionListSyntax WithChanges(SyntaxList<ActionChangeBaseSyntax> changes)
	{
		return Update(ActionsKeyword, OpenBraceToken, changes, CloseBraceToken);
	}

	public PageExtensionActionListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionsKeyword, OpenBraceToken, Changes, closeBraceToken);
	}

	public PageExtensionActionListSyntax AddChanges(params ActionChangeBaseSyntax[] items)
	{
		return WithChanges(Changes.AddRange(items));
	}
}
