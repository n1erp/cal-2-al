using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalWithStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _withKeywordToken;

	internal readonly InternalCodeExpressionSyntax _withId;

	internal readonly InternalSyntaxToken _doKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => WithStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken WithKeywordToken => _withKeywordToken;

	public InternalCodeExpressionSyntax WithId => _withId;

	public InternalSyntaxToken DoKeywordToken => _doKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalWithStatementSyntax(SyntaxKind kind, InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(withKeywordToken);
		_withKeywordToken = withKeywordToken;
		AdjustFlagsAndWidth(withId);
		_withId = withId;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalWithStatementSyntax(SyntaxKind kind, InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(withKeywordToken);
		_withKeywordToken = withKeywordToken;
		AdjustFlagsAndWidth(withId);
		_withId = withId;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalWithStatementSyntax(SyntaxKind kind, InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(withKeywordToken);
		_withKeywordToken = withKeywordToken;
		AdjustFlagsAndWidth(withId);
		_withId = withId;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_withKeywordToken, 
			1 => _withId, 
			2 => _doKeywordToken, 
			3 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new WithStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWithStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitWithStatement(this);
	}

	public InternalWithStatementSyntax Update(InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		if (withKeywordToken != WithKeywordToken || withId != WithId || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			return InternalSyntaxFactory.WithStatement(withKeywordToken, withId, doKeywordToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalWithStatementSyntax(Kind, _withKeywordToken, _withId, _doKeywordToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalWithStatementSyntax(Kind, _withKeywordToken, _withId, _doKeywordToken, _statement, GetDiagnostics(), annotations);
	}
}
