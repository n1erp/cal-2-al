using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalViewAddChangeSyntax : InternalViewChangeBaseSyntax
{
	internal readonly InternalSyntaxToken _changeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _anchor;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _views;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ViewAddChangeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ChangeKeyword => _changeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Anchor => _anchor;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalPageViewSyntax> Views => new InternalSyntaxList<InternalPageViewSyntax>(_views);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalViewAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalViewAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalViewAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (anchor != null)
		{
			AdjustFlagsAndWidth(anchor);
			_anchor = anchor;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (views != null)
		{
			AdjustFlagsAndWidth(views);
			_views = views;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _changeKeyword, 
			1 => _openParenthesisToken, 
			2 => _anchor, 
			3 => _closeParenthesisToken, 
			4 => _openBraceToken, 
			5 => _views, 
			6 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ViewAddChangeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitViewAddChange(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitViewAddChange(this);
	}

	public InternalViewAddChangeSyntax Update(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || views != Views || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ViewAddChange(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalViewAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _closeParenthesisToken, _openBraceToken, _views, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalViewAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _closeParenthesisToken, _openBraceToken, _views, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
