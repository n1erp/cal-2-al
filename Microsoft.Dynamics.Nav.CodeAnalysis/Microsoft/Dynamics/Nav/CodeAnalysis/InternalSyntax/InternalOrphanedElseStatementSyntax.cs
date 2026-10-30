using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOrphanedElseStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _elseKeywordToken;

	internal readonly InternalStatementSyntax _elseStatement;

	internal override SyntaxNodeDefinition Definition => OrphanedElseStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ElseKeywordToken => _elseKeywordToken;

	public InternalStatementSyntax ElseStatement => _elseStatement;

	internal InternalOrphanedElseStatementSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseStatement);
		_elseStatement = elseStatement;
	}

	internal InternalOrphanedElseStatementSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseStatement);
		_elseStatement = elseStatement;
	}

	internal InternalOrphanedElseStatementSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		AdjustFlagsAndWidth(elseStatement);
		_elseStatement = elseStatement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_elseKeywordToken, 
			1 => _elseStatement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OrphanedElseStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrphanedElseStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOrphanedElseStatement(this);
	}

	public InternalOrphanedElseStatementSyntax Update(InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		if (elseKeywordToken != ElseKeywordToken || elseStatement != ElseStatement)
		{
			return InternalSyntaxFactory.OrphanedElseStatement(elseKeywordToken, elseStatement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOrphanedElseStatementSyntax(Kind, _elseKeywordToken, _elseStatement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOrphanedElseStatementSyntax(Kind, _elseKeywordToken, _elseStatement, GetDiagnostics(), annotations);
	}
}
