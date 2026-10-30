using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalWhileStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _whileKeywordToken;

	internal readonly InternalCodeExpressionSyntax _condition;

	internal readonly InternalSyntaxToken _doKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => WhileStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken WhileKeywordToken => _whileKeywordToken;

	public InternalCodeExpressionSyntax Condition => _condition;

	public InternalSyntaxToken DoKeywordToken => _doKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalWhileStatementSyntax(SyntaxKind kind, InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whileKeywordToken);
		_whileKeywordToken = whileKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalWhileStatementSyntax(SyntaxKind kind, InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whileKeywordToken);
		_whileKeywordToken = whileKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalWhileStatementSyntax(SyntaxKind kind, InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whileKeywordToken);
		_whileKeywordToken = whileKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_whileKeywordToken, 
			1 => _condition, 
			2 => _doKeywordToken, 
			3 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new WhileStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitWhileStatement(this);
	}

	public InternalWhileStatementSyntax Update(InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		if (whileKeywordToken != WhileKeywordToken || condition != Condition || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			return InternalSyntaxFactory.WhileStatement(whileKeywordToken, condition, doKeywordToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalWhileStatementSyntax(Kind, _whileKeywordToken, _condition, _doKeywordToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalWhileStatementSyntax(Kind, _whileKeywordToken, _condition, _doKeywordToken, _statement, GetDiagnostics(), annotations);
	}
}
