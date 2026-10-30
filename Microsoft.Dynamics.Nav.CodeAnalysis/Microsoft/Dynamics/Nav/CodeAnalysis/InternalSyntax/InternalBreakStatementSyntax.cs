using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBreakStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _breakKeyword;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => BreakStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken BreakKeyword => _breakKeyword;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalBreakStatementSyntax(SyntaxKind kind, InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		_breakKeyword = breakKeyword;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalBreakStatementSyntax(SyntaxKind kind, InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		_breakKeyword = breakKeyword;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalBreakStatementSyntax(SyntaxKind kind, InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		_breakKeyword = breakKeyword;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_breakKeyword, 
			1 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BreakStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBreakStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBreakStatement(this);
	}

	public InternalBreakStatementSyntax Update(InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken)
	{
		if (breakKeyword != BreakKeyword || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.BreakStatement(breakKeyword, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBreakStatementSyntax(Kind, _breakKeyword, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBreakStatementSyntax(Kind, _breakKeyword, _semicolonToken, GetDiagnostics(), annotations);
	}
}
