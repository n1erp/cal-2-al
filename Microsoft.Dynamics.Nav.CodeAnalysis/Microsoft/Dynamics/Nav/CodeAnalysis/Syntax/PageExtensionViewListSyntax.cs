using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageExtensionViewListSyntax : SyntaxNode
{
	private SyntaxNode _changes;

	public SyntaxToken ViewsKeyword => new SyntaxToken(this, ((InternalPageExtensionViewListSyntax)Node)._viewsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageExtensionViewListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ViewChangeBaseSyntax> Changes => new SyntaxList<ViewChangeBaseSyntax>(GetRed(ref _changes, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageExtensionViewListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageExtensionViewListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPageExtensionViewList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionViewList(this);
	}

	public PageExtensionViewListSyntax Update(SyntaxToken viewsKeyword, SyntaxToken openBraceToken, SyntaxList<ViewChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		if (viewsKeyword != ViewsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			PageExtensionViewListSyntax pageExtensionViewListSyntax = SyntaxFactory.PageExtensionViewList(viewsKeyword, openBraceToken, changes, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageExtensionViewListSyntax.WithAnnotations(annotations);
			}
			return pageExtensionViewListSyntax;
		}
		return this;
	}

	public PageExtensionViewListSyntax WithViewsKeyword(SyntaxToken viewsKeyword)
	{
		return Update(viewsKeyword, OpenBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionViewListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ViewsKeyword, openBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionViewListSyntax WithChanges(SyntaxList<ViewChangeBaseSyntax> changes)
	{
		return Update(ViewsKeyword, OpenBraceToken, changes, CloseBraceToken);
	}

	public PageExtensionViewListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ViewsKeyword, OpenBraceToken, Changes, closeBraceToken);
	}

	public PageExtensionViewListSyntax AddChanges(params ViewChangeBaseSyntax[] items)
	{
		return WithChanges(Changes.AddRange(items));
	}
}
