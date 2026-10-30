using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageViewListSyntax : SyntaxNode
{
	private SyntaxNode _views;

	public SyntaxToken ViewsKeyword => new SyntaxToken(this, ((InternalPageViewListSyntax)Node)._viewsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageViewListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<PageViewSyntax> Views => new SyntaxList<PageViewSyntax>(GetRed(ref _views, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageViewListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageViewListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _views, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _views;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageViewList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageViewList(this);
	}

	public PageViewListSyntax Update(SyntaxToken viewsKeyword, SyntaxToken openBraceToken, SyntaxList<PageViewSyntax> views, SyntaxToken closeBraceToken)
	{
		if (viewsKeyword != ViewsKeyword || openBraceToken != OpenBraceToken || views != Views || closeBraceToken != CloseBraceToken)
		{
			PageViewListSyntax pageViewListSyntax = SyntaxFactory.PageViewList(viewsKeyword, openBraceToken, views, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageViewListSyntax.WithAnnotations(annotations);
			}
			return pageViewListSyntax;
		}
		return this;
	}

	public PageViewListSyntax WithViewsKeyword(SyntaxToken viewsKeyword)
	{
		return Update(viewsKeyword, OpenBraceToken, Views, CloseBraceToken);
	}

	public PageViewListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ViewsKeyword, openBraceToken, Views, CloseBraceToken);
	}

	public PageViewListSyntax WithViews(SyntaxList<PageViewSyntax> views)
	{
		return Update(ViewsKeyword, OpenBraceToken, views, CloseBraceToken);
	}

	public PageViewListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ViewsKeyword, OpenBraceToken, Views, closeBraceToken);
	}

	public PageViewListSyntax AddViews(params PageViewSyntax[] items)
	{
		return WithViews(Views.AddRange(items));
	}
}
