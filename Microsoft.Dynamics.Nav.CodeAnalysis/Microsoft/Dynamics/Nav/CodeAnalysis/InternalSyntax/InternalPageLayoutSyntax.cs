using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageLayoutSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _layoutKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _areas;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageLayoutSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken LayoutKeyword => _layoutKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalPageAreaSyntax> Areas => new InternalSyntaxList<InternalPageAreaSyntax>(_areas);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode areas, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (areas != null)
		{
			AdjustFlagsAndWidth(areas);
			_areas = areas;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _layoutKeyword, 
			1 => _openBraceToken, 
			2 => _areas, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageLayoutSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageLayout(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageLayout(this);
	}

	public InternalPageLayoutSyntax Update(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		if (layoutKeyword != LayoutKeyword || openBraceToken != OpenBraceToken || areas != Areas || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageLayout(layoutKeyword, openBraceToken, areas, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageLayoutSyntax(Kind, _layoutKeyword, _openBraceToken, _areas, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageLayoutSyntax(Kind, _layoutKeyword, _openBraceToken, _areas, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
