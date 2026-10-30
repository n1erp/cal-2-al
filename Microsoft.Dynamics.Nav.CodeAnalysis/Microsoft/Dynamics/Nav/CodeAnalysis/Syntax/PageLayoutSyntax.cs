using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageLayoutSyntax : SyntaxNode
{
	private SyntaxNode _areas;

	public SyntaxToken LayoutKeyword => new SyntaxToken(this, ((InternalPageLayoutSyntax)Node)._layoutKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageLayoutSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<PageAreaSyntax> Areas => new SyntaxList<PageAreaSyntax>(GetRed(ref _areas, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageLayoutSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageLayoutSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPageLayout(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageLayout(this);
	}

	public PageLayoutSyntax Update(SyntaxToken layoutKeyword, SyntaxToken openBraceToken, SyntaxList<PageAreaSyntax> areas, SyntaxToken closeBraceToken)
	{
		if (layoutKeyword != LayoutKeyword || openBraceToken != OpenBraceToken || areas != Areas || closeBraceToken != CloseBraceToken)
		{
			PageLayoutSyntax pageLayoutSyntax = SyntaxFactory.PageLayout(layoutKeyword, openBraceToken, areas, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageLayoutSyntax.WithAnnotations(annotations);
			}
			return pageLayoutSyntax;
		}
		return this;
	}

	public PageLayoutSyntax WithLayoutKeyword(SyntaxToken layoutKeyword)
	{
		return Update(layoutKeyword, OpenBraceToken, Areas, CloseBraceToken);
	}

	public PageLayoutSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(LayoutKeyword, openBraceToken, Areas, CloseBraceToken);
	}

	public PageLayoutSyntax WithAreas(SyntaxList<PageAreaSyntax> areas)
	{
		return Update(LayoutKeyword, OpenBraceToken, areas, CloseBraceToken);
	}

	public PageLayoutSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(LayoutKeyword, OpenBraceToken, Areas, closeBraceToken);
	}

	public PageLayoutSyntax AddAreas(params PageAreaSyntax[] items)
	{
		return WithAreas(Areas.AddRange(items));
	}
}
