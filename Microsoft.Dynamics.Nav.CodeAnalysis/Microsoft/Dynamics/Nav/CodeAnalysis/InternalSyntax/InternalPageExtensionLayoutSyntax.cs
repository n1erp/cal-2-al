using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageExtensionLayoutSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _layoutKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _changes;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageExtensionLayoutSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken LayoutKeyword => _layoutKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalControlChangeBaseSyntax> Changes => new InternalSyntaxList<InternalControlChangeBaseSyntax>(_changes);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageExtensionLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
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

	internal InternalPageExtensionLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
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

	internal InternalPageExtensionLayoutSyntax(SyntaxKind kind, InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(layoutKeyword);
		_layoutKeyword = layoutKeyword;
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
			0 => _layoutKeyword, 
			1 => _openBraceToken, 
			2 => _changes, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageExtensionLayoutSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionLayout(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionLayout(this);
	}

	public InternalPageExtensionLayoutSyntax Update(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		if (layoutKeyword != LayoutKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageExtensionLayout(layoutKeyword, openBraceToken, changes, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageExtensionLayoutSyntax(Kind, _layoutKeyword, _openBraceToken, _changes, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageExtensionLayoutSyntax(Kind, _layoutKeyword, _openBraceToken, _changes, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
