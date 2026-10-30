using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageViewListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _viewsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _views;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageViewListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ViewsKeyword => _viewsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalPageViewSyntax> Views => new InternalSyntaxList<InternalPageViewSyntax>(_views);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
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

	internal InternalPageViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
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

	internal InternalPageViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode views, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
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
			0 => _viewsKeyword, 
			1 => _openBraceToken, 
			2 => _views, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageViewListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageViewList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageViewList(this);
	}

	public InternalPageViewListSyntax Update(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		if (viewsKeyword != ViewsKeyword || openBraceToken != OpenBraceToken || views != Views || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageViewList(viewsKeyword, openBraceToken, views, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageViewListSyntax(Kind, _viewsKeyword, _openBraceToken, _views, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageViewListSyntax(Kind, _viewsKeyword, _openBraceToken, _views, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
