using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ViewMoveChangeSyntax : ViewChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _views;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalViewMoveChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalViewMoveChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalViewMoveChangeSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public SeparatedSyntaxList<IdentifierNameSyntax> Views
	{
		get
		{
			SyntaxNode red = GetRed(ref _views, 4);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(4));
			}
			return default;
		}
	}

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalViewMoveChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ViewMoveChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			4 => GetRed(ref _views, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			4 => _views, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitViewMoveChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitViewMoveChange(this);
	}

	public ViewMoveChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> views, SyntaxToken closeParenthesisToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken != SemicolonToken || views != Views || closeParenthesisToken != CloseParenthesisToken)
		{
			ViewMoveChangeSyntax viewMoveChangeSyntax = SyntaxFactory.ViewMoveChange(changeKeyword, openParenthesisToken, anchor, semicolonToken, views, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return viewMoveChangeSyntax.WithAnnotations(annotations);
			}
			return viewMoveChangeSyntax;
		}
		return this;
	}

	public ViewMoveChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Views, CloseParenthesisToken);
	}

	public ViewMoveChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, SemicolonToken, Views, CloseParenthesisToken);
	}

	public ViewMoveChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, SemicolonToken, Views, CloseParenthesisToken);
	}

	public ViewMoveChangeSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, semicolonToken, Views, CloseParenthesisToken);
	}

	public ViewMoveChangeSyntax WithViews(SeparatedSyntaxList<IdentifierNameSyntax> views)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, views, CloseParenthesisToken);
	}

	public ViewMoveChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Views, closeParenthesisToken);
	}

	public ViewMoveChangeSyntax AddViews(params IdentifierNameSyntax[] items)
	{
		return WithViews(Views.AddRange(items));
	}
}
