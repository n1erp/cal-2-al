using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalAssertErrorStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _assertErrorKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => AssertErrorStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken AssertErrorKeywordToken => _assertErrorKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalAssertErrorStatementSyntax(SyntaxKind kind, InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(assertErrorKeywordToken);
		_assertErrorKeywordToken = assertErrorKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalAssertErrorStatementSyntax(SyntaxKind kind, InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(assertErrorKeywordToken);
		_assertErrorKeywordToken = assertErrorKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalAssertErrorStatementSyntax(SyntaxKind kind, InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(assertErrorKeywordToken);
		_assertErrorKeywordToken = assertErrorKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_assertErrorKeywordToken, 
			1 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new AssertErrorStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAssertErrorStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitAssertErrorStatement(this);
	}

	public InternalAssertErrorStatementSyntax Update(InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement)
	{
		if (assertErrorKeywordToken != AssertErrorKeywordToken || statement != Statement)
		{
			return InternalSyntaxFactory.AssertErrorStatement(assertErrorKeywordToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalAssertErrorStatementSyntax(Kind, _assertErrorKeywordToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalAssertErrorStatementSyntax(Kind, _assertErrorKeywordToken, _statement, GetDiagnostics(), annotations);
	}
}
