using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageExtensionViewListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _viewsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _changes;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageExtensionViewListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ViewsKeyword => _viewsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalViewChangeBaseSyntax> Changes => new InternalSyntaxList<InternalViewChangeBaseSyntax>(_changes);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageExtensionViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageExtensionViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageExtensionViewListSyntax(SyntaxKind kind, InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(viewsKeyword);
		_viewsKeyword = viewsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
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
			2 => _changes, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageExtensionViewListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionViewList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionViewList(this);
	}

	public InternalPageExtensionViewListSyntax Update(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalViewChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		if (viewsKeyword != ViewsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageExtensionViewList(viewsKeyword, openBraceToken, changes, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageExtensionViewListSyntax(Kind, _viewsKeyword, _openBraceToken, _changes, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageExtensionViewListSyntax(Kind, _viewsKeyword, _openBraceToken, _changes, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
