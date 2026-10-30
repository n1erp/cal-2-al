using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageActionListSyntax : SyntaxNode
{
	private SyntaxNode _areas;

	public SyntaxToken ActionsKeyword => new SyntaxToken(this, ((InternalPageActionListSyntax)Node)._actionsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageActionListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<PageActionAreaSyntax> Areas => new SyntaxList<PageActionAreaSyntax>(GetRed(ref _areas, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageActionListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageActionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _areas, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _areas;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageActionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageActionList(this);
	}

	public PageActionListSyntax Update(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<PageActionAreaSyntax> areas, SyntaxToken closeBraceToken)
	{
		if (actionsKeyword != ActionsKeyword || openBraceToken != OpenBraceToken || areas != Areas || closeBraceToken != CloseBraceToken)
		{
			PageActionListSyntax pageActionListSyntax = SyntaxFactory.PageActionList(actionsKeyword, openBraceToken, areas, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageActionListSyntax.WithAnnotations(annotations);
			}
			return pageActionListSyntax;
		}
		return this;
	}

	public PageActionListSyntax WithActionsKeyword(SyntaxToken actionsKeyword)
	{
		return Update(actionsKeyword, OpenBraceToken, Areas, CloseBraceToken);
	}

	public PageActionListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionsKeyword, openBraceToken, Areas, CloseBraceToken);
	}

	public PageActionListSyntax WithAreas(SyntaxList<PageActionAreaSyntax> areas)
	{
		return Update(ActionsKeyword, OpenBraceToken, areas, CloseBraceToken);
	}

	public PageActionListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionsKeyword, OpenBraceToken, Areas, closeBraceToken);
	}

	public PageActionListSyntax AddAreas(params PageActionAreaSyntax[] items)
	{
		return WithAreas(Areas.AddRange(items));
	}
}
