using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEmptyStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => EmptyStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalEmptyStatementSyntax(SyntaxKind kind, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalEmptyStatementSyntax(SyntaxKind kind, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalEmptyStatementSyntax(SyntaxKind kind, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _semicolonToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EmptyStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEmptyStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEmptyStatement(this);
	}

	public InternalEmptyStatementSyntax Update(InternalSyntaxToken semicolonToken)
	{
		if (semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.EmptyStatement(semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEmptyStatementSyntax(Kind, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEmptyStatementSyntax(Kind, _semicolonToken, GetDiagnostics(), annotations);
	}
}
