using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalViewMoveChangeSyntax : InternalViewChangeBaseSyntax
{
	internal readonly InternalSyntaxToken _changeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _anchor;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalSyntaxNode _views;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ViewMoveChangeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ChangeKeyword => _changeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Anchor => _anchor;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Views => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_views));

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalViewMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode views, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalViewMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode views, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalViewMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode views, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _changeKeyword, 
			1 => _openParenthesisToken, 
			2 => _anchor, 
			3 => _semicolonToken, 
			4 => _views, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ViewMoveChangeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitViewMoveChange(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitViewMoveChange(this);
	}

	public InternalViewMoveChangeSyntax Update(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views, InternalSyntaxToken closeParenthesisToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken != SemicolonToken || views != Views || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ViewMoveChange(changeKeyword, openParenthesisToken, anchor, semicolonToken, views, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalViewMoveChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken, _views, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalViewMoveChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken, _views, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
