using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ViewAddChangeSyntax : ViewChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _views;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalViewAddChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalViewAddChangeSyntax)Node)._openParenthesisToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public override SyntaxToken CloseParenthesisToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalViewAddChangeSyntax)Node)._closeParenthesisToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalViewAddChangeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxList<PageViewSyntax> Views => new SyntaxList<PageViewSyntax>(GetRed(ref _views, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalViewAddChangeSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal ViewAddChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			5 => GetRed(ref _views, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			5 => _views, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitViewAddChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitViewAddChange(this);
	}

	public ViewAddChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<PageViewSyntax> views, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || views != Views || closeBraceToken != CloseBraceToken)
		{
			ViewAddChangeSyntax viewAddChangeSyntax = SyntaxFactory.ViewAddChange(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return viewAddChangeSyntax.WithAnnotations(annotations);
			}
			return viewAddChangeSyntax;
		}
		return this;
	}

	public ViewAddChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, CloseParenthesisToken, OpenBraceToken, Views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, closeParenthesisToken, OpenBraceToken, Views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, openBraceToken, Views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithViews(SyntaxList<PageViewSyntax> views)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, views, CloseBraceToken);
	}

	public ViewAddChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Views, closeBraceToken);
	}

	public ViewAddChangeSyntax AddViews(params PageViewSyntax[] items)
	{
		return WithViews(Views.AddRange(items));
	}
}
